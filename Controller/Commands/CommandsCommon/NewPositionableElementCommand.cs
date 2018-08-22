using XCase.UMLModel;

namespace XCase.UMLController.Commands
{
    /// <summary>
    /// Abstract Command for creating new elements that can be positioned 
    /// freely on the diagram (such as classes).
    /// </summary>
    public abstract class NewPositionableElementCommand : DiagramCommandBase
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
		
		public PositionableElementViewHelper ViewHelper { get; set; }

        protected NewPositionableElementCommand(DiagramController diagramController) : base(diagramController) { }
    }
}