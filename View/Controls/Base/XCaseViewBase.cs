using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Interfaces;
using System.Windows;

namespace XCase.View.Controls
{
	/// <summary>
	/// Base class of all view elements. Manages deletion, binding, position and size on canvas.
	/// <seealso cref="PSMElementViewBase"/>
	/// </summary>
	[ViewHelperPropertyMapping("X", "X")]
	[ViewHelperPropertyMapping("Y", "Y")]
	[ViewHelperPropertyMapping("Width", "Width")]
	[ViewHelperPropertyMapping("Height", "Height")]
    public abstract class XCaseViewBase : ConnectableDragThumb, ISelectable, IResizable, IAlignable, IModelElementRepresentant
	{
		protected XCaseViewBase(XCaseCanvas xCaseCanvas)
			: base(xCaseCanvas)
		{
            Focusable = false;
		}

		/// <summary>
		/// Selected flag. Selected elements are highlighted on the canvas and are 
		/// target of commands. 
		/// </summary>
		public abstract bool IsSelected
		{
			get;
			set;
		}

		/// <summary>
		/// Name of the element.
		/// </summary>
		[ModelPropertyMappingAttribute("Name")]
		public abstract string ElementName
		{
			get;
			set;
		}

		/// <summary>
		/// Controller for the represented element
		/// </summary>
		public abstract NamedElementController Controller
		{
			get;
		}

		/// <summary>
		/// Context menu items for this element
		/// </summary>
		/// <value><see cref="ContextMenuItem"/></value>
		internal virtual ContextMenuItem[] ContextMenuItems
		{
			get
			{
                ContextMenuItem addCommentaryItem = new ContextMenuItem("Add commentary");
                addCommentaryItem.Icon = ContextMenuIcon.GetContextIcon("comment");
				addCommentaryItem.Click += delegate {
					NewModelCommentToDiagramCommand command = (NewModelCommentToDiagramCommand)CommandFactoryBase<NewModelCommentaryToDiagramCommandFactory>.Factory().Create(Controller.DiagramController);
					command.AnnotatedElement = this.Controller.Element;
					command.X = this.ActualWidth + 20;
					command.Y = 20;
					command.Set(Controller.DiagramController.ModelController, null);
					command.Execute();		
				    };
				return new ContextMenuItem[] {addCommentaryItem};
			}
		}

		/// <summary>
		/// Represented model element
		/// </summary>
		[ModelElement]
		public NamedElement ModelElement { get { return Controller != null ? Controller.NamedElement : null; } }

		/// <summary>
		/// ViewHelper of the element
		/// </summary>
		[ViewHelperElement]
		public virtual ClassViewHelper ViewHelper { get; protected set; }

		/// <summary>
		/// ViewHelper of the element
		/// </summary>
		PositionableElementViewHelper IResizable.ViewHelper { get { return ViewHelper; } }
		
		/// <summary>
		/// ViewHelper of the element
		/// </summary>
		PositionableElementViewHelper IAlignable.ViewHelper { get { return ViewHelper; } }

		/// <summary>
		/// Initializes view component representing a model element. Works with Representant Registration, part of IModelElementRepresentant interface.
		/// </summary>
		/// <param name="modelElement">Link to the model element represented by this view component</param>
		/// <param name="viewHelper">ViewHelper containing size, position and possibly more</param>
		/// <param name="controller">Controller for this view component</param>
		public abstract void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller);
		
		#region IDeletable Members

		/// <summary>
		/// Deletes the control from canvas (with all the JunctionPoints that it created via 
		/// <see cref="ConnectableDragThumb.CreateJunctionEnd()"/>).
		/// </summary>
		public override void DeleteFromCanvas()
		{
			base.DeleteFromCanvas();
			this.CloseBindings();
		}

		#endregion

        #region Versioned element highlighting support

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            XCaseCanvas.InvokeVersionedElementMouseEnter(this, Controller.Element);
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            XCaseCanvas.InvokeVersionedElementMouseLeave(this, Controller.Element);
        }

        #endregion 
	}
}