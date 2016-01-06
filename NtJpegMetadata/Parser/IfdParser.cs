﻿using Naotaco.Jpeg.MetaData.Misc;
using Naotaco.Jpeg.MetaData.Structure;
using System;
using System.Collections.Generic;

namespace Naotaco.Jpeg.MetaData.Parser
{
    public static class IfdParser
    {
        private const int ENTRY_SIZE = 12;
        
        /// <summary>
        /// Parse IFD section of Jpeg's header.
        /// </summary>
        /// <param name="App1Data">Raw data of App1 section</param>
        /// <param name="IfdOffset">Offset to the target IFD section from start of App1 data.</param>
        /// <param name="IfdSectionEndian">Alignment of all sections of this IFD data. This value is contained in TIFF header.</param>
        /// <returns>All entries in given IFD section.</returns>
        public static IfdData ParseIfd(byte[] App1Data, UInt32 IfdOffset, Definitions.Endian IfdSectionEndian)
        {
            var ifd = new IfdData();
            ifd.Offset = IfdOffset;
            var entries = new Dictionary<UInt32, Entry>();
            var EntryNum = Util.GetUIntValue(App1Data, (int)IfdOffset, 2, IfdSectionEndian);
            // Debug.WriteLine("Entry num: " + EntryNum);

            ifd.NextIfdPointer = Util.GetUIntValue(App1Data, (int)IfdOffset + 2 + (int)EntryNum * ENTRY_SIZE, 4, IfdSectionEndian);

            // if there's no extra data area, (if all data is 4 bytes or less), this is length of this IFD section.
            ifd.Length = 2 + EntryNum * ENTRY_SIZE + 4; // entry num (2 bytes), each entries (12 bytes each), Next IFD pointer (4 byte)
            
            for (int i = 0; i < EntryNum; i++)
            {
                // Debug.WriteLine("--- Entry[" + i + "] ---");
                var EntryOrigin = (int)IfdOffset + 2 + i * ENTRY_SIZE;

                var entry = new Entry();

                // tag
                entry.Tag = Util.GetUIntValue(App1Data, EntryOrigin, 2, IfdSectionEndian);

                var tagTypeName = "Unknown";
                if (Util.TagNames.ContainsKey(entry.Tag))
                {
                    tagTypeName = Util.TagNames[entry.Tag];
                }
                // Debug.WriteLine("Tag: " + entry.Tag.ToString("X") + " " + tagTypeName);

                // type
                var typeValue = Util.GetUIntValue(App1Data, EntryOrigin + 2, 2, IfdSectionEndian);
                entry.Type = Util.ToEntryType(typeValue);
                // Debug.WriteLine("Type: " + entry.Type.ToString());

                // count
                entry.Count = Util.GetUIntValue(App1Data, EntryOrigin + 4, 4, IfdSectionEndian);
                // Debug.WriteLine("Count: " + entry.Count);

                var valueSize = 0;
                valueSize = Util.FindDataSize(entry.Type);
                var TotalValueSize = valueSize * (int)entry.Count;
                // Debug.WriteLine("Total value size: " + TotalValueSize);

                var valueBuff = new byte[TotalValueSize];

                if (TotalValueSize <= 4)
                {
                    // in this case, the value is stored directly here.
                    Array.Copy(App1Data, EntryOrigin + 8, valueBuff, 0, TotalValueSize);
                }
                else
                {
                    // other cases, actual value is stored in separated area
                    var EntryValuePointer = (int)Util.GetUIntValue(App1Data, EntryOrigin + 8, 4, IfdSectionEndian); 
                    // Debug.WriteLine("Entry pointer: " + EntryValuePointer.ToString("X"));

                    Array.Copy(App1Data, EntryValuePointer, valueBuff, 0, TotalValueSize);

                    // If there's extra data, its length should be added to total length.
                    ifd.Length += (UInt32)TotalValueSize;
                }

                if (IfdSectionEndian != Entry.InternalEndian)
                {
                    // to change endian, each sections should be reversed.
                    var ReversedValue = new byte[valueBuff.Length];
                    var valueLength = Util.FindDataSize(entry.Type);
                    if (valueLength == 8)
                    {
                        // for fraction value, each value should be reversed individually
                        valueLength = 4;
                    }
                    var valueNum = valueBuff.Length / valueLength;
                    for (int j = 0; j < valueNum; j++)
                    {
                        var tempValue = new byte[valueLength];
                        Array.Copy(valueBuff, j * valueLength, tempValue, 0, valueLength);
                        Array.Reverse(tempValue);
                        Array.Copy(tempValue, 0, ReversedValue, j * valueLength, valueLength);
                    }
                    entry.value = ReversedValue;
                }
                else
                {
                    // if internal endian and target metadata's one is same, no need to reverse.
                    entry.value = valueBuff;
                }
                
                switch (entry.Type)
                {
                    case Entry.EntryType.Ascii:
                        // Debug.WriteLine("value: " + entry.StringValue + Environment.NewLine + Environment.NewLine);
                        // Debug.WriteLine(" ");
                        break;
                    case Entry.EntryType.Byte:
                    case Entry.EntryType.Undefined:
                        if (entry.Tag == 0x927C)
                        {
                            // Debug.WriteLine("Maker note is too long to print.");
                        }
                        else
                        {
                            foreach (int val in entry.UIntValues)
                            {
                                // Debug.WriteLine("value: " + val.ToString("X"));
                            }
                        }
                        break;
                    case Entry.EntryType.Short:
                    case Entry.EntryType.SShort:
                    case Entry.EntryType.Long:
                    case Entry.EntryType.SLong:

                        // Util.DumpByteArrayAll(entry.value);
                        foreach (int val in entry.UIntValues)
                        {
                            // Debug.WriteLine("value: " + val);
                        }
                        break;
                    case Entry.EntryType.Rational:
                    case Entry.EntryType.SRational:
                        // Util.DumpByteArrayAll(entry.value);
                        foreach (double val in entry.DoubleValues)
                        {
                            // Debug.WriteLine("value: " + val);
                        }
                        break;
                    default:
                        break;
                }

                entries[entry.Tag] = entry;
            }

            
            ifd.Entries = entries;
            return ifd;
        }


    }
}
