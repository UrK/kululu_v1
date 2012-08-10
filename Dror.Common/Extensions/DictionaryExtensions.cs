using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dror.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddRange<T, U>(this IDictionary<T, U> D, IEnumerable<KeyValuePair<T, U>> V)
        {
            foreach (var kvp in V)
            {
                if (D.ContainsKey(kvp.Key))
                {
                    throw new ArgumentException("An item with the same key has already been added.");
                }
                D.Add(kvp);
            }
        }
    }
}
