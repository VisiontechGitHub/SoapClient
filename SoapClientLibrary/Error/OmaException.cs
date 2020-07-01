using Org.Visiontech.Compute;
using System;

namespace SoapClientService.Error
{
    public class OmaException : Exception
    {

        public side Side
        {
            get;
        }

        public OmaStatusCode OmaStatusCode
        {
            get;
        }

        public OmaException(string message, Exception innerException, side Side = side.UNKNOWN, OmaStatusCode OmaStatusCode = OmaStatusCode.General) : base(message, innerException) {
            this.Side = Side;
            this.OmaStatusCode = OmaStatusCode;
        }

    }
}
