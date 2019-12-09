using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Adhoc
{
    class Day5
    {
        static void Main5(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day5\input.txt");
            int[] instructionSet = inputProgram.Trim('\n').Split(',').Select((instr) => Int32.Parse(instr)).ToArray();
            ComputeIntCode(instructionSet);

        }

        private static void ComputeIntCode(int[] instructionSet)
        {
            int activeInstruction = 0;
            while (instructionSet[activeInstruction] != 99)
            {
                int instruction = instructionSet[activeInstruction];
                // opcode is right most two digits
                int opcode = instruction % 100;
                int firstMode = (instruction / 100) % 10;
                int secondMode = (instruction / 1000) % 10;
                int thirdMode = (instruction / 10000) % 10;
                
                int firstParamValue = 0;
                int secondParamValue = 0;
                //int thirdParamValue;
                int[] opcodesNeedingFirstParam = { 1, 2, 4, 5, 6, 7, 8 };
                int[] opcodesNeedingSecondParam = { 1, 2, 5, 6, 7, 8 };

                if (opcodesNeedingFirstParam.Contains(opcode))
                {
                    firstParamValue = firstMode == 0 ? instructionSet[instructionSet[activeInstruction + 1]] : instructionSet[activeInstruction + 1];
                }
                if (opcodesNeedingSecondParam.Contains(opcode))
                {
                    secondParamValue = secondMode == 0 ? instructionSet[instructionSet[activeInstruction + 2]] : instructionSet[activeInstruction + 2];
                }
                //thirdParamValue = thirdMode == 0 ? instructionSet[instructionSet[activeInstruction + 3]] : instructionSet[activeInstruction + 3];

                switch (opcode)
                {
                    // add
                    case 1:
                        instructionSet[instructionSet[activeInstruction + 3]] =
                            firstParamValue + secondParamValue;
                            activeInstruction += 4;
                        break;
                    case 2:
                        instructionSet[instructionSet[activeInstruction + 3]] =
                            firstParamValue * secondParamValue;
                            activeInstruction += 4;
                        break;
                    case 3:
                        instructionSet[instructionSet[activeInstruction + 1]] = Int32.Parse(Console.ReadLine());
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
                            instructionSet[instructionSet[activeInstruction + 3]] = 1;
                        }
                        else
                        {
                            instructionSet[instructionSet[activeInstruction + 3]] = 0;
                        }
                        activeInstruction += 4;
                        break;
                    case 8:
                        if (firstParamValue == secondParamValue)
                        {
                            instructionSet[instructionSet[activeInstruction + 3]] = 1;
                        }
                        else
                        {
                            instructionSet[instructionSet[activeInstruction + 3]] = 0;
                        }
                        activeInstruction += 4;
                        break;
                    default:
                        throw new Exception($"Unexpected opcode {instructionSet[activeInstruction]}");
                }

            }
        }
    }
}