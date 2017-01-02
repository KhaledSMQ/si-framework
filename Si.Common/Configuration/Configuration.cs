using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;

namespace Si.Common.Configuration
{
    public interface IConfiguration
    {
        string GetConfigValueOrError(string key);
        T GetConfigValueOrError<T>(string key);
        string GetConfigValueOrError(string key, string requiredReason);
        T GetConfigValueOrError<T>(string key, string requiredReason);
        bool TryGetConfigValue(string key, out string value);
        bool TryGetConfigValue<T>(string key, out T value);
        bool TryGetConfigValue(string key, string defaultValue, out string value);
        bool TryGetConfigValue<T>(string key, T defaultValue, out T value);
        bool TryGetConfigValue<T>(string key, T defaultValue, Func<string, T> parse, out T value);
    }

    [Export(typeof(IConfiguration))]
    public class Configuration : IConfiguration
    {
        public string GetConfigValueOrError(string key)
        {
            return GetConfigValueOrError<string>(key);
        }

        public T GetConfigValueOrError<T>(string key)
        {
            return GetConfigValueOrError<T>(key, null);
        }

        public string GetConfigValueOrError(string key, string requiredReason)
        {
            return GetConfigValueOrError<string>(key, requiredReason);
        }

        public T GetConfigValueOrError<T>(string key, string requiredReason)
        {
            T value;
            if (!TryGetConfigValue(key, out value))
            {
                string rootError = $"Configuration does not contain a value for key {key}.";
                string errorMessage;

                if (requiredReason == null)
                {
                    errorMessage = rootError;
                }
                else
                {
                    errorMessage = $"{rootError} This setting is required for the following reason: {requiredReason}.";
                }

                throw new Exception(errorMessage);
            }

            return value;
        }

        public bool TryGetConfigValue(string key, out string value)
        {
            return TryGetConfigValue<string>(key, out value);
        }

        public bool TryGetConfigValue<T>(string key, out T value)
        {
            return TryGetConfigValue(key, default(T), out value);
        }

        public bool TryGetConfigValue(string key, string defaultValue, out string value)
        {
            return TryGetConfigValue(key, defaultValue, v => v, out value);
        }

        public bool TryGetConfigValue<T>(string key, T defaultValue, out T value)
        {
            return TryGetConfigValue(key, defaultValue, v => (T) Convert.ChangeType(v, typeof(T)), out value);
        }

        public bool TryGetConfigValue<T>(string key, T defaultValue, Func<string, T> parse, out T value)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                var stringValue = ConfigurationManager.AppSettings[key];
                value = parse(stringValue);
                return true;
            }

            value = defaultValue;
            return false;
        }

    }
}
