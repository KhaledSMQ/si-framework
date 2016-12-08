
namespace Si.Service.ServiceCommunication
{
    public class GracefulShutdownRequest
    {
        public GracefulShutdownRequest(object sender, string reason)
        {
            Sender = sender;
            Reason = reason;
        }

        public object Sender { get; }
        public string Reason { get; }

    }
}
