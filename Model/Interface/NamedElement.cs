using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace XCase.Model
{
    /// <summary>
    /// A named element is an element in a model that may have a name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A named element represents elements that may have a name. 
    /// The name is used for identification of the named element 
    /// within the namespace in which it is defined.
    /// </para>
    /// <para>
    /// A named element also has a qualified name that allows it to be
    /// unambiguously identified within a hierarchy of nested namespaces.
    /// </para>
    /// NamedElement is an abstract metaclass.
    /// </remarks>
    public interface NamedElement : Element
    {
        /// <summary>
        /// Finds a subordinate element identified by its qualified name.
        /// The name (path) has to start at this element.
        /// </summary>
        /// <param name="qName">
        /// Qualified name of the searched child, the root element in the name
        /// has to be this element.
        /// </param>
        /// <returns>Reference to the element if found, null otherwise</returns>
        NamedElement GetChildByQualifiedName(string qName);

        /// <summary>
        /// Gets or sets the name of the element.
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the qualified name of the element.
        /// </summary>
        /// <value>
        /// Qualified name is constructed from the names of the containing 
        /// namespaces starting at the root of the hierarchy and ending 
        /// with the name of the NamedElement itself.<br />
        /// This is a derived attribute.
        /// </value>
        string QualifiedName
        {
            get;
        }

        /// <summary>
        /// Gets or sets the equivalent (id) of this named element in an ontology
        /// </summary>
        string OntologyEquivalent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the visibility of the element.
        /// </summary>
        /// <value>
        /// Type: NUml.Uml2.VisibilityKind<br />
        /// Determines where the NamedElement appears within different 
        /// Namespaces within the overall model, and its accessibility.
        /// </value>
        NUml.Uml2.VisibilityKind Visibility
        {
            get;
            set;
        }
    }
}
