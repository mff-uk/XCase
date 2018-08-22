using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XCase.Controller;
using XCase.Model;
using XCase.View.Geometries;
using XCase.View.Interfaces;

namespace XCase.View.Controls
{
	/// <summary>
	/// Control that represents <see cref="PSMAssociation"/> on canvas
	/// (as an arrow between Parent and Child).
	/// </summary>
	/// <seealso cref="PIM_Association"/>
	public class PSM_Association : Control, 
		IPSM_Connector, ISelectable, IPrimitiveJunctionTarget, IPSMSubordinateComponentRepresentant
	{
		/// <summary>
		/// Reference to an underlying junction
		/// </summary>
		/// <value><see cref="XCaseJunction"/></value>
		public XCaseJunction Junction { get; private set; }

		/// <summary>
		/// Model PSMAssociation that is represented by this control
		/// </summary>
		/// <value><see cref="PSMAssociation"/></value>
		[ModelElement]
		public PSMAssociation Association { get { return (PSMAssociation)Controller.Element; } }

		/// <summary>
		/// ViewHelper of the control, stores visualization information.
		/// </summary>
		/// <value></value>
		[ViewHelperElement]
		public PSMAssociationViewHelper ViewHelper { get; private set; }

		/// <summary>
		/// Controller of the element, provides functionality to change the model.
		/// </summary>
		/// <value></value>
		public PSM_AssociationController Controller { get; private set; }

		/// <summary>
		/// Returns underlying model element as <see cref="PSMSubordinateComponent"/>
		/// </summary>
		/// <value></value>
		public PSMSubordinateComponent ModelSubordinateComponent
    	{
			get { return Association; }
    	}

		/// <summary>
		/// Canvas where the element is placed
		/// </summary>
		/// <value></value>
		public XCaseCanvas XCaseCanvas { get; private set; }

		/// <summary>
		/// Reference to a control representing <see cref="Association"/>'s <see cref="PSMSubordinateComponent.Parent"/>.
		/// </summary>
		/// <value><see cref="IConnectable"/></value>
		public IConnectable ParentRepresentation { get; private set; }

		/// <summary>
		/// Reference to a control representing <see cref="Association"/>'s <see cref="PSMAssociation.Child"/>.
		/// </summary>
		/// <value><see cref="IConnectable"/></value>
		public IConnectable ChildRepresentation { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PSM_Association"/> class.
		/// </summary>
		/// <param name="xCaseCanvas">canvas where the control is placed</param>
		public PSM_Association(XCaseCanvas xCaseCanvas)
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
			Controller = (PSM_AssociationController)controller;
			ViewHelper = (PSMAssociationViewHelper)viewHelper;

			ParentRepresentation = (IConnectable)XCaseCanvas.ElementRepresentations[Association.Parent];
			ChildRepresentation = (IConnectable)XCaseCanvas.ElementRepresentations[Association.Child];

            if (Junction != null)
            {
                Junction.MouseEnter -= junction_MouseEnter;
                Junction.MouseLeave -= junction_MouseLeave;
            }

			if (ViewHelper.Points.Count < 2)
			{
				ViewHelper.Points.Clear();
				ViewHelper.Points.AppendRange(JunctionGeometryHelper.ComputeOptimalConnection(ParentRepresentation, ChildRepresentation));
				if (!ChildRepresentation.IsMeasureValid || !ParentRepresentation.IsMeasureValid)
				{
					ViewHelper.Points.PointsInvalid = true;
				}
			}

			remove = new ContextMenuItem("Remove subtree");
			remove.Icon = ContextMenuIcon.GetContextIcon("delete2");
			remove.Click += delegate { Controller.Remove(); };

			cutToRoot = new ContextMenuItem("Cut association (add child as new root)");
			cutToRoot.Click += delegate { Controller.CutToRoot(); };

			Junction = new XCaseJunction(XCaseCanvas, ViewHelper.Points);
        	Junction.PSM_Association = this;
			Junction.AutoPosModeOnly = true; 
			Junction.SelectionOwner = this;
			Junction.NewConnection(ParentRepresentation, null, ChildRepresentation, null, ViewHelper.Points);
			Junction.Points[1].PositionChanged += AdjustLabelsPositions;
            
            XCaseCanvas.Children.Add(Junction);
			Junction.EndCapStyle = EJunctionCapStyle.Arrow;
			((PSMElementViewBase)ChildRepresentation).Connector = this;
			this.StartBindings();
			Association.PropertyChanged += ModelPropertyChanged;
            if (Junction != null)
            {
                Junction.MouseEnter += junction_MouseEnter;
                Junction.MouseLeave += junction_MouseLeave;
            }

		}

		void ModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Parent")
			{
				if ((Association.Parent) != null &&
					XCaseCanvas.ElementRepresentations.IsElementPresent(Association.Parent))
				{
					UpdateConnection((IConnectable)XCaseCanvas.ElementRepresentations[Association.Parent]);
				}
			}
			cutToRoot.IsEnabled = (Association.Child is PSMClass);
		}

		/// <summary>
		/// Updates the connection when parent of <see cref="Association"/> changes.
		/// </summary>
		/// <param name="newParent">The new parent.</param>
		public void UpdateConnection(IConnectable newParent)
		{
			ParentRepresentation = newParent;
			ViewHelper.Points.Clear();
			ViewHelper.Points.AppendRange(JunctionGeometryHelper.ComputeOptimalConnection(ParentRepresentation,
																						  ChildRepresentation));
			if (!ChildRepresentation.IsMeasureValid || !ParentRepresentation.IsMeasureValid)
			{
				ViewHelper.Points.PointsInvalid = true;
			}
			Junction.NewConnection(ParentRepresentation, null, ChildRepresentation, null, ViewHelper.Points);
		}

