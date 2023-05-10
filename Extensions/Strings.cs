using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Extensions
{
    public static class Strings
    {
        public static int AddChars(this string value)
        {
            int count = 0;
            for (int i = 0; i < value.Length; i++)
                count += value[i];
            return count;
        }
    }
}
