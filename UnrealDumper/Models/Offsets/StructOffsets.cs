namespace UnrealDumper.Net.Models.Offsets
{
    public class StructOffsets
    {
        public ushort Stride { get; set; }
        public FUObjectItem FUObjectItem { get; set; }
        public FName FName { get; set; }
        public FNameEntry FNameEntry { get; set; }
        public UObject UObject { get; set; }
        public UField UField { get; set; }
        public UStruct UStruct { get; set; }
        public UEnum UEnum { get; set; }
        public UFunction UFunction { get; set; }
        public FField FField { get; set; }
        public FProperty FProperty { get; set; }
        public UProperty UProperty { get; set; }
    }
}