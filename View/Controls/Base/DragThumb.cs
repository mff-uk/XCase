using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Interfaces;
using System;

namespace XCase.View.Controls
{
	/// <summary>
	/// 	<para>
	/// This control can be dragged on the canvas.
	/// Dragging issues <see cref="MoveElementCommand"/>s.
	/// </para>
	/// 	<para>
	/// Snappable controls can be snapped to DragThumb and DragThumbs can be snapped to
	/// other DragThumbs and other controls implementing <see cref="IReferentialElement"/>.
	/// </para>
	/// </summary>
	public class DragThumb : Control, ISnappable, IReferentialElement
	{ 
		/// <summary>
		/// Initializes a new instance of the <see cref="DragThumb"/> class.
		/// </summary>
		/// <param name="xCaseCanvas">canvas where the control is created</param>
		public DragThumb(XCaseCanvas xCaseCanvas)
		{
			XCaseCanvas = xCaseCanvas;
			//Placement = EPlacementKind.AbsoluteCanvas;
		}

		/// <summary>
		/// Canvas where the control is placed
		/// </summary>
		/// <value><see cref="XCaseCanvas"/></value>
		public XCaseCanvas XCaseCanvas { get; set; }

		private EPlacementKind placement;

		/// <summary>
		/// Placement defines how the CanvasPosition is computed.
		/// </summary>
		/// <value><see cref="EPlacementKind"/></value>
		public EPlacementKind Placement
		{
			get { return placement; }
			internal set { placement = value; }
		}

		private bool movable = true;

