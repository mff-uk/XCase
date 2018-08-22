using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    /// <summary>
    /// Component was moved from one superordinate container to another 
    /// (property <see cref="PSMSubordinateComponent.Parent"/> changed). 
    /// </summary>
    [ChangeProperties(EChangeScope.Subordinate, EEditType.Migratory)]
    public class SubordinateComponentMovedChange : SubordinateChange, IDoubleTargetChange
    {
        public SubordinateComponentMovedChange(PSMSubordinateComponent subordinate)
            : base(subordinate)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Migratory; }
        }

        public PSMSuperordinateComponent NewParent
        {
            get
            {
                return Subordinate.Parent;
            }
        }

        public PSMSuperordinateComponent OldParent
        {
            get
            {
                return SubordinateOldVersion.Parent;
            }
        }

        public PSMElement Parent
        {
            get { return NewParent; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Subordinate component {0} moved from {1:SN} to {2:SN}.", Subordinate, OldParent, NewParent);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(Subordinate != null);
            Debug.Assert(Subordinate.ExistsInVersion(OldVersion));
            Debug.Assert(Subordinate.GetInVersion(NewVersion) == Subordinate);
            Debug.Assert(NewParent.Components.Contains(Subordinate));
            Debug.Assert(OldParent.Components.Contains(SubordinateOldVersion));
            Debug.Assert(NewParent != OldParent);
            Debug.Assert(NewParent.GetInVersion(OldVersion) != OldParent);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMSubordinateComponent subordinateComponent)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMSubordinateComponent oldVersion = (PSMSubordinateComponent)subordinateComponent.GetInVersion(v1);
            PSMSubordinateComponent newVersion = (PSMSubordinateComponent)subordinateComponent.GetInVersion(v2);
            if (newVersion != null && oldVersion != null
                && oldVersion.Parent != newVersion.Parent.GetInVersion(v1))
            {
                result.Add(new SubordinateComponentMovedChange(subordinateComponent) { OldVersion = v1, NewVersion = v2 });
            }

            return result;
        }

        public PSMElement SecondaryTarget
        {
            get { return OldParent; }
        }

        public override bool InvalidatesAttributes
        {
            get { return Subordinate.EncompassesAttributesForParentSignificantNode(); }           
        }

        public override bool InvalidatesContent
        {
            get { return Subordinate.EncompassesContentForParentSignificantNode(); }
        }
    }
}