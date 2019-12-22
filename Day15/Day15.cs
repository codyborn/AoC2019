using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Adhoc
{
    class Day15
    {
        static void Main(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day15\input.txt");
            long[] instructionSet = inputProgram.Trim('\n').Split(',').Select((instr) => Int64.Parse(instr)).ToArray();
            LargeMemSet memSet = new LargeMemSet(instructionSet);
            var droid = new Droid();
            ComputeIntCode(memSet, droid);
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

        public class Droid
        {
            private Dictionary<Tuple<int, int>, char> droidPosToLocationType = new Dictionary<Tuple<int, int>, char>();
            private Dictionary<Tuple<int, int>, List<int>> posToFutureDirections = new Dictionary<Tuple<int, int>, List<int>>();
            private Tuple<int, int> currentLocation = new Tuple<int, int>(0, 0);
            private Tuple<int, int> nextLocation = new Tuple<int, int>(0, 0);
            private Stack<int> traveled = new Stack<int>();
            private int currentDirection = 0;
            private bool backTracking = false;
            private int maxDistance = int.MinValue;

            public void SetLocationType(int id)
            {
                // 0 wall hit
                // 1 all clear
                // 2 found oxygen system
                switch (id)
                {
                    case 0:
                        droidPosToLocationType[nextLocation] = '#';
                        RemoveOptionsAroundWall(nextLocation);
                        break;
                    case 1:
                        droidPosToLocationType[nextLocation] = '.';
                        currentLocation = nextLocation;
                        if (!backTracking)
                        {
                            traveled.Push(currentDirection);
                            maxDistance = Math.Max(maxDistance, traveled.Count);
                        }
                        break;
                    case 2:
                        droidPosToLocationType[nextLocation] = 'X';
                        if (!backTracking)
                        {
                            traveled.Clear();
                            posToFutureDirections.Clear();
                            maxDistance = int.MinValue;
                            currentDirection = 0;
                        }
                        break;
                }

                PrintToScreen().Wait();
            }

            /// <summary>
            /// DFS through maze
            /// Update nextLocation and currentDirection
            /// </summary>
            /// <returns></returns>
            public int GetDirection()
            {
                // 1 south
                // 2 north
                // 3 west
                // 4 east
                if (!posToFutureDirections.ContainsKey(currentLocation))
                {
                    // Don't unintentionally backtrack
                    RemoveDirectionOption(currentLocation, DirectionInverse(currentDirection));
                }

                int direction;

                // set explore limit to 300
                // backtrack until we find an unexplored path
                if (posToFutureDirections[currentLocation].Count == 0)
                {
                    backTracking = true;
                    if (traveled.Count == 0)
                    {
                        // explored all options
                        Console.WriteLine(maxDistance);
                        Environment.Exit(0);
                    }
                    int previousDirection = traveled.Pop();
                    direction = DirectionInverse(previousDirection);
                }
                else
                {
                    backTracking = false;
                    int nextDirection = 0;
                    direction = posToFutureDirections[currentLocation].ElementAt(nextDirection);
                    posToFutureDirections[currentLocation].RemoveAt(nextDirection);
                }
                currentDirection = direction;
                nextLocation = GetNextLocation(currentLocation, direction);
                return direction;
            }

            private async Task PrintToScreen()
            {
                await Task.Delay(10);
                List<List<char>> consoleScreen = new List<List<char>>();
                int screenSize = 15;
                for (int y = currentLocation.Item2 + screenSize; y >= currentLocation.Item2 - screenSize; y--)
                {
                    for (int x = currentLocation.Item1 - screenSize; x <= currentLocation.Item1 + screenSize; x++)
                    {
                        int consoleX = x - (currentLocation.Item1 - screenSize);
                        int consoleY = Math.Abs(y - (currentLocation.Item2 + screenSize));
                        Console.SetCursorPosition(consoleX, consoleY);
                        var location = new Tuple<int, int>(x, y);

                        if (location.Item1 == currentLocation.Item1 &&
                            location.Item2 == currentLocation.Item2)
                        {
                            Console.Write('O');
                        }
                        else if (droidPosToLocationType.ContainsKey(location))
                        {
                            Console.Write(droidPosToLocationType[location]);
                        }
                        else
                        {
                            Console.Write(' ');
                        }
                    }
                }
            }


            private static Tuple<int, int> GetNextLocation(Tuple<int, int> currentLocation, int direction)
            {
                switch (direction)
                {
                    case 1:
                        return new Tuple<int, int>(currentLocation.Item1, currentLocation.Item2 - 1);
                    case 2:
                        return new Tuple<int, int>(currentLocation.Item1, currentLocation.Item2 + 1);
                    case 3:
                        return new Tuple<int, int>(currentLocation.Item1 - 1, currentLocation.Item2);
                    case 4:
                        return new Tuple<int, int>(currentLocation.Item1 + 1, currentLocation.Item2);
                    default:
                        throw new ArgumentException($"Unexpected direction {direction}");
                }
            }

            private static int DirectionInverse(int direction)
            {
                switch (direction)
                {
                    case 0:
                        return 0;
                    case 1:
                    case 3:
                        return direction + 1;
                    case 2:
                    case 4:
                        return direction - 1;
                    default:
                        throw new ArgumentException($"Unexpected direction {direction}");
                }
            }

            /// <summary>
            /// Small optimization to prevent hitting the same wall twice
            /// </summary>
            private void RemoveOptionsAroundWall(Tuple<int, int> wallLocation)
            {
                RemoveDirectionOption(new Tuple<int, int>(wallLocation.Item1 - 1, wallLocation.Item2), 4);
                RemoveDirectionOption(new Tuple<int, int>(wallLocation.Item1 + 1, wallLocation.Item2), 3);
                RemoveDirectionOption(new Tuple<int, int>(wallLocation.Item1, wallLocation.Item2 - 1), 2);
                RemoveDirectionOption(new Tuple<int, int>(wallLocation.Item1, wallLocation.Item2 + 1), 1);
            }

            private void RemoveDirectionOption(Tuple<int, int> location, int direction)
            {
                if (!posToFutureDirections.ContainsKey(location))
                {
                    posToFutureDirections[location] = new[] { 1, 2, 3, 4 }.ToList();
                }
                posToFutureDirections[location].Remove(direction);
            }
        }

        private static void ComputeIntCode(LargeMemSet instructionSet, Droid droid)
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
                        instructionSet[firstParamIndex] = droid.GetDirection();
                        activeInstruction += 2;
                        break;
                    case 4:
                        droid.SetLocationType((int)firstParamValue);
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