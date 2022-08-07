using System.Threading.Channels;

namespace HedgeR.Spot.Requests
{
    public class SpotRequestChannel
    {
        public readonly Channel<IRequestSpot> Requests = Channel.CreateUnbounded<IRequestSpot>();
    }
}
