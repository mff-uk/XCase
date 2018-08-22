using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XCase.View.Geometries;
using XCase.View.Interfaces;

namespace XCase.View.Controls
{
	/// <summary>
	/// XCasePrimitiveJunction is a simple control that connects connectable element 
	/// (control implementing <see cref="IConnectable"/>) to an element implementing
	/// <see cref="IPrimitiveJunctionTarget"/> - these are usually non-rectangular controls
	/// such as associations and generalizations. Unlike <see cref="XCaseJunction"/>, 
	/// XCasePrimitiveJunction cannot be breaken to a polyline. 
	/// </summary>
	public class XCasePrimitiveJunction : Control 
	{
		/// <summary>
		/// Source element of the junction.
		/// </summary>
		/// <value><see cref="IConnectable"/></value>
		public IHasBounds SourceElement { get; private set; }

		/// <summary>
		/// Target element of the junction
		/// </summary>
		/// <value><see cref="IPrimitiveJunctionTarget"/></value>
		public IPrimitiveJunctionTarget TargetElement { get; private set; }

		/// <summary>
		/// Canvas where the control is placed
		/// </summary>
		/// <value><see cref="XCaseCanvas"/></value>
		public XCaseCanvas XCaseCanvas { get; private set; }

		private Point[] points = new Point[2];

		/// <summary>
		/// Pen used to draw the junction
		/// </summary>
		/// <value><see cref="Pen"/></value>
		public Pen Pen { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="XCasePrimitiveJunction"/> class.
		/// </summary>
		/// <param name="xCaseCanvas">canvas where the control is placed</param>
		/// <param name="sourceElement">source element</param>
		/// <param name="targetElement">target element</param>
		public XCasePrimitiveJunction(XCaseCanvas xCaseCanvas, IHasBounds sourceElement, IPrimitiveJunctionTarget targetElement)
		{
			SourceElement = sourceElement;
			TargetElement = targetElement;
			XCaseCanvas = xCaseCanvas;

			((FrameworkElement) sourceElement).SizeChanged += delegate { InvalidateVisual(); };
			((Control)sourceElement).LayoutUpdated += Source_LayoutUpdated;
			XCaseCanvas.Children.Add(this);
			Source_LayoutUpdated(null, null);
			Pen = MediaLibrary.SolidBlackPen;
		}

		void Source_LayoutUpdated(object sender, EventArgs e)
		{
			Rect bounds = SourceElement.GetBounds();

			Point p = TargetElement.FindClosestPoint(bounds.GetCenter());
			Point[] newPoints = JunctionGeometryHelper.ComputeOptimalConnection(bounds, new Rect(p.X, p.Y, 1, 1), false);

			if (newPoints[0] != points[0] || newPoints[1] != points[1])
			{
				points = newPoints;
				InvalidateVisual();
			}
		}

		/// <summary>
		/// Draws the line from <see cref="SourceElement"/> to <see cref="TargetElement"/> using <see cref="Pen"/>.
		/// </summary>
		/// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			if (points != null && points.Length == 2)
			{
				drawingContext.DrawLine(MediaLibrary.DashedBlackPen, points[0], points[1]);
			}
		}

		/// <summary>
		/// Deletes the control from canvas.
		/// </summary>
		public void DeleteFromCanvas()
		{
			XCaseCanvas.Children.Remove(this);
		}
	}
}
