using System.Collections.Generic;

namespace SoapClientLibrary
{
    public static class DictionaryUtils
    {

        public static void SafeAdd<K, V>(IDictionary<K, V> Dictionary, K key, V value)
        {
            if (!Dictionary.ContainsKey(key))
            {
                Dictionary.Add(key, value);
            }
        }

    }
}
