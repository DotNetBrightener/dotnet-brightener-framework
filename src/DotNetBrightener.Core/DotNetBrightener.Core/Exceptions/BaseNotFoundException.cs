using System.Net;

namespace DotNetBrightener.Core.Exceptions
{
	public class BaseNotFoundException<T> : ExceptionWithStatusCode where T : class
	{
		public BaseNotFoundException() : this("The requested object of type " + typeof(T).Name + " could not be found")
		{

		}
		public BaseNotFoundException(string message) : base(message, HttpStatusCode.NotFound)
		{

		}
	}

	public class BaseNotFoundException : ExceptionWithStatusCode
	{
		public BaseNotFoundException() : this("The requested resource could not be found")
		{

		}
		public BaseNotFoundException(string message) : base(message, HttpStatusCode.NotFound)
		{

		}
	}

	public class DefaultErrorResult
	{
		public string ErrorMessage { get; set; }

		public string FullErrorMessage { get; set; }

		public long? ErrorId { get; set; }

		public string StackTrace { get; set; }

		public string TenantName { get; internal set; }

		public string ErrorType { get; set; }
	}
}
