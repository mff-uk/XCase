using System.Linq;
using System.Windows.Controls;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Geometries;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Initializes dragging comment into the active diagram
	/// </summary>
	/// <seealso cref="XCaseCanvas.DraggingElementState"/>
	public class cmdComment: cmdInsertElement
	{
		public cmdComment(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
		}

		private string text;

		protected override DragButtonData PrepareButtonData()
		{
			text = NameSuggestor<Comment>.SuggestUniqueName(ActiveDiagramView.Controller.ModelController.Model.Comments,
			                                                "Comment", comment => comment.Body);
			return new DragButtonData(NewModelCommentaryToDiagramCommandFactory.Factory(),
			                          new XCaseComment(ActiveDiagramView)
			                          	{
			                          		CommentText = text
			                          	});
		}

		protected override void InitializeCommand(NewPositionableElementMacroCommand command)
		{
			base.InitializeCommand(command);
			((NewModelCommentToDiagramCommand)command).Text = text;
			if (ActiveDiagramView.SelectedRepresentants.Count() == 1)
			{
				Element element = ActiveDiagramView.ElementRepresentations.GetElementRepresentedBy(ActiveDiagramView.SelectedRepresentants.First());
                if (element is Comment)
                {
                    if ((element as Comment).AnnotatedElement != null)
                        ((NewModelCommentToDiagramCommand)command).AnnotatedElement = (element as Comment).AnnotatedElement;
                }
                else ((NewModelCommentToDiagramCommand)command).AnnotatedElement = element; 
				DragThumb thumb = ActiveDiagramView.SelectedRepresentants.First() as DragThumb;
				ISelectable selectable = ActiveDiagramView.SelectedRepresentants.First() as ISelectable;
				if (command.X == 40 && command.Y == 40)
				{
					if (thumb != null)
					{
						command.X = (thumb).ActualWidth + 20;
						command.Y = 20;
					} else if (selectable != null)
					{
						command.X = selectable.GetBounds().GetCenter().X + 40; 
						command.X = selectable.GetBounds().GetCenter().Y; 
					}

				}
				else
				{
					if (thumb != null)
					{
						command.X -= (thumb).Left;
						command.Y -= (thumb).Top;
					}
				}
				
			}
                
		}
	}
}