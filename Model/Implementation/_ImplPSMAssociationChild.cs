namespace XCase.Model.Implementation
{
    /// <summary>
    /// Auxiliary interface for the PSMAssociationChild.
    /// It is used to get access to the adapted nUML Class of the PSM component.
    /// </summary>
    internal interface _ImplPSMAssociationChild : PSMAssociationChild
    {
        /// <summary>
        /// Gets a reference to the adapted nUML Class instance.
        /// </summary>
        NUml.Uml2.Class AdaptedClass
        {
            get;
        }

        /// <summary>
        /// Gets or sets the association that owns this component (if any).
        /// </summary>
        new PSMAssociation ParentAssociation
        {
            get;
            set;
        }
    }
}
