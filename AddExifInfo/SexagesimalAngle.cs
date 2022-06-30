using System;

public class SexagesimalAngle
{
    public bool IsNegative { get; set; }
    public int Degrees { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }
    public int Milliseconds { get; set; }

    public enum DirectionType {NorthSouth, EastWest };


    public static SexagesimalAngle FromDouble(double angleInDegrees)
    {
        //ensure the value will fall within the primary range [-180.0..+180.0]
        while (angleInDegrees < -180.0)
            angleInDegrees += 360.0;

        while (angleInDegrees > 180.0)
            angleInDegrees -= 360.0;

        var result = new SexagesimalAngle();

        //switch the value to positive
        result.IsNegative = angleInDegrees < 0;
        angleInDegrees = Math.Abs(angleInDegrees);

        //gets the degree
        result.Degrees = (int)Math.Floor(angleInDegrees);
        var delta = angleInDegrees - result.Degrees;

        //gets minutes and seconds
        var seconds = (int)Math.Floor(3600.0 * delta);
        result.Seconds = seconds % 60;
        result.Minutes = (int)Math.Floor(seconds / 60.0);
        delta = delta * 3600.0 - seconds;

        //gets fractions
        result.Milliseconds = (int)(1000.0 * delta);

        return result;
    }



    public override string ToString()
    {
        var degrees = this.IsNegative
            ? -this.Degrees
            : this.Degrees;

        return string.Format(
            "{0}° {1:00}' {2:00}\"",
            degrees,
            this.Minutes,
            this.Seconds);
    }



    //GPS Latitude                    : 34 deg 44' 16.44" N
    //GPS Longitude                   : 82 deg 15' 20.75" W
    public string ToString(DirectionType directionType)
    {
        switch (directionType)
        {
            case DirectionType.NorthSouth:
                return $"{this.Degrees} deg {this.Minutes:00}' {this.Seconds:00}\".{this.Milliseconds:000} {(this.IsNegative ? 'S' : 'N')}";

            case DirectionType.EastWest:
                return $"{this.Degrees} deg {this.Minutes:00}' {this.Seconds:00}\".{this.Milliseconds:000} {(this.IsNegative ? 'W' : 'E')}";

            default:
                throw new NotImplementedException();
        }
    }
}