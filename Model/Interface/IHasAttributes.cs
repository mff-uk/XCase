using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Identifies an element that can have attributes.
    /// </summary>
    public interface IHasAttributes
    {
        /// <summary>
        /// Gets a collection of the attributes of this class.
        /// </summary>
        /// <value>
        /// Type: ObservableCollection of XCase.Model.Property<br />
        /// </value>
        ObservableCollection<Property> Attributes
        {
            get;
        }

        /// <summary>
        /// Creates a new attribute and adds it to this class.
        /// </summary>
        /// <returns>Reference to the new attribute</returns>
        Property AddAttribute();
    }
}
