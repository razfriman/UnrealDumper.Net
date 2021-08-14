namespace UnrealDumper.Net.Models.Offsets
{
    public class SdkSettings
    {
        public static readonly StructOffsets DefaultStructOffsets = new()
        {
            Stride = 2,
            FUObjectItem = new FUObjectItem
            {
                Size = 24
            },
            FName = new FName
            {
                Number = 4
            },
            FNameEntry = new FNameEntry
            {
                Info = 0,
                WideBit = 0,
                LenBit = 6,
                HeaderSize = 2
            },
            UObject = new UObject
            {
                Index = 0x0C,
                Class = 0x10,
                Name = 0x18,
                Outer = 0x20
            },
            UField = new UField
            {
                Next = 0x28
            },
            UStruct = new UStruct
            {
                SuperStruct = 0x40,
                Children = 0x48,
                ChildProperties = 0x50,
                PropertiesSize = 0x58
            },
            UEnum = new UEnum
            {
                Names = 0x40,
            },
            UFunction = new UFunction
            {
                FunctionFlags = 0xB0,
                Func = 0xB0 + 0x28
            },
            FField = new FField
            {
                Class = 0x08,
                Next = 0x20,
                Name = 0x28
            },
            FProperty = new FProperty
            {
                ArrayDim = 0x38,
                ElementSize = 0x3C,
                PropertyFlags = 0x40,
                Offset = 0x4C,
                Size = 0x78
            },
            UProperty = new UProperty
            {
                ArrayDim = 0,
                ElementSize = 0,
                PropertyFlags = 0,
                Offset = 0,
                Size = 0
            }
        };
        
        public static readonly DumperSettings FortniteSettings = new()
        {
            ProcessName = "FortniteClient-Win64-Shipping",
            NamesSignature = new byte[]
                { 0x4C, 0x8D, 0x35, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x10, 0x07, 0x83, 0xFB, 0x01 },
            ObjectsSignature = new byte[]
                { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x0C, 0xC8, 0x48, 0x8D, 0x04, 0xD1, 0xEB },
            StructOffsets = DefaultStructOffsets,
        };
    }
}