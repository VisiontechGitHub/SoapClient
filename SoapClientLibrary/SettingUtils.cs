using System;
using System.Configuration;

namespace SoapClientLibrary
{
    public static class SettingUtils
    {

        public static string ReadSetting(KeyValueConfigurationCollection Settings, string Key, string DefaultValue = default)
        {
            return Settings[Key] is object ? Settings[Key].Value : DefaultValue;
        }

        public static T ReadSetting<T>(KeyValueConfigurationCollection Settings, string Key, Func<string, T> parser, T DefaultValue = default)
        {
            return Settings[Key] is object ? parser(Settings[Key].Value) : DefaultValue;
        }

    }
}
