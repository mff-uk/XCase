using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    /// <summary>
    /// Class union was moved to another place in the diagram
    /// (property <see cref="PSMClassUnion.ParentAssociation"/> changed). 
    /// </summary>
    [ChangeProperties(EChangeScope.ClassUnion, EEditType.Migratory)]
    public class ClassUnionMovedChange : ClassUnionChange, IDoubleTargetChange
    {
        public ClassUnionMovedChange(PSMClassUnion classUnion)
            : base(classUnion)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Migratory; }
        }

        public PSMAssociation NewParent
        {
            get
            {
                return ClassUnion.ParentAssociation;
            }
        }

        public PSMAssociation OldParent
        {
            get
            {
                return ClassUnion.ParentAssociation;
            }
        }

        public PSMElement Parent
        {
            get { return NewParent; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Class union {0} moved from {1:SN} to {2:SN}.", ClassUnion, OldParent, NewParent);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(ClassUnion != null);
            Debug.Assert(ClassUnion.ExistsInVersion(OldVersion));
            Debug.Assert(ClassUnion.GetInVersion(NewVersion) == ClassUnion);
            Debug.Assert(NewParent.Child == ClassUnion);
            Debug.Assert(OldParent.Child == ClassUnionOldVersion);
            Debug.Assert(NewParent != OldParent);
            Debug.Assert(NewParent.GetInVersion(OldVersion) != OldParent);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMClassUnion classUnion)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMClassUnion oldVersion = (PSMClassUnion)classUnion.GetInVersion(v1);
            PSMClassUnion newVersion = (PSMClassUnion)classUnion.GetInVersion(v2);
            if (newVersion != null && oldVersion != null
                && oldVersion.ParentAssociation != newVersion.ParentAssociation.GetInVersion(v1))
            {
                result.Add(new ClassUnionMovedChange(classUnion) { OldVersion = v1, NewVersion = v2 });
            }

            return result;
        }

        public PSMElement SecondaryTarget
        {
            get { return OldParent; }
        }

        public override bool InvalidatesAttributes
        {
            get { return ClassUnion.EncompassesAttributesForParentSignificantNode(); }
        }

        public override bool InvalidatesContent
        {
            get { return ClassUnion.EncompassesContentForParentSignificantNode(); }
        }
    }
}