using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Adhoc
{
    class Day10
    {
        static void Main(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day10\input.txt");
            char[][] astroidMap = inputProgram.Trim('\n').Split('\n').Select(line => line.ToCharArray()).ToArray();

            int maxRoids = 0;
            Tuple<int, int> bestPos = null;
            // find each asteroid
            for (int row = 0; row < astroidMap.Length; row++)
            {
                for (int col = 0; col < astroidMap[row].Length; col++)
                {
                    if (astroidMap[row][col] == '#')
                    {
                        HashSet<double> roidAngles = new HashSet<double>();
                        int countVisible = 0;
                        // Add points with unique angles (BFS from input point)
                        searchAstroids(astroidMap, row, col, (double angle, Tuple<int,int> point) =>
                        {
                            if (!roidAngles.Contains(angle))
                            {
                                roidAngles.Add(angle);
                                countVisible++;
                            }
                        });
                        if (countVisible > maxRoids)
                        {
                            maxRoids = Math.Max(maxRoids, countVisible);
                            bestPos = new Tuple<int, int>(row, col);
                        }
                    }
                }
            }
            Console.WriteLine(maxRoids);
            Dictionary<double, List<Tuple<int, int>>> anglesToPoints = new Dictionary<double, List<Tuple<int, int>>>();
            SortedSet<double> sortedAngles = new SortedSet<double>();
            // Add angles to a sorted list to laser in order
            // Add points to dictionary keyed off points
            searchAstroids(astroidMap, bestPos.Item1, bestPos.Item2, (double angle, Tuple<int, int> point) =>
            {
                if (!anglesToPoints.ContainsKey(angle))
                {
                    anglesToPoints[angle] = new List<Tuple<int, int>>();
                    sortedAngles.Add(angle);
                }
                anglesToPoints[angle].Add(point);
            });

            // Fire away
            int laserCount = 0;
            Tuple<int, int> lastPoint = null;
            while (laserCount < 200)
            {
                foreach (double angle in sortedAngles)
                {
                    if (anglesToPoints.ContainsKey(angle))
                    {
                        laserCount++;
                        lastPoint = anglesToPoints[angle].First();
                        anglesToPoints[angle].RemoveAt(0);
                        if (anglesToPoints[angle].Count == 0)
                        {
                            anglesToPoints.Remove(angle);
                        }
                        if (laserCount == 200)
                        {
                            break;
                        }
                    }
                }
            }

            Console.WriteLine(100 * lastPoint.Item2 + lastPoint.Item1);
        }

        private static void searchAstroids(char[][] astroidMap, int row, int col, Action<double, Tuple<int,int>> process)
        {
            HashSet<Tuple<int, int>> pointsEnqueued = new HashSet<Tuple<int, int>>();
            var startingPoint = new Tuple<int, int>(row, col);
            Queue<Tuple<int, int>> toExplore = new Queue<Tuple<int, int>>();

            toExplore.Enqueue(startingPoint);

            while (toExplore.Count > 0)
            {
                var exploring = toExplore.Dequeue();

                // check all that touches this spot
                var adjacentPoints = getAdjacentPoints(astroidMap, exploring.Item1, exploring.Item2);
                foreach (var point in adjacentPoints)
                {
                    if (!pointsEnqueued.Contains(point))
                    {
                        if (startingPoint.Item1 != point.Item1 || startingPoint.Item2 != point.Item2)
                        {
                            if (astroidMap[point.Item1][point.Item2] == '#')
                            {
                                var angle = getAngle(startingPoint, point);
                                process(angle, point);
                            }
                            toExplore.Enqueue(point);
                            pointsEnqueued.Add(point);
                        }
                    }
                }
            }
        }

        private static List<Tuple<int, int>> getAdjacentPoints(char[][] astroidMap, int row, int col)
        {
            var points = new List<Tuple<int, int>>();
            for (int i = Math.Max(row - 1, 0); i <= row + 1; i++)
            {
                for (int j = Math.Max(col - 1, 0); j <= col + 1; j++)
                {
                    if (i == row && j == col)
                    {
                        continue;
                    }
                    if (i < astroidMap.Length && j < astroidMap[i].Length)
                    {
                        points.Add(new Tuple<int, int>(i, j));
                    }
                }
            }

            return points;
        }

        private static double getAngle(Tuple<int,int> p1, Tuple<int, int> p2)
        {
            int dy = p2.Item1 - p1.Item1;
            int dx = p2.Item2 - p1.Item2;

            double inRads = Math.Atan2(dy, dx);
            if (inRads < 0)
                inRads = Math.Abs(inRads);
            else
                inRads = 2 * Math.PI - inRads;

            double degrees = (180 / Math.PI) * inRads;
            // flip the plane to go clockwise
            degrees = (360 - degrees) % 360;
            // Rotate by 90 to start the laser vertically
            return (degrees + 90) % 360;
        }
    }
}