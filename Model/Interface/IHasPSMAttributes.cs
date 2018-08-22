using System;
using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Identifies an element that can have PSM attributes.
    /// </summary>
    public interface IHasPSMAttributes: PSMElement
    {
        /// <summary>
        /// Gets a collection of the attributes of this PSM class.
        /// This collection contains the same attributes as the Attributes
        /// collection but it treats them as the PSMAttribute instances rather
        /// than Attribute.
        /// </summary>
        ObservableCollection<PSMAttribute> PSMAttributes
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the represented PIM class.
        /// </summary>
        PIMClass RepresentedClass
        {
            get;
        }

        /// <summary>
        /// Adds an attribute pimAttrName from the represented PIM class 
        /// to this PSM class / component.
        /// If the attribute does not exist in the PIM class, 
        /// an ArgumentException is thrown.
        /// </summary>
        /// <param name="pimAttrName">Name of the PIM attribute</param>
        /// <returns>Reference to the new PSM atribute</returns>
        /// <exception cref="ArgumentException">
        /// Attribute with given name does not exist in the represented PIM class
        /// </exception>
        PSMAttribute AddAttribute(string pimAttrName);

        /// <summary>
        /// Adds the attribute representing the given one.
        /// The given attribute must be owned by the represented PIM class, 
        /// otherwise an ArgumentException is thrown.
        /// </summary>
        /// <param name="pimAttribute">Reference to the PIM attribute</param>
        /// <returns>Reference to the new PSM attribute</returns>
        /// <exception cref="ArgumentException">
        /// Given attribute does not exist (is not owned) in the represented PIM class.
        /// </exception>
        PSMAttribute AddAttribute(Property pimAttribute);
    }
}
