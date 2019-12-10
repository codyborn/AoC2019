using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Adhoc
{
    class Day9
    {
        static void Main9(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day9\input.txt");
            long[] instructionSet = inputProgram.Trim('\n').Split(',').Select((instr) => Int64.Parse(instr)).ToArray();
            LargeMemSet memSet = new LargeMemSet(instructionSet);
            ComputeIntCode(memSet);
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

        private static void ComputeIntCode(LargeMemSet instructionSet)
        {
            long activeInstruction = 0;
            long relativeBase = 0;
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
                        instructionSet[firstParamIndex] = Int64.Parse(Console.ReadLine());
                        activeInstruction += 2;
                        break;
                    case 4:
                        Console.WriteLine(firstParamValue);
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