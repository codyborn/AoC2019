using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Adhoc
{
    class Day6
    {
        static void Main6(string[] args)
        {
            string inputProgram = File.ReadAllText(@".\Day6\input.txt");
            string[] orbits = inputProgram.Trim('\n').Split('\n');
            Dictionary<string, Node> nodeLookup = new Dictionary<string, Node>();
            List<string> orbiteeBodies = new List<string>();
            HashSet<string> orbiterBodies = new HashSet<string>();

            foreach (string orbit in orbits)
            {

                string leftBody = orbit.Split(')')[0].Trim('\r');
                orbiteeBodies.Add(leftBody);
                string rightBody = orbit.Split(')')[1].Trim('\r');
                orbiterBodies.Add(rightBody);
                if (!nodeLookup.ContainsKey(leftBody))
                {
                    nodeLookup[leftBody] = new Node { name = leftBody, children = new List<Node>(), parents = new List<Node>() };
                }
                if (!nodeLookup.ContainsKey(rightBody))
                {
                    nodeLookup[rightBody] = new Node { name = rightBody, children = new List<Node>(), parents = new List<Node>() };
                }
                nodeLookup[leftBody].children.Add(nodeLookup[rightBody]);
                nodeLookup[rightBody].parents.Add(nodeLookup[leftBody]);
            }

            IEnumerable<string> purelyOrbitees = orbiteeBodies.Where(body => !orbiterBodies.Contains(body));

            int orbitCount = 0;
            //foreach (string orbitee in purelyOrbitees)
            //{
            //    orbitCount += countOrbits(nodeLookup[orbitee], 1);
            //}
            distanceToSanta(nodeLookup["YOU"], -1);
            Console.WriteLine(orbitCount);
        }

        class Node
        {
            public string name;
            public List<Node> children;
            public List<Node> parents;
            public bool visited = false;
        }

        static void distanceToSanta(Node body, int depth)
        {
            if (body.visited)
            {
                return;
            }
            body.visited = true;

            if (body.name == "SAN")
            {
                Console.WriteLine(depth-1);
            }

            foreach (Node child in body.children)
            {
                distanceToSanta(child, depth + 1);
            }
            foreach (Node child in body.parents)
            {
                distanceToSanta(child, depth + 1);
            }
        }

        static int countOrbits(Node body, int depth)
        {
            int orbitCount = 0;
            if (body.visited)
            {
                throw new Exception("Didn't expect a loop");
            }
            body.visited = true;

            foreach (Node child in body.children)
            {
                // Return 1 count for each direct, plus the counts from all its indirect orbits
                orbitCount += depth;
                orbitCount += countOrbits(child, depth+1);
            }

            return orbitCount;
        }
    }
}