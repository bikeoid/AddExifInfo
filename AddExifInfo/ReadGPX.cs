using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace AddExifInfo
{
    /// <summary>
    /// Read GPX text file and return structure of tracks containing Time, Lat, Lon, Ele
    /// </summary>
    public class ReadGPX
    {
        /// <summary>
        /// Load the Xml document for parsing
        /// </summary>
        /// <param name="sFile">Fully qualified file name (local)</param>
        /// <returns>XDocument</returns>
        private XDocument GetGpxDoc(string sFile)
        {
            XDocument gpxDoc = XDocument.Load(sFile);
            return gpxDoc;
        }

        /// <summary>
        /// Load the namespace for a standard GPX document
        /// </summary>
        /// <returns></returns>
        private XNamespace GetGpxNameSpace()
        {
            XNamespace gpx = XNamespace.Get("http://www.topografix.com/GPX/1/1");
            return gpx;
        }

        /// <summary>
        /// When passed a file, open it and parse all waypoints from it.
        /// </summary>
        /// <param name="sFile">Fully qualified file name (local)</param>
        /// <returns>string containing line delimited waypoints from
        /// the file (for test)</returns>
        /// <remarks>Normally, this would be used to populate the
        /// appropriate object model</remarks>
        public string LoadGPXWaypoints(string sFile)
        {
            XDocument gpxDoc = GetGpxDoc(sFile);
            XNamespace gpx = GetGpxNameSpace();

            var waypoints = from waypoint in gpxDoc.Descendants(gpx + "wpt")
                            select new
                            {
                                Latitude = waypoint.Attribute("lat").Value,
                                Longitude = waypoint.Attribute("lon").Value,
                                Elevation = waypoint.Element(gpx + "ele") != null ?
                                  waypoint.Element(gpx + "ele").Value : null,
                                Name = waypoint.Element(gpx + "name") != null ?
                                  waypoint.Element(gpx + "name").Value : null,
                                Dt = waypoint.Element(gpx + "cmt") != null ?
                                  waypoint.Element(gpx + "cmt").Value : null
                            };

            StringBuilder sb = new StringBuilder();
            foreach (var wpt in waypoints)
            {
                // This is where we'd instantiate data
                // containers for the information retrieved.
                sb.Append(
                  string.Format("Name:{0} Latitude:{1} Longitude:{2} Elevation:{3} Date:{4}\n",
                  wpt.Name, wpt.Latitude, wpt.Longitude,
                  wpt.Elevation, wpt.Dt));
            }

            return sb.ToString();
        }

        /// <summary>
        /// When passed a file, open it and parse all tracks
        /// and track segments from it.
        /// </summary>
        /// <param name="sFile">Fully qualified file name (local)</param>
        /// <returns>List of tracks, with each track's trackpoints</returns>
        public List<Track> LoadGPXTracks(string sFile)
        {
            XDocument gpxDoc = GetGpxDoc(sFile);
            XNamespace gpx = GetGpxNameSpace();
            var tracksQuery = from track in gpxDoc.Descendants(gpx + "trk")
                         select new
                         {
                             Name = track.Element(gpx + "name") != null ?
                            track.Element(gpx + "name").Value : null,
                             WayPts = (
                                from trackpoint in track.Descendants(gpx + "trkpt")
                                select new
                                {
                                    Latitude = trackpoint.Attribute("lat").Value,
                                    Longitude = trackpoint.Attribute("lon").Value,
                                    Elevation = trackpoint.Element(gpx + "ele") != null ?
                                    trackpoint.Element(gpx + "ele").Value : null,
                                    Time = trackpoint.Element(gpx + "time") != null ?
                                    trackpoint.Element(gpx + "time").Value : null
                                }
                              )
                         };


            var tracks = new List<Track>();

            StringBuilder sb = new StringBuilder();
            foreach (var trk in tracksQuery)
            {
                var track = new Track();
                track.TrackName = trk.Name;
                tracks.Add(track);

                // Populate track data objects.
                foreach (var wayPt in trk.WayPts)
                {
                    // Populate detailed track segments
                    // in the object model
                    var trackPoint = new Trackpoint();
                    trackPoint.Latitude = GetDouble(wayPt.Latitude);
                    trackPoint.Longitude = GetDouble(wayPt.Longitude);
                    trackPoint.Elevation = GetDouble(wayPt.Elevation);
                    trackPoint.UtcTime = GetTime(wayPt.Time);

                    track.Trackpoints.Add(trackPoint);

                }
            }
            return tracks;
        }

        private Double? GetDouble(string numValue)
        {
            if (numValue == null) return null;

            return Convert.ToDouble(numValue);
        }

        private DateTime? GetTime(string timeValue)
        {
            if (timeValue == null) return null;

            var localTime = DateTime.Parse(timeValue);
            var utcTime = localTime.ToUniversalTime();
            return utcTime;
        }



        public class Track
        {
            public Track()
            {
                Trackpoints = new List<Trackpoint>();
            }

            public String TrackName { get; set; }
            public List<Trackpoint> Trackpoints { get; set; }
        }


        public class Trackpoint
        {
            public bool IsValidTrackpoint()
            {
                if (!(UtcTime.HasValue &&
                    Latitude.HasValue &&
                    Longitude.HasValue &&
                    Elevation.HasValue)) return false;

                if (Longitude.Value == 0.0 || Latitude.Value == 0.0) return false;

                return true;
            }

            public DateTime? UtcTime { get; set; }
            public Double? Latitude { get; set; }
            public Double? Longitude { get; set; }
            public Double? Elevation { get; set; }
        }

    }
}

