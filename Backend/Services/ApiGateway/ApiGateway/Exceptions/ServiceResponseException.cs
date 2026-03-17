namespace ApiGateway.Exceptions
{
    public class ServiceResponseException : Exception
    {
        public string ServiceName { get; }
        public int? StatusCode { get; }

        public ServiceResponseException(string serviceName, string message, int? statusCode = null)
            : base(message)
        {
            ServiceName = serviceName;
            StatusCode = statusCode;
        }

        public ServiceResponseException(string serviceName, string message, Exception innerException, int? statusCode = null)
            : base(message, innerException)
        {
            ServiceName = serviceName;
            StatusCode = statusCode;
        }
    }
}
