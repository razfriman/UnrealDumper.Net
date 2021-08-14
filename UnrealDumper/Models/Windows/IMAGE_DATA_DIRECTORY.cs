using System.Runtime.InteropServices;

namespace UnrealDumper.Net.Models.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_DATA_DIRECTORY
    {
        public uint VirtualAddress;
        public uint Size;
    }
}