using System.Collections.Generic;

namespace SoapClientService.Reader
{
    public class OmaReaderDoubleResult : OmaReaderResult
    {

        public IDictionary<OmaParameter, string> Left {
            get; set;
        }

        public IDictionary<OmaParameter, string> Right
        {
            get; set;
        }

    }
}