		/// <summary>
		/// Returns labels to their default position
		/// </summary>
		void AdjustLabelsPositions()
		{
			if (multiplicityLabel != null && ChildRepresentation.GetBounds().IntersectsWith(multiplicityLabel.GetBounds()))
			{
				ChildRepresentation.GetBounds().AlignLabelsToPoint(Junction.Points[1].Position, new[] { multiplicityLabel });
			}
		}

		private AssociationLabel multiplicityLabel;

		/// <summary>
		/// Multiplicity of <see cref="Association"/> displayed in a label.
		/// </summary>
		/// <value><see cref="String"/></value>
		[ModelPropertyMapping("MultiplicityString")]
		public string Multiplicity
		{
			get { return multiplicityLabel != null ? multiplicityLabel.Text : String.Empty; }
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					if (multiplicityLabel != null)
					{
						//remove unused label
						multiplicityLabel.IsSelected = false;
						DragThumb.UnsnapElement(multiplicityLabel);
						Junction.XCaseCanvas.Children.Remove(multiplicityLabel);
						multiplicityLabel.CloseBindings(TypeBindingData.EBindingSourceType.View);
						multiplicityLabel = null;
					}
				}
				else
				{
					if (multiplicityLabel == null)
					{
						multiplicityLabel = CreateFellowLabel(ViewHelper.MultiplicityLabelViewHelper);
						multiplicityLabel.TextBox.TextEdited += delegate(object sender, StringEventArgs args)
						{
							Controller.ChangeMultiplicity(args.Data);
						};
                        multiplicityLabel.TextBox.mi_Rename.Header = "Change association multiplicity";
                        multiplicityLabel.ToolTip = "Association multiplicity";
					}
					multiplicityLabel.Text = value;
					multiplicityLabel.UpdateLayout();
					AdjustLabelsPositions();
				}
			}
		}

		private AssociationLabel CreateFellowLabel(AssociationLabelViewHelper labelViewHelper)
		{
			AssociationLabel associationLabel = new AssociationLabel(this.XCaseCanvas, labelViewHelper);
			Junction.XCaseCanvas.Children.Add(associationLabel);
			associationLabel.Text = " ";
			associationLabel.SnapTo(Junction.Points.Last(), false);
			associationLabel.UpdateLayout();
			Canvas.SetZIndex(associationLabel, Canvas.GetZIndex(Junction.SourceElement) + 1);
			associationLabel.PSM_Association = this;
			return associationLabel;
		}

		/// <summary>
		/// Deletes the control from canvas.
		/// </summary>
		public void DeleteFromCanvas()
		{
			Association.PropertyChanged -= ModelPropertyChanged;
			if (Junction != null)
				Junction.DeleteFromCanvas();
			if (multiplicityLabel != null)
				XCaseCanvas.Children.Remove(multiplicityLabel);
			this.CloseBindings();
		}

		#region Implementation of ISelectable

		private bool isSelected;

		/// <summary>
		/// Selected flag. Selected elements are highlighted on the canvas and are 
		/// target of commands. 
		/// </summary>
		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				isSelected = value;
				if (Junction.IsSelected != value)
					Junction.IsSelected = value;
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
			get { return false; }
		}

		/// <summary>
		/// Returns bounding rectangle of the element
		/// </summary>
		/// <returns>Bounding rectangle</returns>
		public Rect GetBounds()
		{
			return new Rect();
		}

		#endregion

		#region Implementation of IPrimitiveJunctionTarget

		/// <summary>
		/// Finds the closest point.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <returns></returns>
		Point IPrimitiveJunctionTarget.FindClosestPoint(Point point)
		{
			return JunctionGeometryHelper.FindClosestPoint(Junction, point);
		}

		#endregion


		ContextMenuItem cutToRoot;
		ContextMenuItem remove;

		/// <summary>
		/// Context menu items for PSM Association
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ContextMenuItem> PSM_AssociationMenuItems()
		{
			if (cutToRoot.Parent != null)
				((ContextMenu)cutToRoot.Parent).Items.Remove(cutToRoot);
			if (remove.Parent != null)
				((ContextMenu)remove.Parent).Items.Remove(remove);

		    ContextMenuItem moveToContentContainer = new ContextMenuItem("Move to content container");
		    moveToContentContainer.Click += delegate
		                                        {
		                                            Controller.MoveToContentContainer(null);
		                                        };

            ContextMenuItem moveToContentChoice = new ContextMenuItem("Move to content choice");
            moveToContentChoice.Click += delegate
            {
                Controller.MoveToContentChoice(null);
            };

            ContextMenuItem moveToClassUnion = new ContextMenuItem("Move to class union");
            moveToClassUnion.Click += delegate
            {
                Controller.MoveToClassUnion(null);
            };


            return new[] { cutToRoot, remove, moveToContentContainer, moveToContentChoice, moveToClassUnion };
		}

        #region Versioned element highlighting support

        private void junction_MouseLeave(object sender, MouseEventArgs e)
        {
            XCaseCanvas.InvokeVersionedElementMouseLeave(sender, Association);
        }

        void junction_MouseEnter(object sender, MouseEventArgs e)
        {
            XCaseCanvas.InvokeVersionedElementMouseEnter(sender, Association);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            XCaseCanvas.InvokeVersionedElementMouseEnter(this, Association);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            XCaseCanvas.InvokeVersionedElementMouseLeave(this, Association);
        }

        #endregion 
    }
}