namespace XCase.Model.Implementation
{
    /// <summary>
    /// Auxiliary interface for a package.
    /// </summary>
    internal interface _ImplPackage : Package
    {
        /// <summary>
        /// Gets a reference to the adapted nUML Package element.
        /// </summary>
        NUml.Uml2.Package AdaptedPackage
        {
            get;
        }

        /// <summary>
        /// Gets or sets a reference to the package that owns this one.
        /// </summary>
        new Package NestingPackage
        {
            get;
            set;
        }
    }
}
