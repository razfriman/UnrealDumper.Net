namespace UnrealDumper.Net.Models.Unreal
{
    public class FNameEntryHandle
    {
        public uint Block{ get; set; }
        public uint Offset{ get; set; }

        public FNameEntryHandle()
        {
            
        }

        public FNameEntryHandle(uint id)
        {
            Block = id >> 16;
            Offset = id & 65535;
        }
    }
}