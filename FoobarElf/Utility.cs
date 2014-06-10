using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoobarElf
{
    class Utility
    {
        public static int ToInt(string intString)
        {
            return Convert.ToInt32(intString);
        }

        public static bool ToBool(string boolString)
        {
            return Convert.ToBoolean(Convert.ToInt16(boolString));
        }
    }
}
