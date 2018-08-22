using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Geometries;
using XCase.View.Interfaces;

namespace XCase.View.Controls
{
	/// <summary>
	/// Controls represents UML generalizations on the diagram
	/// (as an arrow).
	/// </summary>
	public class PIM_Generalization : Control, IModelElementRepresentant, ISelectable, IPrimitiveJunctionTarget
	{
        /// <summary>
        /// Initializes a view representation of a model element
        /// </summary>
        /// <param name="modelElement">Element to be represented</param>
        /// <param name="viewHelper">Element's viewHelper</param>
        /// <param name="controller">Element's controller</param>
        public void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller)
	    {
            this.Controller = (GeneralizationController)controller;
            this.ViewHelper = (GeneralizationViewHelper)viewHelper;

            InitializeFromViewHelper();
	    	this.StartBindings();
	    }

		/// <summary>
		/// Initializes a new instance of the <see cref="PIM_Generalization"/> class.
		/// </summary>
		/// <param name="xCaseCanvas">canvas where the control is placed</param>
	    public PIM_Generalization(XCaseCanvas xCaseCanvas)
		{
			this.XCaseCanvas = xCaseCanvas;
		}

		/// <summary>
		/// Initializes the control properties from view helper (especially points are restored).
		/// </summary>
		private void InitializeFromViewHelper()
		{
            IConnectable sourceElement = (IConnectable)XCaseCanvas.ElementRepresentations[Controller.Generalization.Specific];
			IConnectable targetElement = (IConnectable)XCaseCanvas.ElementRepresentations[Controller.Generalization.General];
			if (ViewHelper.Points.Count == 0)
			{
				ViewHelper.Points.AppendRange(JunctionGeometryHelper.ComputeOptimalConnection(sourceElement, targetElement));
			}

			if (GeneralizationJunction != null)
			{
				GeneralizationJunction.DeleteFromCanvas();
			}
			GeneralizationJunction = new XCaseJunction(XCaseCanvas, this, ViewHelper.Points);
			if (sourceElement is PSM_Class || targetElement is PSM_Class)
			{
				GeneralizationJunction.AutoPosModeOnly = true; 
			}
			GeneralizationJunction.NewConnection(sourceElement, null, targetElement, null, ViewHelper.Points);
			XCaseCanvas.Children.Add(GeneralizationJunction);
			GeneralizationJunction.StartCapStyle = EJunctionCapStyle.Straight;
			GeneralizationJunction.EndCapStyle = EJunctionCapStyle.Triangle;
		}

		private XCaseJunction GeneralizationJunction { get; set; }

		/// <summary>
		/// Canvas where the element is placed
		/// </summary>
		/// <value></value>
		public XCaseCanvas XCaseCanvas { get; set; }

		/// <summary>
		/// UML generelization which is represented by this control
		/// </summary>
		/// <value><see cref="Generalization"/></value>
		[ModelElement]
		public Generalization Generalization
		{
			get
			{
				return Controller != null ? Controller.Generalization : null;
			}
		}

		/// <summary>
		/// Controller of <see cref="Generalization"/>.
		/// </summary>
		/// <value><see cref="GeneralizationController"/></value>
		public GeneralizationController Controller { get; protected set; }

		/// <summary>
		/// ViewHelper for the control, stores points.
		/// </summary>
		/// <value><see cref="GeneralizationViewHelper"/></value>
		[ViewHelperElement]
		public GeneralizationViewHelper ViewHelper { get; protected set; }

		/// <summary>
		/// Returns context menu items.
		/// </summary>
		/// <returns></returns>
		internal IEnumerable<ContextMenuItem> GeneralizationMenuItems()
		{
            ContextMenuItem addCommentary = new ContextMenuItem("Add commentary");
			addCommentary.Click += delegate
			{
				NewModelCommentToDiagramCommand command = (NewModelCommentToDiagramCommand)CommandFactoryBase<NewModelCommentaryToDiagramCommandFactory>.Factory().Create(Controller.DiagramController);
				command.AnnotatedElement = Generalization;
				
				Point p = JunctionGeometryHelper.FindClosestPoint(GeneralizationJunction, GeneralizationJunction.GetBounds().GetCenter());
				command.X = p.X + 20;
				command.Y = p.Y + 20;
				
				command.Set(Controller.DiagramController.ModelController, null);
				command.Execute();
			};
			return new ContextMenuItem[] { addCommentary };
		}

		/// <summary>
		/// Removes the control from canvas completely.
		/// </summary>
		public void DeleteFromCanvas()
		{
			if (GeneralizationJunction != null)
			{
				GeneralizationJunction.DeleteFromCanvas();
			}
			this.CloseBindings();
		}

		private bool isSelected;
		
		/// <summary>
		/// Selected flag. Selected elements are highlighted on the canvas and are
		/// target of commands.
		/// </summary>
		/// <value></value>
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
				if (GeneralizationJunction != null)
					GeneralizationJunction.IsSelected = value;
			}
		}

		/// <summary>
		/// 	<para>
		/// If set to true, this object will be dragged when selection is dragged.
		/// </para>
		/// 	<para>
		/// It is usually handy to be able to drag an object in a group, but not desirable for those
		/// objects whose position is determined by position of other objects (like junctions and
		/// some SnapPointHooks).
		/// </para>
		/// </summary>
		/// <value></value>
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
			return new Rect();
		}

		/// <summary>
		/// Finds the closest point.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <returns></returns>
		public Point FindClosestPoint(Point point)
		{
			return JunctionGeometryHelper.FindClosestPoint(GeneralizationJunction, point);
		}

        #region Versioned element highlighting support

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            XCaseCanvas.InvokeVersionedElementMouseEnter(this, Generalization);
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            XCaseCanvas.InvokeVersionedElementMouseLeave(this, Generalization);
        }

        #endregion 
	}
}