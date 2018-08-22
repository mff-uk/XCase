//#define showlabels

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XCase.View.Geometries;
using XCase.View.Interfaces;
using System.Windows.Controls.Primitives;

namespace XCase.View.Controls
{
	/// <summary>
	/// This cotrol represents one point in a junction. Points can be dragged.
	/// Junction is drawn as a polyline connecting its JunctionPoints. 
	/// </summary>
	public class JunctionPoint : DragThumb, ISelectable
	{
		/// <summary>
		/// Element that owns this control. Equals null when the hook was created directly 
		/// and not using <see cref="IConnectable.CreateJunctionEnd()"/>.
		/// </summary>
		public IConnectable OwnerControl { get; set; }

		/// <summary>
		/// Junction where this point belongs to
		/// </summary>
		/// <value><see cref="XCaseJunction"/></value>
		public XCaseJunction Junction { get; set; }

		/// <summary>
		/// Order of the point in <see cref="Junction"/>
		/// </summary>
		/// <value><see cref="Int32"/>, <code>0</code> for the first point in junction</value>
		public int OrderInJunction { get; set; }

#if showlabels
        /// <summary>
        /// Shows point position in a label, must be compiled with showlabels directive
        /// </summary>
        private SnappableLabel l;
#endif

		/// <summary>
		/// Moves the point to specified location.
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		public void SetPreferedPosition(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		/// <summary>
		/// Moves the point to specified location.
		/// </summary>
		/// <param name="preferedPosition">new location</param>
		public void SetPreferedPosition(Point preferedPosition)
		{
			SetPreferedPosition(preferedPosition.X, preferedPosition.Y);
		}

		#region ISelectable Members

		private bool isSelected;

		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				isSelected = value;
				Opacity = value ? MediaLibrary.PointOpacityHover : MediaLibrary.PointOpacityNormal;
			}
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="JunctionPoint"/> class.
		/// </summary>
		/// <param name="xCaseCanvas">Canvas where the point is created.</param>
		public JunctionPoint(XCaseCanvas xCaseCanvas)
			: base(xCaseCanvas)
		{
			Width = 0;
			Height = 0;
			OrderInJunction = -1;
			Background = Brushes.Gold;

			Opacity = MediaLibrary.PointOpacityNormal;
			Cursor = Cursors.Hand;

			ContextMenu m = new ContextMenu();
			m.Items.Add(new ContextMenuItem("Straighten line here"));
			((ContextMenuItem)m.Items[0]).Click += Remove_click;

			ContextMenu = m;

			this.SetValue(TemplateProperty, null);
		}

		/// <summary>
		/// Removes the point from the junction.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void Remove_click(object sender, EventArgs e)
		{
			if (Junction != null && Junction.Association != null)
				Junction.Association.Controller.RemoveBreakPoint(OrderInJunction, Junction.viewHelperPointsCollection);
			if (Junction != null && Junction.Generalization != null)
				Junction.Generalization.Controller.RemoveBreakPoint(OrderInJunction, Junction.viewHelperPointsCollection);
		}

		/// <summary>
		/// OnMouseEnter/OnMouseLeave highlight/unhighlight the point under the mouse cursor.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);
			if (!IsSelected)
				Opacity = MediaLibrary.PointOpacityHover;
		}

