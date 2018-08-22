using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Geometries;
using XCase.View.Interfaces;

namespace XCase.View.Controls
{
	/// <summary>
	/// <see cref="XCaseJunction"/> is a simple object that can draw a line between two elements. 
	/// It can be a direct line or can be broken to a polyline. 
	/// Elements connected by <see cref="XCaseJunction"/> must implement <see cref="IConnectable"/> interface. 
	/// Each point on a junction is a separate control (<see cref="JunctionPoint"/>) 
	/// that can be dragged on the diagram. <see cref="XCaseJunction"/> connects the points. 
	/// </summary>
	public class XCaseJunction : Control, ISelectable, IPrimitiveJunctionTarget
	{
		/// <summary>
		/// Canvas where the control is placed.
		/// </summary>
		public XCaseCanvas XCaseCanvas { get; private set; }

		/// <summary>
		/// If the junction is a part of an association, this property
		/// holds reference to the association.
		/// </summary>
		/// <value><see cref="PIM_Association"/></value>
		public PIM_Association Association { get; set; }

		/// <summary>
		/// If the junction is a part of a generalization, this property
		/// holds reference to the generalization.
		/// </summary>
		/// <value><see cref="PIM_Generalization"/></value>
		public PIM_Generalization Generalization { get; set; }

		/// <summary>
		/// If the junction is a part of a psm association, this property
		/// holds reference to the psm association.
		/// </summary>
		/// <value><see cref="PSM_Association"/></value>
		public PSM_Association PSM_Association { get; set; }

		/// <summary>
		/// Reference to a collection of points (in ViewHelper)
		/// </summary>
		/// <value><see cref="ObservablePointCollection"/></value>
		internal ObservablePointCollection viewHelperPointsCollection { get; private set; }

		/// <summary>
		/// Creates new instance of <see cref="XCaseJunction" />. 
		/// </summary>
		/// <param name="xCaseCanvas">canvas where the control is placed</param>
		public XCaseJunction(XCaseCanvas xCaseCanvas)
		{
			this.XCaseCanvas = xCaseCanvas;

			Points = new List<JunctionPoint>();
			Fill = Brushes.White;
			Pen = MediaLibrary.SolidBlackPen;
			InitializeContextMenu();
		}

