using System.Windows;
using XCase.Model;
using XCase.View.Geometries;
using XCase.View.Interfaces;

namespace XCase.View.Controls
{
	/// <summary>
	/// Simple control used to connect <see cref="PSMSuperordinateComponent"/>
	/// (other then <see cref="PSMAssociation"/> to its parent and also components 
	/// of class union to the class union. 
	/// </summary>
	public class PSM_ComponentConnector : ISelectable, IPrimitiveJunctionTarget, IPSM_Connector
	{
		public XCaseJunction Junction { get; private set; }

		public IConnectable ParentControl { get; private set; }

		public IConnectable ChildControl { get; private set; }

		public XCaseCanvas XCaseCanvas { get; private set; }

		public ComponentConnectorViewHelper ViewHelper { get; private set; }

		/// <summary>
		/// Creates new instance of <see cref="PSM_ComponentConnector" />. 
		/// </summary>
		public PSM_ComponentConnector(XCaseCanvas canvas, IConnectable parentControl, IConnectable childControl, ComponentConnectorViewHelper viewHelper)
		{
			ParentControl = parentControl;
			ChildControl = childControl;
			XCaseCanvas = canvas;
			ViewHelper = viewHelper;
			if (viewHelper.Points.Count < 2)
			{
				viewHelper.Points.Clear();
				viewHelper.Points.AppendRange(JunctionGeometryHelper.ComputeOptimalConnection(parentControl, childControl));
				viewHelper.Points.PointsInvalid = true;
			}
			
			Junction = new XCaseJunction(canvas, viewHelper.Points);
			Junction.AutoPosModeOnly = true; 
			XCaseCanvas.Children.Add(Junction);
			Junction.NewConnection(parentControl, null, childControl, null, viewHelper.Points);
            if (childControl is System.Windows.Controls.Control)
            {
                if (childControl is PSM_ContentContainer)
                {
                    Junction.ContextMenu = ((PSM_ContentContainer)childControl).ContextMenu;
                }
                Junction.ContextMenu = ((System.Windows.Controls.Control) childControl).ContextMenu;
            }
		}

		#region Implementation of ISelectable

		/// <summary>
		/// Selected flag. Selected elements are highlighted on the canvas and are 
		/// target of commands. 
		/// </summary>
		public bool IsSelected
		{
			get { return Junction.IsSelected; }
			set { Junction.IsSelected = value; }
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

		Point IPrimitiveJunctionTarget.FindClosestPoint(Point point)
		{
			return JunctionGeometryHelper.FindClosestPoint(Junction, point);
		}

		#endregion

		public void DeleteFromCanvas()
		{
			Junction.DeleteFromCanvas();
		}

		public EJunctionCapStyle EndCapStyle
		{
			get { return Junction.EndCapStyle; }
			set { Junction.EndCapStyle = value; }
		}
	}
}