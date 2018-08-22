using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Represents an instance of a stereotype.
    /// </summary>
    public interface StereotypeInstance : NamedElement
    {
        /// <summary>
        /// Gets a collection of the attributes of the stereotype.
        /// These attrbiutes are instantiated, i.e. you cannot modify their
        /// properties nor the list of the attributes, you can only access 
        /// (or change) their value.
        /// </summary>
        ReadOnlyCollection<InstantiatedProperty> Attributes
        {
            get;
        }
        
        /// <summary>
        /// Gets a reference to the instantiated stereotype class.
        /// </summary>
        Stereotype Stereotype
        {
            get;
        }
    }
}
