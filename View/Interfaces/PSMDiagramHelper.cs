using System.Collections;
using System.Linq;
using System.Collections.Generic;
using XCase.Model;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Defines support function related to the visual 
	/// representation of a PSM diagram.
	/// </summary>
	public class PSMDiagramHelper
	{
		/// <summary>
		/// Checks whether <paramref name="items"/> are all children of a common parent, that is a <see cref="PSMSuperordinateComponent"/> and all
		/// items are <see cref="PSMSubordinateComponent"/>s. 
		/// </summary>
		/// <param name="items">list of selectable items</param>
		/// <param name="parent">common parent for <paramref name="items"/> (if found)</param>
		/// <param name="components"><see cref="PSMSubordinateComponent"/>s that are represented by <paramref name="items"/></param>
		/// <returns>true if <paramref name="items"/> are all components of a common parent, false otherwise</returns>
		public static bool AreComponentsOfCommonParent(IEnumerable<IModelElementRepresentant> items, out PSMSuperordinateComponent parent, out IList<PSMSubordinateComponent> components)
		{
			parent = null;
			List<PSMSubordinateComponent> result = new List<PSMSubordinateComponent>();
			components = null;
			foreach (IModelElementRepresentant item in items)
			{
				if (!(item is IPSMSubordinateComponentRepresentant) && !(item is Controls.PSM_Class))
				{
					return false;
				}
				else
				{
					PSMSubordinateComponent component = null;
					if (item is Controls.PSM_Class)
					{
						PSMAssociationChild associationChild = (item as Controls.PSM_Class).ClassController.Class;
						if (associationChild != null && associationChild.ParentAssociation != null)
							component = associationChild.ParentAssociation;
					}
					else
					{
						component = ((IPSMSubordinateComponentRepresentant)item).ModelSubordinateComponent;
					}
					if (component == null)
					{
						return false;
					}

					
					PSMSuperordinateComponent itemParent = component.Parent;
					if (parent == null)
						parent = itemParent;
					else
					{
						if (parent != itemParent)
						{
							return false;
						}
					}

					if (!result.Contains(component))
						result.Add(component);
				}
			}
			components = result;
			return true;
		}
	}
}