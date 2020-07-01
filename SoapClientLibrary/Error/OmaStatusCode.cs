using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoapClientService.Error
{
    public enum OmaStatusCode
    {
        NoError = 0,
        General = 3,
        SphereOutOfRange = 1001,
        IncorrectLdname = 1002,
        MissingRequiredParameter = 1003
    }
}
