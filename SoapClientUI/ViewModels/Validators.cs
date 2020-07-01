using SoapClientUI.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoapClientUI.ViewModels
{
    public static class Validators
    {

        public static Func<string, string> NotEmptyString = value => string.IsNullOrWhiteSpace(value) ? Resources.App_Validations_Value_Cannot_Be_Empty : string.Empty;
        public static Func<string, string> EmptyOrNumber = value => {
            double result;
            return !string.IsNullOrWhiteSpace(value) && !double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result) ? Resources.App_Validations_Value_Invalid : string.Empty;
        };

    }
}