		/// <summary>
		/// OnMouseEnter/OnMouseLeave highlight/unhighlight the point under the mouse cursor.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			if (!IsSelected && !Junction.IsSelected)
				Opacity = MediaLibrary.PointOpacityNormal;
		}

	    public void BringUp()
	    {
            if (!IsSelected)
                Opacity = MediaLibrary.PointOpacityHover;
            Canvas.SetZIndex(this, Canvas.GetZIndex(Junction) + 1);
            Visibility = Visibility.Visible;
	    }

        public void CancelBringUp()
        {
            if (!IsSelected)
                Opacity = MediaLibrary.PointOpacityNormal;
        }

		/// <summary>
		/// Ellipse radius (used in <see cref="OnRender"/>).
		/// </summary>
		private const double radius = 5;

		/// <summary>
		/// Draws an ellipse around the point. 
		/// </summary>
		/// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			drawingContext.DrawEllipse(Background, null, new Point(0, 0), radius, radius);
		}

		/// <summary>
		/// Invokes the PositionChanged event
		/// </summary>
		protected override void InvokePositionChanged()
		{
			base.InvokePositionChanged();

			if (Junction != null && Junction.Parent != null)
				Junction.InvalidateGeometry();

			#region update debugging label
#if showlabels
            if (Parent != null && Parent is Canvas)
            {
                if (l == null)
                {

                    l = new SnappableLabel { FontSize = 8 };
                    XCaseCanvas.Children.Add(l);
                    Canvas.SetLeft(l, this.CanvasPosition.X - 20);
                    Canvas.SetTop(l, this.CanvasPosition.Y + 10);
                    this.SnapElementToThumb(l);

                    l.Visibility = Visibility.Visible;
                }
                l.Content = this.ToString();
            
            }
#endif
			#endregion

			InvalidateVisual();
		}

		/// <summary>
		/// Override of <see cref="DragThumb.AdjustDrag" /> aligns dragged point to the borders of its <see cref="OwnerControl"/>
		/// </summary>
		/// <param name="deltaEventArgs"></param>
		protected override void AdjustDrag(ref DragDeltaEventArgs deltaEventArgs)
		{
			base.AdjustDrag(ref deltaEventArgs);

			if (OwnerControl != null)
			{
				Rect bounds = OwnerControl.GetBounds();

				Point p = new Point(CanvasPosition.X + deltaEventArgs.HorizontalChange, CanvasPosition.Y + deltaEventArgs.VerticalChange);
				Point snapped = bounds.SnapPointToRectangle(p);

				deltaEventArgs = new DragDeltaEventArgs(snapped.X - CanvasPosition.X, snapped.Y - CanvasPosition.Y);
			}
			else
			{
				JunctionPoint rightNeighbour = Junction.Points.ElementAtOrDefault(OrderInJunction + 1);
				JunctionPoint leftNeighbour = Junction.Points.ElementAtOrDefault(OrderInJunction - 1);

				const double SNAP_RATIO = 14;

				foreach (JunctionPoint neighbour in new[] { leftNeighbour, rightNeighbour })
				{
					bool isEndPoint = neighbour == Junction.StartPoint || neighbour == Junction.EndPoint;
					if (neighbour != null && (!isEndPoint || neighbour.Placement == EPlacementKind.AbsoluteSubCanvas))
					{
						double nx = neighbour.CanvasPosition.X;
						double ny = neighbour.CanvasPosition.Y;
						double deltax = deltaEventArgs.HorizontalChange;
						double deltay = deltaEventArgs.VerticalChange;
						double diff;
						if (ShouldSnap(MousePoint.Y, ny, SNAP_RATIO, out diff))
						{
							ShouldSnap(CanvasPosition.Y + deltaEventArgs.VerticalChange, ny, SNAP_RATIO, out diff);
							deltay += diff;
							deltaEventArgs = new DragDeltaEventArgs(deltax, deltay);
						}
						else if (ShouldSnap(MousePoint.X, nx, SNAP_RATIO, out diff))
						{
							ShouldSnap(CanvasPosition.X + deltaEventArgs.HorizontalChange, nx, SNAP_RATIO, out diff);
							deltax += diff;
							deltaEventArgs = new DragDeltaEventArgs(deltax, deltay);
						}
					}
				}
			}

		}

		/// <summary>
		/// Returns the string representation of a <see cref="T:System.Windows.Controls.Control"/> object.
		/// </summary>
		/// <returns>A string that represents the control.</returns>
		public override string ToString()
		{
			return String.Format("SPH: rel: [{0:0},{1:0}] abs [{2:0},{3:0}] #{4}", X, Y, CanvasPosition.X, CanvasPosition.Y, OrderInJunction);
		}

		/// <summary>
		/// Deletes control from canvas.
		/// </summary>
		public virtual void DeleteFromCanvas()
		{
			if (Parent is Canvas)
				(Parent as Canvas).Children.Remove(this);
		}
	}
}
