using System.Windows;
using XCase.Controller;
using XCase.Model;
using XCase.View.Geometries;
using XCase.View.Interfaces;

namespace XCase.View.Controls
{
	/// <summary>
	/// Representation of <see cref="XCase.Model.AssociationClass"/>. Joined controls <see cref="PIM_Class"/>
	/// and <see cref="PIM_Association"/>. 
	/// </summary>
	public class PIM_AssociationClass: PIM_Class, IHasSelectionBounds
	{
        /// <summary>
        /// Initializes a view representation of a model element
        /// </summary>
        /// <param name="modelElement">Element to be represented</param>
        /// <param name="viewHelper">Element's viewHelper</param>
        /// <param name="controller">Element's controller</param>
        public override void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller)
        {
            PIM_Association associationRepresentant = new PIM_Association(XCaseCanvas);
            associationRepresentant.InitializeRepresentant(modelElement, ((AssociationClassViewHelper)viewHelper).AssociationViewHelper,
                                                           new AssociationController((Association)modelElement, controller.DiagramController));

            Association = associationRepresentant;
            Association.AssociationClass = this;
        	Association.AssociationName = null;
            Association.ViewHelper.MainLabelViewHelper.LabelVisible = false;

			AssociationClassViewHelper _viewHelper = (AssociationClassViewHelper)viewHelper;
			if (double.IsNaN(_viewHelper.X) || double.IsNaN(_viewHelper.Y))
			{
				Rect r = RectExtensions.GetEncompassingRectangle(associationRepresentant.participantElements.Values);
				if (associationRepresentant.participantElements.Count > 2)
				{
					_viewHelper.X = r.GetCenter().X + 30;
					_viewHelper.Y = r.GetCenter().Y;
				}
				else
				{
					_viewHelper.X = r.GetCenter().X;
					_viewHelper.Y = r.GetCenter().Y + 20;
				}
			}


            base.InitializeRepresentant(modelElement, viewHelper, controller);

            if (associationRepresentant.ViewHelper.UseDiamond)
            {
                if (((AssociationClassViewHelper)ViewHelper).Points.Count == 0)
                {
                    ((AssociationClassViewHelper)ViewHelper).Points.AppendRange(
                        JunctionGeometryHelper.ComputeOptimalConnection(this, associationRepresentant.Diamond));
                	((AssociationClassViewHelper)ViewHelper).Points.PointsInvalid = true;
                }
                junction = new XCaseJunction(XCaseCanvas, ((AssociationClassViewHelper)ViewHelper).Points)
                           	{ Pen = MediaLibrary.DashedBlackPen };
            	XCaseCanvas.Children.Add(junction);
                junction.NewConnection(this, null, Association.Diamond, null,
                                       ((AssociationClassViewHelper)ViewHelper).Points);
            	junction.SelectionOwner = this;
            }
            else
            {
                primitiveJunction = new XCasePrimitiveJunction(XCaseCanvas, this, Association)
                                        { Pen = MediaLibrary.DashedBlackPen };
            }
			this.StartBindings();
        }

	    public PIM_AssociationClass(XCaseCanvas xCaseCanvas)
			: base(xCaseCanvas)
		{
			
		}

		public PIM_Association Association { get; private set; }

		private XCaseJunction junction;

		private XCasePrimitiveJunction primitiveJunction;

		public override bool IsSelected
		{
			get
			{
				return base.IsSelected;
			}
			set
			{
				base.IsSelected = value;
				Association.IsSelected = value;
				if (junction != null)
					junction.IsSelected = value;
			}
		}

		public override void DeleteFromCanvas()
		{
			if (junction != null)
				junction.DeleteFromCanvas();
			if (primitiveJunction != null)
				primitiveJunction.DeleteFromCanvas();
			base.DeleteFromCanvas();
			Association.DeleteFromCanvas();
			this.CloseBindings();
		}

		public virtual Rect GetSelectionBounds()
		{
			Rect r = base.GetBounds();
			r.Union(Association.GetBounds());
			return r;
		}
	}
}