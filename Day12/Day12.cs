using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Adhoc
{
    class Day12
    {
        public class HeavenlyBody
        {
            public int[] xyz;
            public int[] dxdydz;
            public HeavenlyBody(string positionInput)
            {
                xyz = positionInput.Split(' ')
                    .Select(coord => int.Parse(
                        coord.Where(c => char.IsDigit(c) || c == '-')
                        .Aggregate(string.Empty, (agg, num) => agg + num)))
                    .ToArray();
                dxdydz = new int[] { 0, 0, 0};
            }

            public void AccountForGravity(HeavenlyBody bod)
            {
                for (int i = 0; i < xyz.Length; i++)
                {
                    if (bod.xyz[i] > this.xyz[i])
                    {
                        this.dxdydz[i]++;
                    }
                    else if (bod.xyz[i] < this.xyz[i])
                    {
                        this.dxdydz[i]--;
                    }
                }
            }

            public void ApplyVelocity()
            {
                // Could really use some APL here :/
                for (int i = 0; i < dxdydz.Length; i++)
                {
                    xyz[i] += dxdydz[i];
                }
            }

            public int getEnergy()
            {
                return xyz.Aggregate(0, (sum, num) => sum += Math.Abs(num))
                     * dxdydz.Aggregate(0, (sum, num) => sum += Math.Abs(num));
            }
        }

        static void Main12(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day12\input.txt");
            List<HeavenlyBody> bods = inputProgram.Trim('\n').Split('\n').Select(line => new HeavenlyBody(line)).ToList();

            long r = Simulate(bods);
            int totalEnergy = bods.Select(bod => bod.getEnergy()).Sum();
        }

        public static long Simulate(List<HeavenlyBody> bods)
        {
            long steps = 0;
            string firstState = JsonConvert.SerializeObject(bods);
            
            while (true)
            {
                string state = JsonConvert.SerializeObject(bods);
                if (steps > 0 && firstState.Equals(state))
                {
                    return steps;
                }
                // Calculate gravity effect on each body = O(n^2)
                for (int bod1 = 0; bod1 < bods.Count - 1; bod1++)
                {
                    for (int bod2 = bod1 + 1; bod2 < bods.Count; bod2++)
                    {
                        bods[bod1].AccountForGravity(bods[bod2]);
                        bods[bod2].AccountForGravity(bods[bod1]);
                    }
                }

                foreach (HeavenlyBody bod in bods)
                {
                    bod.ApplyVelocity();
                }
                steps++;
            }
        }
    }
}