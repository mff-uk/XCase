using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    /// <summary>
    /// New association child was added. The added association child did not exist 
    /// anywhere in the older version of the diagram (it was not moved from other place).
    /// </summary>
    [ChangeProperties(EChangeScope.Association, EEditType.Addition)]
    public class AssociationChildAdded : AssociationChange, ISubelementAditionChange
    {
        public PSMAssociationChild AddedAssociationChild { get; set; }

        public PSMElement ChangedSubelement
        {
            get { return AddedAssociationChild; }
        }

        public AssociationChildAdded(PSMAssociation association)
            : base(association)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Addition; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "New association child {0} added under association {1}.", AddedAssociationChild, Association);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(AddedAssociationChild != null);
            Debug.Assert(AddedAssociationChild.GetInVersion(OldVersion) == null);
            Debug.Assert(AddedAssociationChild.GetInVersion(NewVersion) == AddedAssociationChild);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAssociation association)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            if (association.Child.GetInVersion(v1) == null)
            {
                AssociationChildAdded change = new AssociationChildAdded(association)
                                                        {
                                                            AddedAssociationChild = association.Child,
                                                            OldVersion = v1,
                                                            NewVersion = v2
                                                        };
                result.Add(change);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return AddedAssociationChild.EncompassesAttributesForParentSignificantNode(); }
        }

        public override bool InvalidatesContent
        {
            get { return AddedAssociationChild.EncompassesContentForParentSignificantNode(); }
        }
    }
}