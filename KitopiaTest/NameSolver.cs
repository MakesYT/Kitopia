using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;
using Silk.NET.DXGI;
using Format = Silk.NET.DXGI.Format;
using PresentParameters = Silk.NET.DXGI.PresentParameters;

namespace KitopiaTest;

public class NameSolver
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var d3d9 = D3D9.GetApi();
        var d3d = d3d9.Direct3DCreate9(SDK);

        var pp = new PresentParameters
        {
            Wi = true,
            SwapEffect = SwapEffect.Discard,
            BackBufferFormat = Format.A8R8G8B8,
            BackBufferWidth = 1920,
            BackBufferHeight = 1080
        };

        nint devicePtr;
        d3d.CreateDevice(0, DeviceType.HAL, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing, ref pp, &devicePtr);
        var device = new IDirect3DDevice9(devicePtr);

        // Create surface for the screenshot
        nint surfacePtr;
        device.CreateOffscreenPlainSurface(1920, 1080, Format.A8R8G8B8, Pool.SystemMem, &surfacePtr, IntPtr.Zero);
        var surface = new IDirect3DSurface9(surfacePtr);

        // Capture the screen
        device.GetFrontBufferData(0, surface.NativePointer);

        // Lock the surface
        var lockedRect = new D3DLockedRect();
        surface.LockRect(&lockedRect, IntPtr.Zero, 0);

        // Copy the data
        var data = new byte[1920 * 1080 * 4];
        Marshal.Copy(lockedRect.PBits, data, 0, data.Length);

        // Unlock the surface
        surface.UnlockRect();

        // Save the data as a bitmap file
        using (var fs = new FileStream("screenshot.bmp", FileMode.Create))
        {
            var bmpFileHeader = new byte[]
            {
                0x42, 0x4D, 0x36, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
                0x00, 0x00, 0x80, 0x07, 0x00, 0x00, 0x38, 0x04, 0x00, 0x00, 0x01, 0x00, 0x20, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            fs.Write(bmpFileHeader, 0, bmpFileHeader.Length);
            fs.Write(data, 0, data.Length);
        }

        Console.WriteLine("Screenshot saved as screenshot.bmp");

        // Clean up
        surface.Dispose();
        device.Dispose();
        d3d.Dispose();
    }
}