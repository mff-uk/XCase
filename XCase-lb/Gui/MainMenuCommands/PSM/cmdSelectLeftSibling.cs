using System.Windows.Controls;
using XCase.Model;
using XCase.View.Controls;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Selects left sibling of a PSM element in the current diagram
	/// </summary>
	public class cmdSelectLeftSibling: cmdSelectRelatedElement
	{
		public cmdSelectLeftSibling(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
		}

		protected override Element GetRelatedElement(Element element)
		{
			return PSMTree.GetLeftSiblingOfElement(element);
		}
	}
}