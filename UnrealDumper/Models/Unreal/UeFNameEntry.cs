using System.Text;

namespace UnrealDumper.Net.Models.Unreal
{
    public class UeFNameEntry
    {
        public long Address { get; init; }
        public ProcessReader Reader { get; init; }

        public UeFNameEntry(ProcessReader reader, long address)
        {
            Reader = reader;
            Address = address;
        }

        public (bool wide, ushort len) Info()
        {
            var reader = Reader.GetMemoryReader(Address + Reader.Settings.StructOffsets.FNameEntry.Info, 2);
            var info = reader.ReadUInt16();
            var len = (ushort)(info >> Reader.Settings.StructOffsets.FNameEntry.LenBit);
            var wide = ((info >> Reader.Settings.StructOffsets.FNameEntry.WideBit) & 1) > 0;
            return (wide, len);
        }

        public string String()
        {
            var (wide, len) = Info();
            return String(wide, len);
        }

        public ushort Size(bool wide, ushort len)
        {
            var bytes = (ushort)(Reader.Settings.StructOffsets.FNameEntry.HeaderSize + len * (wide ? 2 : 1));
            return (ushort)((bytes + Reader.Settings.StructOffsets.Stride - 1u) &
                            ~(Reader.Settings.StructOffsets.Stride - 1u));
        }

        public string String(bool wide, ushort len)
        {
            if (wide)
            {
                var reader = Reader.GetMemoryReader(Address + Reader.Settings.StructOffsets.FNameEntry.HeaderSize,
                    len * 2);
                var bytes = reader.ReadBytes(len);
                return Encoding.Unicode.GetString(bytes);
            }
            else
            {
                var reader = Reader.GetMemoryReader(Address + Reader.Settings.StructOffsets.FNameEntry.HeaderSize, len);
                var bytes = reader.ReadBytes(len);
                // TODO(raz): Decrypt Fortnite strings
                // Decrypt_ANSI(buf, len);
                return Encoding.ASCII.GetString(bytes);
            }
        }
    }
}