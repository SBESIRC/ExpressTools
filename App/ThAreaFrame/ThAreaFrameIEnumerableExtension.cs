using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrame
{
    public static class ThAreaFrameIEnumerableExtension
    {
        public static IEnumerable<List<int>> ConsecutiveSequences(this IEnumerable<int> input)
        {
            var sequences = new List<List<int>>();
            foreach (var i in input)
            {
                var existing = sequences.FirstOrDefault(o => o.Last() + 1 == i);
                if (existing == null)
                {
                    sequences.Add(new List<int> { i });
                }
                else
                {
                    existing.Add(i);
                }
            }

            return sequences.Where(o => o.Count >= 2);
        }

        public static IEnumerable<List<int>> OddSequences(this IEnumerable<int> input)
        {
            var sequences = new List<List<int>>();
            foreach (var i in input)
            {
                if (i % 2 == 0)
                {
                    continue;
                }

                var existing = sequences.FirstOrDefault(o => o.Last() + 2 == i);
                if (existing == null)
                {
                    sequences.Add(new List<int> { i });
                }
                else
                {
                    existing.Add(i);
                }
            }

            return sequences.Where(o => o.Count >= 2);
        }

        public static IEnumerable<List<int>> EvenSequences(this IEnumerable<int> input)
        {
            var sequences = new List<List<int>>();
            foreach (var i in input)
            {
                if (i % 2 == 1)
                {
                    continue;
                }

                var existing = sequences.FirstOrDefault(o => o.Last() + 2 == i);
                if (existing == null)
                {
                    sequences.Add(new List<int> { i });
                }
                else
                {
                    existing.Add(i);
                }
            }

            return sequences.Where(o => o.Count >= 2);
        }
    }
}
