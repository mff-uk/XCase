using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.View.Controls.Containers;
using XCase.View.Geometries;
using Application = System.Windows.Application;
using Control = System.Windows.Controls.Control;
using XCase.Model;
using XCase.View.Interfaces;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Linq;
using XCase.Controller.Dialogs;
using System.Collections.Generic;

namespace XCase.View.Controls
{
	/// <summary>
	/// View representation of PSM Class (XSEM)
	/// </summary>
	public class PSM_Class : PSMElementViewBase, ICreatesAssociationEnd, IPSMSuperordinateComponentRepresentant, IF2Renamable
	{
		#region visual elements
		private StackPanel AttributesSection;
		private Border AttributesBorder, HeaderBorder, ClassBorder;
		private PSMAttributesContainer classAttributes;
		private EditableTextBox txtClassName, txtElementNameLabel, txtRepresentedClassName;
		//private Control resizeDecorator;
		private ContextMenuItem miCollapseAttributes, miCollapseElementNameLabel, miElementNameLabelAlignRight, miAllowAnyAttribute, miAbstract, miConvertToRepresentant, miConvertToRegular, miGroupBy, miLocateRepresentedPSMClass, miFindStructuralRepresentatives;
		private Effect originalEffect;
		#endregion

		#region Properties

		private IPSMAttributesContainer ClassAttributes
		{
			get
			{
				return classAttributes;
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
                    else ClassBorder.BorderBrush = Brushes.Red;
                    //ClassBorder.BorderThickness = new Thickness(3);
					Panel.SetZIndex(this, 2);
					//resizeDecorator.Visibility = Visibility.Visible;
					SetRepresentedHighlight(true);
				}
				else
				{
                    if (HighlightByEffect) Effect = null;
                    else ClassBorder.BorderBrush = Brushes.Black;
                    //ClassBorder.BorderThickness = new Thickness(1); 
                    CancelAllEdits();
					Panel.SetZIndex(this, 0);
					//resizeDecorator.Visibility = Visibility.Collapsed;
					SetRepresentedHighlight(false);
				}
			}
		}

		public override string ElementName
		{
			get { return txtClassName.Text; }
			set
			{
				txtClassName.Text = value;
				UpdateLabels();
			}
		}

		[ModelPropertyMapping("IsAbstract")]
		public bool IsAbstract
		{
			get
			{
				return txtClassName.FontStyle == FontStyles.Italic;
			}
			set
			{
				if (value)
				{
					txtClassName.FontStyle = FontStyles.Italic;
					miAbstract.IsChecked = true;
				}
				else
				{
					txtClassName.FontStyle = FontStyles.Normal;
					miAbstract.IsChecked = false;
				}
			}
		}

		private bool allowAnyAttribute; 
		[ModelPropertyMapping("AllowAnyAttribute")]
		public bool AllowAnyAttribute
		{
			get
			{
				return allowAnyAttribute;
			}
			set
			{
				allowAnyAttribute = value;
				ClassAttributes.DisplayAnyAttribute = allowAnyAttribute;
				miAllowAnyAttribute.IsChecked = allowAnyAttribute;
			}
		}

		private bool isStructuralRepresentative;

        [ModelPropertyMapping("IsStructuralRepresentative")]
        public bool IsStructuralRepresentative
		{
            get
            {
                return isStructuralRepresentative;
            }
			set
			{
                if (isStructuralRepresentative == value) return;
			    isStructuralRepresentative = value;
                if (!isStructuralRepresentative) // PSM Class
				{
                    ClassBorder.Background = Brushes.SeaShell;
					HeaderBorder.Background = Brushes.AntiqueWhite;
					AttributesBorder.Background = Brushes.SeaShell;
                    miConvertToRepresentant.Visibility = Visibility.Visible;
                    miConvertToRegular.Visibility = Visibility.Collapsed;
					if (IsSelected) SetRepresentedHighlight(false);
				}
				else //PSM Structural Representative
				{
                    ClassBorder.Background = Brushes.AliceBlue;
					HeaderBorder.Background = Brushes.PowderBlue;
                    AttributesBorder.Background = Brushes.AliceBlue;
                    miConvertToRepresentant.Visibility = Visibility.Collapsed;
                    miConvertToRegular.Visibility = Visibility.Visible;
                    if (IsSelected) SetRepresentedHighlight(true);
				    miLocateRepresentedPSMClass.IsEnabled = value;
				}
                UpdateLabels();
			}
		}

