using System.Windows.Controls;
using XCase.Model;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Selects child of a control in PSM tree.
	/// </summary>
	public class cmdSelectChild : cmdSelectRelatedElement
	{
		public cmdSelectChild(MainWindow mainWindow, Control control) : base(mainWindow, control)
		{
		}

		protected override Element GetRelatedElement(Element element)
		{
			return PSMTree.GetFirstChildOfElement(element);
		}
	}
}