using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using XCase.View.Geometries;
using XCase.View.Interfaces;
using System.Collections.Generic;
using System;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace XCase.View.Controls
{
	/// <summary>
	/// This is the adorner used for selecting items in canvas
	/// </summary>
    public class RubberbandAdorner : Adorner
    {
        private Point? startPoint, endPoint;
        private Rectangle rubberband;
        private VisualCollection visuals;
        private Canvas adornerCanvas;
        private readonly XCaseCanvas xCaseCanvas;

        protected override int VisualChildrenCount
        {
            get
            {
                return this.visuals.Count;
            }
        }


		/// <summary>
		/// Initializes a new instance of the <see cref="RubberbandAdorner"/> class.
		/// </summary>
		/// <param name="xCaseCanvas">The designer canvas.</param>
		/// <param name="dragStartPoint">The drag start point.</param>
        public RubberbandAdorner(XCaseCanvas xCaseCanvas, Point? dragStartPoint)
            : base(xCaseCanvas)
        {
            this.xCaseCanvas = xCaseCanvas;
            this.startPoint = dragStartPoint;
			this.endPoint = dragStartPoint;

            this.adornerCanvas = new Canvas();
            this.adornerCanvas.Background = Brushes.Transparent;
            this.visuals = new VisualCollection(this);
            this.visuals.Add(this.adornerCanvas);

            this.rubberband = new Rectangle();
            this.rubberband.Stroke = Brushes.Navy;
            this.rubberband.StrokeThickness = 1;
            this.rubberband.StrokeDashArray = new DoubleCollection(new double[] { 2 });

            this.adornerCanvas.Children.Add(this.rubberband);
        }

		/// <summary>
		/// Updates selection according to new cursor position.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                endPoint = e.GetPosition(this);
                UpdateRubberband();
                //UpdateSelection();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }

    	readonly List<ISelectable> newSelection = new List<ISelectable>();
    	readonly List<ISelectable> removedFromSelection = new List<ISelectable>();

        protected override Visual GetVisualChild(int index)
        {
            return this.visuals[index];
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            this.adornerCanvas.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        /// <summary>
		/// Updates the selection.
		/// </summary>
        private void UpdateSelection()
        {
            newSelection.Clear();
            removedFromSelection.Clear();

            Rect rubberBand = new Rect(startPoint.Value, endPoint.Value);
			foreach (UIElement item in xCaseCanvas.Children)
            {
                if (item is ISelectable)
                {
                    //get this items bounds
                    Rect itemRect = VisualTreeHelper.GetDescendantBounds(item as Control);
                    Rect itemBounds = (item is IHasSelectionBounds) ? ((IHasSelectionBounds)item).GetSelectionBounds() : ((ISelectable)item).GetBounds();
                    if (rubberBand.IntersectsWith(itemBounds))
                    {
                        if (!(item as ISelectable).IsSelected)
                        {
                            newSelection.Add(item as ISelectable);
                        }
                    }
                    else
                    {
                        if ((item as ISelectable).IsSelected)
                        {
                            XCaseJunction j = item as XCaseJunction;
                            if (j != null)
                            {
                                if (j.SelectionOwner != null)
                                {
                                    Rect ownerBounds = (j.SelectionOwner is IHasSelectionBounds) ? ((IHasSelectionBounds)j.SelectionOwner).GetSelectionBounds() : j.SelectionOwner.GetBounds();
                                    if (!ownerBounds.IntersectsWith(rubberBand))
                                    {
                                        removedFromSelection.Add(item as ISelectable);
                                    }
                                }
                            }
                            else
                            {
                                if (item is PSMElementViewBase && ((PSMElementViewBase)item).Connector != null)
                                {
                                    Rect bounds = ((PSMElementViewBase)item).Connector.Junction.GetBounds();
                                    rubberBand.IntersectsWith(bounds);
                                }
                                else
                                {
                                    removedFromSelection.Add(item as ISelectable);
                                }
                            }
                        }
                    }
                }
            }
			
            foreach (ISelectable item in newSelection)
            {
				item.IsSelected = true;
                xCaseCanvas.SelectedItems.Add(item);

            }
            foreach (ISelectable item in removedFromSelection)
            {
				item.IsSelected = false; 
				xCaseCanvas.SelectedItems.Remove(item);
            }
        }

        private void UpdateRubberband()
        {
            double left = Math.Min(this.startPoint.Value.X, this.endPoint.Value.X);
            double top = Math.Min(this.startPoint.Value.Y, this.endPoint.Value.Y);

            double width = Math.Abs(this.startPoint.Value.X - this.endPoint.Value.X);
            double height = Math.Abs(this.startPoint.Value.Y - this.endPoint.Value.Y);

            this.rubberband.Width = width;
            this.rubberband.Height = height;
            Canvas.SetLeft(this.rubberband, left);
            Canvas.SetTop(this.rubberband, top);
        }

		/// <summary>
		/// Updates the selection and removes the adorner.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the mouse button was released.</param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            // remove adorner (=this) from adorner layer
            AdornerLayer adornerLayer = this.Parent as AdornerLayer;
            if (adornerLayer != null) 
            {
                UpdateSelection();
                adornerLayer.Remove(this);
            }

            e.Handled = true;
        }

		/*/// <summary>
		/// Draws the selection adordner (as a rectangle).
		/// </summary>
		/// <param name="dc">The dc.</param>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // without a background the OnMouseMove event would not be fired !
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (this.startPoint.HasValue && this.endPoint.HasValue)
                dc.DrawRectangle(Brushes.Transparent, MediaLibrary.SelectionRubberbandPen, new Rect(this.startPoint.Value, this.endPoint.Value));
        }*/

    }
}
