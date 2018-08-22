using System;
using System.Collections.Generic;
using System.Linq;

namespace XCase.Translation.DataGenerator
{
    public static class IEnumerableRandomExtensions
    {
        public static Type ChooseOneRandomly<Type>(this IEnumerable<Type> items)
        {
            int count = items.Count();
            if (count > 0)
            {
                int i = RandomGenerator.Next(count);

                int index = 0;
                foreach (Type item in items)
                {
                    if (index == i)
                    {
                        return item;
                    }
                    index++;
                }
                 
            }
            
            throw new ArgumentException("Collection is empty", "items");
        }
    }
}