		/// <summary>
		/// Creates new instance of <see cref="XCaseJunction"/>.
		/// </summary>
		/// <param name="xCaseCanvas">canvas where the control is placed</param>
		/// <param name="viewHelperPointsCollection">Reference to a collection of points (in ViewHelper)</param>
		public XCaseJunction(XCaseCanvas xCaseCanvas, ObservablePointCollection viewHelperPointsCollection)
			: this(xCaseCanvas)
		{
			this.viewHelperPointsCollection = viewHelperPointsCollection;

			viewHelperPointsCollection.CollectionChanged += viewHelperPointsCollection_CollectionChanged;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XCaseJunction"/> class.
		/// </summary>
		/// <param name="xCaseCanvas">canvas where the control is placed</param>
		/// <param name="association">association that owns the junction</param>
		/// <param name="viewHelperPointsCollection">Reference to a collection of points (in ViewHelper)</param>
		public XCaseJunction(XCaseCanvas xCaseCanvas, PIM_Association association, ObservablePointCollection viewHelperPointsCollection) :
			this(xCaseCanvas, viewHelperPointsCollection)
		{
			this.Association = association;
			this.SelectionOwner = association;
			InitializeContextMenu();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XCaseJunction"/> class.
		/// </summary>
		/// <param name="xCaseCanvas">canvas where the control is placed</param>
		/// <param name="generalization">generalization that owns the junction</param>
		/// <param name="viewHelperPointsCollection">Reference to a collection of points (in ViewHelper)</param>
		public XCaseJunction(XCaseCanvas xCaseCanvas, PIM_Generalization generalization, ObservablePointCollection viewHelperPointsCollection) :
			this(xCaseCanvas, viewHelperPointsCollection)
		{
			this.Generalization = generalization;
			this.SelectionOwner = generalization;
			InitializeContextMenu();
		}

		/// <summary>
		/// Creates context menu items for the junction (if junction is a part
		/// of association or generalization, association or generalization context
		/// menu items are included)
		/// </summary>
		private void InitializeContextMenu()
		{
			ContextMenu menu = new ContextMenu();
			if (Association != null)
			{
				foreach (ContextMenuItem item in Association.AssociationMenuItems())
					menu.Items.Add(item);
			}
			if (Generalization != null)
			{
				foreach (ContextMenuItem item in Generalization.GeneralizationMenuItems())
					menu.Items.Add(item);
			}
			if (PSM_Association != null)
			{
				foreach (ContextMenuItem item in PSM_Association.PSM_AssociationMenuItems())
					menu.Items.Add(item);
			}

			bool sep = true;
			foreach (ContextMenuItem item in JunctionMenuItems())
			{
				if (sep)
				{
					menu.Items.Add(new Separator());
					sep = false;
				}
				menu.Items.Add(item);
			}
			this.ContextMenu = menu;
		}

		/// <summary>
		/// Returns context menu items for junction.
		/// </summary>
		/// <returns></returns>
		internal IEnumerable<ContextMenuItem> JunctionMenuItems()
		{
			if (!AutoPosModeOnly)
			{
				ContextMenuItem breakCommand = new ContextMenuItem("Break line here");
				breakCommand.Click += delegate
										{
											ViewController.BreakLine(downPos,
																	 JunctionGeometryHelper.FindHitSegmentIndex(downPos, Points) + 1,
																	 viewHelperPointsCollection, XCaseCanvas.Controller);
										};
				ContextMenuItem autoPosEndsCommand = new ContextMenuItem("Line ends auto-position");
				autoPosEndsCommand.Click += delegate
				                            	{
				                            		StartPoint.Placement = EPlacementKind.ParentAutoPos;
				                            		EndPoint.Placement = EPlacementKind.ParentAutoPos;
				                            		InvalidateGeometry();
				                            	};

				if (this.Association != null)
				{
					ContextMenuItem removeEndCommand = new ContextMenuItem("Remove from association");
					removeEndCommand.Click += delegate
					                          	{
					                          		Association.Controller.RemoveAssociationEnd(this.AssociationEnd.AssociationEnd);
					                          	};
					return new[] { breakCommand, autoPosEndsCommand, removeEndCommand };
				}
				
				return new[] { breakCommand, autoPosEndsCommand };
			}
			else return new ContextMenuItem[0];
		}

		/// <summary>
		/// First point of the junction
		/// </summary>
		public JunctionPoint StartPoint
		{
			get { return (Points.First()); }
		}

		public PIM_AssociationEnd AssociationEnd
		{
			get { return (PIM_AssociationEnd)Points.Last(); }
		}

		private bool autoPosModeOnly;

		/// <summary>
		/// When set to true, junctions cannot be broken to polylines,
		/// they can not be dragged and junction points are always 
		/// autopositioned.
		/// </summary>
		public bool AutoPosModeOnly
		{
			get
			{
				return autoPosModeOnly;
			}
			set
			{
				autoPosModeOnly = value;
				foreach (JunctionPoint junctionPoint in Points)
				{
					junctionPoint.Visibility = value ? Visibility.Hidden : Visibility.Visible;
					junctionPoint.Movable = !value;
					if (value)
						junctionPoint.Placement = EPlacementKind.ParentAutoPos;
				}
				InitializeContextMenu();
			}
		}

		/// <summary>
		/// Last point of the junction
		/// </summary>
		public JunctionPoint EndPoint
		{
			get
			{
				return Points.Last();
			}
		}

		/// <summary>
		/// Element connected by the junction to <see cref="TargetElement"/>.
		/// </summary>
		public Control SourceElement { get; internal set; }

		/// <summary>
		/// Element connected by the junction to <see cref="SourceElement"/>.
		/// </summary>
		public Control TargetElement { get; internal set; }

		private EJunctionCapStyle startCapStyle;
		/// <summary>
		/// Style of start cap (arrow, diamond etc.)
		/// </summary>
		/// <value><see cref="EJunctionCapStyle"/></value>
		public EJunctionCapStyle StartCapStyle
		{
			get { return startCapStyle; }
			set
			{
				startCapStyle = value;
				SelectProperFill();
				InvalidateGeometry();
			}
		}

		private EJunctionCapStyle endCapStyle;
		/// <summary>
		/// Style of end cap (arrow, diamond etc.)
		/// </summary>
		/// <value><see cref="EJunctionCapStyle"/></value>
		public EJunctionCapStyle EndCapStyle
		{
			get { return endCapStyle; }
			set
			{
				endCapStyle = value;
				SelectProperFill();
				InvalidateGeometry();
			}
		}

		/// <summary>
		/// List of points of the junction, junction is drawn as a polyline
		/// connecting these points. Each point is a separate control, that can
		/// be selected and dragged.
		/// </summary>
		/// <value><see cref="List{JunctionPoint}"/></value>
		public List<JunctionPoint> Points { get; set; }

		#region ISelectable Members

		/// <summary>
		/// <para>
		/// Returns false since junctions can be selected but not dragged in a group
		/// </para>
		/// <para>
		/// It is usually handy to be able to drag an object in a group, but not desirable for those
		/// objects whose position is determined by position of other objects (like junctions and
		/// some SnapPointHooks).
		/// </para>
		/// </summary>
		/// <value></value>
		public bool CanBeDraggedInGroup
		{
			get { return false; }
		}

		/// <summary>
		/// Junctions are usually parts of other controls (i.e. associations). This field
		/// holds the referenc to the owning control.
		/// </summary>
		/// <value><see cref="ISelectable"/></value>
		public ISelectable SelectionOwner { get; set; }

		private bool isSelected = false;

		/// <summary>
		/// Selected flag. Selected elements are highlighted on the canvas and are
		/// target of commands.
		/// </summary>
		/// <value></value>
		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				isSelected = value;
				if (SelectionOwner != null && SelectionOwner.IsSelected != value)
					SelectionOwner.IsSelected = value;
                if (!AutoPosModeOnly && value)
                {
                    foreach (JunctionPoint point in Points)
                    {
                        point.BringUp();
                    }
                }
                if (!AutoPosModeOnly && !value)
                {
                    foreach (JunctionPoint point in Points)
                    {
                        point.CancelBringUp();
                    }
                }
				InvalidateVisual();
			}
		}

