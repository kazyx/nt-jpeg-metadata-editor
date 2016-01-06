﻿using System;

namespace Naotaco.Jpeg.MetaData.Misc
{
    public static class Definitions
    {
        public enum Endian
        {
            Little,
            Big,
        };

        public const UInt32 JPEG_SOI_MARKER = 0xFFD8;
        public const UInt32 APP0_MARKER = 0xFFE0;
        public const UInt32 APP1_MARKER = 0xFFE1;
        public const UInt32 APP1_OFFSET = 0x0C;

        public const UInt32 TIFF_BIG_ENDIAN = 0x4D4D;
        public const UInt32 TIFF_LITTLE_ENDIAN = 0x4949;

        public const UInt32 TIFF_IDENTIFY_CODE = 0x002A;

        public const UInt32 EXIF_IFD_POINTER_TAG = 0x8769;
        public const UInt32 GPS_IFD_POINTER_TAG = 0x8825;

        public const UInt32 GPS_STATUS_TAG = 0x09;
        public const string GPS_STATUS_MEASUREMENT_ACTIVE = "A";
        public const string GPS_STATUS_MEASUREMENT_VOID = "V";
    }
}
