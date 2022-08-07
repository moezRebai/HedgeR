namespace HedgeR.Spot.Requests
{
    public interface IRequestSpot
    {
        int Frequency { get; set; }
    }

    public record RequestStartSpotFeeder : IRequestSpot
    {
        public int Frequency { get; set; }
    }

    public record RequestStopSpotFeeder : IRequestSpot
    {
        //To change interface
        public int Frequency { get; set; }
    }
}