	    private string representedPSMClassNameLabelText;

		[ModelPropertyMapping("RepresentedPSMClassName")]
		public string RepresentedPSMClassNameLabelText
		{
		    get { return representedPSMClassNameLabelText; }
		    set
			{
			    representedPSMClassNameLabelText = value;
			    UpdateLabels();
			}
		}

	    [ModelPropertyMappingAttribute("ElementName")]
		public string ElementNameLabelText
	    {
	        get { return txtElementNameLabel.Text; }
	        set
			{
				txtElementNameLabel.Text = value;
			}
	    }

	    private string pimClassNameLabelText;
	    [ModelPropertyMappingAttribute("RepresentedClassName")]
		public string PIMClassNameLabelText
	    {
	        get { return pimClassNameLabelText; }
	        set
			{
			    pimClassNameLabelText = value;
				UpdateLabels();
			}
	    }

	    [ViewHelperPropertyMapping("AttributesCollapsed")]
		public bool AttributesCollapsed
		{
			private get { return ClassAttributes.Visibility == Visibility.Collapsed; }
			set
			{
				ClassAttributes.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
				miCollapseAttributes.IsChecked = value;
			}
		}

		[ViewHelperPropertyMapping("ElementNameLabelCollapsed")]
		public bool ElementNameLabelCollapsed
		{
			private get { return txtElementNameLabel.Visibility == Visibility.Collapsed; }
			set
			{
				txtElementNameLabel.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
				miCollapseElementNameLabel.IsChecked = value;
			}
		}

		[ViewHelperPropertyMapping("ElementNameLabelAlignedRight")]
		public bool ElementNameLabelAlignedRight
		{
			private get { return txtElementNameLabel.TextAlignment == TextAlignment.Right; }
			set
			{
				txtElementNameLabel.TextAlignment = value ? TextAlignment.Right : TextAlignment.Left;
				miElementNameLabelAlignRight.IsChecked = value;
			}
		}

		private PSM_ClassController classController;

		public override NamedElementController Controller { get { return ClassController; } }

		public PSM_ClassController ClassController
		{
			get { return classController; }
			private set
			{
				Debug.Assert(value != null, "Controller can be assigned non-null value only.");
				classController = value;
				ClassAttributes.AttributeController = classController;
				txtClassName.Text = classController.Class.Name;
				txtElementNameLabel.Text = classController.Class.ElementName;
                UpdateLabels();
			}
		}

		public PSMClass PSMClass { get { return (PSMClass)ModelElement; } }

		public PSMSuperordinateComponent ModelSuperordinateComponent { get { return PSMClass; } }

		#endregion

		private void UpdateLabels()
		{
            if (PSMClass.IsStructuralRepresentative)
            {
                txtRepresentedClassName.ToolTip = "Represented PSM class name";
                if (RepresentedPSMClassNameLabelText != ElementName || PSMClass.IsStructuralRepresentativeExternal)
                {
                    txtRepresentedClassName.Visibility = Visibility.Visible;
                    if (!PSMClass.IsStructuralRepresentativeExternal)
                    {
                        txtRepresentedClassName.Text = RepresentedPSMClassNameLabelText;
                    }
                    else
                    {
                        txtRepresentedClassName.Text = PSMClass.GetStructuralRepresentativeExternalDiagramReference().NamespacePrefix + 
                            ":" + RepresentedPSMClassNameLabelText;
                    }
                }
                else
                {
                    txtRepresentedClassName.Visibility = Visibility.Collapsed;
                    txtRepresentedClassName.Text = string.Empty;
                }
            }
            else
            {
                txtRepresentedClassName.ToolTip = "Represented PIM class name";
                if (PIMClassNameLabelText != ElementName)
                {
                    txtRepresentedClassName.Visibility = Visibility.Visible;
                    txtRepresentedClassName.Text = PIMClassNameLabelText;
                }
                else
                {
                    txtRepresentedClassName.Visibility = Visibility.Collapsed;
                    txtRepresentedClassName.Text = string.Empty;
                }
            }
		}

