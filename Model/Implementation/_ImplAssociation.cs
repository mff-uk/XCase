namespace XCase.Model.Implementation
{
    /// <summary>
    /// Auxiliary interface for an association.
    /// </summary>
    internal interface _ImplAssociation : Association
    {
        /// <summary>
        /// Gets a reference to the adapted nUML Association element.
        /// </summary>
        NUml.Uml2.Association AdaptedAssociation
        {
            get;
        }
    }
}