		/// <summary>
        /// Tells whether this element can be moved or not (PSM/PIM Diagram difference).
        /// Default value is true.
        /// </summary>
		public bool Movable
		{
			get
			{
				return movable;
			}
			set
			{
				movable = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this control can be dragged when selected in group.
		/// If false, the control can be dragged only when it is the only control selected
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can be dragged in group; otherwise, <c>false</c>.
		/// </value>
		public virtual bool CanBeDraggedInGroup
		{
			get
			{
				return (Placement == EPlacementKind.AbsoluteCanvas);
			}
		}

		/// <summary>
		/// Parent canvas
		/// </summary>
		/// <value><see cref="Canvas"/></value>
		protected Canvas Canvas
		{
			get { return Parent as Canvas; }
		}

		private bool IsDragging { get; set; }

		private double x;

		/// <summary>
		/// X coordinate of the contro
		/// </summary>
		/// <value><see cref="Double"/></value>
		public virtual double X
		{
			get { return x; }
			set
			{
				if (value != x || Double.IsNaN(Left))
				{
					x = value;
					UpdatePos(this);
				}
			}
		}

		private double y;

		/// <summary>
		/// Y coordinate of the control
		/// </summary>
		public virtual double Y
		{
			get { return y; }
			set
			{
				if (value != y || Double.IsNaN(Top))
				{
					y = value;
					UpdatePos(this);
				}
			}
		}

		/// <summary>
		/// Position of the control (<see cref="X"/> and <see cref="Y"/> joined)
		/// </summary>
		public Point Position
		{
			get
			{
				return new Point(X, Y);
			}
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}

		#region top left bottom right

		/// <summary>
		/// Gets the y coordiante for the top of the control
		/// </summary>
		/// <value><see cref="Double"/></value>
		public virtual double Top
		{
			get { return Canvas.GetTop(this); }
			//set { Canvas.SetTop(this, value); }
		}

		/// <summary>
		/// Gets the x coordiante for the left of the control
		/// </summary>
		/// <value><see cref="Double"/></value>
		public virtual double Left
		{
			get { return Canvas.GetLeft(this); }
			//set { Canvas.SetLeft(this, value); }
		}
		
		/// <summary>
		/// Gets the y coordiante for the bottom of the control
		/// </summary>
		/// <value><see cref="Double"/></value>
		public virtual double Bottom
		{
			get { return Canvas.GetTop(this) + this.ActualHeight; }
			//set { Canvas.SetTop(this, value - this.ActualHeight); }
		}

		/// <summary>
		/// Gets the x coordiante for the right of the control
		/// </summary>
		/// <value><see cref="Double"/></value>
		public virtual double Right
		{
			get { return Canvas.GetLeft(this) + this.ActualWidth; }
			//set { Canvas.SetLeft(this, value - this.ActualWidth); }
		}
		#endregion

		/// <summary>
		/// Occurs when position of the control changed.
		/// </summary>
		public event Action PositionChanged;

		public event Action Dropped; 

		/// <summary>
		/// Invokes the PositionChanged event
		/// </summary>
		protected virtual void InvokePositionChanged()
		{
			Action positionChangedAction = PositionChanged;
			if (positionChangedAction != null) positionChangedAction();
		}

		/// <summary>
		/// Updates the x and y coordinates of an element
		/// </summary>
		/// <param name="element">The element whose position is updated</param>
		internal static void UpdatePos(ISnappable element)
		{
			if (element is DragThumb)
			{
				DragThumb thumb = (DragThumb)element;
				double _x = Canvas.GetLeft(thumb);
				double _y = Canvas.GetTop(thumb);
				switch (thumb.Placement)
				{
					case EPlacementKind.AbsoluteCanvas:
					case EPlacementKind.ParentAutoPos:
					case EPlacementKind.AbsoluteSubCanvas:
						Canvas.SetLeft(thumb, thumb.X);
						Canvas.SetTop(thumb, thumb.Y);
						break;
					case EPlacementKind.RelativeCanvas:
						if (thumb.ReferentialElement != null)
						{
							Canvas.SetLeft(thumb, thumb.X + thumb.ReferentialElement.CanvasPosition.X);
							Canvas.SetTop(thumb, thumb.Y + thumb.ReferentialElement.CanvasPosition.Y);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				if (_x != Canvas.GetLeft(thumb) || _y != Canvas.GetTop(thumb))
				{
					((DragThumb)element).InvokePositionChanged();
					((DragThumb)element).FellowTravellersUpdate();
				}
			}
			else
			{
				Canvas.SetLeft((UIElement)element, element.SnapOffsetX + element.ReferentialElement.CanvasPosition.X);
				Canvas.SetTop((UIElement)element, element.SnapOffsetY + element.ReferentialElement.CanvasPosition.Y);
			}
		}

		/// <summary>
		/// Position in the coordinate system of the <see cref="XCaseCanvas"/>.
		/// The value depends on <see cref="Placement"/>.
		/// </summary>
		/// <seealso cref="Placement"/>
		/// <seealso cref="EPlacementKind"/>
		/// <value><see cref="Point"/></value>
		public Point CanvasPosition
		{
			get
			{
				switch (Placement)
				{
					case EPlacementKind.AbsoluteCanvas:
						return Position;
					case EPlacementKind.RelativeCanvas:
						return new Point(ReferentialElement.CanvasPosition.X + X, ReferentialElement.CanvasPosition.Y + Y);
					case EPlacementKind.AbsoluteSubCanvas:
					case EPlacementKind.ParentAutoPos:
						return new Point(Canvas.GetLeft(ParentControl) + X, Canvas.GetTop(ParentControl) + Y);
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		/// <summary>
		/// Returns bounding rectangle of the control
		/// </summary>
		/// <returns></returns>
		public virtual Rect GetBounds()
		{
			Rect r1 = new Rect
						  {
							  Y = Math.Round(Canvas.GetTop(this)),
							  X = Math.Round(Canvas.GetLeft(this)),
							  Height = Math.Round(ActualHeight),
							  Width = Math.Round(ActualWidth)
						  };
			
			return r1;
		}

		private Point DragStartPoint { get; set; }

		private Point PrevPoint { get; set; }

		/// <summary>
		/// Position of the mouse cursor during drag.
		/// </summary>
		internal Point MousePoint { get; private set; }

		/// <summary>
		/// Initializes dragging
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. This event data reports details about the mouse button that was pressed and the handled state.</param>
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left && Movable && e.ClickCount != 2)
			{
				DragStartPoint = e.GetPosition(Canvas);
				PrevPoint = DragStartPoint;
				MousePoint = new Point(e.GetPosition(Canvas).X - e.GetPosition(this).X,
									   e.GetPosition(Canvas).Y - e.GetPosition(this).Y);
				IsDragging = true;
				CaptureMouse();
				base.OnMouseDown(e);
				e.Handled = true;
				DragStarted();
			}
		}

		/// <summary>
		/// Proceeds with dragging.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (IsDragging && Movable)
			{
				Point newPoint = e.GetPosition(Canvas);
				Vector delta = newPoint - PrevPoint; 
				PrevPoint = newPoint;
				MousePoint += delta;
				this.DragDelta(new DragDeltaEventArgs(delta.X, delta.Y));
			}
			base.OnMouseMove(e);
		}

		/// <summary>
		/// Ends dragging
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the mouse button was released.</param>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				IsDragging = false;
				ReleaseMouseCapture();
				base.OnMouseUp(e);
				e.Handled = true;
				DragCompleted(PrevPoint, PrevPoint - DragStartPoint);
			}
		}

		/// <summary>
		/// Cancels dragging.
		/// </summary>
		public virtual void CancelDrag()
		{
			IsDragging = false;
			ReleaseMouseCapture();
		}

		private VisualAidsAdorner visualAidsAdorner = null;

		/// <summary>
		/// Returns true when the control is the only control dragged.
		/// </summary>
		/// <param name="item">item.</param>
		/// <returns>Returns true when the control is the only control dragged.</returns>
		private bool DraggingAllone(ISelectable item)
		{
			return (XCaseCanvas.SelectedItems.Count == 1 && (item is DragThumb) && (item as DragThumb).AllowDragIfSelectedAlone);
		}

		/// <summary>
		/// Called when dragging starts, stores old positions of the dragged element
		/// </summary>
		protected virtual void DragStarted()
		{
			startPositions = new Dictionary<DragThumb, rPoint>();

			if (!(this is ISelectable) ||
				!XCaseCanvas.SelectedItems.Contains(this as ISelectable))
			{
				startPositions.Add(this, new rPoint(this.Left, this.Top));
			}
			else
			{
				foreach (ISelectable item in XCaseCanvas.SelectedItems)
				{
					if (item.CanBeDraggedInGroup || DraggingAllone(item))
					{
						DragThumb thumb = item as DragThumb;
						if (thumb != null)
						{
							startPositions.Add(thumb, new rPoint(thumb.Position) { tag = thumb.Placement });
						}
						else
						{
							throw new InvalidOperationException(
								string.Format("Dragging is implemented only for DragThumb and descendants. Object {0} of type {1} does not derive from DrugThumb. ", item, item.GetType()));
						}
					}
				}
			}

			if (this is IAlignable)
			{
				AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(XCaseCanvas);
				visualAidsAdorner = new VisualAidsAdorner(XCaseCanvas, this);
				adornerLayer.Add(visualAidsAdorner);
			}

		}

		/// <summary>
		/// Called during dragging. Checks whether dragging can proceed (not out of bounds) and 
		/// also calls virtual method <see cref="AdjustDrag"/> that allows subclasses of DragThumb
		/// to alter default dragging.
		/// </summary>
		/// <param name="deltaEventArgs">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
		protected virtual void DragDelta(DragDeltaEventArgs deltaEventArgs)
		{
			IEnumerable<ISelectable> draggedElements = XCaseCanvas.SelectedItems.Where(item => item is DragThumb && (item.CanBeDraggedInGroup || DraggingAllone(item)));
			if (draggedElements.Count() > 0)
			{
				double minLeft = draggedElements.Min(item => item.GetBounds().Left);
				double minTop = draggedElements.Min(item => item.GetBounds().Top);

				double deltaHorizontal = Math.Max(-minLeft, deltaEventArgs.HorizontalChange);
				double deltaVertical = Math.Max(-minTop, deltaEventArgs.VerticalChange);
				deltaEventArgs = new DragDeltaEventArgs(deltaHorizontal, deltaVertical);

				AdjustDrag(ref deltaEventArgs);

				foreach (DragThumb element in draggedElements)
				{
					element.x = element.x + deltaEventArgs.HorizontalChange;
					element.y = element.y + deltaEventArgs.VerticalChange;
					if (element.Placement == EPlacementKind.ParentAutoPos)
						element.Placement = EPlacementKind.AbsoluteSubCanvas;
					UpdatePos(element);
				}
				XCaseCanvas.InvalidateMeasure();
				if (visualAidsAdorner != null)
				{
					visualAidsAdorner.d = deltaEventArgs;
					visualAidsAdorner.InvalidateVisual();
				}
			}
		}

		/// <summary>
		/// Drags the completed.
		/// </summary>
		/// <param name="finalPoint">The final point.</param>
		/// <param name="totalShift">The total shift.</param>
		protected virtual void DragCompleted(Point finalPoint, Vector totalShift)
		{
			if (visualAidsAdorner != null)
			{
				AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(XCaseCanvas);
				adornerLayer.Remove(visualAidsAdorner);
				visualAidsAdorner = null;
			}


			if (DragStartPoint != finalPoint)
			{
				DiagramController controller = XCaseCanvas.Controller;

				MacroCommand<DiagramController> moveMacroCommand =
					MacroCommandFactory<DiagramController>.Factory().Create(controller);
				moveMacroCommand.Description = CommandDescription.MOVE_MACRO;

				JunctionPointCommand.PointMoveDataDictionary pointMoveDataCollection = null;

				foreach (KeyValuePair<DragThumb, rPoint> pair in startPositions)
				{
					if (pair.Key is IAlignable)
					{
						IAlignable element = (IAlignable)pair.Key;

						DragThumb dragThumb = element as DragThumb;

						double _x;
						double _y;
						if (dragThumb != null && dragThumb.Placement == EPlacementKind.RelativeCanvas)
						{
							_x = dragThumb.Left - dragThumb.ReferentialElement.CanvasPosition.X;
							_y = dragThumb.Top - dragThumb.ReferentialElement.CanvasPosition.Y;
						}
						else
						{
							_x = element.Left;
							_y = element.Top;
						}

						CommandBase command = ViewController.CreateMoveCommand(
								_x,
								_y,
								element.ViewHelper,
								controller);
						moveMacroCommand.Commands.Add(command);
					}
					else if (pair.Key is JunctionPoint)
					{
						JunctionPoint junctionPoint = (JunctionPoint)pair.Key;

						JunctionPointCommand.PointMoveData data = new JunctionPointCommand.PointMoveData
																	  {
																		  Index = junctionPoint.OrderInJunction,
																		  OldPosition = pair.Value,
																		  NewPosition = new rPoint(junctionPoint.Position) { tag = junctionPoint.Placement },
																	  };

						if (pointMoveDataCollection == null)
							pointMoveDataCollection = new JunctionPointCommand.PointMoveDataDictionary();

						if (!pointMoveDataCollection.ContainsKey(junctionPoint.Junction.viewHelperPointsCollection))
						{
							pointMoveDataCollection[junctionPoint.Junction.viewHelperPointsCollection] = new List<JunctionPointCommand.PointMoveData>();
						}
						pointMoveDataCollection[junctionPoint.Junction.viewHelperPointsCollection].Add(data);
					}
				}

				// add one command for each affected junction 
				if (pointMoveDataCollection != null)
				{
					JunctionPointCommand junctionPointCommand = (JunctionPointCommand)JunctionPointCommandFactory.Factory().Create(controller);
					junctionPointCommand.Action = JunctionPointCommand.EJunctionPointAction.MovePoints;
					junctionPointCommand.PointMoveDataCollection = pointMoveDataCollection;
					junctionPointCommand.Description = CommandDescription.MOVE_JUNCTION_POINTS;
					moveMacroCommand.Commands.Add(junctionPointCommand);
				}

				moveMacroCommand.Execute();

				if (Dropped != null)
					Dropped();
			}
		}

		/// <summary>
		/// Returns 
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that one or more mouse buttons were pressed.</param>
		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);

			if (XCaseCanvas != null && this is ISelectable)
			{
				XCaseCanvas.SelectableItemPreviewMouseDown(this as ISelectable, e);
			}
		}