		private void SetRepresentedHighlight(bool highlight)
		{
			if (IsStructuralRepresentative && !PSMClass.IsStructuralRepresentativeExternal)
			{
				PSM_Class represented = XCaseCanvas.ElementRepresentations[PSMClass.RepresentedPSMClass] as PSM_Class;
				if (represented != null)
				{
					represented.RepresentedHighlight(highlight);
				}
			}

		}

		/// <summary>
		/// Initializes a view representation of a model element
		/// </summary>
		/// <param name="modelElement">Element to be represented</param>
		/// <param name="viewHelper">Element's <c>viewHelper</c></param>
		/// <param name="controller">Element's controller</param>
		public override void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller)
		{
			InitTemplate();

			originalEffect = Effect;
			classAttributes = new PSMAttributesContainer(AttributesSection, XCaseCanvas);
			Border[] stackBorders = new[] { HeaderBorder, AttributesBorder };
			ITextBoxContainer[] stackContainers = new ITextBoxContainer[] { classAttributes };
			classAttributes.StackBorders = stackBorders;
			classAttributes.StackContainers = stackContainers;

			this.ClassController = (PSM_ClassController)controller;
			this.ViewHelper = (PSMElementViewHelper)viewHelper;
			XCaseCanvas.Children.Add(this);

            InitContextMenu();

            this.StartBindings();
			this.InitializeConnector();

			txtClassName.MouseDoubleClick += txtClassName_OnMouseDoubleClick;
			txtClassName.TextEdited += delegate(object sender, StringEventArgs args) { Controller.RenameElement<NamedElement>(args.Data, null); };
			txtElementNameLabel.TextEdited += delegate(object sender, StringEventArgs args) { ClassController.ChangeElementName(args.Data); };
			this.InitializeConnector();
            UpdateLabels();
		}

		public void RepresentedHighlight(bool highlight)
		{
			ClassBorder.Effect = highlight ? MediaLibrary.RepresentedHighlight : originalEffect;
		}

		public PSM_Class(XCaseCanvas xCaseCanvas)
			: base(xCaseCanvas)
		{
			SizeChanged += delegate(object sender, SizeChangedEventArgs e) { xCaseCanvas.InvokeElementSizeChanged(this, e); };
		}

		/// <summary>
		/// Initializes PSM Classes Context Menus
		/// </summary>
		private void InitContextMenu()
        {
            //txtClassName.ResetContextMenu();
            txtClassName.ContextMenuOpening += ContextMenu_ContextMenuOpening;
            txtRepresentedClassName.ContextMenu = null;
            ContextMenuItem miRemove = new ContextMenuItem("Remove");
            miRemove.Icon = ContextMenuIcon.GetContextIcon("delete2");
            miRemove.Click += delegate { ClassController.Remove(); };
            txtClassName.ContextMenu.Items.Add(miRemove);
            txtClassName.ContextMenu.Items.Add(new Separator());
           
            ContextMenuItem miDeriveNew = new ContextMenuItem("Derive another class as root");
            miDeriveNew.Icon = ContextMenuIcon.GetContextIcon("class");
            miDeriveNew.Click += delegate { ClassController.DeriveNewRootPSMClass(); };
            txtClassName.ContextMenu.Items.Add(miDeriveNew);

            ContextMenuItem miLocatePimClass = new ContextMenuItem("Find represented PIM class");
            miLocatePimClass.Icon = ContextMenuIcon.GetContextIcon("magnifier");
            miLocatePimClass.Click += delegate {
                ClassController.FindRepresentedClass();
            };
            txtClassName.ContextMenu.Items.Add(miLocatePimClass);

            miLocateRepresentedPSMClass = new ContextMenuItem("Find represented PSM class");
            miLocateRepresentedPSMClass.Icon = ContextMenuIcon.GetContextIcon("magnifier");
            miLocateRepresentedPSMClass.IsEnabled = false; 
            miLocateRepresentedPSMClass.Click += delegate
            {
                if (PSMClass.IsStructuralRepresentative)
                {
                    if (PSMClass.RepresentedPSMClass.Diagram == PSMClass.Diagram)
                    {
                        XCaseCanvas.SelectElement(PSMClass.RepresentedPSMClass);
                    }
                    else
                    {
                        ClassController.FindRepresentedPSMClass();
                    }
                }    
            };

            txtClassName.ContextMenu.Items.Add(miLocateRepresentedPSMClass);

            miFindStructuralRepresentatives = new ContextMenuItem("Find Structural Representatives");
            miFindStructuralRepresentatives.Icon = ContextMenuIcon.GetContextIcon("magnifier");
            miFindStructuralRepresentatives.IsEnabled = true;
            miFindStructuralRepresentatives.Click += delegate
            {
                if (PSMClass.IsReferencedFromStructuralRepresentative())
                {
                    SelectItemsDialog d = new SelectItemsDialog();
                    List<PSMClass> items = new List<Model.PSMClass>();
                    foreach (PSMClass r in PSMClass.RepresentedClassRepresentants)
                    {
                        if (r.RepresentedPSMClass == PSMClass)
                        {
                            items.Add(r);
                        }
                    }
                    d.SetItems(items);
                    if (d.ShowDialog() == true && d.selectedObjects.Count > 0)
                    {
                        XCaseCanvas.SelectElement((PSMElement)d.selectedObjects.First());
                    }
                }
            };
            txtClassName.ContextMenu.Items.Add(miFindStructuralRepresentatives);

            miConvertToRegular = new ContextMenuItem("Convert to regular PSM class");
            miConvertToRegular.Icon = ContextMenuIcon.GetContextIcon("class");
            miConvertToRegular.Click +=
                delegate
					{
						SetRepresentedHighlight(false);
						ClassController.SetPSMRepresentedClass(true);
					};

            miConvertToRepresentant = new ContextMenuItem("Convert to PSM Structural Representative");
            miConvertToRepresentant.Icon = ContextMenuIcon.GetContextIcon("class");
            miConvertToRepresentant.Click += delegate { ClassController.SetPSMRepresentedClass(false); };

            txtClassName.ContextMenu.Items.Add(miConvertToRepresentant);
            txtClassName.ContextMenu.Items.Add(miConvertToRegular);
            
            txtClassName.ContextMenu.Items.Add(new Separator());
            
			miAbstract = new ContextMenuItem("Abstract class");
			miAbstract.IsCheckable = true;
			miAbstract.Click += delegate { ClassController.ChangeAbstract(!PSMClass.IsAbstract); };
			txtClassName.ContextMenu.Items.Add(miAbstract);

			miAllowAnyAttribute = new ContextMenuItem("Allow any attribute");
			miAllowAnyAttribute.IsCheckable = true;
			miAllowAnyAttribute.Click += delegate { ClassController.ChangeAllowAnyAttributeDefinition(!PSMClass.AllowAnyAttribute); };
			txtClassName.ContextMenu.Items.Add(miAllowAnyAttribute);

            ContextMenuItem miAutosize = new ContextMenuItem("Autosize");
            miAutosize.Click += delegate { ViewController.ResizeElement(double.NaN, double.NaN, ViewHelper, XCaseCanvas.Controller); };
            txtClassName.ContextMenu.Items.Add(miAutosize);
            txtClassName.ContextMenu.Items.Add(new Separator());

            miCollapseAttributes = new ContextMenuItem("Hide attributes") { IsCheckable = true };
            txtClassName.ContextMenu.Items.Add(miCollapseAttributes);
            miCollapseAttributes.Checked +=
                delegate
                	{
						if (!AttributesCollapsed)
							ViewController.ChangeSectionVisibility(ViewHelper, SectionVisibilityCommand.ESectionVisibilityAction.CollapseAttributes, XCaseCanvas.Controller);
					};
            miCollapseAttributes.Unchecked +=
                delegate
                	{
						if (AttributesCollapsed)
							ViewController.ChangeSectionVisibility(ViewHelper, SectionVisibilityCommand.ESectionVisibilityAction.ShowAttributes, XCaseCanvas.Controller);
					};

            miCollapseElementNameLabel = new ContextMenuItem("Hide element name") { IsCheckable = true };
            miCollapseElementNameLabel.Checked +=
                delegate
                {
                    if (!ElementNameLabelCollapsed)
                        ViewController.ChangeSectionVisibility(ViewHelper, SectionVisibilityCommand.ESectionVisibilityAction.CollapseElementNameLabel, XCaseCanvas.Controller);
                };
            miCollapseElementNameLabel.Unchecked +=
                delegate
                {
                    if (ElementNameLabelCollapsed)
                        ViewController.ChangeSectionVisibility(ViewHelper, SectionVisibilityCommand.ESectionVisibilityAction.ShowElementNameLabel, XCaseCanvas.Controller);
                };
            txtClassName.ContextMenu.Items.Add(miCollapseElementNameLabel);
            txtClassName.ContextMenu.Items.Add(new Separator());

            miElementNameLabelAlignRight = new ContextMenuItem("Aligned to right") { IsCheckable = true };
            miElementNameLabelAlignRight.Checked +=
                delegate
                {
                    if (!ElementNameLabelAlignedRight)
                        ViewController.ChangeElementNameLabelAlignment(ViewHelper, true, XCaseCanvas.Controller);
                };

            miElementNameLabelAlignRight.Unchecked +=
                delegate
                {
                    if (ElementNameLabelAlignedRight)
                        ViewController.ChangeElementNameLabelAlignment(ViewHelper, false, XCaseCanvas.Controller);
                };
            //txtElementNameLabel.ResetContextMenu();
            txtElementNameLabel.ContextMenu.Items.Add(miElementNameLabelAlignRight);

            txtElementNameLabel.mi_Rename.Header = "Change XML Element name...";

            ContextMenuItem miDeleteElementName = new ContextMenuItem("Delete element name");
            miDeleteElementName.Icon = ContextMenuIcon.GetContextIcon("delete2");
            miDeleteElementName.Click += delegate { ClassController.ChangeElementName(string.Empty); };
            txtElementNameLabel.ContextMenu.Items.Add(miDeleteElementName);


            foreach (ContextMenuItem item in classAttributes.PropertiesMenuItems)
            {
                txtClassName.ContextMenu.Items.Add(item);
            }
            foreach (ContextMenuItem item in ContextMenuItems)
            {
                txtClassName.ContextMenu.Items.Add(item);
            }

            txtClassName.ContextMenu.Items.Add(new Separator());

            miGroupBy = new ContextMenuItem("Group by...");
			miGroupBy.Click += delegate { ClassController.GroupBy(); };
			txtClassName.ContextMenu.Items.Add(miGroupBy);

            ContextMenuItem miAddChildren = new ContextMenuItem("Add children...");
            miAddChildren.Icon = ContextMenuIcon.GetContextIcon("AddChildren");
            miAddChildren.Click += delegate { ClassController.AddChildren(); };
            txtClassName.ContextMenu.Items.Add(miAddChildren);

            ContextMenuItem miAddAttributes = new ContextMenuItem("Attributes...");
            miAddAttributes.Icon = ContextMenuIcon.GetContextIcon("AddAttributes");
            miAddAttributes.Click += delegate { ClassController.ShowClassDialog(); };
            txtClassName.ContextMenu.Items.Add(miAddAttributes);

            txtClassName.ContextMenu.Items.Add(new Separator());

            ContextMenuItem miProperties = new ContextMenuItem("Properties...");
            miProperties.Icon = ContextMenuIcon.GetContextIcon("props");
            miProperties.Click += delegate { ClassController.ShowClassDialog(); };
            txtClassName.ContextMenu.Items.Add(miProperties);
        }

        void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (ClassController.Class.RepresentedClassRepresentants.Where(psmClass => psmClass != ClassController.Class && psmClass.Diagram == ClassController.DiagramController.Diagram).Count() == 0)
                miConvertToRepresentant.IsEnabled = false;
            else miConvertToRepresentant.IsEnabled = true;

            if (ClassController.Class.IsStructuralRepresentative) //Structural Representative
            {
                miConvertToRepresentant.Visibility = Visibility.Collapsed;
                miConvertToRegular.Visibility = Visibility.Visible;
            }
            else //Regular PSM Class
            {
                miConvertToRepresentant.Visibility = Visibility.Visible;
                miConvertToRegular.Visibility = Visibility.Collapsed;
            }

        	PSMClass tmp = this.PSMClass; //compiler throws wierd error when this variable is not used...
        	miGroupBy.IsEnabled = tmp.Diagram.Roots.Contains(tmp);
        }

		private void InitTemplate()
		{
			base.Template = Application.Current.Resources["PSM_ClassTemplate"] as ControlTemplate;
			ApplyTemplate();
			Grid gr = Template.FindName("PSM_ClassGrid", this) as Grid;

			if (gr != null)
			{
				AttributesSection = gr.FindName("AttributesSection") as StackPanel;
				AttributesBorder = gr.FindName("AttributesBorder") as Border;
				HeaderBorder = gr.FindName("headerBorder") as Border;
				ClassBorder = gr.FindName("classBorder") as Border;
				txtClassName = gr.FindName("txtClassName") as EditableTextBox;
				txtRepresentedClassName = gr.FindName("txtRepresentedClassName") as EditableTextBox;
				txtElementNameLabel = gr.FindName("txtElementNameLabel") as EditableTextBox;
				txtElementNameLabel.CanBeEmpty = true;
				//resizeDecorator = gr.FindName("ResizeDecorator") as Control;
			}
			else
			{
				Debug.Assert(false, "Template not found");
			}

			//resizeDecorator.ApplyTemplate();
			//Grid g = resizeDecorator.Template.FindName("ResizeDecoratorGrid", resizeDecorator) as Grid;
			//foreach (ResizeThumb t in g.Children) t.belongsTo = this;
			connectorDecorator = gr.FindName("ConnectorDecorator") as Control;
			connectorDecorator.ApplyTemplate();
		}

		private void CancelAllEdits()
		{
			ClassAttributes.CancelEdit();
			txtClassName.myEditable = false;
			txtElementNameLabel.myEditable = false;
		}


		public PIM_AssociationEnd CreateAssociationEnd(Point preferredPosition, AssociationEndViewHelper viewHelper, AssociationEnd associationEnd)
		{
			PIM_AssociationEnd xend = new PIM_AssociationEnd(XCaseCanvas, viewHelper, associationEnd) { OwnerControl = this };
			((Canvas)connectorDecorator.Template.FindName("ConnectorDecoratorGrid", connectorDecorator)).Children.Add(xend);
			xend.Visibility = Visibility.Visible;
			xend.SetPreferedPosition(preferredPosition);
			createdJunctionEnds.Add(xend);
			return xend;
		}

		#region Event handlers

		private void txtClassName_OnMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			ClassController.ShowClassDialog();
		}

		#endregion

		public void F2Rename()
		{
			txtClassName.myEditable = true;
		}

        public void ElementRename()
        {
            txtElementNameLabel.myEditable = true;
        }
	}
}
