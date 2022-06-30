using System;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using ExifLibrary;




namespace AddExifInfo
{
    internal class AddExifInfo
    {
        private ImageFile jpgFile;


        public void AddHeading(string jpgFilename, double? heading, ReadGPX.Trackpoint trackPoint)
        {
            if (!trackPoint.IsValidTrackpoint()) return;

            jpgFile = ImageFile.FromFile(jpgFilename);


           

            if (heading.HasValue)
            {
                var headingFraction = new MathEx.Fraction32(heading.Value);
                var headingRational = new ExifSRational(ExifTag.GPSImgDirection, headingFraction);
                if (headingRational.Value.Denominator == 0)
                {
                    //Console.WriteLine($"Division by zero: {heading.Value}");
                    // Fix wonky library with approximation
                    headingRational = new ExifSRational(ExifTag.GPSImgDirection, (int)heading.Value, 1);
                }
                var headingRef = new ExifAscii(ExifTag.GPSImgDirectionRef, "True", Encoding.ASCII);
                jpgFile.Properties.Add(headingRational);
                jpgFile.Properties.Add(headingRef);

            }

            var dateTimeDigitized = new ExifDateTime(ExifTag.DateTimeDigitized, trackPoint.UtcTime.Value.ToLocalTime());
            jpgFile.Properties.Add(dateTimeDigitized);

            var gpsDate = new ExifDateTime(ExifTag.GPSDateStamp, trackPoint.UtcTime.Value);
            jpgFile.Properties.Add(gpsDate);

            //var gpsTime = new ExifDateTime(ExifTag.GPSTimeStamp, trackPoint.UtcTime.Value);
            //jpgFile.Properties.Add(gpsTime);

            var cameraMake = new ExifAscii(ExifTag.Make, "GoPro", Encoding.ASCII);
            var cameraModel = new ExifAscii(ExifTag.Model, "GoPro Max", Encoding.ASCII);
            jpgFile.Properties.Add(cameraMake);
            jpgFile.Properties.Add(cameraModel);

            var latDms = SexagesimalAngle.FromDouble(trackPoint.Latitude.Value);
            var exifLat = new GPSLatitudeLongitude(ExifTag.GPSLatitude, latDms.Degrees, latDms.Minutes, (float) ( (double)latDms.Seconds + (double)latDms.Milliseconds / 1000.0) );
            jpgFile.Properties.Add(exifLat);

            var latRef = GPSLatitudeRef.North;
            if (latDms.IsNegative) latRef = GPSLatitudeRef.South;
            var exifLatRef = new ExifAscii(ExifTag.GPSLatitudeRef, (latRef.ToString())[0].ToString(), Encoding.ASCII);
            jpgFile.Properties.Add(exifLatRef);

            var lonDms = SexagesimalAngle.FromDouble(trackPoint.Longitude.Value);
            var exifLon = new GPSLatitudeLongitude(ExifTag.GPSLongitude, lonDms.Degrees, lonDms.Minutes, (float)((double)lonDms.Seconds + (double)lonDms.Milliseconds / 1000.0));
            jpgFile.Properties.Add(exifLon);

            var LonRef = GPSLongitudeRef.East;
            if (lonDms.IsNegative) LonRef = GPSLongitudeRef.West;
            var exifLonRef = new ExifAscii(ExifTag.GPSLongitudeRef, (LonRef.ToString())[0].ToString(), Encoding.ASCII);
            jpgFile.Properties.Add(exifLonRef);

            var altitudeFraction = new MathEx.UFraction32(trackPoint.Elevation.Value);
            var exifAltitude = new ExifURational(ExifTag.GPSAltitude, altitudeFraction);
            jpgFile.Properties.Add(exifAltitude);

            var altitudeRef = new ExifByte(ExifTag.GPSAltitudeRef, (byte)GPSAltitudeRef.AboveSeaLevel);
            jpgFile.Properties.Add(altitudeRef);

            // Debug
            //using (var sw = new StreamWriter(@".\gpslog.txt", true))
            //{
            //    sw.WriteLine($"{jpgFilename}: Lat {trackPoint.Latitude.Value} ({latDms.Degrees}:{latDms.Minutes}:{latDms.Seconds}.{latDms.Milliseconds}), Lon {trackPoint.Longitude.Value} ({lonDms.Degrees}:{lonDms.Minutes}:{lonDms.Seconds}.{lonDms.Milliseconds})");
            //}

            foreach (var exifProp in jpgFile.Properties)
            {
                // Console.WriteLine($"Name: {exifProp.Name}, IFD={exifProp.IFD}, Tag={exifProp.Tag}, Value={exifProp.Value} ");
                if (exifProp.Name == "ImageDescription") exifProp.Value = "Sampled From TimeLapse Video";
            }

            jpgFile.Save(jpgFilename);

        }
    }
}
