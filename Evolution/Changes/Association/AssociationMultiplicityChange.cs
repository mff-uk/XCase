using System.Collections.Generic;
using System.Diagnostics;
using NUml.Uml2;
using XCase.Model;

namespace XCase.Evolution
{
    public class AssociationMultiplicityChange : AssociationChange, IMultiplicityChange
    {
        public AssociationMultiplicityChange(PSMAssociation association)
            : base(association)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary; }
        }

        public PSMElement Parent
        {
            get { return Association.Parent; }
        }

        public uint? OldLower
        {
            get
            {
                return AssociationOldVersion.Lower;
            }
        }

        public uint? NewLower
        {
            get
            {
                return Association.Lower;
            }
        }

        public UnlimitedNatural OldUpper
        {
            get
            {
                return AssociationOldVersion.Upper;
            }
        }

        public UnlimitedNatural NewUpper
        {
            get
            {
                return Association.Upper;
            }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Association's {0:SN} multiplicity changed from {1} to {2}", Association.XPath, AssociationOldVersion.MultiplicityString, Association.MultiplicityString);
        }

        public override bool InvalidatesAttributes
        {
            get
            {
                // former optional attributes no longer optional and must be generated
                if (this.CanInvalidateDocument() != EDocumentInvalidation.None
                    && OldLower == 0 && NewLower > 1 && Association.Child.EncompassesAttributesForParentSignificantNode())
                    return true;

                return false; 
            }
        }

        public override bool InvalidatesContent
        {
            get 
            {
                return this.CanInvalidateDocument() != EDocumentInvalidation.None && Association.Child.EncompassesContentForParentSignificantNode();            
            }
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(Association != null && AssociationOldVersion != null);
            Debug.Assert(OldLower != NewLower || OldUpper != NewUpper);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAssociation association)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMAssociation psmAssociationOldVersion = (PSMAssociation)association.GetInVersion(v1);
            PSMAssociation psmAssociationNewVersion = (PSMAssociation)association.GetInVersion(v2);
            Debug.Assert(psmAssociationNewVersion != null && psmAssociationOldVersion != null);
            if ((psmAssociationOldVersion.Lower != psmAssociationNewVersion.Lower) ||
                (psmAssociationOldVersion.Upper != psmAssociationNewVersion.Upper))
            {
                AssociationMultiplicityChange c = new AssociationMultiplicityChange(association) { OldVersion = v1, NewVersion = v2 };
                result.Add(c);
            }

            return result;
        }
    }
}