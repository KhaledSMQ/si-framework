using System.Threading.Tasks;

namespace Si.Service
{
    public interface IService
    {
        Task StartAsync();
        Task StopAsync();
    }
}
