using System.ComponentModel;

namespace XCase.Model
{
    /// <summary>
    /// Represents a property of an instance of an UML class.
    /// </summary>
    /// <remarks>
    /// When working with an instance of a class, you can no longer
    /// modify the attributes of the property itself (visibility, multiplicity, etc.).
    /// The only thing you can change is its value.
    /// </remarks>
    public interface InstantiatedProperty : Element
    {
        /// <summary>
        /// Gets the name of this property.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Gets or sets the value of this property.
        /// </summary>
        /// <value>
        /// Type: XCase.Model.ValueSpecification<br />
        /// </value>
        ValueSpecification Value
        {
            get;
            set;
        }
    }
}
