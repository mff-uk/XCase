using System;

namespace XCase.Model
{
    /// <summary>
    /// A typed element has a type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A typed element is an element that has a type 
    /// that serves as a constraint on the range of values 
    /// the element can represent.
    /// </para>
    /// Typed element is an abstract metaclass.
    /// </remarks>
    public interface TypedElement : NamedElement
    {
        /// <summary>
        /// Gets or sets the type of the element.
        /// </summary>
        /// <value>
        /// Type: XCase.Model.DataType<br />
        /// </value>
        /// <exception cref="ArgumentException">
        /// DataType created outside the Model library is passed to the property.
        /// </exception>
        DataType Type
        {
            get;
            set;
        }
    }
}
