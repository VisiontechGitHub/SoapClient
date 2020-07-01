using SoapClientService;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SoapClientLibrary
{
    public static class StringConverter
    {
        public static bool ExtractBoolValue(IDictionary<OmaParameter, string> parameters, OmaParameter parameter, bool defaultValue = false)
        {
            return ExtractValue(parameters, parameter, value => "1".Equals(value), defaultValue);
        }
        public static double ExtractDoubleValue(IDictionary<OmaParameter, string> parameters, OmaParameter parameter, double defaultValue = 0)
        {
            return ExtractValue(parameters, parameter, value => double.Parse(value, CultureInfo.InvariantCulture), defaultValue);
        }
        public static int ExtractIntValue(IDictionary<OmaParameter, string> parameters, OmaParameter parameter, int defaultValue = 0)
        {
            return ExtractValue(parameters, parameter, value => int.Parse(value, CultureInfo.InvariantCulture), defaultValue);
        }
        private static T ExtractValue<T>(IDictionary<OmaParameter, string> parameters, OmaParameter parameter, Func<string, T> parsingFunction, T defaultValue = default)
        {
            if (!parameters.ContainsKey(parameter) || string.Equals(parameters[parameter], "?"))
            {
                return defaultValue;
            }
            else
            {
                try
                {
                    return parsingFunction.Invoke(parameters[parameter]);
                }
                catch (Exception e)
                {
                    throw new Exception(parameter + ": " + e.Message, e);
                }
            }
        }
    }
}
