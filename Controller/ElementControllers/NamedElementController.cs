using System;
using System.Collections.Generic;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller
{
	/// <summary>
	/// Event with a string argument
	/// </summary>
	public delegate void StringEventHandler(object sender, StringEventArgs args);

	/// <summary>
	/// Argument for <see cref="StringEventHandler"/>
	/// </summary>
	public class StringEventArgs : EventArgs
	{
		/// <summary>
		/// Actual data of the event
		/// </summary>
		/// <value><see cref="String"/></value>
		public String Data { get; set; }
	}

	/// <summary>
	/// This is a Controller for a Named element. It is an abstract class inheriting the <see cref="ElementController"/>
	/// and adding the RenameElement method
	/// </summary>
	public abstract class NamedElementController : ElementController
	{
		/// <summary>
		/// Named element
		/// </summary>
		public NamedElement NamedElement
		{
			get { return (NamedElement)Element; }
		}

		protected NamedElementController(NamedElement element, DiagramController diagramController)
			: base(element, diagramController)
		{

		}

		/// <summary>
		/// Renames controller's associated element
		/// </summary>
		/// <typeparam name="ElementType">The type of the element.</typeparam>
		/// <param name="element">The element.</param>
		/// <param name="newName">new name for the element</param>
		/// <param name="controller">The controller that will issue renaming command.</param>
		/// <param name="containingCollection">Collection of elements of which element is a member.
		/// Can be left to null. If not null, it is checked, whether <paramref name="newName"/> is unique
		/// in the collection.</param>
		public static void RenameElement<ElementType>(ElementType element, string newName, ModelController controller, IEnumerable<ElementType> containingCollection)
			where ElementType : NamedElement
		{
			RenameElementCommand<ElementType> command = (RenameElementCommand<ElementType>)RenameElementCommandFactory<ElementType>.Factory().Create(controller);
			command.RenamedElement = element;
			command.ContainingCollection = containingCollection;
			command.NewName = newName;
			command.Execute();
		}

		/// <summary>
		/// Renames controller's associated element
		/// </summary>
		/// <typeparam name="ElementType">The type of the element.</typeparam>
		/// <param name="newName">new name for the element</param>
		/// <param name="containingCollection">Collection of elements of which element is a member.
		/// Can be left to null. If not null, it is checked, whether <paramref name="newName"/> is unique
		/// in the collection.</param>
		public void RenameElement<ElementType>(string newName, IEnumerable<ElementType> containingCollection)
			where ElementType : NamedElement
		{
			RenameElement((ElementType)NamedElement, newName, this.DiagramController.ModelController, containingCollection);
		}

        public void ChangeOntologyEquivalent(string newOntoEquiv)
        {
            ChangeOntologyEquivalent(NamedElement, newOntoEquiv, this.DiagramController.ModelController);
        }
        
        public static void ChangeOntologyEquivalent(NamedElement element, string newOntoEquiv, ModelController controller)
        {
            ChangeOntologyEquivalentCommand command = ChangeOntologyEquivalentCommandFactory.Factory().Create(controller) as ChangeOntologyEquivalentCommand;
            command.Element = element;
            command.NewOntoEquiv = newOntoEquiv;
            command.Execute();
        }
    }
}