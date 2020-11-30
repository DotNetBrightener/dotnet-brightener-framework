using System;

namespace DotNetBrightener.Core.DataAccess.Exceptions
{
    public class RecordNotFoundException<T> : Exception where T : class
    {
        public RecordNotFoundException() : this("The requested object of type " + typeof(T).Name + " could not be found")
        {

        }

        public RecordNotFoundException(string message) : base(message)
        {

        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException() : this("The requested resource could not be found")
        {

        }

        public NotFoundException(string message) : base(message)
        {

        }
    }
}