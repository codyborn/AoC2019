using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Adhoc
{
    class Day7
    {
        static int maxThrust = int.MinValue;
        static MessageBus messageBus = new MessageBus();
        static void Main7(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day7\input.txt");
            int[] instructionSet = inputProgram.Trim('\n').Split(',').Select((instr) => Int32.Parse(instr)).ToArray();
            List<int> phases = new int[] { 5, 6, 7, 8, 9 }.ToList<int>();
            phases.Reverse();

            getMaxThrust(instructionSet, phases, new List<int>(), 0).Wait();
            Console.Write(maxThrust);
        }

        private static async Task getMaxThrust(int[] instructionSet, List<int> phases, List<int> chosenPhases, int depth)
        {
            List<int> phaseClone = phases.Select(item => item).ToList();
            foreach (int phase in phases)
            {
                chosenPhases.Add(phase);
                if (depth == 4)
                {
                    messageBus.Clear();
                    Task<int> a = ComputeIntCode((int[])instructionSet.Clone(), chosenPhases.ElementAt(0), 0, 0);
                    Task<int> b = ComputeIntCode((int[])instructionSet.Clone(), chosenPhases.ElementAt(1), 1);
                    Task<int> c = ComputeIntCode((int[])instructionSet.Clone(), chosenPhases.ElementAt(2), 2);
                    Task<int> d = ComputeIntCode((int[])instructionSet.Clone(), chosenPhases.ElementAt(3), 3);
                    Task<int> e = ComputeIntCode((int[])instructionSet.Clone(), chosenPhases.ElementAt(4), 4);
                    Task.WaitAll(new Task[] { a, b, c, d, e });
                    maxThrust = Math.Max(maxThrust, e.Result);
                }
                if (depth != 4)
                {
                    phaseClone.Remove(phase);
                    await getMaxThrust(instructionSet, phaseClone, chosenPhases, depth + 1);
                    phaseClone.Add(phase);
                }
                chosenPhases.Remove(phase);
            }
        }

        private static async Task<int> ComputeIntCode(int[] instructionSet, int phase, int currentAmp, int? input = null)
        {
            int activeInstruction = 0;
            bool phaseConsumed = false;
            bool inputConsumed = input == null;
            int lastOutputValue = 0;

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
                        int value = phase;
                        if (phaseConsumed)
                        {
                            if (!inputConsumed)
                            {
                                value = (int)input;
                                inputConsumed = true;
                            }
                            else
                            {
                                value = await messageBus.GetMessage(currentAmp);
                            }
                        }
                        phaseConsumed = true;
                        instructionSet[instructionSet[activeInstruction + 1]] = value;//Int32.Parse(Console.ReadLine());
                        activeInstruction += 2;
                        break;
                    case 4:
                        //return firstParamValue;
                        messageBus.PostMessage((currentAmp + 1) % 5, firstParamValue);
                        lastOutputValue = firstParamValue;
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
            return lastOutputValue;
        }
    }

    class MessageBus
    {
        Dictionary<int, List<int>> messageBus = new Dictionary<int, List<int>>();
        public void PostMessage(int forAmp, int message)
        {
            if(!messageBus.ContainsKey(forAmp))
            {
                messageBus[forAmp] = new List<int>();
            }
            messageBus[forAmp].Add(message);
        }

        public async Task<int> GetMessage(int forAmp)
        {
            while (!messageBus.ContainsKey(forAmp) ||
                messageBus[forAmp].Count == 0)
            {
                await Task.Delay(1);
            }

            int result = messageBus[forAmp].First();
            messageBus[forAmp].RemoveAt(0);
            return result;
        }

        public void Clear()
        {
            messageBus = new Dictionary<int, List<int>>();
        }
    }
}