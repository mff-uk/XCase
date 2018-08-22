using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// An association describes a set of tuples whose values 
    /// refer to typed instances.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An association specifies a semantic relationship that can occur 
    /// between typed instances. It has at least two ends represented 
    /// by properties, each of which is connected to the type of the end. 
    /// More than one end of the association may have the same type.
    /// </para>
    /// <para>
    /// An end property of an association that is owned by an end class 
    /// or that is a navigable owned end of the association indicates 
    /// that the association is navigable from the opposite ends; 
    /// otherwise, the association is not navigable from the opposite ends.
    /// </para>
    /// </remarks>
    /// <seealso cref="AssociationEnd"/>
    public interface Association : NamedElement
    {
        /// <summary>
        /// Creates a new end of this association and adds it to the
        /// specified position in the Ends collection.
        /// </summary>
        /// <param name="index">Position of the new end (zero - based)</param>
        /// <param name="end">Reference to the class at the new end</param>
        /// <returns>Reference to the new association end</returns>
        AssociationEnd CreateEnd(int index, Class end);

        /// <summary>
        /// Creates a new end of this association and adds it to the
        /// end of the Ends collection.
        /// </summary>
        /// <param name="end">Reference to the class at the new end</param>
        /// <returns>Reference to the new association end</returns>
        AssociationEnd CreateEnd(Class end);

		/// <summary>
		/// Removes association end from the association
		/// </summary>
		/// <param name="end">removed association end</param>
    	void RemoveEnd(AssociationEnd end);

        /// <summary>
        /// Gets the collection of the association ends.
        /// For adding and removing into the collection use <see cref="CreateEnd(XCase.Model.Class)"/> and 
        /// <see cref="RemoveEnd"/>
        /// </summary>
        /// <value>
        /// Type: ObservableCollection of XCase.Model.AssociationEnd<br />
        /// </value>
        ObservableCollection<AssociationEnd> Ends
        {
            get;
        }

        /// <summary>
        /// Gets a collection of nesting joins that references this association.
        /// </summary>
        ObservableCollection<NestingJoin> ReferencingNestingJoins
        {
            get;
        }
    }
}
