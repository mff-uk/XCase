using System.Windows.Controls;
using XCase.Model;
using System.Linq;
using XCase.View.Controls;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Selects right sibling of a PSM element in the current diagram
	/// </summary>
	public class cmdSelectRightSibling: cmdSelectRelatedElement
	{
		public cmdSelectRightSibling(MainWindow mainWindow, Control control) : base(mainWindow, control)
		{
		}

		protected override Element GetRelatedElement(Element element)
		{
			return PSMTree.GetRightSiblingOfElement(element);
		}
	}
}