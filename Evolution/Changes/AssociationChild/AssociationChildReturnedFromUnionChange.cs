using System;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.AssociationChild, EEditType.Migratory)]
    public class AssociationChildReturnedFromUnionChange : AssociationChildChange, IDoubleTargetChange
    {
        public AssociationChildReturnedFromUnionChange(PSMAssociationChild associationChild) : 
            base(associationChild)
        {

        }

        public PSMElement Parent
        {
            get { return AssociationChild.ParentAssociation; }
        }

        public override EEditType EditType
        {
            get { return EEditType.Migratory; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Association child {0} moved from class union {1} to {2:SN}.", AssociationChild, AssociationChildOldVersion.ParentUnion, AssociationChild.ParentAssociation);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(AssociationChildOldVersion.ParentAssociation == null);
            Debug.Assert(AssociationChildOldVersion.ParentUnion != null);
            Debug.Assert(AssociationChild.ParentAssociation != null || (AssociationChild.ParentAssociation == null && AssociationChild.ParentUnion == null));
            Debug.Assert(AssociationChild.ParentUnion == null);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAssociationChild associationChild)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMAssociationChild associationChildOldVersion = (PSMAssociationChild)associationChild.GetInVersion(v2);
            Debug.Assert(associationChildOldVersion != null);

            if ((associationChild.ParentAssociation != null ||
                 (associationChild.ParentAssociation == null && associationChild.ParentUnion == null))
                && associationChildOldVersion.ParentUnion != null)
            {
                result.Add(new AssociationChildReturnedFromUnionChange(associationChild) { OldVersion = v1, NewVersion = v2 });
            }

            return result;
        }

        public PSMElement SecondaryTarget
        {
            get
            {
                return AssociationChildOldVersion.ParentUnion;
            }
        }

        public override bool InvalidatesAttributes
        {
            get { throw new NotImplementedException("InvalidatesAttributes not implemented in AssociationChildReturnedFromUnionChange."); }
        }

        public override bool InvalidatesContent
        {
            get { throw new NotImplementedException("InvalidatesContent not implemented in AssociationChildReturnedFromUnionChange."); }
        }
    }
}