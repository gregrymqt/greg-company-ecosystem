namespace MeuCrudCsharp.Features.Exceptions
{
    public class AppServiceException : Exception
    {
        public AppServiceException(string message)
            : base(message) { }

        public AppServiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class ExternalApiException : AppServiceException
    {
        public ExternalApiException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException(string message)
            : base(message) { }

        public ResourceNotFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