		#region snapy

		/// <summary>
		/// AdjustDrag is called when DragThumb is dragged via mouse upon its canvas. Inheriting classes are free
		/// to modify <paramref name="deltaEventArgs"/> and so alter the position of the DragThumb after drag. Override 
		/// this method to implement snapping, aligning etc.
		/// </summary>
		/// <example>
		/// <see cref="JunctionPoint"/> overrides AdjustDrag to make it snapped to the borders of its provider. 
		/// </example>
		/// <param name="deltaEventArgs"></param>
		protected virtual void AdjustDrag(ref DragDeltaEventArgs deltaEventArgs)
		{
			//if (this is IAlignable && visualAidsAdorner != null && visualAidsAdorner.adjustmentToSnap != null)
			//{
			//    Vector adj = visualAidsAdorner.adjustmentToSnap.Value;
			//    //deltaEventArgs = new DragDeltaEventArgs(adj.X, adj.Y);
			//    if (Math.Abs(adj.X) > VisualAidsAdorner.ADJUSTMENT_RATIO || Math.Abs(adj.Y) > VisualAidsAdorner.ADJUSTMENT_RATIO)
			//    {
			//        visualAidsAdorner.InvalidateVisual();
			//    }
			//    else
			//    {
			//        deltaEventArgs = new DragDeltaEventArgs(adj.X, adj.Y);
			//    }
			//}
		}