		/// <summary>
		/// Pen used to draw the junction
		/// </summary>
		/// <value><see cref="Pen"/></value>
		public Pen Pen { get; set; }

		/// <summary>
		/// Returns bounding rectangle of the element (rectangle containing all points of the junction). 
		/// </summary>
		/// <returns>Bounding rectangle</returns>
		public Rect GetBounds()
		{
			Point[] p = new Point[Points.Count()];
			int i = 0;
			foreach (JunctionPoint point in Points)
			{
				//p[i] = point.TranslatePoint(point.Position, XCaseCanvas);
				p[i] = point.CanvasPosition;
				i++;
			}
			return RectExtensions.GetEncompassingRectangle(p);
		}

		#endregion

		/// <summary>
		/// Fill for the junction cap (black is used for diamonds, white for triangle arrows..)
		/// </summary>
		private Brush Fill;

		/// <summary>
		/// Selects the proper fill for current cap styles. 
		/// </summary>
		protected void SelectProperFill()
		{
			if (StartCapStyle == EJunctionCapStyle.FullArrow || StartCapStyle == EJunctionCapStyle.FullDiamond
				|| EndCapStyle == EJunctionCapStyle.FullDiamond || EndCapStyle == EJunctionCapStyle.FullArrow)
				Fill = Brushes.Black;
			else
				Fill = Brushes.White;
		}

		private Geometry geometry;

		private bool sourceMeasureValid = false;

		private bool targetMeasureValid = false;

		/// <summary>
		/// Adjusts the end points position to an optimal position
		/// (when their <see cref="DragThumb.Placement"/> is set 
		/// to <see cref="EPlacementKind.ParentAutoPos"/>).
		/// </summary>
		public void AdjustEndPoints()
		{
			#region set source junctionEnd position

			double angle = SourceElement != null ? ((IConnectable)SourceElement).BoundsAngle : 0;
			if (AutoPosModeOnly && StartPoint.Placement != EPlacementKind.ParentAutoPos)
				StartPoint.Placement = EPlacementKind.ParentAutoPos;
			if (SourceElement != null && SourceElement.IsMeasureValid && (viewHelperPointsCollection == null || Points.Count == viewHelperPointsCollection.Count)
				&& (StartPoint.Placement == EPlacementKind.ParentAutoPos || viewHelperPointsCollection == null || viewHelperPointsCollection.PointsInvalid))
			{
				Rect r1 = JunctionGeometryHelper.GetFirstElementBounds(this);
				Rect r2 = JunctionGeometryHelper.GetFirstButOneElementBounds(this);

				Point p1 = JunctionGeometryHelper.RectangleRectangleCenterIntersection(r1, r2, true, angle);
				if (viewHelperPointsCollection == null)
				{
				    StartPoint.SetPreferedPosition(p1);
				}
				else if (!viewHelperPointsCollection.First().AlmostEqual(p1))
				{
					if (viewHelperPointsCollection.PointsInvalid)
					{
						viewHelperPointsCollection[0].Set(p1);
						StartPoint.SetPreferedPosition(p1);
						viewHelperPointsCollection.PointsInvalid = false;
					}
					else
					{
						if (!sourceMeasureValid)
						{
							if (p1 != StartPoint.Position)
							{
                                if (!AutoPosModeOnly)
                                {
                                    StartPoint.Placement = EPlacementKind.AbsoluteSubCanvas;
                                    Point snapped = r1.Normalize().SnapPointToRectangle(StartPoint.Position);
                                    if (snapped != StartPoint.Position)
                                        StartPoint.SetPreferedPosition(snapped);
                                }
							}
						}
						else
						{
							viewHelperPointsCollection[0].Set(p1);
							StartPoint.SetPreferedPosition(p1);
						}
					}
				}
                else
				{
                    StartPoint.SetPreferedPosition(p1);
                    StartPoint.Placement = EPlacementKind.ParentAutoPos;
				}
				sourceMeasureValid = true;
			}

			#endregion

			#region set end junctionEnd position

			angle = TargetElement != null ? ((IConnectable)TargetElement).BoundsAngle : 0;
			if (AutoPosModeOnly && EndPoint.Placement != EPlacementKind.ParentAutoPos)
				EndPoint.Placement = EPlacementKind.ParentAutoPos;
			if (TargetElement != null && TargetElement.IsMeasureValid && (viewHelperPointsCollection == null || Points.Count == viewHelperPointsCollection.Count)
				&& (EndPoint.Placement == EPlacementKind.ParentAutoPos || viewHelperPointsCollection == null || viewHelperPointsCollection.PointsInvalid))
			{
				Rect r1 = JunctionGeometryHelper.GetLastElementBounds(this);
				Rect r2 = JunctionGeometryHelper.GetLastButOneElementBounds(this);

				Point p2 = JunctionGeometryHelper.RectangleRectangleCenterIntersection(r1, r2, true, angle);
				if (viewHelperPointsCollection == null)
				{
				    EndPoint.SetPreferedPosition(p2);
				}
				else if (!viewHelperPointsCollection.Last().AlmostEqual(p2))
				{
					if (viewHelperPointsCollection.PointsInvalid)
					{
						viewHelperPointsCollection.Last().Set(p2);
						EndPoint.SetPreferedPosition(p2);
						viewHelperPointsCollection.PointsInvalid = false;
					}
					else
					{
						if (!targetMeasureValid)
						{
							if (p2 != EndPoint.Position)
							{
								EndPoint.Placement = EPlacementKind.AbsoluteSubCanvas;
                                Point snapped = r1.Normalize().SnapPointToRectangle(EndPoint.Position);
                                if (snapped != EndPoint.Position)
                                    EndPoint.SetPreferedPosition(snapped);
							}
						}
						else
						{
							viewHelperPointsCollection.Last().Set(p2);
							EndPoint.SetPreferedPosition(p2);
						}
					}
				}
                else
                {
                    EndPoint.SetPreferedPosition(p2);
                    EndPoint.Placement = EPlacementKind.ParentAutoPos;
                }
				targetMeasureValid = true;
			}

			#endregion
		}

