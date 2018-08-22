using XCase.Model;
using XCase.View.Interfaces;

namespace XCase.View.Controls
{
	/// <summary>
	/// Parent class to classes representing elements on PSM diagrams
	/// (elements are connected in a tree structure - see <see cref="Connector"/>)
	/// </summary>
	public abstract class PSMElementViewBase : XCaseViewBase
	{
        protected bool HighlightByEffect = false;
        
        protected PSMElementViewBase(XCaseCanvas xCaseCanvas) : base(xCaseCanvas)
		{
            Movable = false;
		}

		private IPSM_Connector connector;

		/// <summary>
		/// Control that connects the element to its parent element in PSM tree
		/// </summary>
		public IPSM_Connector Connector
		{
			get
			{
				return connector;
			}
			internal set
			{
				if (Connector != null)
					Connector.DeleteFromCanvas();
				connector = value;
			}
		}

		/// <summary>
		/// Returns parent of the element represented by this control.
		/// </summary>
		/// <returns></returns>
		private Element GetModelParent()
		{
			if (ModelElement is PSMSubordinateComponent)
				return ((PSMSubordinateComponent)ModelElement).Parent;
			if (ModelElement is PSMClass)
				return ((PSMClass)ModelElement).ParentUnion;
			if (ModelElement is PSMClassUnion)
				return ((PSMClassUnion)ModelElement).ParentUnion;
			return null;
		}

		private bool parentChangeBound = false; 

		/// <summary>
		/// Connects the control to its parent via <see cref="PSM_ComponentConnector"/> control. 
		/// </summary>
		public void InitializeConnector()
		{
			IConnectable visualParentInPSMTree;

			if (!parentChangeBound)
			{
				ModelElement.PropertyChanged += ModelPropertyChanged;
				parentChangeBound = true; 
			}

			if (GetModelParent() != null)
			{
				visualParentInPSMTree = XCaseCanvas.ElementRepresentations[GetModelParent()] as IConnectable;
				InitializeConnector(visualParentInPSMTree);
			}
		}

		/// <summary>
		/// Connects the control <param name="visualParentInPSMTree"/> via <see cref="PSM_ComponentConnector"/> control. 
		/// </summary>
		public void InitializeConnector(IConnectable visualParentInPSMTree)
		{
			if (visualParentInPSMTree != null)
			{
				Connector = new PSM_ComponentConnector(XCaseCanvas, visualParentInPSMTree, this,
				                                       ((PSMElementViewHelper)ViewHelper).ConnectorViewHelper);
				Connector.Junction.SelectionOwner = this;
			}
		}

		/// <summary>
		/// Updates the connection to parent control when parent control changes
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		void ModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Parent" || e.PropertyName == "ParentUnion")
			{
				if ((GetModelParent()) != null &&
					XCaseCanvas.ElementRepresentations.IsElementPresent((GetModelParent())))
				{
					((PSMElementViewHelper)ViewHelper).ConnectorViewHelper.Points.Clear();
					InitializeConnector();
				}
				else
				{
					if (Connector is PSM_ComponentConnector)
						Connector = null;
				}
			}
		}

		private bool isSelected;

		/// <summary>
		/// Selected flag. Selected elements are highlighted on the canvas and are 
		/// target of commands. 
		/// </summary>
		public override bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
				if (Connector is PSM_ComponentConnector)
					((PSM_ComponentConnector)Connector).IsSelected = value;
			}
		}

		/// <summary>
		/// Deletes the control from canvas (with all the JunctionPoints that it created via 
		/// <see cref="ConnectableDragThumb.CreateJunctionEnd()"/>).
		/// </summary>
		public override void DeleteFromCanvas()
		{
			if (parentChangeBound)
			{
				ModelElement.PropertyChanged -= ModelPropertyChanged;
				parentChangeBound = false; 
			}
			base.DeleteFromCanvas();
			if (Connector != null)
				Connector.DeleteFromCanvas();
		}
	}
}