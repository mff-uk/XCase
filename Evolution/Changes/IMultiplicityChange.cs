using System.Linq;
using System.Collections.Generic;
using System;
using NUml.Uml2;

namespace XCase.Evolution
{
    public interface IMultiplicityChange : ICanBeIgnoredOnTarget
    {
        uint? OldLower { get; }
        uint? NewLower { get; }
        UnlimitedNatural OldUpper { get; }
        UnlimitedNatural NewUpper { get; }
    }

    public static class IMultiplicityChangeExt
    {
        public static EDocumentInvalidation CanInvalidateDocument(this IMultiplicityChange multiplicityChange)
        {
            // NL ol ou NU
            if (multiplicityChange.NewLower <= multiplicityChange.OldLower
                && multiplicityChange.OldUpper <= multiplicityChange.NewUpper)
                return EDocumentInvalidation.None;

            // ol NL ou NU (some items may need to be genereated)
            if (multiplicityChange.OldLower <= multiplicityChange.NewLower
                && multiplicityChange.OldUpper <= multiplicityChange.NewUpper)
                return EDocumentInvalidation.Possible;

            // ol NL NU ou (items may need to be add and/or deleted)
            if (multiplicityChange.OldLower <= multiplicityChange.NewLower
                && multiplicityChange.NewUpper <= multiplicityChange.OldUpper)
                return EDocumentInvalidation.Possible;

            // NL ol NU ou (items may need to be deleted)
            if (multiplicityChange.NewLower <= multiplicityChange.OldLower
                && multiplicityChange.NewUpper <= multiplicityChange.OldUpper)
                return EDocumentInvalidation.Possible;

            // ol ou NL NU (items must be generated)
            // NL NU ol ou (items must be removed)
            return EDocumentInvalidation.Certain;
        }

        public static bool CanRequireGenerating(this IMultiplicityChange multiplicityChange)
        {
            // ol NL ou NU (some items may need to be genereated)
            return (multiplicityChange.OldLower < multiplicityChange.NewLower);
        }

        public static bool CanRequireDeleting(this IMultiplicityChange multiplicityChange)
        {
            if (multiplicityChange.NewUpper.IsInfinity && multiplicityChange.OldUpper.IsInfinity)
                return false;
            return (multiplicityChange.NewUpper < multiplicityChange.OldUpper);
        }

        public static bool WasMandatoryIsOptional(this IMultiplicityChange multiplicityChange)
        {
            return multiplicityChange.OldLower == 1 && multiplicityChange.NewLower == 0; 
        }

        public static bool WasOptionalIsMandatory(this IMultiplicityChange multiplicityChange)
        {
            return multiplicityChange.OldLower == 0 && multiplicityChange.NewLower == 1;
        }
    }

    public enum EDocumentInvalidation
    {
        None, 
        Certain, 
        Possible
    }

    public static class MultiplicityElementExt
    {
        public static bool MultiplicityOneOrNone(this Model.MultiplicityElement element)
        {
            return (element.Lower == 1 || element.Lower == 0) && element.Upper == 1;
        }
    }
}