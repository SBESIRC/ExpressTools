using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ThAreaFrame
{
    class ThAreaFrameUtils
    {
        public static List<int> ParseStoreyString(string str)
        {
            var floors = new List<int>();

            // 匹配X^Y
            string pattern = @"-?\d+[\^]-?\d+";
            Match m = Regex.Match(str, pattern);
            while (m.Success)
            {
                int[] numbers = Array.ConvertAll(m.Value.Split('^'), int.Parse);
                floors.AddRange(Enumerable.Range(numbers[0], (numbers[1] - numbers[0] + 1)));

                m = m.NextMatch();
            }

            // 匹配X'Y
            pattern = @"-?\d+'-?\d+";
            m = Regex.Match(str, pattern);
            while (m.Success)
            {
                int[] numbers = Array.ConvertAll(m.Value.Split('\''), int.Parse);
                floors.AddRange(numbers);

                m = m.NextMatch();
            }

            // 匹配数字
            pattern = @"-?\d+";
            m = Regex.Match(str, pattern);
            while (m.Success)
            {
                floors.Add(Int16.Parse(m.Value));

                m = m.NextMatch();
            }

            // 处理前缀
            if (str.StartsWith("c"))
            {
                // 所有楼层
                return floors.Distinct().ToList();
            }
            else if (str.StartsWith("o"))
            {
                // 所有偶数楼层
                return floors.Distinct().Where(i => i % 2 == 0).ToList();

            }
            else if (str.StartsWith("j"))
            {
                // 所有奇数楼层
                return floors.Distinct().Where(i => i % 2 == 1).ToList();
            }

            return floors.Distinct().ToList();
        }

        public static List<int> ParseStandardStoreyString(string str)
        {
            var floors = new List<int>();

            // 匹配X^Y
            string pattern = @"-?\d+[\^]-?\d+";
            Match m = Regex.Match(str, pattern);
            while (m.Success)
            {
                int[] numbers = Array.ConvertAll(m.Value.Split('^'), int.Parse);
                floors.AddRange(Enumerable.Range(numbers[0], (numbers[1] - numbers[0] + 1)));

                m = m.NextMatch();
            }

            // 匹配X'Y
            pattern = @"-?\d+'-?\d+";
            m = Regex.Match(str, pattern);
            while (m.Success)
            {
                int[] numbers = Array.ConvertAll(m.Value.Split('\''), int.Parse);
                floors.AddRange(numbers);

                m = m.NextMatch();
            }

            // 处理前缀
            if (str.StartsWith("c"))
            {
                // 所有楼层
                return floors.Distinct().ToList();
            }
            else if (str.StartsWith("o"))
            {
                // 所有偶数楼层
                return floors.Distinct().Where(i => i % 2 == 0).ToList();

            }
            else if (str.StartsWith("j"))
            {
                // 所有奇数楼层
                return floors.Distinct().Where(i => i % 2 == 1).ToList();
            }

            return floors.Distinct().ToList();
        }
    }
}
