using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Adhoc
{
    class Day2
    {
        static void Main2(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day2\2.input.txt");
            int[] instructionSet = inputProgram.Trim('\n').Split(',').Select((instr) => Int32.Parse(instr)).ToArray();
            int desiredResult = 19690720;
            
            for (int noun = 0; noun <= 99; noun++)
            {
                for (int verb = 0; verb <= 99; verb++)
                {
                    int[] tmpInstrSet = (int[]) instructionSet.Clone();
                    tmpInstrSet[1] = noun;
                    tmpInstrSet[2] = verb;
                    if (ComputeIntCode(tmpInstrSet) == desiredResult)
                    {
                        Console.WriteLine(100 * noun + verb);
                    }
                }
            }

        }

        private static int ComputeIntCode(int[] instructionSet)
        {
            int activeInstruction = 0;
            while (instructionSet[activeInstruction] != 99)
            {
                switch (instructionSet[activeInstruction])
                {
                    // add
                    case 1:
                        instructionSet[instructionSet[activeInstruction + 3]] =
                            instructionSet[instructionSet[activeInstruction + 1]]
                            + instructionSet[instructionSet[activeInstruction + 2]];
                        break;
                    case 2:
                        instructionSet[instructionSet[activeInstruction + 3]] =
                            instructionSet[instructionSet[activeInstruction + 1]] *
                            instructionSet[instructionSet[activeInstruction + 2]];
                        break;
                    default:
                        throw new Exception($"Unexpected opcode {instructionSet[activeInstruction]}");
                }

                activeInstruction += 4;
            }
            return instructionSet[0];
        }
    }
}