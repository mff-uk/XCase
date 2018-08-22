using XCase.Model;

namespace XCase.View.Controls
{
	/// <summary>
	/// Context menu item using a reference to a diagram.
	/// </summary>
    public class ContextMenuDiagramItem : ContextMenuItem
    {
		/// <summary>
		/// Reference to a diagram
		/// </summary>
		/// <value><see cref="Diagram"/></value>
        public Diagram Diagram { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ContextMenuDiagramItem"/> class.
		/// </summary>
		/// <param name="Text">caption of the item</param>
		/// <param name="diagram">The diagram.</param>
		public ContextMenuDiagramItem(string Text, Diagram diagram) : base(Text)
        {
            Diagram = diagram;
        }
    }
}
