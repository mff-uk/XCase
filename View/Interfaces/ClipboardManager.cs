using System;
using System.Linq;
using System.Collections.Generic;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using XCase.View.Controls;

namespace XCase.View.Interfaces
{
	public static class ClipboardManager
	{
		public enum EState
		{
			Empty,
			ContainsPIM,
			ContainsPSM
		}

		public static EState State { get; private set; }

		private static readonly IDictionary<Element, ViewHelper> content;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		static ClipboardManager()
		{
			content = new Dictionary<Element, ViewHelper>();
			State = EState.Empty;
		}

		public static void Clear()
		{
			State = EState.Empty;
			content.Clear();
		}

		public static void PutToClipboard(EState contentType, IDictionary<Element, ViewHelper> content)
		{
			if (contentType == EState.Empty)
			{
				throw new ArgumentException("Cannot use value EState.Empty. ");
			}
			Clear();
			if (content.Count > 0)
			{
				State = contentType;

				foreach (KeyValuePair<Element, ViewHelper> kvp in content)
				{
					content.Add(kvp);
				}
			}
		}

		public static void PasteContentToDiagram(XCaseCanvas targetDiagram, RegistrationSet registrationSet)
		{
            if (targetDiagram == null)
                return;
			if (State == EState.ContainsPIM)
			{
				List<Element> alreadyProcessed = new List<Element>();
				Dictionary<Element, ViewHelper> createdCopies = new Dictionary<Element, ViewHelper>();
				IncludeElementsCommand includeElementsCommand =
					(IncludeElementsCommand)IncludeElementsCommandFactory.Factory().Create(targetDiagram.Controller);

				/* Elements in PIM diagram are loaded in the order of their LoadPriority in registration set */
				foreach (RepresentantRegistration registration in registrationSet.OrderBy(reg => reg.LoadPriority))
				{
					foreach (KeyValuePair<Element, ViewHelper> pair in content)
					{
						Element element = pair.Key;
						ViewHelper viewHelper = pair.Value;

						if (!alreadyProcessed.Contains(element) && registration.ModelElementType.IsInstanceOfType(element))
						{
							if (!targetDiagram.Diagram.IsElementPresent(element)
								&& element.CanBePutToDiagram(targetDiagram.Diagram, includeElementsCommand.IncludedElements.Keys))
							{
								ViewHelper copiedViewHelper = viewHelper.CreateCopy(targetDiagram.Diagram, null);
								createdCopies.Add(element, copiedViewHelper);
								includeElementsCommand.IncludedElements.Add(element, copiedViewHelper);
							}
							alreadyProcessed.Add(element);
						}
					}
				}

				includeElementsCommand.Execute();
			}
			else if (State == EState.ContainsPSM)
			{
				IList<Element> ordered;
				PSMDiagram sourceDiagram = (PSMDiagram) content.First().Value.Diagram;
				if (!content.Keys.All(element => sourceDiagram.DiagramElements.Keys.Contains(element)))
				{
					throw new XCaseException("Cannot paste content. Source diagram was modified.") { ExceptionTitle = "Cannot paste content."};
				}
				// identify tree roots
				IEnumerable<Element> roots = PSMTree.IdentifySubtreeRoots(content.Keys);
				List<AddPSMClassToRootsCommand> addToRootsCommands = new List<AddPSMClassToRootsCommand>();

				// order 
				PSMTree.ReturnElementsInPSMOrder(content.Keys.Union(roots), out ordered, false);

				IncludeElementsCommand includeElementsCommand =
					(IncludeElementsCommand)IncludeElementsCommandFactory.Factory().Create(targetDiagram.Controller);

				// clone the selection 
                ElementCopiesMap createdCopies = new ElementCopiesMap();
				foreach (Element element in ordered)
				{
					Element copy = element.CreateCopy(targetDiagram.Controller.ModelController.Model, createdCopies);
					createdCopies[element] = copy;
				}
				// representants must be handled separately after all copies are created 
				PSMTree.CopyRepresentantsRelations(createdCopies);

				foreach (Element root in roots)
				{
					PSMClass _root = createdCopies[root] as PSMClass;
					if (_root != null)
					{
						AddPSMClassToRootsCommand command = (AddPSMClassToRootsCommand) AddPSMClassToRootsCommandFactory.Factory().Create(targetDiagram.Controller);
						command.Set(new ElementHolder<PSMClass>(_root));
						addToRootsCommands.Add(command);
					}
				}

				// clone viewhelpers
				Dictionary<Element, ViewHelper> createdViewHelpers = new Dictionary<Element, ViewHelper>();

				foreach (Element element in ordered)
				{
					ViewHelper viewHelper = sourceDiagram.DiagramElements[element];
					ViewHelper copiedViewHelper = viewHelper.CreateCopy(targetDiagram.Diagram, createdCopies);

					createdViewHelpers.Add(element, copiedViewHelper);
					includeElementsCommand.IncludedElements.Add(createdCopies[element], copiedViewHelper);
				}

				IMacroCommand macro = targetDiagram.Controller.BeginMacro();
				macro.CheckFirstOnlyInCanExecute = true; 
				// put to diagram
				includeElementsCommand.Execute();
				// new roots
				foreach (AddPSMClassToRootsCommand command in addToRootsCommands)
				{
					command.Execute();
				}
				targetDiagram.Controller.CommitMacro();
                
				#if DEBUG
				Tests.ModelIntegrity.ModelConsistency.CheckEverything(targetDiagram.Controller.ModelController.Project);
				#endif	
			}
		}

		public static void PutToClipboard(XCaseCanvas diagram)
		{
            if (diagram == null)
                return;
			Clear();
			if (diagram.SelectedRepresentants.Count() > 0)
			{
				State = diagram.Diagram is PSMDiagram ? EState.ContainsPSM : EState.ContainsPIM;
				foreach (IModelElementRepresentant elementRepresentant in diagram.SelectedRepresentants)
				{
					Element element = diagram.ElementRepresentations.GetElementRepresentedBy(elementRepresentant);
					content.Add(element, diagram.Diagram.DiagramElements[element]);
				}
			}
		}
	}
}