using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using XCase.Model;
using XCase.Controller.Interfaces;

namespace XCase.View.Controls
{
	/// <summary>
	/// Textbox displaying attribute
	/// </summary>
	public class AttributeTextBox : EditableTextBox
	{
		private Property property;

		private IControlsAttributes classController;

        public AttributeTextBox(Property property, IControlsAttributes classController)
		{
			this.property = property;
			this.classController = classController;

			#region property context menu

			this.ContextMenu = new ContextMenu();
            ContextMenuItem remove = new ContextMenuItem("Remove attribute");
            remove.Icon = ContextMenuIcon.GetContextIcon("delete2");
            remove.Click += delegate { classController.RemoveAttribute(property); };
			this.ContextMenu.Items.Add(remove);

            ContextMenuItem change = new ContextMenuItem("Properties...");
            change.Icon = ContextMenuIcon.GetContextIcon("pencil");
            change.Click += delegate { classController.ShowAttributeDialog(property); };
			this.ContextMenu.Items.Add(change);

			#endregion
			
			property.PropertyChanged += OnPropertyChangedEvent;
        	MouseDoubleClick += OnMouseDoubleClick;
			RefreshTextContent();
        	BindType();
		}

		
		private void OnPropertyChangedEvent(object sender, PropertyChangedEventArgs e)
		{
			RefreshTextContent();
			if (e.PropertyName == "Type")
			{
				BindType();
			}
		}

		private DataType type;

		private void BindType()
		{
			if (type != null)
			{
				type.PropertyChanged -= Type_PropertyChanged;
			}

			if (property.Type != null)
			{
				type = property.Type;
				type.PropertyChanged += Type_PropertyChanged;
			}
		}

		void Type_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			RefreshTextContent();
		}

		private void RefreshTextContent()
		{
			if (property.Type != null)
				this.Text = string.Format("{0} : {1}", property.Name, property.Type);
			else
				this.Text = property.Name;

			if (property.Default != null)
				this.Text += string.Format(" [{0}]", property.Default);

			if (!String.IsNullOrEmpty(property.MultiplicityString) && property.MultiplicityString != "1") 
			{
				this.Text += String.Format(" {{{0}}}", property.MultiplicityString);
			}
		}

		private void OnMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (classController != null)
				classController.ShowAttributeDialog(property);
		}

        #region Versioned element highlighting support

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            XCaseCanvas.InvokeVersionedElementMouseEnter(this, property);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            XCaseCanvas.InvokeVersionedElementMouseLeave(this, property);
        }

        #endregion 
	}
}
