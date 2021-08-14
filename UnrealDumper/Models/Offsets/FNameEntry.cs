namespace UnrealDumper.Net.Models.Offsets
{
    public class FNameEntry
    {
        public ushort Info{ get; set; }
        public ushort WideBit{ get; set; }
        public ushort LenBit{ get; set; }
        public ushort HeaderSize{ get; set; }
    }
}