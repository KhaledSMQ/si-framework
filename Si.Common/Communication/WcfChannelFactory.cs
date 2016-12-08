using System;
using System.ServiceModel;

namespace Si.Common.Communication
{
    public interface IWcfChannelFactory<T>
    {
        void Execute(string endpointConfigurationName, Action<T> action);
        TResult Execute<TResult>(string endpointConfigurationName, Func<T, TResult> func);
    }

    public class WcfChannelFactory<T> : IWcfChannelFactory<T>
    {
        public void Execute(string endpointConfigurationName, Action<T> action)
        {
            Execute(endpointConfigurationName,
            channel =>
            {
                action(channel);
                return true;
            });
        }

        public TResult Execute<TResult>(string endpointConfigurationName, Func<T, TResult> func)
        {
            using (var factory = new ChannelFactory<T>(endpointConfigurationName))
            {
                T channel = factory.CreateChannel();
                return func(channel);
            }
        }

    }
}
