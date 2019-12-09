using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Adhoc
{
    class Program
    {
        static void Main1(string[] args)
        {
            string inputMasses = File.ReadAllText("1.input.txt");
            string[] masses = inputMasses.Trim('\n').Split('\n');
            var totalFuelNeeded = masses.Select((mass) => (Int32.Parse(mass) / 3) - 2)
                .Aggregate(0, (sum, next) => sum + GetFuelFuel(next));

            Console.WriteLine(totalFuelNeeded);
        }

        private static int GetFuelFuel(int fuel)
        {
            if (fuel <= 0)
            {
                return 0;
            }
            return fuel + GetFuelFuel((fuel / 3) - 2);
        }
    }
}