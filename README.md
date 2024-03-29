### Add GPS location info to the EXIF section of 360 degree JPGs extracted from GoPro

Toolset -

 * Visual Studio 2022 + .NET Core 8
 * Video equipment: GoPro Max, but could be applicable to other types

## Overall work flow, including steps not part of this package

 1. Acquire 360 video using GoPro: Suggest recording in Time Lapse mode, speeds up to 0.5 seconds.  This creates *.360 video
 1. Extract GPX trace from original *.360 video using Python script [gopro2gpx](https://github.com/juanmcasillas/gopro2gpx 'gopro2gpx')
 1. Use GoPro app to export to 5.6K .mov H264 video
 1. Use FFMPEG to extract each frame image from .MP4 video
    * Sample command line: ffmpeg.exe  -i $1 -qmin 1 -qscale:v 1  -nostdin "$outputDir/$baseFilename/$baseFilename"_%06d.jpg
    * Where $1 is the .MP4 export file from the GoPro app and $outputDire and $baseFilename are batch file variables
 5. Run this app (AddExifInfo) on the output directory to match the GPX to images and update EXIF tags.
 1. Upload to Mapillary / Kartaview


