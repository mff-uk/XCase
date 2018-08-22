using System;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.AssociationChild, EEditType.Migratory)]
    public class AssociationChildMovedToUnionChange : AssociationChildChange, IDoubleTargetChange
    {
        public AssociationChildMovedToUnionChange(PSMAssociationChild associationChild)
            : base(associationChild)
        {   
        }

        public override EEditType EditType
        {
            get { return EEditType.Migratory; }
        }

        public PSMElement Parent
        {
            get { return AssociationChild.ParentUnion; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Association child {0} moved from {1:SN} to class union {2}.", AssociationChild, AssociationChildOldVersion.ParentAssociation, AssociationChild.ParentUnion);
        }

        public override void Verify()
        {
            base.Verify();
            // class can be a root class in that case both parent union and parent association are null
            Debug.Assert(AssociationChildOldVersion.ParentAssociation != null || (AssociationChildOldVersion.ParentUnion == null && AssociationChildOldVersion.ParentAssociation == null));
            Debug.Assert(AssociationChildOldVersion.ParentUnion == null);
            Debug.Assert(AssociationChild.ParentAssociation == null);
            Debug.Assert(AssociationChild.ParentUnion != null);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAssociationChild associationChild)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMAssociationChild associationChildOldVersion = (PSMAssociationChild) associationChild.GetInVersion(v2);
            Debug.Assert(associationChildOldVersion != null);

            if ((associationChildOldVersion.ParentAssociation != null || 
                 (associationChildOldVersion.ParentAssociation == null && associationChildOldVersion.ParentUnion == null)) 
                && associationChild.ParentUnion != null)
            {
                result.Add(new AssociationChildMovedToUnionChange(associationChild) { OldVersion = v1, NewVersion = v2});
            }
            
            return result;
        }

        public PSMElement SecondaryTarget
        {
            get
            {
                if (AssociationChildOldVersion.ParentAssociation != null)
                {
                    return AssociationChildOldVersion.ParentAssociation;
                }
                else
                {
                    Debug.Assert(AssociationChildOldVersion.ParentUnion == null && AssociationChildOldVersion.ParentAssociation == null);
                    return AssociationChildOldVersion.ParentUnion;
                }
            }
        }

        public override bool InvalidatesAttributes
        {
            get { throw new NotImplementedException("InvalidatesAttributes not implemented in AssociationChildMovedToUnionChange."); }
        }

        public override bool InvalidatesContent
        {
            get { throw new NotImplementedException("InvalidatesContent not implemented in AssociationChildMovedToUnionChange."); }
        }
    }
}