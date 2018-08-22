namespace XCase.Model.Implementation
{
    /// <summary>
    /// Auxiliary interface for a data type.
    /// </summary>
    internal interface _ImplDataType : DataType
    {
        /// <summary>
        /// Gets a reference to the adapted nUML Type element.
        /// </summary>
        NUml.Uml2.Type AdaptedType
        {
            get;
        }

        /// <summary>
        /// Gets or sets a reference to the package that owns this type.
        /// </summary>
        new Package Package
        {
            get;
            set;
        }
    }
}
