using System;
using System.Windows.Controls;
using System.Windows;
using XCase.View.Interfaces;
using System.Windows.Media;
using System.Diagnostics;
using XCase.Controller;
using XCase.Model;
using XCase.View.Geometries;

namespace XCase.View.Controls
{
	/// <summary>
	/// View representation of Content Choice (XSEM)
	/// </summary>
	public class PSM_ContentChoice : PSMElementViewBase, IPSMSubordinateComponentRepresentant, IPSMSuperordinateComponentRepresentant
	{
		readonly Border Border;

		#region Properties

		public override string ElementName
		{
			get;
			set;
		}

		public override NamedElementController Controller { get { return ContentChoiceController; } }

		public PSMContentChoice ContentChoice { get { return ContentChoiceController != null ? ContentChoiceController.ContentChoice : null; } }

		private PSM_ContentChoiceController controller;

		public PSM_ContentChoiceController ContentChoiceController
		{
			get { return controller; }
			set
			{
				Debug.Assert(controller == null, "Controller should be assigned only once.");
				Debug.Assert(value != null, "Controller can be assigned only with not null value.");
				controller = value;
			}
		}

		public override bool IsSelected
		{
			get { return base.IsSelected; }
			set
			{
				base.IsSelected = value;
				if (value)
				{
                    if (HighlightByEffect) Effect = MediaLibrary.SelectedHighlight;
                    else Border.BorderBrush = Brushes.Red;
					Canvas.SetZIndex(this, 2);
				}
				else
				{
                    if (HighlightByEffect) Effect = null;
                    else Border.BorderBrush = Brushes.Black;
					Canvas.SetZIndex(this, 0);
				}
			}
		}
		#endregion

		public PSMSubordinateComponent ModelSubordinateComponent
		{
			get { return (PSMSubordinateComponent)ModelElement; }
		}

		public PSMSuperordinateComponent ModelSuperordinateComponent
		{
			get { return (PSMSuperordinateComponent)ModelElement; }
		}

        /// <summary>
        /// Initializes a view representation of a model element
        /// </summary>
        /// <param name="modelElement">Element to be represented</param>
        /// <param name="viewHelper">Element's viewHelper</param>
        /// <param name="controller">Element's controller</param>
        public override void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller)
		{
			this.ContentChoiceController = (PSM_ContentChoiceController)controller;
			this.ViewHelper = (ClassViewHelper)viewHelper;
			XCaseCanvas.Children.Add(this);
			this.StartBindings();
			this.UpdateComponentsConnectors();
			InitializeConnector();
		}

		public PSM_ContentChoice(XCaseCanvas xCaseCanvas)
			: base(xCaseCanvas)
		{
			#region Template init
			Template = Application.Current.Resources["PSM_ContentChoiceTemplate"] as ControlTemplate;
			ApplyTemplate();

			PSM_ContentChoiceTemplate gr = Template.FindName("PSM_ContentChoiceGrid", this) as PSM_ContentChoiceTemplate;
			Border = gr.FindName("Border") as Border;
			connectorDecorator = gr.FindName("ConnectorDecorator") as Control;
			connectorDecorator.ApplyTemplate();
			#endregion

            ContextMenu = new ContextMenu();
            ContextMenuItem m = new ContextMenuItem("Remove");
            m.Icon = ContextMenuIcon.GetContextIcon("delete2");
            m.Click += delegate(object sender, RoutedEventArgs e) { ContentChoiceController.Remove(); };
            ContextMenu.Items.Add(m);

            ContextMenuItem moveToContentContainer = new ContextMenuItem("Move to content container");
            moveToContentContainer.Click += delegate
            {
                ContentChoiceController.MoveToContentContainer(null);
            };

		    ContextMenu.Items.Add(moveToContentContainer);

            ContextMenuItem moveToContentChoice = new ContextMenuItem("Move to content choice");
            moveToContentChoice.Click += delegate
            {
                ContentChoiceController.MoveToContentChoice(null);
            };

            ContextMenu.Items.Add(moveToContentChoice);

            ContextMenu.Items.Add(new Separator());
            foreach (ContextMenuItem item in ContextMenuItems)
            {
                ContextMenu.Items.Add(item);
            }
		}
	}
}
