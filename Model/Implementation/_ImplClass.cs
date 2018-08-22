namespace XCase.Model.Implementation
{
    /// <summary>
    /// Auxiliary interface for a class.
    /// </summary>
    internal interface _ImplClass : Class
    {
        /// <summary>
        /// Gets a reference to the adapted nUML Class element.
        /// </summary>
        NUml.Uml2.Class AdaptedClass
        {
            get;
        }

        /// <summary>
        /// Gets or sets a reference to the parent package of this class.
        /// </summary>
        new Package Package
        {
            get;
            set;
        }
    }
}
