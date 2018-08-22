using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    /// <summary>
    /// AssociationChild is removed (not moved to another place).
    /// </summary>
    [ChangeProperties(EChangeScope.Association, EEditType.Removal)]
    public class AssociationChildRemoved : AssociationChange, ISubelementRemovalChange
    {
        public PSMAssociationChild RemovedAssociationChild { get; set; }

        public PSMElement ChangedSubelement
        {
            get { return RemovedAssociationChild; }
        }

        public AssociationChildRemoved(PSMAssociation association)
            : base(association)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Removal; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Association child {0} was removed under association {1}.", RemovedAssociationChild, Association);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(RemovedAssociationChild != null);
            Debug.Assert(RemovedAssociationChild.GetInVersion(NewVersion) == null);
            Debug.Assert(RemovedAssociationChild.GetInVersion(OldVersion) == RemovedAssociationChild);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAssociation association)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMAssociation associationOldVersion = (PSMAssociation) association.GetInVersion(v1);

            if (associationOldVersion.Child.GetInVersion(v2) == null)
            {
                AssociationChildRemoved change = new AssociationChildRemoved(association)
                                                          {
                                                              RemovedAssociationChild = associationOldVersion.Child,
                                                              OldVersion = v1,
                                                              NewVersion = v2
                                                          };
                result.Add(change);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return RemovedAssociationChild.EncompassesAttributesForParentSignificantNode(); }
        }

        public override bool InvalidatesContent
        {
            get { return RemovedAssociationChild.EncompassesContentForParentSignificantNode(); }
        }
    }
}