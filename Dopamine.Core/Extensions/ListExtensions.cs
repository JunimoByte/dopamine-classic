using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dopamine.Core.Extensions
{
    public static class ListExtensions
    {
        public static string FirstNonEmpty(this IEnumerable<string> strings, string alternateString)
        {
            foreach (string item in strings)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    return item;
                }
            }

            return alternateString;
        }

        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        private static readonly System.Threading.ThreadLocal<Random> ThreadRandom = new System.Threading.ThreadLocal<Random>(() =>
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                byte[] buffer = new byte[4];
                rng.GetBytes(buffer);
                return new Random(BitConverter.ToInt32(buffer, 0));
            }
        });

        public static List<T> Randomize<T>(this List<T> list)
        {
            if (list == null) return new List<T>();
            
            var randomList = new List<T>(list); // Create a new list to not mutate original
            var r = ThreadRandom.Value;

            // O(N) Fisher-Yates Shuffle (In-place)
            for (int i = randomList.Count - 1; i > 0; i--)
            {
                int j = r.Next(0, i + 1);
                T temp = randomList[i];
                randomList[i] = randomList[j];
                randomList[j] = temp;
            }

            return randomList;
        }

        public static IEnumerable<string> SortNaturally(this IEnumerable<string> strings)
        {
            return strings.OrderBy(x => Regex.Replace(x, @"\d+", match => match.Value.PadLeft(4, '0')));
        }
    }
}
