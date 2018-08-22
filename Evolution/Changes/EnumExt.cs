using System;
using System.Linq;

namespace XCase.Evolution
{
    public static class EnumExt
    {
        public static bool OneOf<TYPE>(this TYPE @enum, params TYPE[] values)
        {
            return values.Any(c => c.Equals(@enum));
        }
    }
}