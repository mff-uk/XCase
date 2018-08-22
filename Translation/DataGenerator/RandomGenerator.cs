using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace XCase.Translation.DataGenerator
{
    public static class RandomGenerator
    {
        private static Random r;


        static RandomGenerator()
        {
            r = new Random(DateTime.Now.Millisecond);
        }

        public static int Next()
        {
            return r.Next();
        }

        public static int Next(int max)
        {
            return r.Next(max);
        }

        public static int Next(int min, int max)
        {
            return r.Next(min, max);
        }

        public static int Next(int min, int max, int elementOccurrences)
        {
            return r.Next(min, Math.Max(max - 2 * elementOccurrences, min));
        }

        public static bool Toss()
        {
            return r.Next(2) == 1; 
        }

        public static bool Toss(int forTrue, int forFalse)
        {
            return r.Next(forTrue + forFalse) < forTrue;
        }

        public static DateTime RandomDateTime()
        {
            return new DateTime(Next(1990, 2009), Next(1, 13), Next(1, 28), Next(0, 24), Next(0, 24), Next(0, 60));
        }
    }
}