		/// <summary>
		/// Checks, whether <paramref name="point"/> lies within snap <paramref name="ratio"/>
		/// of the <paramref name="refPoint"/>
		/// </summary>
		/// <param name="point">point</param>
		/// <param name="refPoint">point to which <paramref name="point"/> should be snapped</param>
		/// <param name="ratio">snap ratio around <paramref name="refPoint"/></param>
		/// <param name="diff">distance between <paramref name="point"/> and <paramref name="refPoint"/></param>
		/// <returns><code>true</code> if point lies within <paramref name="ratio"/> around <paramref name="refPoint"/>, <code>false</code> otherwise</returns>
		protected static bool ShouldSnap(double point, double refPoint, double ratio, out double diff)
		{
			diff = refPoint - point;
			return Math.Abs(diff) < ratio;
		}

		private IReferentialElement referentialElement;

		/// <summary>
		/// Control that the element is snapped to.
		/// </summary>
		/// <value>control implementing <see cref="IReferentialElement"/></value>
		public IReferentialElement ReferentialElement
		{
			get { return referentialElement; }
			set { referentialElement = value; UpdatePos(this); }
		}

		private Control parentControl;

		/// <summary>
		/// Control that owns this control (see <see cref="EPlacementKind.AbsoluteSubCanvas"/> 
		/// and <see cref="EPlacementKind.ParentAutoPos"/>)
		/// </summary>
		/// <value><see cref="Control"/></value>
		internal Control ParentControl
		{
			get { return parentControl; }
			set
			{
				if (parentControl != null)
				{
					parentControl.LayoutUpdated -= parentControl_LayoutUpdated;
				}
				parentControl = value;
				parentControl.LayoutUpdated += parentControl_LayoutUpdated;
				UpdatePos(this);

			}
		}

