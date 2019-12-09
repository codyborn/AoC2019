using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Adhoc
{
    class Day3
    {
        static void Main3(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day3\input.txt");
            string[] wires = inputProgram.Trim('\n').Split('\n');
            string[] wire0 = wires[0].Split(',');
            string[] wire1 = wires[1].Split(',');

            Dictionary<string, int> points = new Dictionary<string, int>();
            int minSteps = Int32.MaxValue;

            // Write for first wire
            TraverseWire(wire0, (x, y, steps) =>
            {
                string point = x + "," + y;
                if (!points.ContainsKey(point))
                {
                    points.Add(point, steps);
                }
            });

            // Read for second wire
            TraverseWire(wire1, (x, y, steps) =>
            {
                string point = x + "," + y;
                if (points.ContainsKey(point))
                {
                    int sumSteps = points[point] + steps;
                    minSteps = Math.Min(minSteps, sumSteps);
                }
            });

            Console.WriteLine(minSteps);
        }

        private static void TraverseWire(string[] wire, Action<int, int, int> check)
        {
            int x = 0;
            int y = 0;
            int steps = 0;
            foreach (string direction in wire)
            {
                int distance = Int32.Parse(direction.Remove(0, 1));
                switch (direction.ElementAt(0))
                {
                    case 'R':
                        for (int i = 1; i <= distance; i++)
                        {
                            x++;
                            check(x, y, ++steps);
                        }
                        break;
                    case 'U':
                        for (int i = 1; i <= distance; i++)
                        {
                            y++;
                            check(x, y, ++steps);
                        }
                        break;
                    case 'L':
                        for (int i = 1; i <= distance; i++)
                        {
                            x--;
                            check(x, y, ++steps);
                        }
                        break;
                    case 'D':
                        for (int i = 1; i <= distance; i++)
                        {
                            y--;
                            check(x, y, ++steps);
                        }
                        break;
                    default:
                        throw new NotImplementedException("Wire can only move in two dimensions");
                }
            }
        }
    }
}