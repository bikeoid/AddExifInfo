using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AddExifInfo
{
    public class ProcessVideoSequence
    {
        private const string FilterPattern = "G*";


        internal void Migrate(string goproImages)
        {

            // Typical sequence is GS#######
            var options = new EnumerationOptions();
            options.RecurseSubdirectories = false;
            options.ReturnSpecialDirectories = false;
            var sequenceFolders = Directory.GetDirectories(goproImages, FilterPattern, options);
            Console.WriteLine($"Found {sequenceFolders.Length} existing sequence folders in the images directory");
            foreach (var sequenceFolder in sequenceFolders)
            {
                ProcessImages(sequenceFolder);
            }

        }

        /// <summary>
        /// Read GPX to get locations, then apply to images.
        /// </summary>
        /// <param name="sequenceFolder"></param>
        private void ProcessImages(string sequenceFolder)
        {

            var gpxFiles = Directory.GetFiles(sequenceFolder, "*.gpx");

            if (gpxFiles.Length != 1)
            {
                throw new Exception ($"Expected a single GPX file, found {gpxFiles.Length} files in {sequenceFolder}.");
            }

            var readGpx = new ReadGPX();
            var tracks = readGpx.LoadGPXTracks(gpxFiles[0]);

            if (tracks.Count != 1)
            {
                throw new Exception($"Expected a single GPX Track, found {tracks.Count} tracks in {gpxFiles[0]}.");
            }

            var track = tracks[0];

            AdjustTimestamps(tracks);

            var imageFiles = Directory.GetFiles(sequenceFolder, "*.JPG");

            if (track.Trackpoints.Count < imageFiles.Length)
            {
                throw new Exception($"Fewer trackpoints than images.   Expected at least {track.Trackpoints.Count} trackpoints, but found {imageFiles.Length} images in {sequenceFolder}.");
            }
            if (track.Trackpoints.Count > imageFiles.Length + 10)
            {
                //throw new Exception($"Many more trackpoints than images.   Have {imageFiles.Length} in {sequenceFolder} but found {track.Trackpoints.Count} trackpoints.");
            }

            var addExifHeading = new AddExifInfo();


            Console.Write($"Checking {imageFiles.Length} images in {sequenceFolder}...");



            int copyCount = 0;
            int gpxIndex = 0;
            int skipped = 0;
            var lastFrameTime = new DateTime(2000, 1, 1); // Very old timestamp to start first data point
            foreach (var frameImage in imageFiles)
            {
                var trackPoint = track.Trackpoints[gpxIndex];

                if (trackPoint.IsValidTrackpoint())
                {

                    var elapsed = (trackPoint.UtcTime.Value - lastFrameTime).TotalMilliseconds;
                    // Rate limit based on elapsed?

                    addExifHeading.AddHeading(frameImage, null, trackPoint);
                    copyCount++;
                } else
                {
                    skipped++;
                }

                gpxIndex++;


            }

            Console.WriteLine($"GeoTagged {copyCount}, skipped {skipped}");
        }

        /// <summary>
        /// GPX file is created with timestamps to the nearest second.  Adjust time
        /// stamps to add fractional time when duplicates detected.
        /// </summary>
        /// <param name="tracks"></param>
        private void AdjustTimestamps(List<ReadGPX.Track> tracks)
        {

            double TimeLapseInterval = 0.5;  // Time lapse interval

            foreach (var track in tracks)
            {
                DateTime? lastTime = null;
                foreach (var trackPoint in track.Trackpoints)
                {
                    if (trackPoint.UtcTime.HasValue && lastTime.HasValue)
                    {
                        if (trackPoint.UtcTime.Value == lastTime.Value) // Assume max of 2 per second
                        {
                            trackPoint.UtcTime = trackPoint.UtcTime.Value.AddSeconds(TimeLapseInterval);
                        }
                    }
                    lastTime = trackPoint.UtcTime;
                }
            }
        }


    }



}
