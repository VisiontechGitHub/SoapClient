using SoapClientService;
using SoapClientService.Reader;
using System;

namespace SoapClientLibrary
{
    public static class EventUtils
    {

        public static OmaEventWithDuration ToOmaEventWithDuration(OmaReaderResult OmaReaderResult)
        {
            OmaEventWithDuration OmaEvent = default;
            OmaParameter leftDesignName = default;
            OmaParameter rightDesignName = default;
            switch (OmaReaderResult)
            {
                case OmaReaderDoubleResult OmaReaderDoubleResult:
                    leftDesignName = OmaReaderDoubleResult.Left.ContainsKey(OmaParameter.LDNAM) ? OmaParameter.LDNAM : OmaParameter.LNAM;
                    rightDesignName = OmaReaderDoubleResult.Left.ContainsKey(OmaParameter.LDNAM) ? OmaParameter.LDNAM : OmaParameter.LNAM;
                    OmaEvent = new OmaEventWithDuration(OmaReaderDoubleResult.Left[OmaParameter.JOB], OmaReaderDoubleResult.Left[leftDesignName], OmaReaderDoubleResult.Right[rightDesignName], StringConverter.ExtractBoolValue(OmaReaderDoubleResult.Left, OmaParameter._PRECALC), DateTime.Now);
                    break;
                case OmaReaderLeftResult OmaReaderLeftResult:
                    leftDesignName = OmaReaderLeftResult.Result.ContainsKey(OmaParameter.LDNAM) ? OmaParameter.LDNAM : OmaParameter.LNAM;
                    OmaEvent = new OmaEventWithDuration(OmaReaderLeftResult.Result[OmaParameter.JOB], OmaReaderLeftResult.Result[leftDesignName], default, StringConverter.ExtractBoolValue(OmaReaderLeftResult.Result, OmaParameter._PRECALC), DateTime.Now);
                    break;
                case OmaReaderRightResult OmaReaderRightResult:
                    rightDesignName = OmaReaderRightResult.Result.ContainsKey(OmaParameter.LDNAM) ? OmaParameter.LDNAM : OmaParameter.LNAM;
                    OmaEvent = new OmaEventWithDuration(OmaReaderRightResult.Result[OmaParameter.JOB], default, OmaReaderRightResult.Result[rightDesignName], StringConverter.ExtractBoolValue(OmaReaderRightResult.Result, OmaParameter._PRECALC), DateTime.Now);
                    break;
            }
            return OmaEvent;
        }

    }
}
