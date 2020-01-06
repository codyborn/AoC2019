using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adhoc
{
    class Day16
    {
        private const int inputRepeat = 10000;
        private const int phaseCount = 100;

        static void Main16(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day16\input.txt");

            int[] input = inputProgram.Trim('\n').Select(n => int.Parse(n.ToString())).ToArray();
            int[] pattern = new int[] { 0, 1, 0, -1 };
            int resultOffset = 0;
            for (int i = 0; i < 7; i++)
            {
                resultOffset *= 10;
                resultOffset += input[i];
            }

            for (int phase = 0; phase < phaseCount; phase++)
            {
                int[] nextInput = new int[input.Length * inputRepeat];
                Dictionary<string, int> sumLookup = new Dictionary<string, int>();

                for (int writeIndex = 0; writeIndex < input.Length * inputRepeat; writeIndex++)
                {
                    int instance = writeIndex;
                    int sum = 0;
                    for (int readIndex = 0; readIndex < input.Length * inputRepeat; readIndex += input.Length)
                    {
                        int partialSum = 0;
                        for (int i = readIndex; i < readIndex + input.Length; i++)
                        {
                            // Repeat index multiple times 
                            int patternIndex = ((i + 1) / (writeIndex + 1)) % pattern.Length;
                            partialSum += input.ElementAt(i % input.Length) * pattern.ElementAt(patternIndex);
                        }
                        sum += partialSum;
                    }

                    var stringResult = sum.ToString();
                    // Take last digit of the number
                    nextInput[writeIndex] = int.Parse(stringResult[stringResult.Length - 1].ToString());
                    if (writeIndex % 10000 == 0)
                    {
                        Console.WriteLine(writeIndex);
                    }
                }

                input = nextInput;
            }

            string signal = input.Aggregate(string.Empty, (agg, n) => agg += n);
            int result = int.Parse(signal.Substring(resultOffset, 8));
        }
    }
}