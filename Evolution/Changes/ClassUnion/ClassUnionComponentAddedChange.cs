using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    /// <summary>
    /// New component was added to a superordinate component. The added component did not exist 
    /// anywhere in the older version of the diagram (it was not moved from other place).
    /// </summary>
    [ChangeProperties(EChangeScope.ClassUnion, EEditType.Addition, MayRequireRevalidation = false)]
    public class ClassUnionComponentAddedChange : ClassUnionChange, ISubelementAditionChange 
    {
        public PSMAssociationChild AddedComponent { get; set; }

        public PSMElement ChangedSubelement
        {
            get { return AddedComponent;  }
        }

        public ClassUnionComponentAddedChange(PSMClassUnion classUnion)
            : base(classUnion)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Addition; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Subordinate component {0} added in superordinate component {1}.", AddedComponent, ClassUnion);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(AddedComponent != null);
            Debug.Assert(AddedComponent.GetInVersion(OldVersion) == null);
            Debug.Assert(AddedComponent.GetInVersion(NewVersion) == AddedComponent);
            Debug.Assert(ClassUnionOldVersion == null || !ClassUnionOldVersion.Components.Contains(AddedComponent));
            Debug.Assert(ClassUnion.Components.Contains(AddedComponent));
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMClassUnion classUnion)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();
            
            {
                foreach (PSMAssociationChild associationChild in classUnion.Components)
                {
                    /* must not exist in old version, testing whether it is not among components of the old 
                     * version is not enough. It could have been moved, but not created as new.
                     */
                    if (associationChild.GetInVersion(v1) == null)
                    {
                        ClassUnionComponentAddedChange change =
                            new ClassUnionComponentAddedChange(classUnion)
                                {
                                    AddedComponent = associationChild,
                                    OldVersion = v1,
                                    NewVersion = v2
                                };
                        result.Add(change);
                    }
                }
            }
            
            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return true; }
        }

        public override bool InvalidatesContent
        {
            get { return true; }
        }
    }
}