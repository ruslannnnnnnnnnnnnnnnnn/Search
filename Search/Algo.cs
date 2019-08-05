using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace Search
{
    class Algo
    {
        public static KeyValuePair<string, long>[] InvertList(string invertList)
        {
            if (invertList == null) return new KeyValuePair<string, long>[0];

            var matches = Regex.Matches(invertList, @"(?<=\[)\w+, \d+(?=\])");
            var res = new KeyValuePair<string, long>[matches.Count];
            int count = 0;

            foreach (Match match in matches)
            {
                int index = match.Value.IndexOf(',');
                res[count++] = new KeyValuePair<string, long>(match.Value.Substring(0, index), long.Parse(match.Value.Substring(index + 1)));
            }

            return res;
        }
    }
}
