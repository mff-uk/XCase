using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using XCase.Model;
using XCase.View.Interfaces;

namespace XCase.View.Controls
{
	public class ComponentData
	{
		public PSM_ComponentConnector Connector { get; set; }
		public IConnectable ConnectedChild { get; set; }
		public IModelElementRepresentant ComponentRepresentant { get; set; }
	}

	/// <summary>
	/// Handles visualisation of <see cref="PSMSuperordinateComponent">PSMSuperOrdinateComponent</see>'s <see cref="PSMSuperordinateComponent.Components">Components</see> 
	/// collection. Each time this collection changes (in model), PSM_ComponentsManager creates or updates 
	/// visualization of the added or removed component and also creates or removes 
	/// <see cref="PSM_ComponentConnector"/> between the representation of the superordinate element
	/// and the component
	/// </summary>
	public class PSM_ComponentsManager
	{
		public XCaseCanvas XCaseCanavs { get; private set; }

		public PSMDiagram Diagram { get { return (PSMDiagram)XCaseCanavs.Diagram; } }

		public PSMSuperordinateComponent SuperordinateElement { get; private set; }

		public XCaseViewBase SuperordinateRepresentation { get; private set; }

		public Dictionary<PSMSubordinateComponent, ComponentData> ComponentsData { get; private set; }

		/// <summary>
		/// Creates new instance of <see cref="PSM_ComponentsManager" />.
		/// </summary>
		/// <param name="xCaseCanvas">canvas of the PSM diagram</param>
		/// <param name="superordinateElement">element whose components are visualized by the created PSM_ComponentsManager</param>
		/// <param name="superordinateRepresentation">representation of <paramref name="superordinateElement"/> on <paramref name="xCaseCanvas"/></param>
		public PSM_ComponentsManager(XCaseCanvas xCaseCanvas, PSMSuperordinateComponent superordinateElement, XCaseViewBase superordinateRepresentation)
		{
			if (!(xCaseCanvas.Diagram is PSMDiagram))
			{
				throw new ArgumentException("Component manager can be put only on canvas representing PSM diagram. ", "xCaseCanvas");
			}

			XCaseCanavs = xCaseCanvas;
			SuperordinateElement = superordinateElement;
			SuperordinateRepresentation = superordinateRepresentation;

			ComponentsData = new Dictionary<PSMSubordinateComponent, ComponentData>();

			SuperordinateElement.Components.CollectionChanged += Components_CollectionChanged;
		}

		void Components_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (var item in e.NewItems)
					{
						AddComponentRepresentation((PSMSubordinateComponent)item);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (var item in e.NewItems)
					{
						RemoveComponentRepresentation((PSMSubordinateComponent)item);
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					throw new NotImplementedException("Method or operation is not implemented.");
					break;
				case NotifyCollectionChangedAction.Move:
					// ignored and handled by auto layouting ?
					break;
				case NotifyCollectionChangedAction.Reset:
					throw new NotImplementedException("Method or operation is not implemented.");
					break;
			}
		}

		private void AddComponentRepresentation(PSMSubordinateComponent component)
		{
			IModelElementRepresentant componentRepresentant = null;
			IConnectable connectedChild = null;
			PSM_ComponentConnector connector = null;
			
			if (component is PSMAssociation)
			{
				connectedChild = (IConnectable)((PSMAssociation)component).Child;
				ComponentConnectorViewHelper viewHelper = ((PSMElementViewHelper)((PSMElementViewBase)connectedChild).ViewHelper).ConnectorViewHelper;
				connector = new PSM_ComponentConnector(XCaseCanavs, SuperordinateRepresentation, connectedChild, viewHelper);
			}
			else
			{
				componentRepresentant = XCaseCanavs.ElementRepresentations[component];
				connectedChild = (IConnectable)componentRepresentant;
				ComponentConnectorViewHelper viewHelper = ((PSMElementViewHelper)((PSMElementViewBase)connectedChild).ViewHelper).ConnectorViewHelper;
				connector = new PSM_ComponentConnector(XCaseCanavs, SuperordinateRepresentation, connectedChild, viewHelper);
			}
			
			ComponentData data = new ComponentData()
			                     	{
			                     		ComponentRepresentant = componentRepresentant,
			                     		ConnectedChild = connectedChild,
			                     		Connector = connector
			                     	};
			ComponentsData[component] = data;
		}

		private void RemoveComponentRepresentation(PSMSubordinateComponent component)
		{
			ComponentData data = ComponentsData[component];	
			ComponentsData.Remove(component);
			XCaseCanavs.Children.Remove(data.Connector.Junction);
		}

		public void Unbind()
		{
			SuperordinateElement.Components.CollectionChanged -= Components_CollectionChanged;
		}
	}
}
