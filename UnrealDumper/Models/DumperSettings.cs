using System;
using UnrealDumper.Net.Models.Offsets;

namespace UnrealDumper.Net
{
    public class DumperSettings
    {
        public byte[] NamesSignature { get; set; }
        public byte[] ObjectsSignature { get; set; }
        public StructOffsets StructOffsets { get; set; }
        public string ProcessName { get; set; }
    }
}