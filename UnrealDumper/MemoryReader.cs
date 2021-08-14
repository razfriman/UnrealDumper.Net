using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace UnrealDumper.Net
{
    public sealed unsafe class MemoryReader : IDisposable
    {
	    private readonly byte[] _data;
        private readonly MemoryStream _stream;
        private readonly BinaryReader _reader;
        
        public long Size => _stream.Length;
        public long Position => _stream.Position;

        public MemoryReader(byte[] data)
        {
            _data = data;
            _stream = new MemoryStream(_data);
            _reader = new BinaryReader(_stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Seek(int offset, SeekOrigin origin) => _stream.Seek(offset, origin);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Skip(int byteCount) => Seek(byteCount, SeekOrigin.Current);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadUnmanaged<T>() where T : unmanaged
		{
			var result = Unsafe.ReadUnaligned<T>(ref _data[Position]);
			var size = sizeof(T);
			Seek(size, SeekOrigin.Current);
			return result;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadStruct<T>() {
			var bytes = _reader.ReadBytes(Marshal.SizeOf(typeof(T)));
			var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			var theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			handle.Free();
			return theStructure;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadBoolean() => _reader.ReadBoolean();
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte ReadByte() => _reader.ReadByte();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public sbyte ReadSByte() => _reader.ReadSByte();
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public short ReadInt16() => _reader.ReadInt16();
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort ReadUInt16() => _reader.ReadUInt16();
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadInt32() => _reader.ReadInt32();
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint ReadUInt32() => _reader.ReadUInt32();
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long ReadInt64() => _reader.ReadInt64();
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong ReadUInt64() => _reader.ReadUInt64();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte[] ReadBytes(int length) => _reader.ReadBytes(length);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ReadString(Encoding enc)
		{
			var length = _reader.ReadInt32();
			return ReadString(length, enc);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ReadString(int length, Encoding enc)
		{
			var result = enc.GetString(_data, (int)Position, length);
			Seek(length, SeekOrigin.Current);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ReadFString()
		{
			var length = ReadInt32();

            if (length == 0)
            {
                return string.Empty;
            }

            var isUnicode = length < 0;
            var encoding = isUnicode ? Encoding.Unicode : Encoding.Default;
            if (isUnicode)
            {
                length *= -2;
            }

            Span<byte> buffer = stackalloc byte[length];
            _reader.Read(buffer);
            return encoding.GetString(buffer).Trim(' ', '\0');
		}

	
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string[] ReadFStringArray()
		{
			var length = _reader.ReadInt32();
			return ReadFStringArray(length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string[] ReadFStringArray(int length)
		{
			var result = new string[length];

			for (var i = 0; i < length; i++)
			{
				result[i] = ReadFString();
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ReadArray<T>() where T : unmanaged
		{
			var length = _reader.ReadInt32();
			return ReadArray<T>(length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ReadArray<T>(int length) where T : unmanaged
		{
			if (length == 0)
			{
				return Array.Empty<T>();
			}

			var size = length * sizeof(T);
			var result = new T[length];
			Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref result[0]), ref _data[Position], (uint)size);
			Seek(size, SeekOrigin.Current);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ReadArray<T>(Func<T> getter)
		{
			var length = ReadInt32();
			return ReadArray(length, getter);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ReadArray<T>(Func<MemoryReader, T> getter)
		{
			var length = _reader.ReadInt32();
			return ReadArray(length, getter);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ReadArray<T>(int length, Func<T> getter)
		{
			if (length == 0)
			{
				return Array.Empty<T>();
			}

			var result = new T[length];

			for (var i = 0; i < length; i++)
			{
				result[i] = getter();
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ReadArray<T>(int length, Func<MemoryReader, T> getter)
		{
			if (length == 0)
			{
				return Array.Empty<T>();
			}

			var result = new T[length];

			for (var i = 0; i < length; i++)
			{
				result[i] = getter(this);
			}

			return result;
		}
		
		public int? SearchPattern(byte[] pattern, int start = 0, int end = int.MaxValue)
		{
			start = Math.Max(start, 0);
			end = Math.Min(end, _data.Length);
			end = end - pattern.Length + 1;
			for (var i = start; i < end; i++)
			{
				if (_data[i] != pattern[0])
				{
					continue;
				}

				for (var j = pattern.Length - 1; j >= 1; j--)
				{
					if (pattern[j] != 0 && _data[i + j] != pattern[j])
					{
						break;
					}

					if (j == 1)
					{
						return i;
					}
				}
			}

			return null;
		}

		public int? FindPointer(byte[] pattern, int start = 0, int end = int.MaxValue)
		{
			var address = SearchPattern(pattern, start, end);

			if (!address.HasValue)
			{
				return null;
			}

			var k = 0;
			while (pattern[k] > 0)
			{
				k++;
			}

			Seek(address.Value + k, SeekOrigin.Begin);
			var offset = ReadInt32();
			return address + k + 4 + offset;
		}
		
		public void Dispose()
		{
			_stream?.Dispose();
			_reader?.Dispose();
		}
    }
}