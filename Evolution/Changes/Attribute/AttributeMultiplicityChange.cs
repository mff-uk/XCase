using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    public class AttributeMultiplicityChange : 
        AttributeChange, IMultiplicityChange, IDoubleTargetChange
    {
        public AttributeMultiplicityChange(PSMAttribute attribute)
            : base(attribute)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary; }
        }

        public PSMElement Parent
        {
            get { return AttributeIsInClass ? (PSMElement) PSMClass : AttributeContainer; }
        }

        public PSMElement SecondaryTarget
        {
            get
            {
                if (AttributeWasInClass)
                    return PSMClass;
                else
                    return AttributeContainer;
            }
        }

        public uint? OldLower
        {
            get
            {
                return AttributeOldVersion.Lower;
            }
        }

        public uint? NewLower
        {
            get
            {
                return Attribute.Lower;
            }
        }

        public NUml.Uml2.UnlimitedNatural OldUpper
        {
            get
            {
                return AttributeOldVersion.Upper;
            }
        }

        public NUml.Uml2.UnlimitedNatural NewUpper
        {
            get
            {
                return Attribute.Upper;
            }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Attribute's {0:SN} multiplicity changed from {1} to {2}", Attribute.XPath, AttributeOldVersion.MultiplicityString, Attribute.MultiplicityString);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(OldLower != NewLower || OldUpper != NewUpper);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAttribute psmAttribute)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            if ((((PSMAttribute)psmAttribute.GetInVersion(v1)).Lower != ((PSMAttribute)psmAttribute.GetInVersion(v2)).Lower) ||
                (((PSMAttribute)psmAttribute.GetInVersion(v1)).Upper != ((PSMAttribute)psmAttribute.GetInVersion(v2)).Upper))
            {
                AttributeMultiplicityChange c = new AttributeMultiplicityChange(psmAttribute) { OldVersion = v1, NewVersion = v2 };
                result.Add(c);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return AttributeIsInClass && this.CanInvalidateDocument() != EDocumentInvalidation.None; }
        }

        public override bool InvalidatesContent
        {
            get { return AttributeIsInAttributeContainer && this.CanInvalidateDocument() != EDocumentInvalidation.None; }
        }
    }
}