using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Abstract MacroCommand for creating new elements that can be positioned 
	/// freely on the diagram (such as classes).
	/// </summary>
	public abstract class NewPositionableElementMacroCommand : MacroCommand<DiagramController>
	{
		private double x = 100;
		/// <summary>
		/// X coordinate of the newly placed element
		/// </summary>
		public double X
		{
			get { return x; }
			set { x = value; }
		}

		private double y = 100;
		/// <summary>
		/// Y coordinate of the newly placed element
		/// </summary>
		public double Y
		{
			get { return y; }
			set { y = value; }
		}

		double width = double.NaN;
		/// <summary>
		/// Width the newly placed element
		/// </summary>
		public double Width
		{
			get { return width; }
			set { width = value; }
		}

		double height = double.NaN;
		/// <summary>
		/// Height of the newly placed element
		/// </summary>
		public double Height
		{
			get { return height; }
			set { height = value; }
		}

		/// <summary>
		/// Returns true if element can be added
		/// </summary>
		public override bool CanExecute()
		{
			return CanExecuteFirst();
		}

		public PositionableElementViewHelper ViewHelper { get; set; }

		protected NewPositionableElementMacroCommand(DiagramController diagramController) : base(diagramController) { }

		/// <summary>
		/// Perpares this command for execution.
		/// </summary>
		/// <param name="modelController">The ModelController, which will store this command in its undo/redo stacks</param>
		/// <param name="package">The Model Package, in which the class will be created</param>
		public abstract void Set(ModelController modelController, Package package);
	}
}