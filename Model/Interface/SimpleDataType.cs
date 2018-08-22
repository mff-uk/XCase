namespace XCase.Model
{
    /// <summary>
    /// A simple type constraints values of simple data values (strings, integers, dates, etc.).
    /// </summary>
    public interface SimpleDataType : DataType
    {
        /// <summary>
        /// Gets a reference to a base simple data type.
        /// </summary>
        SimpleDataType Parent
        {
            get;
        }

        /// <summary>
        /// Gets or sets a default implementation of the data type in XSD.
        /// </summary>
        /// <remarks>
        /// The value must be a valid XSD simple type definition.
        /// </remarks>
        string DefaultXSDImplementation
        {
            get;
            set;
        }
    }
}
