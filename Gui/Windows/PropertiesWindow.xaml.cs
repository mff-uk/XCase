using System.Windows;
using System.Windows.Media;
using AvalonDock;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;

namespace XCase.Gui
{
    /// <summary>
    /// Properties window for <c>XCase</c> GUI.
    /// Displays properties of visual element currently selected on the canvas or 
    /// in the Navigator window.
    /// 
    /// The type of selected object determines the xaml grid which is displayed
    /// in the Properties window.
    /// </summary>
    public partial class PropertiesWindow
    {
        /// <summary>
        /// Reference to the Main Window
        /// </summary> 
        public MainWindow MainWindowRef { get; set;}
        private XCaseCanvas activeDiagram;
        
        /// <summary>
        /// Diagram (tab) currently active in the editor
        /// </summary>
        public XCaseCanvas ActiveDiagram
        {
            get
            {
                return activeDiagram;
            }
            set
            {
                activeDiagram = value;
                Clear();
                if (activeDiagram != null /* && activeDiagram.SelectedItems.Count == 1 */)
                    SelectionChanged(activeDiagram.SelectedItems, null);
            }
        }

        /// <summary>
        /// Last displayed grid in Properties window
        /// </summary>
        private XCaseGridBase currentGrid;

        /// <summary>
        /// Initializes components of Properties window
        /// </summary>
        public PropertiesWindow()
        {
            InitializeComponent();

            //this.Icon = (ImageSource)FindResource("props");
        }

        /// <summary>
        /// Registers new diagram (canvas) for displaying its components in the Properties window.
        /// </summary>
        /// <param name="dm"></param>
        public void BindDiagram(ref DockingManager dm)
        {
            if (dm.ActiveDocument != null && dm.ActiveDocument is PanelWindow)

                (dm.ActiveDocument as PanelWindow).xCaseDrawComponent.Canvas.SelectedItems.CollectionChanged
                += SelectionChanged;
        }

        public static string tPIM_Class = typeof(PIM_Class).Name;
        public static string tPSM_Class = typeof(PSM_Class).Name;
        public static string tXCaseComment = typeof(XCaseComment).Name;
        public static string tPIM_Association = typeof(PIM_Association).Name;
        public static string tAssociationDiamond = typeof(AssociationDiamond).Name;
        public static string tPIM_AssociationClass = typeof(PIM_AssociationClass).Name;
        public static string tAssociationLabel = typeof(AssociationLabel).Name;
        public static string tPSM_ContentContainer = typeof(PSM_ContentContainer).Name;
        public static string tPSM_Association = typeof(PSM_Association).Name;
        public static string tPSM_AttributeContainer = typeof(PSM_AttributeContainer).Name;

        /// <summary>
        /// Invoked when a collection of selected elements on the canvas is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SelectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //return;
            SelectedItemsCollection selection = (SelectedItemsCollection)sender;

            if (currentGrid != null)
            {
                currentGrid.UpdateContent();
                currentGrid = null;
            }

            Clear();

            if (selection.Count == 1)
            {
                ISelectable selectedItem = selection[0];
                string selectedType = selectedItem.GetType().Name;
                if (selectedType == tPIM_Class)
                {
                    DisplaySelectedPIMClass((XCaseViewBase) selectedItem);
                }
                else if (selectedType == tPSM_Class)
                {
                    DisplaySelectedPSMClass((XCaseViewBase) selectedItem);
                }
                else if (selectedType == tXCaseComment)
                {
                    DisplaySelectedComment((XCaseComment) selectedItem);
                }
                else if (selectedType == tPIM_Association)
                {
                    DisplaySelectedAssociation((PIM_Association) selectedItem);
                }
                else if (selectedType == tAssociationDiamond)
                {
                    if (((AssociationDiamond) selectedItem).Association.AssociationClass != null)
                        DisplaySelectedAssociationClass(((AssociationDiamond) selectedItem).Association.AssociationClass);
                    else
                        DisplaySelectedAssociation(((AssociationDiamond) selectedItem).Association);
                }
                else if (selectedType == tPIM_AssociationClass)
                {
                    DisplaySelectedAssociationClass((PIM_AssociationClass) selectedItem);
                }
                else if (selectedType == tAssociationLabel)
                {
                    if (((AssociationLabel) selectedItem).Association != null)
                    {
                        if (((AssociationLabel) selectedItem).Association.AssociationClass == null)
                            DisplaySelectedAssociation(((AssociationLabel) selectedItem).Association);

                        else
                            DisplaySelectedAssociationClass(
                                ((AssociationLabel) selectedItem).Association.AssociationClass);
                    }
                    else if (((AssociationLabel) selectedItem).PSM_Association != null)
                    {
                        DisplaySelectedPSMAssociation(((AssociationLabel) selectedItem).PSM_Association);
                    }
                }
                else if (selectedType == tPSM_ContentContainer)
                {
                    DisplaySelectedPSMContentContainer((PSM_ContentContainer) selectedItem);
                }
                else if (selectedType == tPSM_Association)
                {
                    DisplaySelectedPSMAssociation((PSM_Association) selectedItem);
                }
                else if (selectedType == tPSM_AttributeContainer)
                {
                    DisplayAttributeContainer(((PSM_AttributeContainer) selectedItem));
                }
                else
                {
                }
            }
        }

