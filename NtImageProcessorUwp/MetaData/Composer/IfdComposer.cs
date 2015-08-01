﻿using Naotaco.ImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Naotaco.ImageProcessor.MetaData.Misc;
using System.Diagnostics;

namespace Naotaco.ImageProcessor.MetaData.Composer
{
    public static class IfdComposer
    {
        /// <summary>
        /// Build byte data from IFD  data.
        /// </summary>
        /// <param name="ifd">IFD structure which will be composed to binary data.</param>
        /// <param name="SectionOffset">Offset of this IFD section from TIFF header.</param>
        /// <returns></returns>
        public static byte[] ComposeIfdsection(IfdData ifd, Definitions.Endian MetadataEndian)
        {
            var data = ifd.Entries;
            
            // calcurate total size of IFD
            var TotalSize = 2; // TIFF HEader +  number of entry
            UInt32 count = 0;
            foreach (Entry entry in data.Values)
            {
                count++;
                TotalSize += 12;

                // if value is more than 4 bytes, need separated section to store all data.
                if (entry.value.Length > 4)
                {
                    TotalSize += entry.value.Length;
                }
            }

            // area for pointer to next IFD section.
            TotalSize += 4;

            var ComposedData = new byte[TotalSize];

            // set data of entry num.
            var EntryNum = Util.ToByte(count, 2, MetadataEndian);
            Array.Copy(EntryNum, ComposedData, EntryNum.Length);

            // set Next IFD pointer
            var ifdPointerValue = Util.ToByte(ifd.NextIfdPointer, 4, MetadataEndian);
            
            // Debug.WriteLine("Nexf IFD: " + ifd.NextIfdPointer.ToString("X")); 

            Array.Copy(ifdPointerValue, 0, ComposedData, 2 + 12 * (int)count, 4);
            // TIFF header, number of entry, each entries, Nexf IFD pointer.
            var ExtraDataSectionOffset = (UInt32)(2 + 12 * (int)count + 4);
            // Debug.WriteLine("ExtraDataSectionOffset: " + ExtraDataSectionOffset.ToString("X"));

            // Debug.WriteLine("ifd.offset: " + ifd.Offset.ToString("X"));

            var keys = data.Keys.ToArray<UInt32>();
            Array.Sort(keys);

            int pointer = 2;
            foreach (UInt32 key in keys)
            {
                // tag in 2 bytes.
                var tag = Util.ToByte(data[key].Tag, 2, MetadataEndian);
                Array.Copy(tag, 0, ComposedData, pointer, 2);
                pointer += 2;
                // Debug.WriteLine("Tag: " + data[key].Tag.ToString("X"));

                // type
                var type = Util.ToByte(Util.ToUInt32(data[key].Type), 2, MetadataEndian);
                Array.Copy(type, 0, ComposedData, pointer, 2);
                pointer += 2;

                // count
                var c = Util.ToByte(data[key].Count, 4, MetadataEndian);
                Array.Copy(c, 0, ComposedData, pointer, 4);
                pointer += 4;

                if (data[key].value.Length <= 4)
                {
                    // upto 4 bytes, copy value directly.
                    Array.Copy(data[key].value, 0, ComposedData, pointer, data[key].value.Length);
                }
                else
                {
                    // save actual data to extra area
                    Array.Copy(data[key].value, 0, ComposedData, (int)ExtraDataSectionOffset, data[key].value.Length);

                    // store pointer for extra area. Origin of pointer should be position of TIFF header.
                    var offset = Util.ToByte(ExtraDataSectionOffset + ifd.Offset, 4, MetadataEndian);
                    Array.Copy(offset, 0, ComposedData, pointer, 4);
                    // Util.DumpFirst16byte(offset);
                    // Util.DumpByteArray(ComposedData, pointer, 4);

                    ExtraDataSectionOffset += (UInt32)data[key].value.Length;

                }
                pointer += 4;

            }
            // Util.DumpByteArrayAll(ComposedData);
            return ComposedData;
        }
    }
}
