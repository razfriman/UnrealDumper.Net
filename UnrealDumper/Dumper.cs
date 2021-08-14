﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnrealDumper.Net.Models.Unreal;
using UnrealDumper.Net.Models.Windows;

namespace UnrealDumper.Net
{
    public class Dumper
    {
        private Process _process;
        private ProcessReader _processReader;
        private MemoryReader _reader;
        private readonly DumperSettings _settings;
        
        public int? NamesIndex { get; set; }
        public long[] Blocks { get; set; }
        public int? ObjectsIndex { get; set; }

        public Dumper(DumperSettings settings) => _settings = settings;

        public void Start()
        {
            FindProcess();
            _processReader = new ProcessReader(_process, _settings);
            _reader = GetModuleReader();
            ResolveIndexes();
            DumpNames();
            DumpObjects();
        }

        public void FindProcess()
        {
            Console.WriteLine($"Find Process: [{_settings.ProcessName}]");
            _process = Process.GetProcessesByName(_settings.ProcessName).FirstOrDefault();
        }

        private void ResolveIndexes()
        {
            Console.WriteLine("Resolve Name/Object Index");
            var codeSections = GetCodeSections();
            foreach (var codeSection in codeSections)
            {
                var start = (int)codeSection.PointerToRawData;
                var end = (int)(start + codeSection.SizeOfRawData);
                NamesIndex ??= _reader.FindPointer(_settings.NamesSignature, start, end);
                ObjectsIndex ??= _reader.FindPointer(_settings.ObjectsSignature, start, end);
            }
        }

        private void DumpObjects()
        {
            Console.WriteLine($"Dumping Objects at [{ObjectsIndex}]");
            _reader.Seek(ObjectsIndex.Value, SeekOrigin.Begin);
            var objectsPtr = _reader.ReadInt64();
            var preallocated = _reader.ReadInt64();
            var maxElements = _reader.ReadInt32();
            var numElements = _reader.ReadInt32();
            var maxChunks = _reader.ReadInt32();
            var numChunks = _reader.ReadInt32();
           
            for (uint i = 0; i < numElements; i++) {
                var obj = GetObjectPtr(i);
                if (obj is null or 0)
                {
                    continue;
                }
                Console.WriteLine($"Found Object Pointer [{obj}]");
            }
            
            long? GetObjectPtr(uint id) {
                if (id >= numElements) return null;
                long chunkIndex = id / 65536;
                if (chunkIndex >= numChunks) return null;
                var chunk = _processReader.GetMemoryReader(objectsPtr + chunkIndex, 8).ReadInt64();
                if (chunk == 0) return null;
                var withinChunkIndex = id % 65536 * _settings.StructOffsets.FUObjectItem.Size;
                var item = _processReader.GetMemoryReader(chunk + withinChunkIndex, 8).ReadInt64();
                return item;
            }
        }
        
        private void DumpNames()
        {
            Console.WriteLine($"Dumping Names at [{NamesIndex}]");
            _reader.Seek(NamesIndex.Value, SeekOrigin.Begin);
            var _ = _reader.ReadInt64();
            var currentBlock = _reader.ReadUInt32();
            var currentByteCursor = _reader.ReadInt32();
            Blocks = _reader.ReadArray(8192, r => r.ReadInt64());
            for (uint blockId = 0; blockId < currentBlock; blockId++)
            {
                DumpNameBlock(blockId, _settings.StructOffsets.Stride * 65536);
            }
            DumpNameBlock(currentBlock, currentByteCursor);
        }

        private void DumpNameBlock(uint blockId, int blockSize)
        {
            var blockStart = Blocks[blockId];
            var blockEnd = blockStart + (uint)blockSize;
            var position = blockStart;
            var entryHandle = new FNameEntryHandle
            {
                Block = blockId,
                Offset = 0
            };

            while (position < blockEnd)
            {
                var entry = new UeFNameEntry(_processReader, position);
                var (wide, len) = entry.Info();
                if (len > 0) {
                    var s = entry.String(wide, len);
                    var size = entry.Size(wide, len);
                    entryHandle.Offset += (uint)(size / _settings.StructOffsets.Stride);
                    position += size;
                } else {
                    break;
                };
            }
        }

        private List<IMAGE_SECTION_HEADER> GetCodeSections()
        {
            var result = new List<IMAGE_SECTION_HEADER>();

            var dosHeader = _reader.ReadStruct<IMAGE_DOS_HEADER>();
            _reader.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);
            var ntHeadersSignature = _reader.ReadUInt32();
            var fileHeader = _reader.ReadStruct<IMAGE_FILE_HEADER>();
            var optionalHeader64 = _reader.ReadStruct<IMAGE_OPTIONAL_HEADER64>();
            for (var headerNo = 0; headerNo < fileHeader.NumberOfSections; headerNo++)
            {
                var header = _reader.ReadStruct<IMAGE_SECTION_HEADER>();
                if (header.Characteristics.HasFlag(DataSectionFlags.ContentCode))
                {
                    result.Add(header);
                }
            }

            return result;
        }

        private MemoryReader GetModuleReader()
        {
            var module = FindModule(_process);
            return _processReader.GetMemoryReader(module.BaseAddress.ToInt64(), module.ModuleMemorySize);
        }

        private static ProcessModule FindModule(Process process)
        {
            var targetModuleName = $"{process.ProcessName}.exe";
            for (var i = 0; i < process.Modules.Count; i++)
            {
                var module = process.Modules[i];
                if (module.ModuleName == targetModuleName)
                {
                    return module;
                }
            }

            return null;
        }
    }
}