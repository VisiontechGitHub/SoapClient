using Org.Visiontech.Compute;
using SoapClientLibrary;
using SoapClientService.Error;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoapClientService.Reader
{
    public class OmaReader
    {

        public static readonly Regex ldsKeyRegex = new Regex("([^=]+)=[^=]+");
        public static readonly Regex ldsSingleValueRegex = new Regex("[^=]+=([^=]+)");
        public static readonly Regex ldsDoubleValueRegex = new Regex("[^=]+=([^;]*);([^;]*)");
        
        public static OmaReaderResult Read(string path)
        {
            IDictionary<OmaParameter, string> right = new Dictionary<OmaParameter, string>();
            IDictionary<OmaParameter, string> left = new Dictionary<OmaParameter, string>();

            string DO_VALUE = default;

            using (StreamReader sr = new StreamReader(path))
            {
                for (string line = sr.ReadLine(); line != null; line = sr.ReadLine())
                {
                    if (ldsKeyRegex.IsMatch(line) && ldsKeyRegex.Match(line) is Match keyMatch && Enum.TryParse(keyMatch.Groups[1].Value, out OmaParameter key))
                    {
                        if (ldsDoubleValueRegex.IsMatch(line) && ldsDoubleValueRegex.Match(line) is Match doubleValueMatch)
                        {
                            DictionaryUtils.SafeAdd(right, key, doubleValueMatch.Groups[1].Value);
                            DictionaryUtils.SafeAdd(left, key, doubleValueMatch.Groups[2].Value);
                        }
                        else if (ldsSingleValueRegex.IsMatch(line) && ldsSingleValueRegex.Match(line) is Match singleValueMatch && singleValueMatch.Groups[1].Value is string value)
                        {
                            DictionaryUtils.SafeAdd(right, key, value);
                            DictionaryUtils.SafeAdd(left, key, value);
                            if (OmaParameter.DO.Equals(key))
                            {
                                DO_VALUE = value;
                            }
                        }
                    }
                }
            }

            OmaReaderResult result = default;

            switch (DO_VALUE) {
                case "B":
                    result = new OmaReaderDoubleResult
                    {
                        Left = left,
                        Right = right
                    };
                    CheckRequiredParameters(left, side.LEFT);
                    CheckRequiredParameters(right, side.RIGHT);
                    break;
                case "L":
                    result = new OmaReaderLeftResult
                    {
                        Result = left
                    };
                    CheckRequiredParameters(left, side.LEFT);
                    break;
                case "R":
                    result = new OmaReaderRightResult
                    {
                        Result = right
                    };
                    CheckRequiredParameters(right, side.RIGHT);
                    break;
                default:
                    throw new OmaException("Missing Required Parameter: DO", null, side.UNKNOWN, OmaStatusCode.MissingRequiredParameter);
            }

            return result;
        }

        private static void CheckRequiredParameters(IDictionary<OmaParameter, string> parameters, side side)
        {
            OmaParameter[][] requirements = new[] {
                new[] { OmaParameter.LNAM, OmaParameter.LDNAM },
                new[] { OmaParameter.SPH },
                new[] { OmaParameter.LIND },
                /*new[] { OmaParameter.BLKD },*/
                new[] { OmaParameter.CRIB }
            };

            requirements.Where(Keys => !Keys.Any(Key => parameters.ContainsKey(Key))).ToList().ForEach(
                Keys =>
                {
                    throw new OmaException("Missing Required Parameter: " + string.Join(",", Keys), null, side, OmaStatusCode.MissingRequiredParameter);
                });
        }

    }
}
