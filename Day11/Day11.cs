using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Adhoc
{
    class Day11
    {
        static void Main(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day11\input.txt");
            long[] instructionSet = inputProgram.Trim('\n').Split(',').Select((instr) => Int64.Parse(instr)).ToArray();
            LargeMemSet memSet = new LargeMemSet(instructionSet);
            PaintBot pb = new PaintBot();
            ComputeIntCode(memSet, pb);

            Console.WriteLine(pb.TotalPanelsPainted);
            pb.printPainting();
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

        public class PaintBot
        {
            Dictionary<Tuple<int, int>, int> craftPosToColor = new Dictionary<Tuple<int, int>, int>();
            Tuple<int, int> currPos = new Tuple<int, int>(0, 0);
            public int TotalPanelsPainted
            {
                get;
                set;
            }
            // we'll just add this value when moving forward one space
            // 0, -1 indicates add 0 to x axis and -1 to y axis (aka moving upward)
            Tuple<int, int> currDirection = new Tuple<int, int>(0, -1);

            public PaintBot()
            {
                TotalPanelsPainted = 0;
                craftPosToColor[currPos] = 1;
            }

            public void printPainting()
            {
                // get ranges
                int xMax = int.MinValue;
                int yMax = int.MinValue;
                int xMin = int.MaxValue;
                int yMin = int.MaxValue;
                foreach(Tuple<int, int> pos in craftPosToColor.Keys)
                {
                    xMax = Math.Max(xMax, pos.Item1);
                    yMax = Math.Max(yMax, pos.Item2);
                    yMin = Math.Min(yMin, pos.Item2);
                    xMin = Math.Min(xMin, pos.Item1);
                }
                for (int x = xMax; x >= xMin; x--)
                {
                    for (int y = yMin; y <= yMax; y++)
                    {
                        var currPos = new Tuple<int, int>(x, y);
                        if (craftPosToColor.ContainsKey(currPos))
                        {
                            Console.Write(craftPosToColor[currPos] == 0 ? ' ' : '■');
                        }
                        else
                        {
                            Console.Write(' ');
                        }
                    }
                    Console.Write('\n');
                }
            }

            public int getCameraReading()
            {
                if (craftPosToColor.ContainsKey(currPos))
                {
                    return craftPosToColor[currPos];
                }

                // board starts black
                return 0;
            }

            public void paint(int color)
            {
                if (!craftPosToColor.ContainsKey(currPos))
                {
                    TotalPanelsPainted++;
                }

                craftPosToColor[currPos] = color;
            }

            public void changeDirectionAndMove(int direction)
            {
                // turn left
                if (direction == 0)
                {
                    // if going left or right
                    if (currDirection.Item2 == 0)
                    {
                        currDirection = new Tuple<int, int>(currDirection.Item2, currDirection.Item1 * -1);
                    }
                    else
                    {
                        currDirection = new Tuple<int, int>(currDirection.Item2, currDirection.Item1);
                    }
                }
                // turn right
                else
                {
                    // if going left or right
                    if (currDirection.Item2 == 0)
                    {
                        currDirection = new Tuple<int, int>(currDirection.Item2, currDirection.Item1);
                    }
                    else
                    {
                        currDirection = new Tuple<int, int>(currDirection.Item2 * -1, currDirection.Item1);
                    }
                }

                // Move
                currPos = new Tuple<int, int>(currPos.Item1 + currDirection.Item1, currPos.Item2 + currDirection.Item2);
            }
        }

        private static void ComputeIntCode(LargeMemSet instructionSet, PaintBot bot)
        {
            long activeInstruction = 0;
            long relativeBase = 0;
            bool givenPaintInstr = false;
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
                        instructionSet[firstParamIndex] = bot.getCameraReading();
                        activeInstruction += 2;
                        break;
                    case 4:
                        if (!givenPaintInstr)
                        {
                            bot.paint((int)firstParamValue);
                            givenPaintInstr = true;
                        }
                        else
                        {
                            bot.changeDirectionAndMove((int)firstParamValue);
                            givenPaintInstr = false;
                        }
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