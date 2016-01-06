﻿using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Naotaco.Jpeg.MetaData.Structure;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Naotaco.JpegMetadataTest
{
    public static class TestUtil
    {
        public static void AreEqual(byte[] expected, byte[] actual, string message = "")
        {
            Assert.AreEqual(expected.Length, actual.Length, message + " at length comparison");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], message + " at element comparison. i: " + i);
            }
        }

        public static void AreEqual(Int32[] expected, Int32[] actual, string message = "")
        {
            Assert.AreEqual(expected.Length, actual.Length, message + " at length comparison");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], message + " at element comparison. i: " + i);
            }
        }

        public static void AreEqual(UInt32[] expected, UInt32[] actual, string message = "")
        {
            Assert.AreEqual(expected.Length, actual.Length, message + " at length comparison");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], message + " at element comparison. i: " + i);
            }
        }

        public static byte[] GetLastElements(byte[] array, int newLength)
        {
            var newArray = new byte[newLength];
            Array.Copy(array, array.Length - newLength, newArray, 0, newLength);
            return newArray;
        }

        public static byte[] GetFirstElements(byte[] array, int newLength)
        {
            var newArray = new byte[newLength];
            Array.Copy(array, newArray, newLength);
            return newArray;
        }

        public static Stream GetResourceStream(string filename)
        {
            return Task.Run<Stream>(async () =>
            {
                return await GetResourceStreamAsync(filename);
            }).GetAwaiter().GetResult();
        }

        public static async Task<Stream> GetResourceStreamAsync(string filename)
        {
            Debug.WriteLine(Windows.ApplicationModel.Package.Current.InstalledLocation.Name);
            Debug.WriteLine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path);
            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            var f2 = await folder.GetFolderAsync("TestImages");
            Debug.WriteLine(f2.DisplayName);
            StorageFile file = await f2.GetFileAsync(filename);
            Debug.WriteLine(file.DisplayName);
            byte[] buff;
            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                using (var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    buff = new byte[stream.Size];
                    reader.ReadBytes(buff);
                }
            }
            var memStream = new MemoryStream();
            memStream.Write(buff, 0, buff.Length);
            return memStream;
        }

        public static async Task<byte[]> GetResourceByteArrayAsync(string filename)
        {
            Stream myFileStream = await GetResourceStreamAsync(filename);
            myFileStream.Seek(0, SeekOrigin.Begin);
            byte[] buf = new byte[50000000];
            if (myFileStream.CanRead)
            {
                int read;
                read = myFileStream.Read(buf, 0, (int)myFileStream.Length);

                var image = new byte[read];
                Array.Copy(buf, image, read);
                myFileStream.Dispose();
                return image;
            }
            return null;
        }

        public static byte[] GetResourceByteArray(string filename)
        {
            Stream myFileStream = GetResourceStream(filename);
            byte[] buf = new byte[50000000];
            if (myFileStream.CanRead)
            {
                int read;
                read = myFileStream.Read(buf, 0, (int)myFileStream.Length);
                if (read > 0)
                {
                    var image = new byte[read];
                    Array.Copy(buf, image, read);
                    myFileStream.Dispose();
                    return image;
                }
            }
            return null;
        }

        public static void CompareJpegMetaData(JpegMetaData meta1, JpegMetaData meta2, string filename, bool GpsIfdExists, bool ExifIfdExists = true)
        {
            Debug.WriteLine("file: " + filename);
            TestUtil.AreEqual(meta1.App1Data, meta2.App1Data, filename + " App1 data");
            TestUtil.AreEqual(meta1.PrimaryIfd, meta2.PrimaryIfd, filename + "Primary IFD");

            if (ExifIfdExists)
            {
                TestUtil.AreEqual(meta1.ExifIfd, meta2.ExifIfd, filename + "Exif IFD");
            }
            else
            {
                Assert.IsNull(meta1.ExifIfd);
                Assert.IsNull(meta2.ExifIfd);
            }

            if (GpsIfdExists)
            {
                TestUtil.AreEqual(meta1.GpsIfd, meta2.GpsIfd, filename + "Gps IFD");
            }
            else
            {
                Assert.IsNull(meta1.GpsIfd);
                Assert.IsNull(meta2.GpsIfd);
            }
        }

        public static void IsGpsDataAdded(JpegMetaData original, JpegMetaData added)
        {
            if (original.ExifIfd != null && added.ExifIfd != null)
            {
                TestUtil.AreEqual(original.ExifIfd, added.ExifIfd, "Exif IFD");
            }
            Assert.IsTrue(added.PrimaryIfd.Entries.ContainsKey(Definitions.GPS_IFD_POINTER_TAG));
            Assert.IsNotNull(added.GpsIfd);
        }


        public static void AreEqual(IfdData data1, IfdData data2, string message)
        {
            message = message + " ";
            Assert.AreEqual(data1.NextIfdPointer, data2.NextIfdPointer, message + "Next IFD pointer");
            Assert.AreEqual(data1.Length, data2.Length, message + "length");
            Assert.AreEqual(data1.Entries.Count, data2.Entries.Count, message + "entry num");

        }
    }
}
