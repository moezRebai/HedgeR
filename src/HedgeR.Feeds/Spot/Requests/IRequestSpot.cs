namespace HedgeR.Spot.Requests
{
    public interface IRequestSpot
    {

    }

    public record RequestStartSpotFeeder : IRequestSpot;

    public record RequestStopSpotFeeder : IRequestSpot;
}