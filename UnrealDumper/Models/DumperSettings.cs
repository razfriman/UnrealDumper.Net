using System;
using UnrealDumper.Net.Models.Offsets;

namespace UnrealDumper.Net.Models
{
    public class DumperSettings
    {
        public byte[] NamesSignature { get; set; }
        public byte[] ObjectsSignature { get; set; }
        public StructOffsets StructOffsets { get; set; }
        public Action<byte[]> DecryptString { get; set; } = (_) => { };
        public string ProcessName { get; set; }
    }
}