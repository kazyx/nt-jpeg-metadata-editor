﻿using Naotaco.ImageProcessor.MetaData.Composer;
using Naotaco.ImageProcessor.MetaData.Misc;
using Naotaco.ImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Naotaco.ImageProcessor.MetaData
{
    public static class MetaDataOperator
    {

        /// <summary>
        /// Add geometory information to given image as Exif data asynchronously.
        /// </summary>
        /// <param name="image">Raw data of Jpeg file</param>
        /// <param name="position">Geometory information</param>
        /// <param name="overwrite">In case geotag is already exists, it throws GpsInformationAlreadyExistsException by default.
        ///  To overwrite current one, set true here.</param>
        /// <returns>Jpeg data with geometory information.</returns>
        public static async Task<byte[]> AddGeopositionAsync(byte[] image, Geoposition position, bool overwrite = false)
        {
            // It seems thrown exceptions will be raised by this "async" ...
            return await Task<byte[]>.Run(async () =>
            {
                return AddGeoposition(image, position, overwrite);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Add geometory information to given image as Exif data.
        /// </summary>
        /// <param name="image">Raw data of Jpeg file</param>
        /// <param name="position">Geometory information</param>
        /// <param name="overwrite">In case geotag is already exists, it throws GpsInformationAlreadyExistsException by default.
        ///  To overwrite current one, set true here.</param>
        /// <returns>Jpeg data with geometory information.</returns>
        public static byte[] AddGeoposition(byte[] image, Geoposition position, bool overwrite = false)
        {
#if WINDOWS_APP
            Debug.WriteLine("Longitude : " + position.Coordinate.Point.Position.Longitude + " Latitude: " + position.Coordinate.Point.Position.Latitude);
#elif WINDOWS_PHONE
            Debug.WriteLine("Longitude : " + position.Coordinate.Longitude + " Latitude: " + position.Coordinate.Latitude);
#endif

            // parse given image first
            var exif = JpegMetaDataParser.ParseImage(image);

            if (!overwrite && exif.IsGeotagExist)
            {
                Debug.WriteLine("This image contains GPS information.");
                throw new GpsInformationAlreadyExistsException("This image contains GPS information.");
            }

            exif = RemoveGeoinfo(exif);

            // Create IFD structure from given GPS info
            var gpsIfdData = GpsIfdDataCreator.CreateGpsIfdData(position);

            // Add GPS info to exif structure
            exif.GpsIfd = gpsIfdData;

            return JpegMetaDataProcessor.SetMetaData(image, exif);
        }

        /// <summary>
        /// Add geometory information to given image as Exif data asynchronously.
        /// </summary>
        /// <param name="image">Raw data of Jpeg file as a stream.
        /// Given stream will be disposed after adding location info.</param>
        /// <param name="position">Geometory information</param>
        /// /// <param name="overwrite">In case geotag is already exists, it throws GpsInformationAlreadyExistsException by default.
        ///  To overwrite current one, set true here.</param>
        /// <returns>Jpeg data with geometory information.</returns>
        public static async Task<Stream> AddGeopositionAsync(Stream image, Geoposition position, bool overwrite = false)
        {
            // It seems thrown exceptions will be raised by this "async" ...
            return await Task<Stream>.Run(async () =>
            {
                return AddGeoposition(image, position, overwrite);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Add geometory information to given image as Exif data. Blocks thread.
        /// </summary>
        /// <param name="image">Raw data of Jpeg file as a stream.
        /// Given stream will be disposed after adding location info.</param>
        /// <param name="position">Geometory information</param>
        /// /// <param name="overwrite">In case geotag is already exists, it throws GpsInformationAlreadyExistsException by default.
        ///  To overwrite current one, set true here.</param>
        /// <returns>Jpeg data with geometory information.</returns>
        public static Stream AddGeoposition(Stream image, Geoposition position, bool overwrite = false)
        {
#if WINDOWS_APP
            Debug.WriteLine("Longitude : " + position.Coordinate.Point.Position.Longitude + " Latitude: " + position.Coordinate.Point.Position.Latitude);
#elif WINDOWS_PHONE
            Debug.WriteLine("Longitude : " + position.Coordinate.Longitude + " Latitude: " + position.Coordinate.Latitude);
#endif

            // parse given image first
            var exif = JpegMetaDataParser.ParseImage(image);

            if (!overwrite && exif.IsGeotagExist)
            {
                Debug.WriteLine("This image contains GPS information.");
                throw new GpsInformationAlreadyExistsException("This image contains GPS information.");
            }

            exif = RemoveGeoinfo(exif);

            // Create IFD structure from given GPS info
            var gpsIfdData = GpsIfdDataCreator.CreateGpsIfdData(position);

            // Add GPS info to exif structure
            exif.GpsIfd = gpsIfdData;

            // create a new image with given location info
            var newImage = JpegMetaDataProcessor.SetMetaData(image, exif);
            image.Dispose();
            return newImage;
        }

        /// <summary>
        /// Remove GPS information from metadata.
        /// </summary>
        /// <param name="meta">Metadata with geotag</param>
        /// <returns></returns>
        public static JpegMetaData RemoveGeoinfo(JpegMetaData meta)
        {
            if (meta.PrimaryIfd.Entries.ContainsKey(Definitions.GPS_IFD_POINTER_TAG))
            {
                meta.PrimaryIfd.Entries.Remove(Definitions.GPS_IFD_POINTER_TAG);
            }
            meta.GpsIfd = null;
            return meta;
        }
    }
}
