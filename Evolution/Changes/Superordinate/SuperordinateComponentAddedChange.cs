using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    /// <summary>
    /// New component was added to a superordinate component. The added component did not exist 
    /// anywhere in the older version of the diagram (it was not moved from other place).
    /// </summary>
    [ChangeProperties(EChangeScope.Superordinate, EEditType.Addition)]
    public class SuperordinateComponentAddedChange : SuperordinateChange, ISubelementAditionChange
    {
        public PSMSubordinateComponent AddedComponent { get; set; }

        public PSMElement ChangedSubelement
        {
            get { return AddedComponent; }
        }

        public SuperordinateComponentAddedChange(PSMSuperordinateComponent superordinate)
            : base(superordinate)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Addition;  }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Subordinate component {0} added in superordinate component {1}.", AddedComponent, Superordinate);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(AddedComponent != null);
            Debug.Assert(AddedComponent.GetInVersion(OldVersion) == null);
            Debug.Assert(AddedComponent.GetInVersion(NewVersion) == AddedComponent);
            Debug.Assert(SuperordinateOldVersion == null || !SuperordinateOldVersion.Components.Contains(AddedComponent));
            Debug.Assert(Superordinate.Components.Contains(AddedComponent));
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMSuperordinateComponent superordinateComponent)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            foreach (PSMSubordinateComponent subordinate in superordinateComponent.Components)
            {
                /* must not exist in old version, testing whether it is not among components of the old 
                 * version is not enough. It could have been moved, but not created as new.
                 */
                if (subordinate.GetInVersion(v1) == null)
                {
                    SuperordinateComponentAddedChange change =
                        new SuperordinateComponentAddedChange(superordinateComponent)
                            {
                                AddedComponent = subordinate, OldVersion = v1, NewVersion = v2
                            };
                    result.Add(change);
                }
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get 
            {
                if (AddedComponent is PSMAssociation && ((PSMAssociation)AddedComponent).Lower == 0)
                    return false; 
                return AddedComponent.EncompassesAttributesForParentSignificantNode() && !(Superordinate is PSMContentChoice); 
            }
        }

        public override bool InvalidatesContent
        {
            get
            {
                if (AddedComponent is PSMAssociation && ((PSMAssociation)AddedComponent).Lower == 0)
                    return false;
                return AddedComponent.EncompassesContentForParentSignificantNode() && !(Superordinate is PSMContentChoice);
            }
        }
    }
}