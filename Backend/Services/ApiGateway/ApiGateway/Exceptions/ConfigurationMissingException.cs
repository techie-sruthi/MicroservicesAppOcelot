namespace ApiGateway.Exceptions
{
    public class ConfigurationMissingException : Exception
    {
        public string ConfigurationKey { get; }

        public ConfigurationMissingException(string configurationKey)
            : base($"{configurationKey} is missing in configuration.")
        {
            ConfigurationKey = configurationKey;
        }

        public ConfigurationMissingException(string configurationKey, Exception innerException)
            : base($"{configurationKey} is missing in configuration.", innerException)
        {
            ConfigurationKey = configurationKey;
        }
    }
}