		private Point parentLastPos = new Point();

		private void parentControl_LayoutUpdated(object sender, EventArgs e)
		{
			Point parentPos = new Point(Canvas.GetLeft(ParentControl), Canvas.GetTop(ParentControl));
			if (parentPos != parentLastPos)
			{
				InvokePositionChanged();
				FellowTravellersUpdate();
				parentLastPos = parentPos;
			}
		}

		private List<ISnappable> _fellowTravellers;

		private Dictionary<DragThumb, rPoint> startPositions;

		/// <summary>
		/// List of controls that move with this controll
		/// </summary>
		public IList<ISnappable> FellowTravellers
		{
			get
			{
				if (_fellowTravellers == null)
					_fellowTravellers = new List<ISnappable>();
				return _fellowTravellers;
			}
		}

		/// <summary>
		/// Returns true if the control can be dragged when it 
		/// is the only control selected. Default is true, 
		/// can be overriden in subclasses. 
		/// </summary>
		public virtual bool AllowDragIfSelectedAlone
		{
			get { return true; }
		}

		/// <summary>
		/// Updates positiosn of <see cref="FellowTravellers"/>
		/// </summary>
		private void FellowTravellersUpdate()
		{
			if (FellowTravellers != null)
			{
				foreach (ISnappable element in FellowTravellers)
				{
					UpdatePos(element);
					if (element is DragThumb)
						((DragThumb)element).FellowTravellersUpdate();
				}
			}
		}

