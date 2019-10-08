using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonDal
{
    internal static class Utils
    {
        public static bool DictionaryEquals(this IDictionary<string, object> dictionary1, IDictionary<string, object> dictionary2, StringComparison keyComparison)
        {
            if (dictionary1 == dictionary2)
                return true;

            if (dictionary1 == null || dictionary2 == null || dictionary1.Count != dictionary2.Count)
                return false;

            var ary1 = dictionary1.OrderBy(kvp => kvp.Key).ToArray();
            var ary2 = dictionary2.OrderBy(kvp => kvp.Key).ToArray();

            for (int i = 0; i < ary1.Length; i++)
            {
                var (name1, value1) = ary1[0];
                var (name2, value2) = ary2[0];

                if (!string.Equals(name1, name2, keyComparison) || !object.Equals(value1, value2))
                    return false;
            }

            return true;
        }

        public static void Deconstruct<K, V>(this KeyValuePair<K, V> tuple, out K key, out V value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }
    }
}
