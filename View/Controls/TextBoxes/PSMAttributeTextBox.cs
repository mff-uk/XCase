using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XCase.Controller;
using XCase.Model;
using XCase.Controller.Interfaces;
using System.ComponentModel;

namespace XCase.View.Controls
{
	/// <summary>
	/// Textbox displaying PSM attribute.
	/// </summary>
	public class PSMAttributeTextBox : EditableTextBox
	{
		private PSMAttribute attribute;

		private IControlsPSMAttributes classController;

        ContextMenuItem moveUp;

        ContextMenuItem moveDown;

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);

            moveUp.IsEnabled = classController.AttributeHolder.PSMAttributes.IndexOf(attribute) > 0;
            moveDown.IsEnabled = classController.AttributeHolder.PSMAttributes.IndexOf(attribute) < classController.AttributeHolder.PSMAttributes.Count - 1;
        }

	    public PSMAttributeTextBox(PSMAttribute Attribute, IControlsPSMAttributes attributesController)
		{
            this.attribute = Attribute;
			this.classController = attributesController;

			#region property context menu

			this.ContextMenu = new ContextMenu();

            ContextMenuItem miPropagatePIMLess = new ContextMenuItem("Propagate to PIM");
            miPropagatePIMLess.Icon = ContextMenuIcon.GetContextIcon("AddAttributes");
            miPropagatePIMLess.Click += delegate { attributesController.PropagatePIMLess(attribute); };
            miPropagatePIMLess.IsEnabled = attribute.RepresentedAttribute == null;
            attribute.PropertyChanged +=
                delegate(object sender, PropertyChangedEventArgs e)
                {
                    if (e.PropertyName == "RepresentedAttribute")
                        miPropagatePIMLess.IsEnabled = attribute.RepresentedAttribute == null;
                };
            this.ContextMenu.Items.Add(miPropagatePIMLess);

            ContextMenuItem remove = new ContextMenuItem("Remove attribute");
            remove.Icon = ContextMenuIcon.GetContextIcon("delete2");
            remove.Click += delegate { attributesController.RemoveAttribute(attribute); };
			this.ContextMenu.Items.Add(remove);

			if (attributesController is PSM_AttributeContainerController)
			{
				ContextMenuItem returnAttribute = new ContextMenuItem("Move back to class");
				returnAttribute.Click += delegate { ((PSM_AttributeContainerController)attributesController).MoveAttributeBackToClass(attribute); };
				this.ContextMenu.Items.Add(returnAttribute);
			}
            else
			{
                ContextMenuItem miMoveToAC = new ContextMenuItem("Move to attribute container");
                miMoveToAC.Click += delegate { ((PSM_ClassController)attributesController).MoveAttributesToAttributeContainer(null, attribute); };
                this.ContextMenu.Items.Add(miMoveToAC);
			}

        	ContextMenuItem change = new ContextMenuItem("Properties...");
            change.Icon = ContextMenuIcon.GetContextIcon("pencil");
            change.Click += delegate { attributesController.ShowAttributeDialog(attribute); };
			this.ContextMenu.Items.Add(change);

            moveUp = new ContextMenuItem("Move up");
            moveUp.Click += delegate { attributesController.MoveAttributeUp(attribute); };
            this.ContextMenu.Items.Add(moveUp);


            moveDown = new ContextMenuItem("Move down");
            moveDown.Click += delegate { attributesController.MoveAttributeDown(attribute); };
            this.ContextMenu.Items.Add(moveDown);


			#endregion

            attribute.PropertyChanged += delegate { RefreshTextContent(); };
			MouseDoubleClick += OnMouseDoubleClick;
			RefreshTextContent();
		}

		public PSMAttributeTextBox(IControlsPSMAttributes attributesController)
		{
			this.classController = attributesController;
			this.Text = "{ any attribute }";
			this.ContextMenu = new ContextMenu();
			                   	
			ContextMenuItem removeDefinition = new ContextMenuItem("Remove");
			removeDefinition.Click += delegate
			                          	{
			                          		(this.classController as PSM_ClassController).ChangeAllowAnyAttributeDefinition(false);
			                          	};
			this.ContextMenu.Items.Add(removeDefinition);
		}

		private void RefreshTextContent()
		{
			if (!string.IsNullOrEmpty(attribute.Alias) && attribute.Alias != attribute.Name)
				this.Text = string.Format("{0} AS {1}", attribute.Name, attribute.Alias);
			else
				this.Text = attribute.Name;
            if (attribute.Type != null)
                this.Text += string.Format(" : {0}", attribute.Type);

			if (attribute.Default != null)
				this.Text += string.Format(" [{0}]", attribute.Default);

			if (!string.IsNullOrEmpty(attribute.MultiplicityString) && attribute.MultiplicityString != "1")
			{
				this.Text += string.Format(" {{{0}}}", attribute.MultiplicityString);
			}
		}

		private void OnMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (classController != null)
				classController.ShowAttributeDialog(attribute);
		}

        #region Versioned element highlighting support

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            XCaseCanvas.InvokeVersionedElementMouseEnter(this, attribute);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            XCaseCanvas.InvokeVersionedElementMouseLeave(this, attribute);
        }

        #endregion 
	}
}
