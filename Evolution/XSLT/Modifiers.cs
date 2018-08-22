using System;

namespace XCase.Evolution
{
    /// <summary>
    /// Flags that can change behavior of the template generating algorithm 
    /// </summary>
    [Flags]
    internal enum Modifiers
    {
        /// <summary>
        /// 
        /// </summary>
        Default = 0,
        /// <summary>
        /// Templates will always be created as callable 
        /// and will always create new elements instead of 
        /// matching existing
        /// </summary>
        ForceCallable = 2 << 1,
        /// <summary>
        /// Template is generated as group aware. This is needed when a 
        /// represented class is not under group (so it's template is not
        /// group aware) but the structural representative of this class
        /// is under group and thus the template for the represented
        /// class must be regenerated 
        /// </summary>
        ForceGroupAwareSRContent = 2 << 6
    }

    internal static class GenerFactorsExt
    {
        internal static bool Is(this Modifiers factors, Modifiers factor)
        {
            return (factors & factor) == factor;
        }

        internal static Modifiers Without(this Modifiers factors, Modifiers factor)
        {
            return factors & ~factor; 
        }
    }
}