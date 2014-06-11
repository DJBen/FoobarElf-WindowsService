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
            int result = -1;
            try
            {
                result = Convert.ToInt32(intString);
            }
            catch
            {
                return -1;
            }
            return result;
        }

        public static bool ToBool(string boolString)
        {
            bool result = false;
            try
            {
                result = Convert.ToBoolean(Convert.ToInt16(boolString));
            }
            catch
            {
                Console.WriteLine("Warning");
                return false;
            }
            return result;
        }
    }
}
