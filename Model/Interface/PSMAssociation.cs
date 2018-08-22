using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Represents an association between two PSM components.
    /// The association is oriented, the parent component can be any of PSM superordinate components:
    /// <list type="bullet">
    ///     <item>PSM Class</item>
    ///     <item>PSM Content Container</item>
    ///     <item>PSM Content Choice</item>
    /// </list>
    /// and the child any of the PSM association childs:
    /// <list type="bullet">
    ///     <item>PSM Class</item>
    ///     <item>PSM Class Union</item>
    /// </list>
    /// The semantics of the association is described as a union of the nesting joins.
    /// </summary>
    public interface PSMAssociation : PSMSubordinateComponent, MultiplicityElement
    {
        /// <summary>
        /// Creates a new nesting join and adds it to the end of the NestingJoins collection.
        /// </summary>
        /// <param name="coreClass">References the core class</param>
        /// <returns>Reference to the new nesting join</returns>
        NestingJoin AddNestingJoin(PIMClass coreClass);

        /// <summary>
        /// Gets or sets the PSM component which is the child of this PSM association.
        /// The child can be either a PSM class or a PSM class union.
        /// </summary>
        PSMAssociationChild Child
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of generalizations that were used to import this association.
        /// </summary>
        ObservableCollection<Generalization> UsedGeneralizations
        {
            get;
        }

        /// <summary>
        /// Gets the collection of nesting joins describing the semantics of this association.
        /// </summary>
        ObservableCollection<NestingJoin> NestingJoins
        {
            get;
        }

        /// <summary>
        /// Gets or sets the minimal number of occurrences of Child instances 
        /// in a Parent instance.
        /// Initiated by nesting joins.
        /// </summary>
        new uint? Lower
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximal number of occurrences of Child instances 
        /// in a Parent instance.
        /// Initiated by nesting joins.
        /// </summary>
        new NUml.Uml2.UnlimitedNatural Upper
        {
            get;
            set;
        }
    }
}
