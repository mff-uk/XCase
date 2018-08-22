using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    /// <summary>
    /// A component was removed from a superordinate component. The component does not exist
    /// anywhere in the new version (it was not moved, but completle deleted).
    /// </summary>
    [ChangeProperties(EChangeScope.Superordinate, EEditType.Removal)]
    public class SuperordinateComponentRemovedChange : SuperordinateChange, ISubelementRemovalChange
    {
        public PSMSubordinateComponent RemovedComponent { get; set; }

        public PSMElement ChangedSubelement
        {
            get { return RemovedComponent; }
        }

        public SuperordinateComponentRemovedChange(PSMSuperordinateComponent superordinate)
            : base(superordinate)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Removal; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Subordinate component {0} removed from superordinate component {1}.", RemovedComponent, Superordinate);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(RemovedComponent != null);
            Debug.Assert(RemovedComponent.GetInVersion(OldVersion) == RemovedComponent);
            Debug.Assert(RemovedComponent.GetInVersion(NewVersion) == null);
            Debug.Assert(SuperordinateOldVersion.Components.Contains(RemovedComponent));
            Debug.Assert(!Superordinate.Components.Contains(RemovedComponent));
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMSuperordinateComponent superordinateComponent)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMSuperordinateComponent superordinateComponentOldVersion =
                (PSMSuperordinateComponent)superordinateComponent.GetInVersion(v1);

            foreach (PSMSubordinateComponent subordinate in superordinateComponentOldVersion.Components)
            {
                /* must not exist in new version, testing whether it disappeard from components of the new 
                 * version is not enough. It could have been moved, but not deleted.
                 */
                if (subordinate.GetInVersion(v2) == null)
                {
                    SuperordinateComponentRemovedChange change =
                        new SuperordinateComponentRemovedChange(superordinateComponent)
                            {
                                RemovedComponent = subordinate, OldVersion = v1, NewVersion = v2 
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