		/// <summary>
		/// Recreates the junction's geometry and causes it to redraw. 
		/// </summary>
		public void InvalidateGeometry()
		{
			AdjustEndPoints();

			PathGeometry path = new PathGeometry();

			PathFigure startFigure = null;
			PathFigure endFigure = null;
			Point startPointShifted = StartPoint.CanvasPosition;
			Point endPointShifted = EndPoint.CanvasPosition;

			#region define startFigure

			if (StartCapStyle != EJunctionCapStyle.Straight)
			{
				Point start = StartPoint.CanvasPosition;
				Point end = Points[1].CanvasPosition;

				startFigure = new PathFigure { StartPoint = start };
				PolyLineSegment seg = new PolyLineSegment();
				seg.Points.Add(end);
				startFigure.Segments.Add(seg);

				switch (StartCapStyle)
				{
					case EJunctionCapStyle.FullArrow:
					case EJunctionCapStyle.Arrow:
					case EJunctionCapStyle.Triangle:
						Point dummy = new Point();
						if (StartCapStyle == EJunctionCapStyle.Arrow)
							startFigure = JunctionGeometryHelper.CalculateArrow(startFigure, end, start,
																				StartCapStyle != EJunctionCapStyle.Arrow, ref dummy);
						else
							startFigure = JunctionGeometryHelper.CalculateArrow(startFigure, end, start,
																				StartCapStyle != EJunctionCapStyle.Arrow,
																				ref startPointShifted);
						break;
					case EJunctionCapStyle.FullDiamond:
					case EJunctionCapStyle.Diamond:
						startFigure = JunctionGeometryHelper.CalculateDiamond(startFigure, end, start, ref startPointShifted);
						break;
				}
			}

			#endregion

			#region define endFigure

			if (EndCapStyle != EJunctionCapStyle.Straight)
			{
				endFigure = new PathFigure();
				Point start = EndPoint.CanvasPosition;
				Point end = Points[Points.Count - 2].CanvasPosition;

				PolyLineSegment seg = new PolyLineSegment();
				seg.Points.Add(end);
				endFigure.Segments.Add(seg);
				switch (EndCapStyle)
				{
					case EJunctionCapStyle.Arrow:
					case EJunctionCapStyle.FullArrow:
					case EJunctionCapStyle.Triangle:
						Point dummy = new Point();
						if (StartCapStyle == EJunctionCapStyle.Arrow)
							endFigure = JunctionGeometryHelper.CalculateArrow(endFigure, end, start, EndCapStyle != EJunctionCapStyle.Arrow,
																			  ref dummy);
						else
							endFigure = JunctionGeometryHelper.CalculateArrow(endFigure, end, start, EndCapStyle != EJunctionCapStyle.Arrow,
																			  ref endPointShifted);
						break;
					case EJunctionCapStyle.Diamond:
					case EJunctionCapStyle.FullDiamond:
						endFigure = JunctionGeometryHelper.CalculateDiamond(endFigure, end, start, ref endPointShifted);
						break;
				}
			}

			#endregion

			#region create junctionFigure

			PathFigure junctionFigure = new PathFigure { StartPoint = startPointShifted };
			PolyLineSegment segment = new PolyLineSegment();
			for (int i = 1; i < Points.Count - 1; i++)
			{
				segment.Points.Add(Points[i].CanvasPosition);
			}
			segment.Points.Add(endPointShifted);
			segment.IsSmoothJoin = true;
			junctionFigure.Segments.Add(segment);
			junctionFigure.IsFilled = false;

			#endregion

			if (startFigure != null) path.Figures.Add(startFigure);
			if (endFigure != null) path.Figures.Add(endFigure);

			path.FillRule = FillRule.Nonzero;
			path.Figures.Add(junctionFigure);

			geometry = path;
			InvalidateVisual();
			if (Association != null)
			{
				Association.UpdateNameLabelPosition();
			}
		}

