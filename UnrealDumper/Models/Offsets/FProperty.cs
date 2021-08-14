namespace UnrealDumper.Net.Models.Offsets
{
    public class FProperty
    {
        public ushort ArrayDim{ get; set; }
        public ushort ElementSize{ get; set; }
        public ushort PropertyFlags{ get; set; }
        public ushort Offset{ get; set; }
        public ushort Size{ get; set; }
    }
}