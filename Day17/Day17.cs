using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace AdventOfCode2019
{
    class Day17
    {
        static void Main(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day17\input.txt");
            long[] instructionSet = inputProgram.Trim('\n').Split(',').Select((instr) => Int64.Parse(instr)).ToArray();
            LargeMemSet memSet = new LargeMemSet(instructionSet);
            var vBot = new VacuumRobot();
            Task intCodeComputer = ComputeIntCode(memSet, vBot);
            int result = vBot.SumAlignmentParams();
            vBot.findThreeSegments();
            intCodeComputer.Wait();
            Console.WriteLine("\nDust collected:" + vBot.LastOutput);
        }

        public class LargeMemSet
        {
            private Dictionary<long, long> largeMemInstructionSet = new Dictionary<long, long>();
            public LargeMemSet(long[] instructionSet)
            {
                for (int i = 0; i < instructionSet.Length; i++)
                {
                    largeMemInstructionSet[i] = instructionSet[i];
                }
            }

            public long this[long key]
            {
                get
                {
                    if (!largeMemInstructionSet.ContainsKey(key))
                    {
                        return 0;
                    }
                    return largeMemInstructionSet[key];
                }
                set
                {
                    largeMemInstructionSet[key] = value;
                }
            }
        }

        public class VacuumRobot
        {
            private const bool DEBUG = false;
            private Dictionary<Tuple<int, int>, char> map = new Dictionary<Tuple<int, int>, char>();
            private Tuple<int, int> currPos = new Tuple<int, int>(0, 0);
            private Tuple<int, int> currBotPos = new Tuple<int, int>(0, 0);
            private int maxX = 0;
            private int maxY = 0;
            private List<Vector> path = new List<Vector>();
            private Tuple<int, int> currentDirection;
            private List<char> botDirections = null;
            private bool sentLastDirectionInstruction = false;
            
            public long LastOutput
            {
                get;
                set;
            }

            public class Vector : ICloneable
            {
                public Direction direction { get; set; }
                public int distance { get; set; }

                public Vector(Direction direction, int distance)
                {
                    this.direction = direction;
                    this.distance = distance;
                }

                public object Clone()
                {
                    return new Vector(this.direction, this.distance);
                }

                public override string ToString()
                {
                    // ex. R,10
                    return direction.ToString()[0] + "," + distance.ToString();
                }
                public override bool Equals(object obj)
                {
                    return obj is Vector &&
                        this.direction == ((Vector)obj).direction &&
                        this.distance == ((Vector)obj).distance;
                }

                public enum Direction
                {
                    Right,
                    Left
                }
            }

            public void SetCameraOutput(int ascii)
            {
                if (this.sentLastDirectionInstruction)
                {
                    this.LastOutput = ascii;
                }

                char symbol = (char)ascii;
                if (symbol == '\n')
                {
                    maxX = Math.Max(maxX, currPos.Item1);
                    currPos = new Tuple<int, int>(0, currPos.Item2 + 1);
                    maxY = currPos.Item2;
                }
                else
                {
                    map[currPos] = symbol;
                    if (symbol == '<' ||
                        symbol == 'v' ||
                        symbol == '^' ||
                        symbol == '>')
                    {
                        currBotPos = currPos;
                        if (symbol == '<')
                        {
                            currentDirection = new Tuple<int, int>(-1, 0);
                        }
                        else if (symbol == '^')
                        {
                            currentDirection = new Tuple<int, int>(0, -1);
                        }
                        else if (symbol == '>')
                        {
                            currentDirection = new Tuple<int, int>(1, 0);
                        }
                        else
                        {
                            currentDirection = new Tuple<int, int>(0, 1);
                        }
                    }

                    currPos = new Tuple<int, int>(currPos.Item1 + 1, currPos.Item2);
                }

                Console.Write((char)ascii);
            }

            public async Task<long> GetNextDirectionInput()
            {
                // Wait for map processing to complete
                while (botDirections == null)
                {
                    await Task.Delay(10);
                }

                long next = (long)botDirections.First();
                botDirections.RemoveAt(0);
                if (botDirections.Count == 0)
                {
                    sentLastDirectionInstruction = true;
                }

                return next;
            }

            /// <summary>
            /// Find alignment params and return the sum
            /// </summary>
            /// <returns></returns>
            public int SumAlignmentParams()
            {
                int sum = 0;
                for (int x = 0; x < maxX; x++)
                {
                    for (int y = 0; y < maxY; y++)
                    {
                        var curr = new Tuple<int, int>(x, y);
                        var topLeft = new Tuple<int, int>(x - 1, y - 1);
                        var top = new Tuple<int, int>(x, y - 1);
                        var topTop = new Tuple<int, int>(x, y - 2);
                        var topRight = new Tuple<int, int>(x + 1, y - 1);

                        // Looking for intersections
                        // .#.
                        // ###
                        // .#.
                        if (isScaffolding(curr) &&
                            isScaffolding(topLeft) &&
                            isScaffolding(top) &&
                            isScaffolding(topRight) &&
                            isScaffolding(topTop))
                        {
                            sum += top.Item1 * top.Item2;
                        }
                    }
                }

                return sum;
            }

            public void findThreeSegments()
            {
                buildPath();
                int[] patternLocations;
                List<List<Vector>> segments = recurseSegments(path, out patternLocations);
                string asciiInput = patternLocations.Aggregate(string.Empty, (pattern, n) =>
                    pattern += n == 0 ? string.Empty : ((char)((int)'A' + (n - 1))).ToString() + ","
                );
                asciiInput = asciiInput.Trim(',');
                asciiInput += '\n';

                for (int patternIndex = 0; patternIndex < segments.Count; patternIndex++)
                {
                    List<Vector> pattern = segments[patternIndex];
                    foreach (Vector vector in pattern)
                    {
                        asciiInput += vector.ToString();
                        asciiInput += ",";
                    }
                    asciiInput = asciiInput.Trim(',');
                    asciiInput += '\n';
                }

                botDirections = asciiInput.ToList();
                // No to video feed
                botDirections.Add('n');
                botDirections.Add('\n');
            }

            /// <summary>
            /// Construct each subset pattern (A,B,C) recursively and test to see if it consumes all subsets
            /// At each step, create a copy of the path list and remove the current pattern being tested
            /// If the current copy ends up empty, we have found 3 good patterns
            /// </summary>
            /// <param name="itemsRemaining">Current working copy of the path</param>
            /// <param name="depth">A=0, B=1, C=2</param>
            /// <returns>List of patterns at depth; null if patterns don't consume all subsets</returns>
            private List<List<Vector>> recurseSegments(List<Vector> itemsRemaining, out int[] patternPositions, int depth = 0)
            {
                if (depth > 2)
                {
                    patternPositions = null;
                    return null;
                }

                string consoleIndent = string.Empty;
                for (int i = 0; i < depth; i++)
                {
                    consoleIndent += "\t";
                }

                // subpath = A, B, C
                var subPath = new List<Vector>();
                for (int i = 0; i < itemsRemaining.Count && i < 20; i++)
                {
                    if (itemsRemaining.ElementAt(i) == null)
                    {
                        subPath = new List<Vector>();
                        continue;
                    }

                    Vector[] leftovers = itemsRemaining.Clone().ToArray();
                    patternPositions = new int[itemsRemaining.Count];
                    subPath.Add(itemsRemaining.ElementAt(i));
                    if (DEBUG)
                    {
                        string subPathString = subPath.Aggregate(string.Empty, (result, vector) => result += vector.direction.ToString()[0] + vector.distance.ToString());
                        Console.WriteLine(consoleIndent + subPathString);
                    }

                    // Remove all subpath's from path
                    for (int pathPointer = 0; pathPointer < itemsRemaining.Count - (subPath.Count - 1); pathPointer++)
                    {
                        bool match = true;
                        for (int subPathPointer = 0; subPathPointer < subPath.Count; subPathPointer++)
                        {
                            if (leftovers[pathPointer + subPathPointer] == null ||
                                !leftovers[pathPointer + subPathPointer].Equals(subPath[subPathPointer]))
                            {
                                match = false;
                                break;
                            }
                        }
                        if (match)
                        {
                            // Adding 1, so we can use '0' value as unset
                            patternPositions[pathPointer] = depth + 1;
                            // Found a match; remove it from the copy
                            for (int subPathPointer = 0; subPathPointer < subPath.Count; subPathPointer++)
                            {
                                leftovers[pathPointer + subPathPointer] = null;
                            }
                        }
                    }

                    if (DEBUG)
                    {
                        string leftoversString = leftovers.Aggregate(string.Empty, (result, vector) => result += vector == null ? " " : vector.direction.ToString()[0] + vector.distance.ToString());
                        Console.WriteLine(leftoversString);
                    }

                    bool allEmpty = leftovers.Where(v => v != null).FirstOrDefault() == null;
                    if (allEmpty)
                    {
                        // Found a set of subpaths that matches entire path
                        return new List<Vector>[] { subPath }.ToList();
                    }

                    int[] lowerPatternPositions;
                    List<List<Vector>> nestedResult = recurseSegments(leftovers.ToList(), out lowerPatternPositions, depth + 1);
                    if (nestedResult != null)
                    {
                        nestedResult.Insert(0, subPath);
                        // Merge the pattern locations together
                        for(int posIndex = 0; posIndex < lowerPatternPositions.Length; posIndex++)
                        {
                            int patternDepth = lowerPatternPositions[posIndex];
                            if (patternDepth != 0)
                            {
                                patternPositions[posIndex] = patternDepth;
                            }
                        }
                        return nestedResult;
                    }
                }

                // Could not find a solution down this path; continue on
                patternPositions = null;
                return null;
            }

            private bool isScaffolding(Tuple<int, int> pos)
            {
                return map.ContainsKey(pos) && map[pos] == '#';
            }

            private void buildPath()
            {
                while (true)
                {
                    var nextBotPos = new Tuple<int, int>(currBotPos.Item1 + currentDirection.Item1, currBotPos.Item2 + currentDirection.Item2);
                    // we've hit a corner
                    if (!isScaffolding(nextBotPos))
                    {
                        // assumes there's only one path (no 'T' shapes)
                        if (currentDirection.Item1 == -1)
                        {
                            // we're traveling left
                            if (isScaffolding(new Tuple<int, int>(currBotPos.Item1, currBotPos.Item2 - 1)))
                            {
                                path.Add(new Vector(Vector.Direction.Right, 0));
                                currentDirection = new Tuple<int, int>(0, -1);
                            }
                            else if (isScaffolding(new Tuple<int, int>(currBotPos.Item1, currBotPos.Item2 + 1)))
                            {
                                path.Add(new Vector(Vector.Direction.Left, 0));
                                currentDirection = new Tuple<int, int>(0, 1);
                            }
                            else
                            {
                                // reached end
                                break;
                            }
                        }
                        else if (currentDirection.Item1 == 1)
                        {
                            // we're traveling right
                            if (isScaffolding(new Tuple<int, int>(currBotPos.Item1, currBotPos.Item2 + 1)))
                            {
                                path.Add(new Vector(Vector.Direction.Right, 0));
                                currentDirection = new Tuple<int, int>(0, 1);
                            }
                            else if (isScaffolding(new Tuple<int, int>(currBotPos.Item1, currBotPos.Item2 - 1)))
                            {
                                path.Add(new Vector(Vector.Direction.Left, 0));
                                currentDirection = new Tuple<int, int>(0, -1);
                            }
                            else
                            {
                                // reached end
                                break;
                            }
                        }
                        else if (currentDirection.Item2 == 1)
                        {
                            // we're traveling down
                            if (isScaffolding(new Tuple<int, int>(currBotPos.Item1 - 1, currBotPos.Item2)))
                            {
                                path.Add(new Vector(Vector.Direction.Right, 0));
                                currentDirection = new Tuple<int, int>(-1, 0);
                            }
                            else if (isScaffolding(new Tuple<int, int>(currBotPos.Item1 + 1, currBotPos.Item2)))
                            {
                                path.Add(new Vector(Vector.Direction.Left, 0));
                                currentDirection = new Tuple<int, int>(1, 0);
                            }
                            else
                            {
                                // reached end
                                break;
                            }
                        }
                        else
                        {
                            // we're traveling up
                            if (isScaffolding(new Tuple<int, int>(currBotPos.Item1 - 1, currBotPos.Item2)))
                            {
                                path.Add(new Vector(Vector.Direction.Left, 0));
                                currentDirection = new Tuple<int, int>(-1, 0);
                            }
                            else if (isScaffolding(new Tuple<int, int>(currBotPos.Item1 + 1, currBotPos.Item2)))
                            {
                                path.Add(new Vector(Vector.Direction.Right, 0));
                                currentDirection = new Tuple<int, int>(1, 0);
                            }
                            else
                            {
                                // reached end
                                break;
                            }
                        }
                    }

                    var currPathSegment = path.Last();
                    currPathSegment.distance++;
                    currBotPos = new Tuple<int, int>(currBotPos.Item1 + currentDirection.Item1, currBotPos.Item2 + currentDirection.Item2);
                }
            }
        }

        private static async Task ComputeIntCode(LargeMemSet instructionSet, VacuumRobot vBot)
        {
            long activeInstruction = 0;
            long relativeBase = 0;
            Queue<int> outputQueue = new Queue<int>();
            while (instructionSet[activeInstruction] != 99)
            {
                long instruction = instructionSet[activeInstruction];
                // opcode is right most two digits
                long opcode = instruction % 100;
                long firstMode = (instruction / 100) % 10;
                long secondMode = (instruction / 1000) % 10;
                long thirdMode = (instruction / 10000) % 10;

                long firstParamValue = 0;
                long firstParamIndex = 0;
                long secondParamValue = 0;
                long thirdParamIndex = 0;
                long[] opcodesNeedingFirstParam = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                long[] opcodesNeedingSecondParam = { 1, 2, 5, 6, 7, 8 };
                long[] opcodesNeedingThirdParam = { 1, 2, 7, 8 };

                if (opcodesNeedingFirstParam.Contains(opcode))
                {
                    switch (firstMode)
                    {
                        // Position
                        case 0:
                            firstParamIndex = instructionSet[activeInstruction + 1];
                            break;
                        // Immediate
                        case 1:
                            firstParamIndex = activeInstruction + 1;
                            break;
                        // Relative
                        case 2:
                            firstParamIndex = instructionSet[activeInstruction + 1] + relativeBase;
                            break;
                    }
                    firstParamValue = instructionSet[firstParamIndex];
                }
                if (opcodesNeedingSecondParam.Contains(opcode))
                {
                    switch (secondMode)
                    {
                        case 0:
                            secondParamValue = instructionSet[instructionSet[activeInstruction + 2]];
                            break;
                        case 1:
                            secondParamValue = instructionSet[activeInstruction + 2];
                            break;
                        case 2:
                            secondParamValue = instructionSet[instructionSet[activeInstruction + 2] + relativeBase];
                            break;
                    }
                }
                if (opcodesNeedingThirdParam.Contains(opcode))
                {
                    switch (thirdMode)
                    {
                        case 0:
                            thirdParamIndex = instructionSet[activeInstruction + 3];
                            break;
                        case 1:
                            throw new Exception("Cannot store at immediate position");
                        case 2:
                            thirdParamIndex = instructionSet[activeInstruction + 3] + relativeBase;
                            break;
                    }
                }

                switch (opcode)
                {
                    // add
                    case 1:
                        instructionSet[thirdParamIndex] =
                            firstParamValue + secondParamValue;
                            activeInstruction += 4;
                        break;
                    case 2:
                        instructionSet[thirdParamIndex] =
                            firstParamValue * secondParamValue;
                            activeInstruction += 4;
                        break;
                    case 3:
                        instructionSet[firstParamIndex] = await vBot.GetNextDirectionInput();
                        activeInstruction += 2;
                        break;
                    case 4:
                        vBot.SetCameraOutput((int)firstParamValue);
                        activeInstruction += 2;
                        break;
                    case 5:
                        if (firstParamValue != 0)
                        {
                            activeInstruction = secondParamValue;
                        }
                        else
                        {
                            activeInstruction += 3;
                        }
                        break;
                    case 6:
                        if (firstParamValue == 0)
                        {
                            activeInstruction = secondParamValue;
                        }
                        else
                        {
                            activeInstruction += 3;
                        }
                        break;
                    case 7:
                        if (firstParamValue < secondParamValue)
                        {
                            instructionSet[thirdParamIndex] = 1;
                        }
                        else
                        {
                            instructionSet[thirdParamIndex] = 0;
                        }
                        activeInstruction += 4;
                        break;
                    case 8:
                        if (firstParamValue == secondParamValue)
                        {
                            instructionSet[thirdParamIndex] = 1;
                        }
                        else
                        {
                            instructionSet[thirdParamIndex] = 0;
                        }
                        activeInstruction += 4;
                        break;
                    case 9:
                        relativeBase += firstParamValue;
                        activeInstruction += 2;
                        break;
                    default:
                        throw new Exception($"Unexpected opcode {instructionSet[activeInstruction]}");
                }

            }
        }
    }
}