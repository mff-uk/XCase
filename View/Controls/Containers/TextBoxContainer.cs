using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using XCase.View.Controls.Containers;

namespace XCase.View.Controls
{
	/// <summary>
	/// Interface for a container of textboxes. 
	/// <para>
	/// If more of <see cref="ITextBoxContainer"/> objects are placed 
	/// above each other, the borders are managed to show/hide as the 
	/// content of the control changes. 
	/// </para>
	/// <para>
	/// If more controls are used in
	/// this way, pass references to these controls to 
	/// <see cref="StackContainers"/> property and references to their
	/// borders to <see cref="StackBorders"/> property (in the same 
	/// order in which are the controls placed).
	/// </para>
	/// </summary>
	/// <example>
	/// <see cref="AttributesContainer"/> and <see cref="OperationsContainer"/> are 
	/// used to display attributes and operations of a class. If there are both 
	/// attributes and operations in the class, both containers are shown and a
	/// border between them is rendered to act as separator. 
	/// </example>
	public interface ITextBoxContainer
	{
		/// <summary>
		/// Visibility of the control
		/// </summary>
		/// <value><see cref="Visibility"/></value>
		Visibility Visibility { get; set; }

		/// <summary>
		/// Reference to an array of borders of those controls which form
		/// one logical control
		/// </summary>
		/// <value><see cref="Border"/></value>
		Border[] StackBorders { get; set; }

		/// <summary>
		/// Reference to an array of ITextBoxContainer which form
		/// one logical control
		/// </summary>
		/// <value><see cref="Border"/></value>
		ITextBoxContainer[] StackContainers { get; set; }

		/// <summary>
		/// Number of items in the collection
		/// </summary>
		/// <value><see cref="Int32"/></value>
		int ItemCount { get; }
	}

	/// <summary>
	/// Container of <see cref="EditableTextBox">EditableTextBoxes</see> 
	/// <para>
	/// If more of <see cref="ITextBoxContainer"/> objects are placed 
	/// above each other, the borders are managed to show/hide as the 
	/// content of the control changes. 
	/// </para>
	/// <para>
	/// If more controls are used in
	/// this way, pass references to these controls to 
	/// <see cref="StackContainers"/> property and references to their
	/// borders to <see cref="StackBorders"/> property (in the same 
	/// order in which are the controls placed).
	/// </para>
	/// </summary>
	/// <example>
	/// <see cref="AttributesContainer"/> and <see cref="OperationsContainer"/> are 
	/// used to display attributes and operations of a class. If there are both 
	/// attributes and operations in the class, both containers are shown and a
	/// border between them is rendered to act as separator. 
	/// </example>
	public class TextBoxContainer<T> : ITextBoxContainer
		where T : EditableTextBox
	{
		/// <summary>
		/// <see cref="XCaseCanvas"/> containing the control
		/// </summary>
		/// <value><see cref="XCaseCanvas"/></value>
		internal XCaseCanvas XCaseCanvas { get; set; }

		/// <summary>
		/// Panel where <see cref="EditableTextBox">EditableTextBoxes</see>
		/// are created
		/// </summary>
		protected Panel container;

		/// <summary>
		/// Reference to an array of borders of those controls which form
		/// one logical control
		/// </summary>
		/// <value><see cref="Border"/></value>
		public Border[] StackBorders { get; set; }

		/// <summary>
		/// Reference to an array of ITextBoxContainer which form
		/// one logical control
		/// </summary>
		/// <value><see cref="Border"/></value>
		public ITextBoxContainer[] StackContainers { get; set; }

		/// <summary>
		/// Number of items in the collection
		/// </summary>
		/// <value><see cref="Int32"/></value>
		public int ItemCount
		{
			get
			{
				return container.Children.Count;
			}
		}

		/// <summary>
		/// Visibility of the control (affects visibiity of <see cref="StackBorders"/>)
		/// </summary>
		/// <value><see cref="Visibility"/></value>
		public Visibility Visibility
		{
			get
			{
				return container.Visibility;
			}
			set
			{
				container.Visibility = value;
				ManageBorders();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextBoxContainer&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="container">Panel where <see cref="EditableTextBox">EditableTextBoxes</see>
		/// are created</param>
		/// <param name="xCaseCanvas"><see cref="XCaseCanvas"/> containing the control</param>
		public TextBoxContainer(Panel container, XCaseCanvas xCaseCanvas)
		{
			if (container == null)
				throw new ArgumentNullException("container");
			if (xCaseCanvas == null)
				throw new ArgumentNullException("xCaseCanvas");
			this.container = container;
			this.XCaseCanvas = xCaseCanvas;

			
		}

		/// <summary>
		/// Adds one item to <see cref="container"/>.
		/// </summary>
		/// <param name="item">adde item.</param>
		public void AddItem(T item)
		{
			if (Visibility == Visibility.Collapsed && container.Children.Count == 0)
				Visibility = Visibility.Visible;
			container.Children.Add(item);
			ManageBorders();
		}

		/// <summary>
		/// Removes item from <see cref="container"/>
		/// </summary>
		/// <param name="item">removed item.</param>
		public void RemoveItem(T item)
		{
			container.Children.Remove(item);
			ManageBorders();
		}

		/// <summary>
		/// Clears the container
		/// </summary>
		public void Clear()
		{
			this.container.Children.Clear();
			ManageBorders();
		}

		private void ManageBorders()
		{
			if (StackContainers != null && StackBorders != null)
			{
				ITextBoxContainer lastNonEmpty = StackContainers.LastOrDefault(cont => cont.ItemCount != 0 && cont.Visibility == Visibility.Visible);
				// use first bottom border only when some of the container is non-empty
				if (lastNonEmpty != null)
				{
					StackBorders[0].BorderThickness = new Thickness(0, 0, 0, 1);
				}
				else
				{
					StackBorders[0].BorderThickness = new Thickness(0);
				}

				// for all non-empty containers use bottom border except for the last one
				for (int i = 0; i < StackContainers.Length; i++)
				{
					ITextBoxContainer textBoxContainer = StackContainers[i];
					if (Visibility == Visibility.Visible &&
						StackBorders[i + 1].Visibility != Visibility.Visible)
					{
						StackBorders[i + 1].Visibility = Visibility.Visible;
					}
					if (textBoxContainer.ItemCount > 0 && textBoxContainer != lastNonEmpty && textBoxContainer.Visibility == Visibility.Visible)
					{
						StackBorders[i + 1].BorderThickness = new Thickness(0, 0, 0, 1);
						if (textBoxContainer != StackContainers.First())
							StackBorders[i + 1].Padding = new Thickness(2, 2, 2, 3);
					}
					else
					{
						StackBorders[i + 1].BorderThickness = new Thickness(0, 0, 0, 0);
						if (textBoxContainer != StackContainers.First())
							StackBorders[i + 1].Padding = new Thickness(0);
					}
				}

				
			}
		}

		/// <summary>
		/// Cancels editing in textboxes.
		/// </summary>
		public void CancelEdit()
		{
			foreach (object o in container.Children)
			{
				if (o is EditableTextBox)
				{
					(o as EditableTextBox).myEditable = false; 
				}
			}
		}
	}
}