namespace XCase.Model
{
    /// <summary>
    /// Base interface of all the PSM components.
    /// </summary>
    public interface PSMElement : NamedElement
    {
        /// <summary>
        /// Gets or sets the diagram that this element belongs to.
        /// </summary>
        PSMDiagram Diagram
        {
            get;
            set;
        }

        string XPath
        { 
            get;
        }
    }
}
