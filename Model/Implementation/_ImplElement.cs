using XCase.Model.Serialization;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Auxiliary interface for the element.
    /// </summary>
    internal interface _ImplElement : Element
    {
        XCaseGuid Guid { get; set; }

        /// <summary>
        /// Gets a reference to the adapted nUML element.
        /// </summary>
        NUml.Uml2.Element AdaptedElement
        {
            get;
        }
    }
}
