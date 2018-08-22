using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    /// <summary>
    /// A component was removed from a superordinate component. The component does not exist
    /// anywhere in the new version (it was not moved, but completle deleted).
    /// </summary>
    [ChangeProperties(EChangeScope.ClassUnion, EEditType.Removal)]
    public class ClassUnionComponentRemovedChange : ClassUnionChange, ISubelementRemovalChange 
    {
        public PSMAssociationChild RemovedComponent { get; set; }

        public PSMElement ChangedSubelement
        {
            get { return RemovedComponent; }
        }

        public ClassUnionComponentRemovedChange(PSMClassUnion classUnion)
            : base(classUnion)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Removal; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Subordinate component {0} removed from class union {1}.", RemovedComponent, ClassUnion);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(RemovedComponent != null);
            Debug.Assert(RemovedComponent.GetInVersion(OldVersion) == RemovedComponent);
            Debug.Assert(RemovedComponent.GetInVersion(NewVersion) == null);
            Debug.Assert(ClassUnionOldVersion.Components.Contains(RemovedComponent));
            Debug.Assert(!ClassUnion.Components.Contains(RemovedComponent));
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMClassUnion classUnion)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMClassUnion classUnionOldVersion =
                (PSMClassUnion) classUnion.GetInVersion(v1);
            
            foreach (PSMAssociationChild associationChild in classUnionOldVersion.Components)
            {
                /* must not exist in new version, testing whether it disappeard from components of the new 
                     * version is not enough. It could have been moved, but not deleted.
                     */
                if (associationChild.GetInVersion(v2) == null)
                {
                    ClassUnionComponentRemovedChange change =
                        new ClassUnionComponentRemovedChange(classUnion)
                            {
                                RemovedComponent = associationChild,
                                OldVersion = v1,
                                NewVersion = v2
                            };
                    result.Add(change);
                }
            }
            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return RemovedComponent.EncompassesAttributesForParentSignificantNode(); }
        }

        public override bool InvalidatesContent
        {
            get { return RemovedComponent.EncompassesContentForParentSignificantNode(); }
        }
    }
}