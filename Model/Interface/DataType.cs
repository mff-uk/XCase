namespace XCase.Model
{
    /// <summary>
    /// A type constrains the values represented by a typed element.
    /// </summary>
    /// <remarks>
    /// A type serves as a constraint on the range of values 
    /// represented by a typed element.<br />
    /// Type is an abstract metaclass.
    /// </remarks>
    public interface DataType : NamedElement
    {
        /// <summary>
        /// Gets a reference to the package that owns this type.
        /// </summary>
        Package Package
        {
            get;
        }
    }
}
