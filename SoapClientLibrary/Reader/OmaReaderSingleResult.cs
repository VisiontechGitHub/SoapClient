using System.Collections.Generic;

namespace SoapClientService.Reader
{
    public abstract class OmaReaderSingleResult : OmaReaderResult
    {

        public IDictionary<OmaParameter, string> Result
        {
            get; set;
        }

    }
}
