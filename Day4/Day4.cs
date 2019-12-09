using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Adhoc
{
    class Day4
    {
        static void Main4(string[] args)
        {
            int start = 356261;
            int end = 846303;
            int count = 0;
            for (int i = start; i <= end; i++)
            {
                if (!twoAdjSame(i))
                {
                    continue;
                }
                if (!increasing(i))
                {
                    continue;
                }

                count++;
            }

            Console.WriteLine(count);
        }

        private static bool increasing(int input)
        {
            char[] a = input.ToString().ToCharArray();
            int last = -1;
            foreach (char c in a)
            {
                int curr = Int32.Parse(c.ToString());
                if (curr < last)
                {
                    return false;
                }
                last = curr;
            }
            return true;
        }

        private static bool twoAdjSame(int input)
        {
            char[] a = input.ToString().ToCharArray();
            char last = ' ';
            int run = 0;
            foreach (char c in a)
            {
                if (c == last)
                {
                    run++;
                }
                else
                {
                    if (run == 2)
                    {
                        return true;
                    }
                    run = 1;
                }
                last = c;
            }
            if (run == 2)
            {
                return true;
            }
            return false;
        }
    }
}