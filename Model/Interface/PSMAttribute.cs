using System.Collections.ObjectModel;
namespace XCase.Model
{
    /// <summary>
    /// Represents a PSMAttribute described in the XSem specification.
    /// For further details about the construct consult http://www.necasky.net/xcase/xsem-spec.html
    /// </summary>
    public interface PSMAttribute : Property, PSMElement
    {
        /// <summary>
        /// Gets or sets a reference to the parent PSM class.
        /// </summary>
        new PSMClass Class
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a reference to the attribute container that owns this attribute.
        /// If this attribute is not in an attribute container the value is null.
        /// </summary>
        PSMAttributeContainer AttributeContainer
        {
            get;
        }

        /// <summary>
        /// Gets a collection of generalizations that were used to import
        /// this attribute.
        /// </summary>
        ObservableCollection<Generalization> UsedGeneralizations
        {
            get;
        }

        /// <summary>
        /// Gets or sets the alias of this attribute in the derived
        /// XML schema.<br />
        /// The default value is the name of the represented attribute
        /// </summary>
        string Alias
        {
            get;
            set;
        }

        /// <summary>
        /// Returns <see cref="Alias"/> if it is defined, otherwise returns 
        /// <see cref="NamedElement.Name">Name</see>.
        /// </summary>
        string AliasOrName
        { 
            get;
        }

        /// <summary>
        /// Gets or sets reference to the represented PIM attribute.
        /// </summary>
        Property RepresentedAttribute
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an XSD implementation of the simple data type assigned to the represented PIM attribute.
        /// </summary>
        string XSDImplementation
        {
            get;set;
        }
    }
}
