using System;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.Association, EEditType.Sedentary)]
    public abstract class AssociationChange : EvolutionChange
    {
        protected AssociationChange(PSMAssociation psmAssociation)
        {
            Element = psmAssociation;
        }

        public override EChangeScope Scope
        {
            get { return EChangeScope.Association; }
        }

        public PSMAssociation Association
        {
            get { return Element as PSMAssociation; }
        }

        public PSMAssociation AssociationOldVersion
        {
            get { return (PSMAssociation)Association.GetInVersion(OldVersion); }
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(Association != null);
        }
    }
}