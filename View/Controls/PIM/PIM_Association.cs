using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NUml.Uml2;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Geometries;
using XCase.View.Interfaces;
using Association = XCase.Model.Association;
using Element = XCase.Model.Element;

namespace XCase.View.Controls
{
	/// <summary>
	/// Control representing PIM associations.
	/// </summary>
	public class PIM_Association : Control, IModelElementRepresentant, ISelectable, IPrimitiveJunctionTarget
	{
		public PIM_Association(XCaseCanvas xCaseCanvas)
		{
			this.XCaseCanvas = xCaseCanvas;
		}

        /// <summary>
        /// Initializes a view representation of a model element
        /// </summary>
        /// <param name="modelElement">Element to be represented</param>
        /// <param name="viewHelper">Element's viewHelper</param>
        /// <param name="controller">Element's controller</param>
        public void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller)
		{
			this.Controller = (AssociationController)controller;
			this.ViewHelper = (AssociationViewHelper)viewHelper;
			InitializeFromViewHelper();
			Controller.Association.Ends.CollectionChanged += AssociationEnds_CollectionChanged;
			this.StartBindings();
		}

		void AssociationEnds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			Diagram currentDiagram = Controller.DiagramController.Diagram;
			ViewHelper vh = currentDiagram.DiagramElements[Controller.Association];
			currentDiagram.RemoveModelElement(Controller.Association);
			if (e.OldItems != null && e.OldItems.Count >= 0 && ViewHelper.UseDiamond)
			{
				ViewHelper.ForceUseDiamond = true; 	
			}
			currentDiagram.AddModelElement(Controller.Association, vh);
		}

		[ModelElement]
		public Association Association
		{
			get
			{
				return Controller != null ? Controller.Association : null;
			}
		}

		public PIM_AssociationClass AssociationClass { get; set; }

		public AssociationController Controller { get; protected set; }

		public IList<AssociationEnd> Ends
		{
			get
			{
				return Association.Ends;
			}
		}

		[ViewHelperElement]
		public AssociationViewHelper ViewHelper { get; protected set; }

		public XCaseCanvas XCaseCanvas { get; private set; }

		internal AssociationDiamond Diamond { get; private set; }

		private readonly Dictionary<AssociationEnd, XCaseJunction> associationJunctions = new Dictionary<AssociationEnd, XCaseJunction>();

	    private IEnumerable<Control> subControls
	    {
	        get
	        {
                return associationJunctions.Values.Cast<Control>().Concat(new Control[] { Diamond, simpleAssociationJunction });
	        }
	    }

		internal XCaseJunction GetJunctionConnectingEnd(AssociationEnd end)
		{
			return ViewHelper.UseDiamond ? associationJunctions[end] : simpleAssociationJunction;
		}

		public Point FindClosestPoint(Point point)
		{
			if (simpleAssociationJunction != null)
				return JunctionGeometryHelper.FindClosestPoint(simpleAssociationJunction, point);
			else if (Diamond != null)
				return Diamond.GetBounds().GetCenter();
			else 
				return GetBounds().GetCenter();
		}

		/// <summary>
		/// Junction used for associations without diamond
		/// </summary>
		private XCaseJunction simpleAssociationJunction = null;

		internal Dictionary<AssociationEndViewHelper, IConnectable> participantElements { get; private set; }

		private void InitializeFromViewHelper()
		{
			if (ViewHelper.AssociationEndsViewHelpers.Count != Association.Ends.Count)
				ViewHelper.CreateEndsViewHelpers(this.Association);
			if (ViewHelper.ForceUseDiamond)
			{
				ViewHelper.UseDiamond = true; 
			}
			ViewHelper.ForceUseDiamond = false;
			participantElements = new Dictionary<AssociationEndViewHelper, IConnectable>();
			foreach (AssociationEndViewHelper endViewHelper in ViewHelper.AssociationEndsViewHelpers)
			{
				if (!(XCaseCanvas.ElementRepresentations[endViewHelper.AssociationEnd.Class] is IConnectable))
				{
					throw new InvalidOperationException("One of the association ends is not visualized as IConnectable");
				}
				participantElements[endViewHelper]
					= XCaseCanvas.ElementRepresentations[endViewHelper.AssociationEnd.Class] as IConnectable;
			}

            foreach (Control c in subControls)
            {
                if (c != null)
                {
                    c.MouseEnter -= junction_MouseEnter;
                    c.MouseLeave -= junction_MouseLeave;
                }
            }

			if (ViewHelper.UseDiamond)
			{
				// simpleAssociationJunction field won't be used if diamond is used
				if (simpleAssociationJunction != null)
				{
					simpleAssociationJunction.DeleteFromCanvas();
					ViewHelper.AssociationEndsViewHelpers[0].Points.Clear();
				}

				// prepare the diamond
				if (Diamond == null)
				{
					Diamond = new AssociationDiamond(XCaseCanvas, this, ViewHelper);
					XCaseCanvas.Children.Add(Diamond);
				}
				Rect r = RectExtensions.GetEncompassingRectangle(participantElements.Values);
				if (ViewHelper.X == 0 && ViewHelper.Y == 0)
				{
					Diamond.X = r.GetCenter().X - Diamond.ActualWidth / 2;
					Diamond.Y = r.GetCenter().Y - Diamond.ActualHeight / 2;
					ViewHelper.X = Diamond.X;
					ViewHelper.Y = Diamond.Y;
				}
				Diamond.UpdateLayout();

				/*
				 * for each association end there will be one junction between the diamond and the 
				 * participating element. Start point of the junction will lay on the diamond, end 
				 * point of the junction will lay on the participating element and will be bound to
				 * an association end.
				 */
				foreach (AssociationEndViewHelper endViewHelper in ViewHelper.AssociationEndsViewHelpers)
				{
					//compute the start and end point of each junction if they are not already computed
					if (endViewHelper.Points.Count < 2)
					{
						endViewHelper.Points.Clear();
						endViewHelper.Points.AppendRange(JunctionGeometryHelper.ComputeOptimalConnection(Diamond, participantElements[endViewHelper]));
						if (!Diamond.IsMeasureValid || !participantElements[endViewHelper].IsMeasureValid)
						{
							endViewHelper.Points.PointsInvalid = true;
						}
					}
					XCaseJunction junction = new XCaseJunction(XCaseCanvas, this, endViewHelper.Points);
					junction.NewConnection(Diamond, null, participantElements[endViewHelper], endViewHelper, endViewHelper.Points);
					junction.StartPoint.Movable = false;
					junction.StartPoint.Visibility = Visibility.Hidden;
					associationJunctions[endViewHelper.AssociationEnd] = junction;
					XCaseCanvas.Children.Add(junction);
				}
			}
			else
			{
				foreach (XCaseJunction junction in associationJunctions.Values)
				{
					junction.DeleteFromCanvas();
					
				}
				if (Diamond != null)
				{
					Diamond.DeleteFromCanvas();
					Diamond = null;
				}
			    
			    associationJunctions.Clear();

				IConnectable SourceElement = (IConnectable)XCaseCanvas.ElementRepresentations[ViewHelper.AssociationEndsViewHelpers[0].AssociationEnd.Class];
				IConnectable TargetElement;

				if (ViewHelper.AssociationEndsViewHelpers.Count == 2
					&& ViewHelper.AssociationEndsViewHelpers[0].AssociationEnd.Class == ViewHelper.AssociationEndsViewHelpers[1].AssociationEnd.Class)
				{
					TargetElement = SourceElement;

					if (ViewHelper.Points.Count == 0)
					{
						ViewHelper.Points.AppendRange(JunctionGeometryHelper.ComputePointsForSelfAssociation(SourceElement.GetBounds()));
						if (!SourceElement.IsMeasureValid)
						{
							ViewHelper.Points.PointsInvalid = true;
						}
					}
				}
				/*
				 * two different classes are connected directly without using central diamond, 
				 * association will consist of two points
				 * in optimal position to connect two rectangles. 
				 */
				else if (ViewHelper.AssociationEndsViewHelpers.Count == 2)
				{
					TargetElement = (IConnectable)XCaseCanvas.ElementRepresentations[ViewHelper.AssociationEndsViewHelpers[1].AssociationEnd.Class];
					if (ViewHelper.Points.Count < 2)
					{
						ViewHelper.Points.Clear();
						ViewHelper.Points.AppendRange(JunctionGeometryHelper.ComputeOptimalConnection(SourceElement, TargetElement));
						if (!SourceElement.IsMeasureValid)
						{
							ViewHelper.Points.PointsInvalid = true;
						}
					}
				}
				else
				{
					throw new InvalidOperationException("UseDiamond must be true if there are more association ends than 2");
				}

				/* 
				 * diamond is not used, simpleAssociationJunction field will be used to 
				 * represent the junction 
				 */
				if (simpleAssociationJunction != null)
				{
					simpleAssociationJunction.DeleteFromCanvas();
				}
				simpleAssociationJunction = new XCaseJunction(XCaseCanvas, this, ViewHelper.Points);
				if (Association.Ends.Count == 1)
				{
					simpleAssociationJunction.NewConnection(SourceElement, null, TargetElement, ViewHelper.AssociationEndsViewHelpers[0], ViewHelper.Points);
				}
				else
				{
					simpleAssociationJunction.NewConnection(SourceElement, ViewHelper.AssociationEndsViewHelpers[0], TargetElement, ViewHelper.AssociationEndsViewHelpers[1], ViewHelper.Points);
				}
				XCaseCanvas.Children.Add(simpleAssociationJunction);
                
			}

            foreach (Control c in subControls)
            {
                if (c != null)
                {
                    c.MouseEnter += junction_MouseEnter;
                    c.MouseLeave += junction_MouseLeave;
                }
            }
		}

	    

		#region Context menu

		internal IEnumerable<ContextMenuItem> AssociationMenuItems()
		{
            ContextMenuItem addCommentary = new ContextMenuItem("Add commentary");
            addCommentary.Icon = ContextMenuIcon.GetContextIcon("comment");
			addCommentary.Click += delegate
									{
										NewModelCommentToDiagramCommand command = (NewModelCommentToDiagramCommand)CommandFactoryBase<NewModelCommentaryToDiagramCommandFactory>.Factory().Create(Controller.DiagramController);
										command.AnnotatedElement = Association;
										if (Diamond != null)
										{
											command.X = Diamond.Left + Diamond.ActualWidth + 20;
											command.Y = Diamond.Top + 20;
										}
										else
										{
											Point p = FindClosestPoint(simpleAssociationJunction.GetBounds().GetCenter());
											command.X = p.X + 20;
											command.Y = p.Y + 20;
										}
										command.Set(Controller.DiagramController.ModelController, null);
										command.Execute();
									};
            ContextMenuItem demo = new ContextMenuItem("Binding demo - show that binding to association properties works");
			demo.Click += demo_Click;
            ContextMenuItem resetLabels = new ContextMenuItem("Reset all labels position");
			resetLabels.Click += delegate { ResetAllLabelsPositions(); };

			ContextMenuItem switchDiamond = new ContextMenuItem("Switch diamond ");
			switchDiamond.Click += delegate
			                       	{
			                       		ViewController.SwitchAssociationDiamond(ViewHelper, Association, Controller.DiagramController);
			                       	};

			return new[]
			       	{
			       		resetLabels,
						#if DEBUG
						demo, 
						#endif
						addCommentary,
						switchDiamond
			       	};
		}

		void demo_Click(object sender, RoutedEventArgs e)
		{
			Association.Name = "Association name" + (DateTime.Now.Second / 2);

			uint i = 0;
			foreach (AssociationEnd associationEnd in Association.Ends)
			{
				associationEnd.Name = "Role" + DateTime.Now.Millisecond;
				associationEnd.Lower = (uint)(i + DateTime.Now.Second);
				associationEnd.Upper = (uint)(i + DateTime.Now.Second + 1);
				i++;

				associationEnd.Aggregation = (AggregationKind)(DateTime.Now.Millisecond % 3);
			}
		}

		#endregion

		#region Name Label

		private AssociationLabel nameLabel;

		private string associationName;

		[ModelPropertyMapping("Name")]
		public string AssociationName
		{
			get { return associationName; }
			set
			{
				associationName = value;
				if (nameLabel != null && (String.IsNullOrEmpty(value) || AssociationClass != null))
				{
					//remove unused label
					nameLabel.IsSelected = false;
					XCaseCanvas.Children.Remove(nameLabel);
					nameLabel.CloseBindings(TypeBindingData.EBindingSourceType.View);
					nameLabel = null;
				}
				if (nameLabel == null && !String.IsNullOrEmpty(value) && AssociationClass == null)
				{
					//create label
					nameLabel = new AssociationLabel(XCaseCanvas, ViewHelper.MainLabelViewHelper)
					{
						TextAlignment = TextAlignment.Center,
						OriginalBackgroundBrush = Brushes.White,
						OriginalTextBrush = Brushes.Black,
						Association = this,
					};
                    nameLabel.TextBox.mi_Rename.Header = "Change association label";
                    nameLabel.TextBox.ToolTip = "Association label";

					XCaseCanvas.Children.Add(nameLabel);
					Canvas.SetZIndex(nameLabel, Canvas.GetZIndex(nameLabel) + 1);
					nameLabel.SnapTo(new VirtualReferentialThumb(), false);
					nameLabel.UpdateLayout();
					nameLabel.TextBox.TextEdited += delegate(object sender, StringEventArgs args) { Controller.RenameElement(args.Data); };
				}
				if (!String.IsNullOrEmpty(value) && nameLabel != null)
				{
					nameLabel.Text = value;
					nameLabel.UpdateLayout();
					UpdateNameLabelPosition();
				}
			}
		}

		internal void UpdateNameLabelPosition()
		{
			if (nameLabel != null)  
			{
				((VirtualReferentialThumb)nameLabel.ReferentialElement).CanvasPosition = GetOptimalNameLabelPos().Value;
				DragThumb.UpdatePos(nameLabel);
			}
		}

		private Point? GetOptimalNameLabelPos()
		{
			if (nameLabel == null)
			{
				return null;
			}

			if (!ViewHelper.UseDiamond)
			{
				Point p = FindClosestPoint(
					new Point((simpleAssociationJunction.StartPoint.CanvasPosition.X + simpleAssociationJunction.AssociationEnd.CanvasPosition.X) / 2,
							  (simpleAssociationJunction.StartPoint.CanvasPosition.Y + simpleAssociationJunction.AssociationEnd.CanvasPosition.Y) / 2));

				return new Point(p.X - nameLabel.ActualWidth / 2, p.Y - nameLabel.ActualHeight / 2);
			}
			else
			{
				Diamond.UpdateLayout();
				return new Point(Diamond.Left - nameLabel.ActualWidth / 2,
								 Diamond.Top - nameLabel.ActualHeight - 5);
			}
		}

		public void ResetAllLabelsPositions()
		{
			if (nameLabel != null)
			{
				nameLabel.X = 0;
				nameLabel.Y = 0;
			}

			if (ViewHelper.UseDiamond)
			{
				foreach (XCaseJunction junction in associationJunctions.Values)
				{
					junction.AssociationEnd.ResetLabelsPositions();
				}
			}
			else
			{
				if (simpleAssociationJunction.StartPoint is PIM_AssociationEnd)
					((PIM_AssociationEnd)simpleAssociationJunction.StartPoint).ResetLabelsPositions();
				simpleAssociationJunction.AssociationEnd.ResetLabelsPositions();
			}
		}

		#endregion

		#region Highlighting
		/// <summary>
		/// Highlights the control on canvas
		/// </summary>
		public void Highlight()
		{
			if (simpleAssociationJunction != null)
			{
				simpleAssociationJunction.Highlight();
			}
			foreach (XCaseJunction junction in associationJunctions.Values)
			{
				junction.Highlight();
			}
		}

		/// <summary>
		/// Unhighlights the control on canvas
		/// </summary>
		public void UnHighlight()
		{
			if (simpleAssociationJunction != null)
			{
				simpleAssociationJunction.UnHighlight();
			}
			foreach (XCaseJunction junction in associationJunctions.Values)
			{
				junction.UnHighlight();
			}
		}
		#endregion

		/// <summary>
		/// Removes the element from <see cref="XCaseCanvas"/>.
		/// </summary>
		public void DeleteFromCanvas()
		{
			AssociationName = null;
			foreach (KeyValuePair<AssociationEnd, XCaseJunction> pair in associationJunctions)
			{
				pair.Value.DeleteFromCanvas();
			}
			if (simpleAssociationJunction != null)
			{
				simpleAssociationJunction.DeleteFromCanvas();
			}
			if (Diamond != null)
			{
				Diamond.DeleteFromCanvas();
			}
			this.CloseBindings();
			Controller.Association.Ends.CollectionChanged -= AssociationEnds_CollectionChanged;
		}

		#region ISelectable members

		private bool isSelected;

		/// <summary>
		/// Selected flag. Selected elements are highlighted on the canvas and are 
		/// target of commands. 
		/// </summary>
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
				if (simpleAssociationJunction != null)
					simpleAssociationJunction.IsSelected = value;
				foreach (XCaseJunction junction in associationJunctions.Values)
				{
					junction.IsSelected = value;
				}
				if (Diamond != null)
				{
					if (value)
						Diamond.Highlight();
					else
						Diamond.UnHighlight();
				}
				if (AssociationClass != null && (AssociationClass.IsSelected != value))
				{
					AssociationClass.IsSelected = value;
				}
			}
		}

		/// <summary>
		/// <para>
		/// If set to true, this object will be dragged when selection is dragged. 
		/// </para>
		/// <para>
		/// It is usually handy to be able to drag an object in a group, but not desirable for those 
		/// objects whose position is determined by position of other objects (like junctions and 
		/// some SnapPointHooks).
		/// </para>
		/// </summary>
		public bool CanBeDraggedInGroup
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Returns bounding rectangle of the element
		/// </summary>
		/// <returns>Bounding rectangle</returns>
		public Rect GetBounds()
		{
			if (simpleAssociationJunction != null)
				return simpleAssociationJunction.GetBounds();
			else
			{
				Rect r = Rect.Empty;
				foreach (XCaseJunction junction in associationJunctions.Values)
				{
					r.Union(junction.GetBounds());
				}
				return r;
			}
			
		}

		#endregion

        #region Versioned element highlighting support

        private void junction_MouseLeave(object sender, MouseEventArgs e)
        {
            XCaseCanvas.InvokeVersionedElementMouseLeave(sender, Association);
        }

        void junction_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            XCaseCanvas.InvokeVersionedElementMouseEnter(sender, Association);
        }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            XCaseCanvas.InvokeVersionedElementMouseEnter(this, Association);
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            XCaseCanvas.InvokeVersionedElementMouseLeave(this, Association);
        }

        #endregion 
    }
}
