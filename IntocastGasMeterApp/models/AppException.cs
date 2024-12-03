using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntocastGasMeterApp.models
{
    internal abstract class AppException : Exception
    {
        public AppException(string message) : base(message) { }
    }

    class NoInternetException : AppException
    {
        public NoInternetException(string message) : base(message) { }
    }

    class NoDataAvailableException: AppException
    {
        public NoDataAvailableException(string message): base(message) { }
    }

    class ApiChangedException: AppException
    {
        public ApiChangedException(string message) : base(message) { }
    }

    class BadLoginException: AppException
    {
        public BadLoginException(string message) : base(message) { }
    }
}
