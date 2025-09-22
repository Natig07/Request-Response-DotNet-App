namespace Exceptions
{
    public abstract class AppException : Exception
    {
        protected AppException(string message) : base(message) { }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class ValidationExceptionC : AppException
    {
        public ValidationExceptionC(string message) : base(message) { }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    public class ForbiddenException(string message) : AppException(message)
    {
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message) : base(message) { }
    }

    public class ServiceUnavailableException : AppException
    {
        public ServiceUnavailableException(string message) : base(message) { }
    }

    public class InternalServerException : AppException
    {
        public InternalServerException(string message) : base(message) { }
    }

    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base(message) { }
    }

}
