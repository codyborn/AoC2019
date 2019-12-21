using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Adhoc
{
    class Day14
    {
        static void Main(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day14\input.txt");
            string[] elementMappingRaw = inputProgram.Trim('\n').Split('\n');

            // Build a dictionary of reverse mappings
            var elementMapping = new Dictionary<string, Mapping>();
            foreach (string rawMapping in elementMappingRaw)
            {
                string[] mappingSplit = rawMapping.Split(new string[] { " => " }, StringSplitOptions.RemoveEmptyEntries);
                var keyElement = new ElementAmount(mappingSplit[1].Trim('\r'));
                
                foreach(string element in mappingSplit[0].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!elementMapping.ContainsKey(keyElement.name))
                    {
                        elementMapping[keyElement.name] = new Mapping();
                        elementMapping[keyElement.name].cost = new List<ElementAmount>();
                    }
                    elementMapping[keyElement.name].amountProduced = keyElement.quantity;
                    elementMapping[keyElement.name].cost.Add(new ElementAmount(element));
                }
            }

            ulong totalOre = 1000000000000;
            ulong fuelAquired = 0;
            var scraps = new InitializedDictionary<string, ulong>();
            var scrapsProducedPerCycle = new InitializedDictionary<string, ulong>();

            // DFS starting from FUEL
            var startingElement = new ElementAmount { name = "FUEL", quantity = 1 };
            ulong oreProducedPerCycleBeforeDiscounts = FindElementCost(elementMapping, startingElement.name, 1, scrapsProducedPerCycle);
            ulong oreProducedPerCycle = oreProducedPerCycleBeforeDiscounts;
            oreProducedPerCycle -= SellBackAllElements(elementMapping, scrapsProducedPerCycle.Clone());

            while (true)
            {
                // close the gap by running many cycles at once
                // this will be a lowerbound 
                ulong cycles = totalOre / oreProducedPerCycle;
                if (cycles == 0)
                {
                    // we don't have enough ore for even one cycle
                    break;
                }
                // ore produced
                ulong oreProduced = (cycles * oreProducedPerCycleBeforeDiscounts);
                // scraps produced
                foreach (var scrap in scrapsProducedPerCycle.Keys.ToList())
                {
                    scraps[scrap] += scrapsProducedPerCycle[scrap] * cycles;
                }


                oreProduced -= SellBackAllElements(elementMapping, scraps);
                fuelAquired += cycles;
                totalOre -= oreProduced;
            }

            Console.WriteLine(fuelAquired);
        }

        // 🔥Hot🔥 new dictionary type that returns default value if key not present
        public class InitializedDictionary<TKey, TValue>
        {
            private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

            public TValue this[TKey key]
            {
                get
                {
                    if (!_dictionary.ContainsKey(key))
                    {
                        return default(TValue);
                    }
                    return _dictionary[key];
                }
                set
                {
                    _dictionary[key] = value;
                }
            }

            public Dictionary<TKey, TValue>.KeyCollection Keys
            {
                get
                {
                    return _dictionary.Keys;
                }
            }

            // Returns a copy of the object
            public InitializedDictionary<TKey, TValue> Clone()
            {
                var clone = new InitializedDictionary<TKey, TValue>();
                foreach(TKey key in this.Keys)
                {
                    clone[key] = this._dictionary[key];
                }
                return clone;
            }
        }

        private static ulong FindElementCost(
            Dictionary<string, Mapping> elementMapping, 
            string currentName,
            ulong amountNeeded,
            InitializedDictionary<string, ulong> scraps)
        {
            Mapping current = elementMapping[currentName];
            ulong oreNeeded = 0;

            ulong cyclesNeeded = (ulong)Math.Ceiling((float)amountNeeded / (float)current.amountProduced);
            // Add leftovers to scraps for future elements
            scraps[currentName] += (current.amountProduced * cyclesNeeded) - amountNeeded;
            foreach (ElementAmount element in current.cost)
            {
                if (element.name.Equals("ORE"))
                {
                    // ex. 10 ORE * 3
                    return element.quantity * cyclesNeeded;
                }
                else
                {
                    oreNeeded += FindElementCost(elementMapping, element.name, element.quantity * cyclesNeeded, scraps);
                }
            }

            return oreNeeded;
        }


        // sell back scraps to reduce ore costs
        // continue attempting to sell until there's no change to balance
        private static ulong SellBackAllElements(
            Dictionary<string, Mapping> elementMapping,
            InitializedDictionary<string, ulong> scraps)
        {
            bool someChange = true;
            ulong oreReceived = 0;
            while (someChange)
            {
                someChange = false;
                foreach (var scrap in scraps.Keys.ToList())
                {
                    if (scraps[scrap] > 0)
                    {
                        ulong sellback = SellBack(elementMapping, scrap, scraps);
                        if (sellback > 0)
                        {
                            someChange = true;
                        }
                        oreReceived += sellback;
                    }
                }
            }

            return oreReceived;
        }

        private static ulong SellBack(
            Dictionary<string, Mapping> elementMapping,
            string currentName,
            InitializedDictionary<string, ulong> scraps)
        {
            Mapping current = elementMapping[currentName];

            ulong scrapOfElement = scraps[currentName];
            ulong cyclesNeeded = scrapOfElement / current.amountProduced;
            ulong scrapConsumed = (cyclesNeeded * current.amountProduced);
            scraps[currentName] -= scrapConsumed;
            if (cyclesNeeded == 0)
            {
                return 0;
            }
            ulong maxOres = 0;
            foreach (ElementAmount element in current.cost)
            {
                if (element.name.Equals("ORE"))
                {
                    // Discounts!
                    return element.quantity * cyclesNeeded;
                }
                else
                {
                    scraps[element.name] += (element.quantity * cyclesNeeded);
                    ulong usingScraps = SellBack(elementMapping, element.name, scraps);
                    maxOres += usingScraps;
                }
            }
            return maxOres;
        }

        public class Mapping
        {
            public ulong amountProduced;
            public List<ElementAmount> cost = new List<ElementAmount>();
        }

        public class ElementAmount
        {
            public ElementAmount()
            { }

            public ElementAmount(string rawInput)
            {
                string[] input = rawInput.Split(' ');
                name = input[1];
                quantity = ulong.Parse(input[0]);
            }

            public string name
            { 
                get;
                set;
            }

            public ulong quantity
            {
                get;
                set;
            }
        }
    }
}