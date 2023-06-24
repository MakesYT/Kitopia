using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Core.SDKs.Tools;

public class UWPAPPsTools
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern int GetPackageInfo(
        IntPtr packageFullName,
        int flags,
        ref int bufferLength,
        IntPtr buffer,
        out int count);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern int OpenPackageInfoByFullName(
        string packageFullName,
        uint reserved,
        out IntPtr packageInfoReference);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern int FindPackagesByPackageFamily(
        string packageFamilyName,
        uint packageFilters,
        ref uint count,
        IntPtr packageFullNames,
        ref uint bufferLength,
        IntPtr buffer,
        IntPtr packageProperties);

    // Define the package info structure
    [StructLayout(LayoutKind.Sequential)]
    struct PACKAGE_INFO
    {
        public int reserved;
        public int flags;
        public IntPtr path;
        public IntPtr packageFullName;
        public IntPtr packageFamilyName;
        public PACKAGE_ID packageId;
    }

    // Define the package id structure
    [StructLayout(LayoutKind.Sequential)]
    struct PACKAGE_ID
    {
        public int reserved;
        public AppxPackageArchitecture processorArchitecture;
        public ushort versionRevision;
        public ushort versionBuild;
        public ushort versionMinor;
        public ushort versionMajor;
        public IntPtr name;
        public IntPtr publisher;
        public IntPtr resourceId;
        public IntPtr publisherId;
    }

    // Define the package architecture enum
    enum AppxPackageArchitecture
    {
        x86 = 0,
        Arm = 5,
        x64 = 9,
        Neutral = 11,
        Arm64 = 12
    }

    [StructLayout(LayoutKind.Sequential)]
    public class INET_FIREWALL_APP_CONTAINER
    {
        public uint Count;
        public IntPtr AppContainerList;
        public IntPtr Sid;
        public IntPtr DisplayName;
        public IntPtr Description;
        public IntPtr AppContainerName;
    }

// Import the Firewallapi.dll library
    [DllImport("Firewallapi.dll", SetLastError = true)]
    public static extern uint NetworkIsolationEnumAppContainers(
        uint Flags,
        out uint pdwNumPublicAppCs,
        out IntPtr ppPublicAppCs
    );

    [DllImport("Firewallapi.dll", SetLastError = true)]
    public static extern void NetworkIsolationFreeAppContainers(
        IntPtr pPublicAppCs
    );

// Define the NETISO_FLAG constant
    public const uint NETISO_FLAG_FORCE_COMPUTE_BINARIES = 0x00000001;

// Define a helper method to get a string from a pointer
    public static string GetStringFromPointer(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return null;
        return Marshal.PtrToStringUni(ptr);
    }

// Define a helper method to get an array of INET_FIREWALL_APP_CONTAINER from a pointer
    public static INET_FIREWALL_APP_CONTAINER[] GetAppContainersFromPointer(IntPtr ptr, uint count)
    {
        if (ptr == IntPtr.Zero || count == 0) return null;
        var result = new INET_FIREWALL_APP_CONTAINER[count];
        var size = Marshal.SizeOf(typeof(INET_FIREWALL_APP_CONTAINER));
        for (int i = 0; i < count; i++)
        {
            result[i] = Marshal.PtrToStructure<INET_FIREWALL_APP_CONTAINER>(ptr + i * size);
        }

        return result;
    }

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern int SHLoadIndirectString(string pszSource, System.Text.StringBuilder pszOutBuf, int cchOutBuf,
        IntPtr ppvReserved);

    public static void GetAll(List<SearchViewItem> items)
    {
        uint numAppContainers;
        IntPtr appContainersPtr;

        uint result1 = NetworkIsolationEnumAppContainers(NETISO_FLAG_FORCE_COMPUTE_BINARIES, out numAppContainers,
            out appContainersPtr);

        if (result1 == 0) // ERROR_SUCCESS
        {
            Console.WriteLine($"Found {numAppContainers} app containers.");

            var appContainers = GetAppContainersFromPointer(appContainersPtr, numAppContainers);

            if (appContainers != null)
            {
                foreach (var appContainer in appContainers)
                {
                    string? name = Marshal.PtrToStringAuto(appContainer.AppContainerName);
                    string? displayName = Marshal.PtrToStringAuto(appContainer.DisplayName);
                    string? description = Marshal.PtrToStringAuto(appContainer.Description);
                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(displayName) ||
                        string.IsNullOrWhiteSpace(description))
                    {
                        continue;
                    }

                    if (name == "ࠁ" || displayName == "ࠁ" || description == "ࠁ" || name.Length <= 1 ||
                        displayName.Length <= 1 || description.Length <= 1)
                    {
                        continue;
                    }

                    if (!Regex.IsMatch(displayName, "^[A-Za-z0-9_.]+$") &&
                        !Regex.IsMatch(description, "^[A-Za-z0-9_.]+$"))
                    {
                        continue;
                    }

                    if (name.Length > 1)
                    {
                        if (name.Contains("ms-resource:"))
                        {
                            System.Text.StringBuilder displayName1 = new System.Text.StringBuilder(256);

                            // Call SHLoadIndirectString with the resource string and the StringBuilder
                            int result = SHLoadIndirectString(name, displayName1, displayName1.Capacity, IntPtr.Zero);
                            name = displayName1.ToString();
                        }

                        Console.WriteLine($"Name:{name},\nDName:{displayName}\nD:{description}\n");
                        HashSet<string> keys = new HashSet<string>();
                        AppTools.NameSolver(keys, name);
                        items.Add(new SearchViewItem()
                        {
                            Url = displayName,
                            FileName = name,
                            FileType = FileType.UWP应用,
                            OnlyKey = displayName,
                            Keys = keys,
                            Icon = null,
                            IsVisible = true
                        });
                    }
                }

                // Free the memory
                NetworkIsolationFreeAppContainers(appContainersPtr);
            }
        }
        else // Error value
        {
            Console.WriteLine($"Error: {result1}");
        }
    }
}