using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// A class describes a set of objects that share 
    /// the same specifications of features, constraints, and semantics.
    /// </summary>
    /// <remarks>
    /// Class is a kind of classifier whose features are attributes and operations. 
    /// Attributes of a class are represented by instances of Property 
    /// that are owned by the class. 
    /// Some of these attributes may represent the navigable ends of binary associations.
    /// </remarks>
	public interface Class : DataType, IHasAttributes, IHasOperations, IAssociationSource, IAssociationTarget
    {
        /// <summary>
        /// Gets a list grouping all the associations from the Associations
        /// collection of this class and all the more general classes (recursively).
        /// </summary>
        List<Association> AllAssociations
        {
            get;
        }

        /// <summary>
        /// Gets a list grouping all the attributes from the Attributes
        /// collection of this class and all the more general classes (recursively).
        /// </summary>
        List<Property> AllAttributes
        {
            get;
        }

        /// <summary>
        /// Gets a list grouping all ancestors
        /// of this class including this class and all the more general classes (recursively).
        /// </summary>
        List<Class> MeAndAncestors
        {
            get;
        }

        /// <summary>
        /// Gets a collection of associations that this class is part of.
        /// </summary>
        /// <value>
        /// Type: Observable collection of XCase.Model.Association<br />
        /// </value>
        ObservableCollection<Association> Assocations
        {
            get;
        }

        /// <summary>
        /// Indicates whether this class is abstract or not.
        /// </summary>
        bool IsAbstract
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of UML Generalization elements that
        /// lead to more general classes, i.e. this class is referenced
        /// in their Specific property.
        /// </summary>
        ObservableCollection<Generalization> Generalizations
        {
            get;
        }

        /// <summary>
        /// Gets a collection of UML Generalization elements that
        /// lead to more specific classes, i.e. this class is referenced
        /// in their General property.
        /// </summary>
        ObservableCollection<Generalization> Specifications
        {
            get;
        }

        /// <summary>
        /// Finds a path to the given ancestor of the class.
        /// </summary>
        /// <param name="ancestor">Reference to the searched ancestor</param>
        /// <returns>
        /// List of generalizations used to get from ancestor to this class 
        /// or null if the ancestor was not found.
        /// </returns>
        List<Generalization> GetPathToAncestor(Class ancestor);
    }
}