        /// <summary>
        /// Clears Properties window so nothing is displayed here.
        /// </summary>
        private void Clear()
        {
            pimClassGrid.Visibility = Visibility.Collapsed;
            commentGrid.Visibility = Visibility.Collapsed;
            associationGrid.Visibility = Visibility.Collapsed;
            associationClassGrid.Visibility = Visibility.Collapsed;
            psmClassGrid.Visibility = Visibility.Collapsed;
            contentContainerGrid.Visibility = Visibility.Collapsed;
            psmAssociationGrid.Visibility = Visibility.Collapsed;
            attributeContainerGrid.Visibility = Visibility.Collapsed;
        }

        #region Display methods

		/// <summary>
		/// Displays PIM class selected in Navigator window. Only model properties of the PIM class
		/// are displayed, not appearance properties.
		/// </summary>
		/// <param name="c">class</param>
        public void DisplayModelClass(Class c)
        {
            if (currentGrid != null)
            {
                try
                {
                    currentGrid.UpdateContent();
                }
                catch { }
                currentGrid = null;
            }

            Clear();

            if (c is PIMClass)
            {
                currentGrid = pimClassGrid;
                pimClassGrid.Display((PIMClass)c, "PIM", MainWindowRef);
                pimClassGrid.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Displays currently selected PIM class in Properties window
        /// </summary>
        /// <param name="c"></param>
        private void DisplaySelectedPIMClass(XCaseViewBase c)
        { 
            currentGrid = pimClassGrid;
            pimClassGrid.Display(c, "PIM", MainWindowRef);
            pimClassGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Displays currently PSM class in Properties window.
        /// </summary>
        /// <param name="c">psm class</param>
        private void DisplaySelectedPSMClass(XCaseViewBase c)
        {
            currentGrid = psmClassGrid;
            psmClassGrid.Display(c, MainWindowRef);
            psmClassGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Displays currently selected comment in Properties window.
        /// </summary>
        /// <param name="c">comment</param>
        private void DisplaySelectedComment(XCaseComment c)
        {
            currentGrid = commentGrid;
            commentGrid.Display(c);
            commentGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Displays currently selected Association in Properties window.
        /// </summary>
        /// <param name="a"></param>
        private void DisplaySelectedAssociation(PIM_Association a)
        {
            currentGrid = associationGrid;
            associationGrid.Display(a);
            associationGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Displays currently selected Association class in Properties window.
        /// </summary>
        /// <param name="c">association class</param>
        private void DisplaySelectedAssociationClass(PIM_AssociationClass c)
        {
            currentGrid = associationClassGrid;
            associationClassGrid.Display(c, MainWindowRef);
            associationClassGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Displays currently selected <see cref="PSMContentContainer"/> in Properties window.
        /// </summary>
        /// <param name="c">content container</param>
        private void DisplaySelectedPSMContentContainer(PSM_ContentContainer c)
        {
            currentGrid = contentContainerGrid;
            contentContainerGrid.Display(c);
            contentContainerGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Displays currently selected <see cref="PSMAssociation"/> in Properties window.
        /// </summary>
        /// <param name="p"></param>
        private void DisplaySelectedPSMAssociation(PSM_Association p)
        {
            currentGrid = psmAssociationGrid;
            psmAssociationGrid.Display(p);
            psmAssociationGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Displays currently selected <see cref="PSMAttributeContainer"/> in Properties window.
        /// </summary>
        /// <param name="c">attribute container</param>
        private void DisplayAttributeContainer(PSM_AttributeContainer c)
        {
            currentGrid = attributeContainerGrid;
            attributeContainerGrid.Display(c);
            attributeContainerGrid.Visibility = Visibility.Visible;
        }

        #endregion

    }
}