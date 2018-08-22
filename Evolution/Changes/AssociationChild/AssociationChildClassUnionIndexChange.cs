using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.AssociationChild, EEditType.Migratory, MayRequireRevalidation = false)]
    public class AssociationChildClassUnionIndexChange : AssociationChildChange
    {
        public AssociationChildClassUnionIndexChange(PSMAssociationChild associationChild) : base(associationChild) { }

        public override EEditType EditType
        {
            get { return EEditType.Migratory; }
        }

        public int NewIndex
        {
            get
            {
                return AssociationChild.ParentUnion.Components.IndexOf(AssociationChild);
            }
        }

        public int OldIndex
        {
            get
            {
                return AssociationChildOldVersion.ParentUnion.Components.IndexOf(AssociationChildOldVersion);
            }
        }

        public PSMElement Parent
        {
            get { return AssociationChild.ParentUnion; }
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(AssociationChild.ParentUnion != null);
            Debug.Assert(AssociationChildOldVersion.ParentUnion != null);
            Debug.Assert(AssociationChild.ParentUnion.GetInVersion(OldVersion) == AssociationChildOldVersion.ParentUnion);
            Debug.Assert(AssociationChild.ParentUnion == AssociationChildOldVersion.ParentUnion.GetInVersion(NewVersion));
            Debug.Assert(NewIndex != OldIndex);
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Class {0} moved in class union {1} from index {2} to {3}.", AssociationChild, AssociationChild.ParentUnion, OldIndex, NewIndex);
        }


        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAssociationChild associationChild)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMAssociationChild associationChildOldVersion = (PSMAssociationChild)associationChild.GetInVersion(v2);

            if (associationChild.ParentUnion != null && associationChildOldVersion.ParentUnion != null &&
                associationChild.ParentUnion.GetInVersion(v1) == associationChildOldVersion.ParentUnion &&
                associationChild.ComponentIndex() != associationChildOldVersion.ComponentIndex())
            {
                AssociationChildClassUnionIndexChange change = new AssociationChildClassUnionIndexChange(associationChild) { OldVersion = v1, NewVersion = v2 };
                result.Add(change);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return false; }
        }

        public override bool InvalidatesContent
        {
            get { return false; }
        }
    }
}