		/// <summary>
		/// Snaps the <paramref name="element"/> to this control
		/// </summary>
		/// <param name="element">snapped element</param>
		/// <param name="recalcPosition">if set to <c>true</c> offset is recalculated from current position.</param>
		public void SnapElementToThumb(ISnappable element, bool recalcPosition)
		{
			if (element is DragThumb)
				((DragThumb)element).SnapTo(this, recalcPosition);
			else
			{
				this.FellowTravellers.Add(element);
				element.ReferentialElement = this;

				element.SnapOffsetX = Canvas.GetLeft((UIElement)element) - this.CanvasPosition.X;
				element.SnapOffsetY = Canvas.GetTop((UIElement)element) - this.CanvasPosition.Y;
				UpdatePos(element);
			}
		}

		/// <summary>
		/// Snaps this control to <paramref name="referentialElement"/>. 
		/// </summary>
		/// <param name="referentialElement">the control that this element is snapped to</param>
		/// <param name="recalcPosition">if set to <c>true</c> offset is recalculated from current position.</param>
		public void SnapTo(IReferentialElement referentialElement, bool recalcPosition)
		{
			ReferentialElement = referentialElement;
			referentialElement.FellowTravellers.Add(this);
			Placement = EPlacementKind.RelativeCanvas;
			UIElement _uie = referentialElement as UIElement;
			if (recalcPosition && _uie != null)
			{
				this.x = Canvas.GetLeft(this) - Canvas.GetLeft(_uie);
				this.x = !double.IsNaN(x) ? x : 0;
				this.y = Canvas.GetTop(this) - Canvas.GetTop(_uie);
				this.y = !double.IsNaN(y) ? y : 0;
			}
			UpdatePos(this);
		}

		/// <summary>
		/// Unsnaps the element from its referential element
		/// </summary>
		/// <param name="element">The element which is unsnapped</param>
		public static void UnsnapElement(ISnappable element)
		{
			element.ReferentialElement.FellowTravellers.Remove(element);
			element.ReferentialElement = null;
			if (element is DragThumb)
			{
				((DragThumb)element).Placement = EPlacementKind.AbsoluteCanvas;
				UpdatePos(element);
			}
		}

		/// <summary>
		/// X coordinate of the element in the coordinate system of <see cref="ReferentialElement"/>/
		/// </summary>
		/// <value><see cref="Double"/></value>
		public double SnapOffsetX
		{
			get { return X; }
			set { X = value; }
		}

		/// <summary>
		/// Y coordinate of the element in the coordinate system of <see cref="ReferentialElement"/>/
		/// </summary>
		/// <value><see cref="Double"/></value>
		public double SnapOffsetY
		{
			get { return Y; }
			set { Y = value; }
		}

		#endregion

		/// <summary>
		/// Gets the visual aids points. These points are used in <see cref="VisualAidsAdorner"/> to 
		/// draw lines between elements when elements are dragged. 
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Point> GetVisualAidsPoints()
		{
			Rect bounds = GetBounds();
			return new Point[] { bounds.TopLeft, bounds.TopRight, bounds.BottomLeft, bounds.BottomRight };
		}

		#if DEBUGBORDERS
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			drawingContext.DrawRectangle(null, MediaLibrary.DashedBlackPen, GetBounds().Normalize());
			drawingContext.DrawText(new FormattedText(String.Format("TL: {0}, BR: {1}", GetBounds().TopLeft, GetBounds().BottomRight), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10, Brushes.Black), new Point(0, 0));
		}
		#endif
	}
}
