using System;

namespace XCase.Model
{
    /// <summary>
    /// Contains data for the element added to the diagram event.
    /// </summary>
    public class ElementAddedEventArgs: EventArgs
    {
		public ElementAddedEventArgs(Element element, ViewHelper visualization)
        {
            this.Element = element;
            this.Visualization = visualization;
        }

    	/// <summary>
    	/// Gets a reference to the added element.
    	/// </summary>
    	public Element Element { get; protected set; }

    	/// <summary>
    	/// Gets a reference to the visualization of the element.
    	/// </summary>
		public ViewHelper Visualization { get; protected set; }
    }

	/// <summary>
	/// Arguments of the event, contains the removed element.
	/// </summary>
	public class ElementRemovedEventArgs: EventArgs
	{
		public Element Element { get; private set; }

		public ElementRemovedEventArgs(Element element)
		{
			Element = element;
		}
	}

    /// <summary>
    /// Contains Diagram involved in DiagramAdded or DiagramRemovet event
    /// </summary>
    public class DiagramEventArgs : EventArgs
    {
        public Diagram Diagram;
        
        public DiagramEventArgs(Diagram diag)
        {
            Diagram = diag;
        }
    }

}	