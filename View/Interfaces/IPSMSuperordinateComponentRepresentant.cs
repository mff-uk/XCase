using XCase.Model;
using XCase.View.Controls;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Extension of <see cref="IModelElementRepresentant"/> interface
	/// for representants of <see cref="PSMSuperordinateComponent"/>s. 
	/// </summary>
	public interface IPSMSuperordinateComponentRepresentant : IModelElementRepresentant, IConnectable
	{
		/// <summary>
		/// Returns undrlying model element as <see cref="PSMSuperordinateComponent"/>
		/// </summary>
		PSMSuperordinateComponent ModelSuperordinateComponent
		{
			get;
		}
	}

	/// <summary>
	/// Defines extension methods for IPSMSuperordinateComponentRepresentant interface.
	/// </summary>
	public static class IPSMSuperordinateComponentRepresentantSupport
	{
		/// <summary>
		/// Updates the components connectors for each component in <paramref name="parent"/>'s
		/// <see cref="PSMSuperordinateComponent.Components"/>. 
		/// </summary>
		/// <param name="parent">The superordinate component.</param>
		public static void UpdateComponentsConnectors(this IPSMSuperordinateComponentRepresentant parent)
		{
			foreach (PSMSubordinateComponent component in parent.ModelSuperordinateComponent.Components)
			{
				if (parent.XCaseCanvas.ElementRepresentations.IsElementPresent(component))
				{
					PSMElementViewBase componentView = parent.XCaseCanvas.ElementRepresentations[component] as PSMElementViewBase;
					if (componentView != null)
					{
						((PSMElementViewHelper)componentView.ViewHelper).ConnectorViewHelper.Points.Clear();
						componentView.InitializeConnector(parent);
					}				  
					PSM_Association associationView = parent.XCaseCanvas.ElementRepresentations[component] as PSM_Association;
					if (associationView != null)
					{
						associationView.UpdateConnection(parent);
					}
				}
			}
		}
	}

}