using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Adhoc
{
    class Day8
    {
        static int maxThrust = int.MinValue;
        static MessageBus messageBus = new MessageBus();
        static void Main8(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day8\input.txt");
            var data = inputProgram.Trim('\n').ToCharArray().Select((c) => Int32.Parse(c.ToString()));
            var layerSize = 25 * 6;
            int[] result = new int[layerSize];

            for (int i = 0; i < layerSize; i++)
            {
                result[i] = 2;
            }

            for (int i = 0; i < (data.Count() / layerSize); i++)
            {
                int startingIndex = i * layerSize;
                for (int j = startingIndex; j < startingIndex + layerSize; j++)
                {
                    int writeIndex = j % layerSize;
                    if (result[writeIndex] == 2)
                    {
                        result[writeIndex] = data.ElementAt(j);
                    }
                }
            }

            string output = string.Empty;
            for (int i = 0; i < layerSize; i++)
            {
                if (i % 25 == 0)
                {
                    Console.WriteLine(output);
                    output = string.Empty;
                }
                output += result[i] == 1 ? '■' : ' ';
            }
            Console.WriteLine(output);
        }
    }
}