		/// <summary>
		/// Connects <see cref="Points"/> to draw the junction. 
		/// </summary>
		/// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			if (Points.Count > 0)
			{
				if (geometry == null)
					InvalidateGeometry();

				drawingContext.DrawGeometry(Brushes.Transparent, MediaLibrary.JunctionTransparentPen, geometry);
				if (IsSelected)
					drawingContext.DrawGeometry(Fill, MediaLibrary.JunctionSelectedPen, geometry);
				drawingContext.DrawGeometry(Fill, Pen, geometry);
			}
		}

		/// <summary>
		/// Remembers the mouse position to identify the segment when breaking the junction.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the right mouse button was pressed.</param>
		protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
		{
			downPos = e.GetPosition(XCaseCanvas);
			base.OnPreviewMouseRightButtonDown(e);
		}

		private Point downPos;

		void viewHelperPointsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e is ObservablePointCollection.PointCollectionChangedEventArgs &&
				((ObservablePointCollection.PointCollectionChangedEventArgs)e).PointPositionsMovedOnly)
			{
				for (int i = 0; i < viewHelperPointsCollection.Count; i++)
				{
					Point point = viewHelperPointsCollection[i];
					Points[i].X = point.X;
					Points[i].Y = point.Y;
				}
			}
			else
			{
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Add:
						if (e.NewItems.Count != 1)
							throw new ArgumentOutOfRangeException("e", "Only one added point expected.");
						BreakLine(viewHelperPointsCollection[e.NewStartingIndex], e.NewStartingIndex);
						break;
					case NotifyCollectionChangedAction.Remove:
						if (e.OldItems.Count != 1)
							throw new ArgumentOutOfRangeException("e", "Only one removed point expected.");
						StraightenLine(e.OldStartingIndex);
						break;
					case NotifyCollectionChangedAction.Replace:
						if (e.NewItems.Count != 1)
							throw new ArgumentOutOfRangeException("e", "Only one replaced point expected.");
						rPoint newPoint = (rPoint)e.NewItems[0];
						if (newPoint.tag is EPlacementKind)
							Points[e.NewStartingIndex].Placement = (EPlacementKind)newPoint.tag;
						viewHelperPointsCollection[e.NewStartingIndex].Set(newPoint);
						break;
					case NotifyCollectionChangedAction.Reset:
						break;
					default:
						throw new ArgumentOutOfRangeException("e", "Unknown action.");
				}
			}
		}

		public Point FindClosestPoint(Point point)
		{
			return JunctionGeometryHelper.FindClosestPoint(this, point);
		}

		/// <summary>
		/// Adds new internal point to the junction.
		/// </summary>
		/// <param name="position">position where new point is added</param>
		/// <param name="index">index where new point is inserted in <see cref="Points"/> collection</param>
		public void BreakLine(Point position, int index)
		{
			if (AutoPosModeOnly) return;
			JunctionPoint newPoint = new JunctionPoint(XCaseCanvas)
										{
											Junction = this,
											OrderInJunction = index
										};

			XCaseCanvas.Children.Add(newPoint);

			if (SourceElement == TargetElement)
			{
				newPoint.SnapTo(SourceElement as DragThumb, true);
			}

			//newPoint.SetPreferedPosition(position.X - (newPoint.ReferentialElement != null ? newPoint.ReferentialElement.CanvasPosition.X : 0), position.Y - (newPoint.ReferentialElement != null ? newPoint.ReferentialElement.CanvasPosition.Y : 0));
			newPoint.SetPreferedPosition(position.X, position.Y);

			Canvas.SetZIndex(newPoint, Canvas.GetZIndex(this) + 1);

			for (int i = index; i < Points.Count; i++)
			{
				Points[i].OrderInJunction++;
			}

			Points.Insert(newPoint.OrderInJunction, newPoint);

			//only re-rendering is not enough, whole geometry must be invalidated...
			InvalidateGeometry();
			UpdateLayout();
		}

		/// <summary>
		/// Removes internal point from the junction. 
		/// </summary>
		/// <param name="index">index in <see cref="Points"/> collection from which point is removed</param>
		public void StraightenLine(int index)
		{
			if (AutoPosModeOnly) return;
			for (int i = index; i < Points.Count; i++)
			{
				Points[i].OrderInJunction--;
			}
			XCaseCanvas.Children.Remove(Points[index]);
			Points.RemoveAt(index);
			InvalidateGeometry();
		}

		/// <summary>
		/// Draws junction between <paramref name="c1"/> and <paramref name="endPoint"/>. This method is supposed 
		/// to be used when connection is dragged between two elements (one of them is <paramref name="c1"/>)
		/// and <paramref name="endPoint"/> is being dragged "on the way" to the second element. 
		/// </summary>
		/// <param name="c1">connected element</param>
		/// <param name="endPoint">dragged point</param>
		/// <param name="canvas">canvas where the junction is created</param>
		internal void DragConnection(IConnectable c1, JunctionPoint endPoint, XCaseCanvas canvas)
		{
			SourceElement = c1 as Control;

			JunctionPoint startPoint = c1.CreateJunctionEnd();
			startPoint.Junction = this;
			startPoint.OrderInJunction = 0;
			endPoint.Junction = this;
			endPoint.OrderInJunction = 1;
			Points.Add(startPoint);
			Points.Add(endPoint);
			TargetElement = null;
		}

		/// <summary>
		/// Changes the <see cref="TargetElement"/> from current target element to 
		/// <paramref name="newEndPoint"/>
		/// </summary>
		/// <param name="newEndPoint">new end point</param>
		internal void ReconnectTargetElement(JunctionPoint newEndPoint)
		{
			if (TargetElement != null)
			{
				TargetElement.SizeChanged -= TargetElement_SizeChanged;
				((IConnectable)TargetElement).UnHighlight();
				TargetElement = null;
			}
			if (EndPoint.Parent != null)
				((Canvas)EndPoint.Parent).Children.Remove(EndPoint);
			Points.RemoveAt(Points.Count - 1);
			newEndPoint.Junction = this;
			newEndPoint.OrderInJunction = Points.Count;
			Points.Add(newEndPoint);
			Cursor = Cursors.Arrow;
			InvalidateGeometry();
		}

		/// <summary>
		/// Changes the <see cref="TargetElement"/> from current target element to
		/// <paramref name="targetElement"/>
		/// </summary>
		/// <param name="targetElement">new target element.</param>
		internal void ReconnectTargetElement(IConnectable targetElement)
		{
			if (targetElement == TargetElement)
				return;
			if (Points.Count < 2 /*|| EndPoint.Parent == null*/)
				return;
			if (TargetElement != null)
			{
				TargetElement.SizeChanged -= TargetElement_SizeChanged;
				((IConnectable)TargetElement).UnHighlight();
				TargetElement = null;
			}

			Points.RemoveAt(Points.Count - 1);
			TargetElement = (Control)targetElement;
			((IConnectable)TargetElement).Highlight();
			JunctionPoint endPoint = targetElement.CreateJunctionEnd();
			endPoint.Junction = this;
			endPoint.OrderInJunction = 1;
			endPoint.ContextMenu = null;
			Points.Add(endPoint);
			//TargetElement.SizeChanged += TargetElement_SizeChanged;
			Cursor = Cursors.Hand;
			InvalidateGeometry();
		}

		/// <summary>
		/// News the connection.
		/// </summary>
		/// <param name="sourceElement">source element</param>
		/// <param name="sourceViewHelper">view helper for the association end that is related to <paramref name="sourceElement"/>,
		/// can be set to null if <paramref name="sourceElement"/> has not an association end for this connection.</param>
		/// <param name="targetElement">target element</param>
		/// <param name="targetViewHelper">view helper for the association end that is related to <paramref name="targetElement"/>,
		/// can be set to null if <paramref name="targetElement"/> has not an association end for this connection.</param>
		/// <param name="points">collection of points for the junction (usually part of some ViewHelper), the junction reflects
		/// all changes in the collection</param>
		public void NewConnection(
			IConnectable sourceElement,
			AssociationEndViewHelper sourceViewHelper,
			IConnectable targetElement,
			AssociationEndViewHelper targetViewHelper,
			ObservablePointCollection points)
		{
			foreach (JunctionPoint point in Points)
			{
				point.DeleteFromCanvas();
			}
			Points.Clear();
			if (points == null) throw new ArgumentNullException("points");
			if (points.Count < 2) throw new ArgumentException("There must be at last two points to set a connection.", "points");

			SourceElement = (Control)sourceElement;
			TargetElement = (Control)targetElement;

			JunctionPoint startPoint;
			if (sourceViewHelper != null)
			{
				startPoint = ((ICreatesAssociationEnd)sourceElement).CreateAssociationEnd(points.First(), sourceViewHelper, sourceViewHelper.AssociationEnd);
				((PIM_AssociationEnd)startPoint).Association = Association;
			}
			else
			{
				startPoint = (sourceElement).CreateJunctionEnd(points.First());
			}

			startPoint.ContextMenu = null;
			startPoint.Junction = this;
			startPoint.OrderInJunction = 0;
			Points.Add(startPoint);

			JunctionPoint endPoint;
			if (targetViewHelper != null)
			{
				endPoint = ((ICreatesAssociationEnd)targetElement).CreateAssociationEnd(points.Last(), targetViewHelper, targetViewHelper.AssociationEnd);
				((PIM_AssociationEnd)endPoint).Association = Association;
			}
			else
			{
				endPoint = targetElement.CreateJunctionEnd(points.Last());
			}

			endPoint.ContextMenu = null;
			endPoint.Junction = this;
			endPoint.OrderInJunction = 1;
			Points.Add(endPoint);

			if (startPoint is PIM_AssociationEnd) ((PIM_AssociationEnd)startPoint).StartBindings();
			if (endPoint is PIM_AssociationEnd) ((PIM_AssociationEnd)endPoint).StartBindings();

			int zindex = Math.Min(Panel.GetZIndex(SourceElement), Panel.GetZIndex(TargetElement));
			zindex--;
			Canvas.SetZIndex(this, zindex - 1);

			UpdateLayout();
			for (int i = 1; i < points.Count - 1; i++)
			{
				Point point = points[i];
				BreakLine(point, i);
			}

			SourceElement.SizeChanged += SourceElement_SizeChanged;
			TargetElement.SizeChanged += TargetElement_SizeChanged;
			AutoPosModeOnly = AutoPosModeOnly;
			InvalidateGeometry();
		}

		/// <summary>
		/// Handles the SizeChanged event of the TargetElement control, adjusts end points 
		/// to the new size.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
		void TargetElement_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (EndPoint.Placement != EPlacementKind.ParentAutoPos)
			{
				Rect bounds = ((IConnectable)TargetElement).GetBounds().Normalize();
				if (!(bounds.Contains(EndPoint.Position)))
				{
					Point newPoint = bounds.SnapPointToRectangle(EndPoint.Position);
					EndPoint.Position = newPoint;
					viewHelperPointsCollection.Last().Set(newPoint);
				}
			}
			InvalidateGeometry();
		}

		/// <summary>
		/// Handles the SizeChanged event of the SourceElement control, adjusts end points 
		/// to the new size.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
		void SourceElement_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (StartPoint.Placement != EPlacementKind.ParentAutoPos)
			{
				Rect bounds = ((IConnectable)SourceElement).GetBounds().Normalize();
				if (!(bounds.Contains(StartPoint.Position)))
				{
					Point newPoint = bounds.SnapPointToRectangle(StartPoint.Position);
					StartPoint.Position = newPoint;
					viewHelperPointsCollection[0].Set(newPoint);
				}
			}
			InvalidateGeometry();
		}

		/// <summary>
		/// Calls <see cref="Controls.XCaseCanvas"/>.<see cref="Controls.XCaseCanvas.SelectableItemPreviewMouseDown"/> on <see cref="XCaseCanvas"/>
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the left mouse button was pressed.</param>
		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);

			XCaseCanvas.SelectableItemPreviewMouseDown(this, e);
		}

		#region segment dragging

		private bool segmentDragging = false;
		private Point dragLast;
		private JunctionPoint[] affectedPoints;
		private Dictionary<DragThumb, rPoint> startPositions;

		/// <summary>
		/// Initializes dragging of a line segment. 
		/// </summary>
		/// <seealso cref="AutoPosModeOnly"/>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. This event data reports details about the mouse button that was pressed and the handled state.</param>
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left && !AutoPosModeOnly)
			{
				segmentDragging = true;
				dragLast = e.GetPosition(XCaseCanvas);
				int i = JunctionGeometryHelper.FindHitSegmentIndex(dragLast, Points);
				affectedPoints = new JunctionPoint[] { Points[i], Points[i + 1] };
				startPositions = new Dictionary<DragThumb, rPoint>();
				if (affectedPoints[0].Movable)
					startPositions[affectedPoints[0]] = new rPoint(affectedPoints[0].Position) { tag = affectedPoints[0].Placement };
				if (affectedPoints[1].Movable)
					startPositions[affectedPoints[1]] = new rPoint(affectedPoints[1].Position) { tag = affectedPoints[1].Placement };
				this.CaptureMouse();
			}
			base.OnMouseDown(e);
		}

		/// <summary>
		/// Proceeds with dragging of a line segment. 
		/// </summary>
		/// <seealso cref="AutoPosModeOnly"/>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (segmentDragging && XCaseCanvas.State != ECanvasState.DraggingConnection)
			{
				Vector delta = dragLast - e.GetPosition(XCaseCanvas);

				foreach (JunctionPoint junctionPoint in affectedPoints)
				{
					if (!junctionPoint.Movable)
						continue;
					if (junctionPoint.Placement == EPlacementKind.ParentAutoPos)
					{
						junctionPoint.Placement = EPlacementKind.AbsoluteSubCanvas;
					}
					if ((junctionPoint == StartPoint && ((IConnectable)SourceElement).BoundsAngle != 0)
						|| (junctionPoint == EndPoint && ((IConnectable)TargetElement).BoundsAngle != 0))
					{
						continue;
					}

					if (junctionPoint == StartPoint || junctionPoint == EndPoint)
					{
						Rect r = (junctionPoint == StartPoint) ? ((IConnectable)SourceElement).GetBounds() : ((IConnectable)TargetElement).GetBounds();
						Point newPos = new Point(r.Left + junctionPoint.X - delta.X, r.Top + junctionPoint.Y - delta.Y);
						newPos = r.SnapPointToRectangle(newPos, 0);
						junctionPoint.X = newPos.X - r.Left;
						junctionPoint.Y = newPos.Y - r.Top;
					}
					else
					{
						junctionPoint.X = junctionPoint.X - delta.X;
						junctionPoint.Y = junctionPoint.Y - delta.Y;
					}
				}

				dragLast -= delta;
				InvalidateGeometry();
			}
			base.OnMouseMove(e);
		}

		/// <summary>
		/// Finalizes dragging of a line segment.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the mouse button was released.</param>
		/// <seealso cref="AutoPosModeOnly"/>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (segmentDragging)
			{
				segmentDragging = false;

				JunctionPointCommand.PointMoveDataDictionary pointMoveDataCollection =
					new JunctionPointCommand.PointMoveDataDictionary();
				foreach (KeyValuePair<DragThumb, rPoint> pair in startPositions)
				{
					JunctionPoint junctionPoint = (JunctionPoint)pair.Key;

					if (junctionPoint.Position == pair.Value)
						continue;

					JunctionPointCommand.PointMoveData data = new JunctionPointCommand.PointMoveData
																{
																	Index = junctionPoint.OrderInJunction,
																	OldPosition = pair.Value,
																	NewPosition =
																		new rPoint(junctionPoint.Position) { tag = junctionPoint.Placement },
																};

					if (!pointMoveDataCollection.ContainsKey(viewHelperPointsCollection))
						pointMoveDataCollection[viewHelperPointsCollection] = new List<JunctionPointCommand.PointMoveData>();
					pointMoveDataCollection[viewHelperPointsCollection].Add(data);
				}

				if (pointMoveDataCollection.Count > 0)
				{
					JunctionPointCommand junctionPointCommand =
						(JunctionPointCommand)JunctionPointCommandFactory.Factory().Create(XCaseCanvas.Controller);
					junctionPointCommand.Action = JunctionPointCommand.EJunctionPointAction.MovePoints;
					junctionPointCommand.PointMoveDataCollection = pointMoveDataCollection;
					junctionPointCommand.Description = CommandDescription.MOVE_JUNCTION_SEGMENT;
					junctionPointCommand.Execute();
				}
			}
			this.ReleaseMouseCapture();
			base.OnMouseUp(e);
		}

		#endregion

		/// <summary>
		/// Shows <see cref="Controller.Dialogs.AssociationDialog"/> if junction is a part of an association
		/// </summary>
		/// <param name="e">The event data.</param>
		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			if (Association != null)
			{
				Association.Controller.ShowAssociationDialog();
			}

			segmentDragging = false;
		}

        #region Highlighting
		/// <summary>
		/// Highlights the junction
		/// </summary>
		public void Highlight()
		{
			Effect = MediaLibrary.DropShadowEffect;
		}

		/// <summary>
		/// Unhighlights the junction
		/// </summary>
		public void UnHighlight()
		{
			Effect = null;
		}
		#endregion

		/// <summary>
		/// Deletes the control from canvas (with all points).
		/// </summary>
		public void DeleteFromCanvas()
		{
			if (viewHelperPointsCollection != null)
			{
				viewHelperPointsCollection.CollectionChanged -= viewHelperPointsCollection_CollectionChanged;
			}

			if (SourceElement != null)
				SourceElement.SizeChanged -= SourceElement_SizeChanged;
			if (TargetElement != null)
				TargetElement.SizeChanged -= TargetElement_SizeChanged;

			if (XCaseCanvas != null)
			{
				foreach (JunctionPoint junctionPoint in Points)
				{
					if (junctionPoint.Parent is Canvas)
					{
						junctionPoint.DeleteFromCanvas();
					}
				}

				XCaseCanvas.Children.Remove(this);

			}
		}
	}
}
