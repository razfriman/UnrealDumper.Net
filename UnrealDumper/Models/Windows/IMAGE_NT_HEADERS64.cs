using System.Runtime.InteropServices;

namespace UnrealDumper.Net.Models.Windows
{
    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_NT_HEADERS64
    {
        [FieldOffset(0)] [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] Signature;

        [FieldOffset(4)] public IMAGE_FILE_HEADER FileHeader;

        [FieldOffset(24)] public IMAGE_OPTIONAL_HEADER64 OptionalHeader;

        private string _Signature
        {
            get { return new string(Signature); }
        }

        public bool isValid
        {
            get
            {
                return _Signature == "PE\0\0" && OptionalHeader.Magic == MagicType.IMAGE_NT_OPTIONAL_HDR64_MAGIC;
            }
        }
    }
}