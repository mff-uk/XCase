using System.Windows.Controls;
using System.Windows.Input;
using XCase.Controller;
using XCase.Model;
using XCase.Controller.Interfaces;

namespace XCase.View.Controls
{
	/// <summary>
	/// Textbox displaying operation.
	/// </summary>
	public class OperationTextBox : EditableTextBox
	{
		private Operation operation;

		private IControlsOperations classController;

        public OperationTextBox(Operation operation, IControlsOperations classController)
		{
			this.operation = operation;
			this.classController = classController;

			#region Operation context menu

			this.ContextMenu = new ContextMenu();
			ContextMenuItem remove = new ContextMenuItem("Remove operation");
            remove.Icon = ContextMenuIcon.GetContextIcon("delete2");
			remove.Click += delegate { classController.RemoveOperation(operation); };
			this.ContextMenu.Items.Add(remove);

			#endregion

			operation.PropertyChanged += delegate { RefreshTextContent(); };
			RefreshTextContent();
		}

		private void RefreshTextContent()
		{
			if (operation.Type != null)
				this.Text = string.Format("{0} {1}()", operation.Type, operation.Name);
			else
				this.Text = string.Format("{0}()", operation.Name);
		}

        #region Versioned element highlighting support

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            XCaseCanvas.InvokeVersionedElementMouseEnter(this, operation);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            XCaseCanvas.InvokeVersionedElementMouseLeave(this, operation);
        }

        #endregion 
	}
}