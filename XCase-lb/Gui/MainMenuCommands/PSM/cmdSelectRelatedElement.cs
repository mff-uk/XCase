using System.Linq;
using System.Windows.Controls;
using XCase.Model;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Common parent for commands that traverse PSM tree
	/// </summary>
	public abstract class cmdSelectRelatedElement : MainMenuCommandBase
	{
		protected cmdSelectRelatedElement(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
		}

		public override void Execute(object parameter)
		{
			if (ActiveDiagramView.Diagram is PSMDiagram && ActiveDiagramView.SelectedItems.Count() == 1)
			{
				IModelElementRepresentant representant = ActiveDiagramView.SelectedItems.First() as IModelElementRepresentant;
				if (representant != null)
				{
					Element element = ActiveDiagramView.ElementRepresentations.GetElementRepresentedBy(representant);
					if (element != null)
					{
						Element relatedElement = GetRelatedElement(element);
						if (relatedElement != null && ActiveDiagramView.ElementRepresentations.IsElementPresent(relatedElement))
						{
							ISelectable selectable = ActiveDiagramView.ElementRepresentations[relatedElement] as ISelectable;
							if (selectable != null)
							{
								ActiveDiagramView.SelectedItems.SetSelection(selectable);
							}
						}
					}
				}
			}
		}

		protected abstract Element GetRelatedElement(Element element);

		public override bool CanExecute(object parameter)
		{
			if (ActiveDiagramView.Diagram is PSMDiagram && ActiveDiagramView.SelectedItems.Count() == 1)
			{
				IModelElementRepresentant representant = ActiveDiagramView.SelectedItems.First() as IModelElementRepresentant;
				if (representant != null)
				{
					Element element = ActiveDiagramView.ElementRepresentations.GetElementRepresentedBy(representant);
					if (element != null)
					{
						Element relatedElement = GetRelatedElement(element);
						if (relatedElement != null && ActiveDiagramView.ElementRepresentations.IsElementPresent(relatedElement))
						{
							ISelectable selectable = ActiveDiagramView.ElementRepresentations[relatedElement] as ISelectable;
							if (selectable != null)
							{
								return true; 
								//ActiveDiagramView.SelectedItems.SetSelection(selectable);
							}
						}
					}
				}
			}
			return false; 
		}
	}
}