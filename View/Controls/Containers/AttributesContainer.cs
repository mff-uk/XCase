using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using XCase.Model;
using XCase.Controller.Interfaces;

namespace XCase.View.Controls.Containers
{
	/// <summary>
	/// Interface for control displaying attributes. 
	/// </summary>
	public interface IAttributesContainer : ITextBoxContainer
	{
		/// <summary>
		/// Reference to <see cref="IControlsAttributes"/>
		/// </summary>
		IControlsAttributes AttributeController
		{
			get;
			set;
		}

		/// <summary>
		/// Visualized collection 
		/// </summary>
		ObservableCollection<Property> AttributesCollection
		{
			get;
		}

		/// <summary>
		/// Adds visualization of <paramref name="attribute"/> to the control
		/// </summary>
		/// <param name="attribute">visualized attribute</param>
		/// <returns>Control displaying the attribute</returns>
		AttributeTextBox AddAttribute(Property attribute);

		/// <summary>
		/// Removes visualization of <paramref name="attribute"/>/
		/// </summary>
		/// <param name="attribute">removed attribute</param>
		void RemoveAttribute(Property attribute);
		
		/// <summary>
		/// Reflects changs in <see cref="AttributesCollection"/>.
		/// </summary>
		/// <param name="sender">sender of the event</param>
		/// <param name="e">event arguments</param>
		void attributesCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e);
		
		/// <summary>
		/// Removes all attriutes
		/// </summary>
		void Clear();
		
		/// <summary>
		/// Cancels editing if any of the displayed attributes is being edited. 
		/// </summary>
		void CancelEdit();
	}

	/// <summary>
	/// Implementation of <see cref="IAttributesContainer"/>, displays attributes
	/// using <see cref="AttributeTextBox">AttributeTextBoxes</see>.
	/// </summary>
	public class AttributesContainer : TextBoxContainer<AttributeTextBox>, IAttributesContainer
	{
        private IControlsAttributes attributeController;

		/// <summary>
		/// Reference to <see cref="IControlsAttributes"/>
		/// </summary>
		public IControlsAttributes AttributeController
		{
			get
			{
				return attributeController;
			}
			set
			{
				attributeController = value;
				attributeController.AttributeHolder.Attributes.CollectionChanged += attributesCollection_CollectionChanged;
				attributesCollection_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                this.container.Visibility = attributeController.AttributeHolder.Attributes.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		/// <summary>
		/// Visualized collection 
		/// </summary>
		public ObservableCollection<Property> AttributesCollection
		{
			get
			{
                return attributeController.AttributeHolder.Attributes;
			}
		}

		/// <summary>
		/// Returns context menu items for operations provided by the control.
		/// </summary>
		internal IEnumerable<ContextMenuItem> PropertiesMenuItems
		{
			get
			{
				ContextMenuItem addPropertyItem = new ContextMenuItem("Add new attribute...");
                addPropertyItem.Icon = 
                    ContextMenuIcon.GetContextIcon("AddAttributes");
				addPropertyItem.Click += delegate
				                         	{
				                         		attributeController.AddNewAttribute(null);
				                         	};

				return new ContextMenuItem[]
				       	{
				       		addPropertyItem
				       	};
			}
		}

		/// <summary>
		/// Creates new instance of <see cref="AttributesContainer" />. 
		/// </summary>
		/// <param name="container">Panel used to display the items</param>
		/// <param name="xCaseCanvas">canvas owning the control</param>
		public AttributesContainer(Panel container, XCaseCanvas xCaseCanvas)
			: base(container, xCaseCanvas)
		{

		}

		/// <summary>
		/// Adds visualization of <paramref name="attribute"/> to the control
		/// </summary>
		/// <param name="attribute">visualized attribute</param>
		/// <returns>Control displaying the attribute</returns>
		public AttributeTextBox AddAttribute(Property attribute)
		{
			AttributeTextBox t = new AttributeTextBox(attribute, attributeController);
			base.AddItem(t);
			return t;
		}

		/// <summary>
		/// Removes visualization of <paramref name="attribute"/>/
		/// </summary>
		/// <param name="attribute">remoed attribute</param>
		public void RemoveAttribute(Property attribute)
		{
			attributeController.RemoveAttribute(attribute);
		}

		/// <summary>
		/// Reflects changs in <see cref="IAttributesContainer.AttributesCollection"/>.
		/// </summary>
		/// <param name="sender">sender of the event</param>
		/// <param name="e">event arguments</param>
		public void attributesCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Clear();

			foreach (Property attribute in AttributesCollection)
			{
				AddAttribute(attribute);
			}
		}
	}
}