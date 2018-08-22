namespace XCase.Model
{
    /// <summary>
    /// Represents one end of an association.
    /// </summary>
    /// <remarks>
    /// The AssociationEnd is inherited from a property. 
    /// In the UML specification an end of an association is represented
    /// directly by a property. However, in this specialized UML implementation
    /// we provide a dedicated class. Mainly for an ease of use, secondly to 
    /// ensure a correct semantics of the model.
    /// </remarks>
    public interface AssociationEnd : Property
    {
		/// <summary>
		/// Returns reference of the association that owns this end.
		/// </summary>
		Association Association{ get; }
    }
}
