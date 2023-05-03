using System.Text;

namespace Core.SDKs
{
    public class GetInstalledSoftware
    {

        public static string GetLnkTargetPath(string filepath)
        {
            using (var br = new BinaryReader(System.IO.File.OpenRead(filepath)))
            {
                // skip the first 20 bytes (HeaderSize and LinkCLSID)
                br.ReadBytes(0x14);
                // read the LinkFlags structure (4 bytes)
                uint lflags = br.ReadUInt32();
                // if the HasLinkTargetIDList bit is set then skip the stored IDList 
                // structure and header
                if ((lflags & 0x01) == 1)
                {
                    br.ReadBytes(0x34);
                    var skip = br.ReadUInt16(); // this counts of how far we need to skip ahead
                    br.ReadBytes(skip);
                }
                // get the number of bytes the path contains
                var length = br.ReadUInt32();
                // skip 12 bytes (LinkInfoHeaderSize, LinkInfoFlgas, and VolumeIDOffset)
                br.ReadBytes(0x0C);
                // Find the location of the LocalBasePath position
                var lbpos = br.ReadUInt32();
                // Skip to the path position 
                // (subtract the length of the read (4 bytes), the length of the skip (12 bytes), and
                // the length of the lbpos read (4 bytes) from the lbpos)
                //br.ReadBytes((int)lbpos + 0x14);
                var size = length - lbpos - 0x02;
                var bytePath = br.ReadBytes((int)size);
                var path = Encoding.UTF8.GetString(bytePath, 0, bytePath.Length);
                return path;
            }
        }
    }

}
