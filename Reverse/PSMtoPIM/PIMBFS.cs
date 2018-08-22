using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;
using System.Collections;
using XCase.Controller.Commands;

namespace XCase.Reverse
{
    internal static class PIMBFS
    {
        private static Dictionary<PIMClass, int> D;

        private static Dictionary<PIMClass, List<NestingJoinStep>> Paths;
        
        /// <summary>
        /// Returns the distance and PIMPaths from <paramref name="startClass"/> to all PIMClasses in the PIM
        /// </summary>
        /// <param name="startClass">Starting PIMClass</param>
        /// <returns>Dictrionary indexed by PIMClass, contains Tuples - 1: distance 2: PIMPath</returns>
        internal static Dictionary<PIMClass, Tuple<int, List<NestingJoinStep>>> BFS(PIMClass startClass)
        {
            D = new Dictionary<PIMClass, int>();
            Paths = new Dictionary<PIMClass, List<NestingJoinStep>>();
            Queue<PIMClass> Q = new Queue<PIMClass>();

            //First step is customized, because we want the startclass to be unreachable (distance = +inf) from itself
            //(normally, distance is 0 for the start node)
            foreach (KeyValuePair<PIMClass, Association> neighborPair in GetNeighbors(startClass))
            {
                PIMClass neighbor = neighborPair.Key;
                D.Add(neighbor, 1);
                Paths.Add(neighbor, new List<NestingJoinStep>());
                Paths[neighbor].Add(new NestingJoinStep() { Association = neighborPair.Value, Start = startClass, End = neighbor });
                Q.Enqueue(neighbor);
            }            
            
            while (Q.Count > 0)
            {
                PIMClass current = Q.Dequeue();
                foreach (KeyValuePair<PIMClass, Association> neighborPair in GetNeighbors(current))
                {
                    PIMClass neighbor = neighborPair.Key;
                    if (D.ContainsKey(neighbor))
                    {
                        if (D[neighbor] > D[current] + 1)
                        {
                            D[neighbor] = D[current] + 1;
                            Paths[neighbor] = new List<NestingJoinStep>(Paths[current]);
                            Paths[neighbor].Add(new NestingJoinStep() { Association = neighborPair.Value, Start = current, End = neighbor });
                        }
                    }
                    else
                    {
                        D.Add(neighbor, D[current] + 1);
                        Paths.Add(neighbor, new List<NestingJoinStep>(Paths[current]));
                        Paths[neighbor].Add(new NestingJoinStep() { Association = neighborPair.Value, Start = current, End = neighbor });
                        Q.Enqueue(neighbor);
                    }
                }
        
            }
            
            Dictionary<PIMClass, Tuple<int, List<NestingJoinStep>>> output = new Dictionary<PIMClass, Tuple<int, List<NestingJoinStep>>>();

            foreach (PIMClass pimClass in D.Keys)
            {
                output.Add(pimClass, new Tuple<int, List<NestingJoinStep>>(D[pimClass], Paths[pimClass]));
            }
            D = null;
            Paths = null;
            return output;
        }

        private static Dictionary<PIMClass, Association> GetNeighbors(PIMClass pimClass)
        {
            Dictionary<PIMClass, Association> n = new Dictionary<PIMClass, Association>();

            foreach (Association A in pimClass.Assocations)
            {
                foreach (AssociationEnd E in A.Ends)
                {
                    if (E.Class != pimClass && !n.ContainsKey(E.Class as PIMClass))
                    {
                        n.Add(E.Class as PIMClass, A);
                    }
                }
            }

            return n;
        }
    }
}
