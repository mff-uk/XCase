using System.Collections.ObjectModel;
namespace XCase.Model
{
    /// <summary>
    /// A generalization is a taxonomic relationship 
    /// between a more general classifier and a more specific classifier.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each instance of the specific classifier is also an indirect 
    /// instance of the general classifier. 
    /// Thus, the specific classifier inherits the features of the more general classifier.
    /// </para>
    /// A generalization relates a specific classifier to a more 
    /// general classifier, and is owned by the specific classifier.
    /// </remarks>
    public interface Generalization : Element
    {
        /// <summary>
        /// Indicates whether the specific classifier 
        /// can be used wherever the general classifier can be used.
        /// </summary>
        /// <value>
        /// If true, the execution traces of the specific classifier 
        /// will be a superset of the execution traces of the general classifier.
        /// </value>
        bool IsSubstituable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the reference to the general classifier in the Generalization relationship.
        /// </summary>
        Class General
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the reference to the specializing classifier in the Generalization relationship.
        /// </summary>
        Class Specific
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of PSM Attributes that uses this generalization.
        /// </summary>
        ObservableCollection<PSMAttribute> ReferencingPSMAttributes
        {
            get;
        }

        /// <summary>
        /// Gets a collection of PSM Associations that references this generalization.
        /// </summary>
        ObservableCollection<PSMAssociation> ReferencingPSMAssociations
        {
            get;
        }
    }
}
