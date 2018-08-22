using System;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.Subordinate, EEditType.Migratory)]
    public class SubordinateComponentComponentIndexChange : SubordinateChange, IDoubleTargetChange
    {
        public SubordinateComponentComponentIndexChange(PSMSubordinateComponent subordinate) : base(subordinate) { }

        public override EEditType EditType
        {
            get { return EEditType.Migratory; }
        }

        public int NewIndex
        {
            get
            {
                return Subordinate.ComponentIndex();
            }
        }

        public int OldIndex
        {
            get
            {
                return SubordinateOldVersion.ComponentIndex();
            }
        }

        public PSMElement Parent
        {
            get
            {
                return Subordinate.Parent;
            }
        }

        public PSMElement SecondaryTarget
        {
            get { return Parent; }
        }

        public override void Verify()
        {
            base.Verify();

            Debug.Assert(Subordinate.Parent.GetInVersion(OldVersion) == SubordinateOldVersion.Parent);
            Debug.Assert(Subordinate.Parent == SubordinateOldVersion.Parent.GetInVersion(NewVersion));
            Debug.Assert(NewIndex != OldIndex);
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider,
                                 "Subordinate component {0} moved from index {1} to {2}.", Subordinate, OldIndex, NewIndex);
        }


        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMSubordinateComponent subordinateComponent)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMSubordinateComponent subordinateOldVersion = (PSMSubordinateComponent)subordinateComponent.GetInVersion(v1);

            if (subordinateComponent.Parent.GetInVersion(v1) == subordinateOldVersion.Parent &&
                subordinateComponent.ComponentIndex() != subordinateOldVersion.ComponentIndex())
            {
                SubordinateComponentComponentIndexChange change = new SubordinateComponentComponentIndexChange(subordinateComponent) { OldVersion = v1, NewVersion = v2 };
                result.Add(change);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return Subordinate.EncompassesAttributesForParentSignificantNode() && !(Parent is PSMContentChoice); }
        }

        public override bool InvalidatesContent
        {
            get { return Subordinate.EncompassesContentForParentSignificantNode() && !(Parent is PSMContentChoice); }
        }
    }
}