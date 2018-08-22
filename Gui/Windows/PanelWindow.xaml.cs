using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using XCase.Controller;
using XCase.Translation.DataGenerator;
using XCase.View.Controls;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.Controller.Commands.Helpers;
using XCase.View.Interfaces;
using XCase.View;

namespace XCase.Gui
{
    /// <summary>
    /// Interaction logic for PanelWindow.xaml
    /// </summary>
    public partial class PanelWindow
    {
		public DiagramController DiagramController { get; private set; }

		public ModelController ModelController { get; set; }

        public Diagram Diagram
        {
            get { return DiagramController.Diagram; }
        }
        
        public PanelWindow()
        {
            InitializeComponent();

            xCaseDrawComponent.Canvas.Activated += Canvas_Activated;

			//#if DEBUG
            xCaseDrawComponent.Canvas.VersionedElementMouseEnter += Canvas_VersionedElementMouseEnter;
            xCaseDrawComponent.Canvas.VersionedElementMouseLeave += Canvas_VersionedElementMouseLeave;
			//#else
        	demoBar.Visibility = Visibility.Collapsed;
			//#endif
        }

        private readonly VersionedElementInfo infoWindow = new VersionedElementInfo();

        //#if DEBUG

        void Canvas_VersionedElementMouseEnter(object sender, XCaseCanvas.VersionedElementEventArgs eventArgs)
        {
            if (Manager.GetMainWindow() != null && infoWindow.Element != eventArgs.Element && !closing
                && Manager.GetMainWindow().CurrentProject.VersionManager != null
                && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                infoWindow.Element = eventArgs.Element;
                Point pointToScreen = this.PointToScreen(Mouse.GetPosition(this));
                infoWindow.Left = pointToScreen.X + 50;
                infoWindow.Top = pointToScreen.Y + 30;
                infoWindow.Show();
            }
        }

        private void Canvas_VersionedElementMouseLeave(object sender, XCaseCanvas.VersionedElementEventArgs eventargs)
        {
            if (infoWindow.Element == eventargs.Element)
            {
                infoWindow.Hide();
                infoWindow.Element = null;
            }
        }
        //#endif

        void Canvas_Activated()
        {
            Activate();
        }

        public void BindToDiagram(Diagram diagram, ModelController modelController)
        {
            PropertyPath propertyPath = new PropertyPath("Caption");
            Binding titleBinding = new Binding { Source = diagram, Path = propertyPath };
        	SetBinding(TitleProperty, titleBinding);

            // bound view to controller and controller to model
            DiagramController = new DiagramController(diagram, modelController);
        	ModelController = modelController;
        	
			xCaseDrawComponent.Canvas.Controller = DiagramController;
			try
			{
				Mouse.SetCursor(Cursors.Wait);

				#region load items on the diagram

				List<Element> alreadyProcessed = new List<Element>();
				RegistrationSet registrationSet;
				if (diagram is PIMDiagram)
				{
					registrationSet = MainWindow.PIMRepresentantsSet;
					/* Elements in PIM diagram are loaded in the order of their LoadPriority in registration set */
					foreach (RepresentantRegistration registration in registrationSet.OrderBy(reg => reg.LoadPriority))
					{
						foreach (KeyValuePair<Element, ViewHelper> pair in diagram.DiagramElements)
						{
							if (!alreadyProcessed.Contains(pair.Key)
							    && registration.ModelElementType.IsInstanceOfType(pair.Key))
							{
								diagram.NotifyElementAdded(this, pair.Key, pair.Value);
								alreadyProcessed.Add(pair.Key);
							}
						}
					}
				}
				else
				{
					/* order of the elements in PSM diagram is more complex, an ordering function 
					 * is called to order the elements (basically BFS)
					 */
					TreeLayout.SwitchOff();
					IList<Element> ordered;
					if (PSMTree.ReturnElementsInPSMOrder(((PSMDiagram)diagram).Roots, out ordered, true))
					{
						foreach (Element element in ordered)
						{
							diagram.NotifyElementAdded(this, element, diagram.DiagramElements[element]);
						}
					    foreach (Comment comment in diagram.DiagramElements.Keys.OfType<Comment>())
					    {
					        diagram.NotifyElementAdded(this, comment, diagram.DiagramElements[comment]);
					    }
                        foreach (PSMDiagramReference psmDiagramReference in diagram.DiagramElements.Keys.OfType<PSMDiagramReference>())
					    {
					        diagram.NotifyElementAdded(this, psmDiagramReference, diagram.DiagramElements[psmDiagramReference]);
					    }
					}
					xCaseDrawComponent.Canvas.Loaded += Canvas_Loaded;
				}

				#endregion
			}
			finally
			{
				Mouse.SetCursor(Cursors.Arrow);
			}
        }

        #if DEBUG
        public void BindToLogWIndow(LogWindow.LogWindow logWindow)
        {
            if (Manager.GetMainWindow() != null)
            {
                DiagramController.ExecutingCommand += logWindow.Controller_CommandExecuting;
                ModelController.ExecutingCommand += logWindow.Controller_CommandExecuting;
                DiagramController.ExecutedCommand += logWindow.Controller_CommandExecuted;
                ModelController.ExecutedCommand += logWindow.Controller_CommandExecuted;
            }
        }
		#endif

        void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            TreeLayout.SwitchOn();
            TreeLayout.LayoutDiagram(xCaseDrawComponent.Canvas);
        }

        private bool closing;

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
            #if DEBUG
            LogWindow.LogWindow logWindow =
                (LogWindow.LogWindow)(Manager.GetMainWindow()).GetContentFromTypeString(typeof(LogWindow.LogWindow).ToString());
            DiagramController.ExecutingCommand -= logWindow.Controller_CommandExecuting;
            ModelController.ExecutingCommand -= logWindow.Controller_CommandExecuting;
            DiagramController.ExecutedCommand -= logWindow.Controller_CommandExecuted;
            ModelController.ExecutedCommand -= logWindow.Controller_CommandExecuted;
            #endif
            base.OnClosing(e);
        }

		protected override void OnClosed()
		{
			xCaseDrawComponent.Canvas.Unbind();
            infoWindow.Hide();
		    infoWindow.Close();
			base.OnClosed();
		}
		
		public void RenameDiagram(String newCaption)
        {
            RenameDiagramCommand renameDiagramCommand = (RenameDiagramCommand)RenameDiagramCommandFactory.Factory().Create(DiagramController);
            renameDiagramCommand.NewCaption = newCaption;
            renameDiagramCommand.RenamedDiagram = Diagram;
            renameDiagramCommand.Project = ModelController.Project;
            renameDiagramCommand.Execute();
        }

        public override string ToString()
        {
            return base.ToString() +  Diagram != null ? " " + Diagram.Caption : String.Empty;
        }

    	#region demo

    	public void diagramDemo_Click(object sender, RoutedEventArgs e)
    	{
			Stopwatch watch = new Stopwatch();
			Debug.WriteLine("Starting demo...");
			watch.Start();
    		DrawPIMExample(100, 0);
			watch.Stop();
    		Debug.WriteLine("Demo took: " + watch.Elapsed);
    	}

		public void diagramDemo2_Click(object sender, RoutedEventArgs e)
		{
			DrawPIMPSMExample();
		}

    	#region old PSM Example 

    	//public void DrawPSMExample(int x, int y)
    	//{
    	//    PIM_Class clPurchase, clCustomer1, clCustomer2, clShop,
    	//        clDealer, clItem, clAddress, clProduct, clRegion;
    	//    XCaseAttributeContainer acCustomer1, acCustomer2,
    	//        acAddress, acRegion, acProduct, acItem;
    	//    XCaseAssociationChoice asc1, asc2;
    	//    XCaseAssociationContainer ascoItems;
    	//    List<string> prPurchase = new List<string>() { "date" },
    	//        prShop = new List<string>() { "shop-number" },
    	//        prDealer = new List<string>() { "dealer-number" },
    	//        prProduct = new List<string>() { "product-number" },
    	//        atCustomer1 = new List<string>() { "customer-number" },
    	//        atCustomer2 = new List<string>() { "name", "email" },
    	//        atAddress = new List<string>() { "street", "postcode", "city" },
    	//        atRegion = new List<string>() { "name AS region" },
    	//        atProduct = new List<string>() { "unit-price", "title" },
    	//        atItem = new List<string>() { "amount" };

    	//    clPurchase = AddClass("Purchase", null, "purchase", prPurchase, null, 364 + x, 65 + y);
    	//    clCustomer1 = AddClass("Customer", null, null, null, null, 51 + x, 256 + y);
    	//    clCustomer2 = AddClass("Customer", null, "new-customer", null, null, 172 + x, 239 + y);
    	//    clShop = AddClass("Shop", null, "from-shop", prShop, null, 309 + x, 254 + y);
    	//    clDealer = AddClass("Dealer", null, "from-dealer", prDealer, null, 430 + x, 229 + y);
    	//    clItem = AddClass("Item", null, "item", null, null, 561 + x, 273 + y);
    	//    clAddress = AddClass("Address", null, "delivery-address", null, null, 262 + x, 352 + y);
    	//    clProduct = AddClass("Product", null, null, prProduct, null, 443 + x, 379 + y);
    	//    clRegion = AddClass("Region", null, null, null, null, 344 + x, 460 + y);

    	//    acCustomer1 = AddAttributeContainer(atCustomer1, 36 + x, 320 + y);
    	//    acCustomer2 = AddAttributeContainer(atCustomer2, 153 + x, 332 + y);
    	//    acAddress = AddAttributeContainer(atAddress, 193 + x, 434 + y);
    	//    acRegion = AddAttributeContainer(atRegion, 353 + x, 515 + y);
    	//    acProduct = AddAttributeContainer(atProduct, 468 + x, 455 + y);
    	//    acItem = AddAttributeContainer(atItem, 586 + x, 364 + y);

    	//    ascoItems = AddAssociationContainer("items", 532 + x, 147 + y);

    	//    asc1 = AddAssociationChoice(150 + x, 150 + y);
    	//    asc2 = AddAssociationChoice(395 + x, 150 + y);

    	//    AddAssociation(clPurchase, asc1);
    	//    AddAssociation(asc1, clCustomer1, EJunctionCapStyle.Straight, EJunctionCapStyle.FullArrow, "buyer", "0..*", "1");
    	//    AddAssociation(asc1, clCustomer2, EJunctionCapStyle.Straight, EJunctionCapStyle.FullArrow, "buyer", "0..*", "1");
    	//    AddAssociation(clCustomer1, acCustomer1);
    	//    AddAssociation(clCustomer2, acCustomer2);
    	//    AddAssociation(clCustomer2, clAddress, EJunctionCapStyle.Straight, EJunctionCapStyle.FullArrow, "deliver to", "0..1", "0..1");
    	//    AddAssociation(clAddress, acAddress);
    	//    AddAssociation(clAddress, clRegion, EJunctionCapStyle.Straight, EJunctionCapStyle.FullArrow, "in", "0..*", "1");
    	//    AddAssociation(clRegion, acRegion);
    	//    AddAssociation(clPurchase, asc2);
    	//    AddAssociation(asc2, clShop, EJunctionCapStyle.Straight, EJunctionCapStyle.FullArrow, "from shop", "0..*", "1");
    	//    AddAssociation(asc2, clDealer, EJunctionCapStyle.Straight, EJunctionCapStyle.FullArrow, "from dealer", "0..*", "1");
    	//    AddAssociation(clPurchase, ascoItems);
    	//    AddAssociation(ascoItems, clItem, EJunctionCapStyle.Straight, EJunctionCapStyle.FullArrow, "<<ordered>>\ncontained in", "1", "1..*");
    	//    AddAssociation(clItem, clProduct, EJunctionCapStyle.Straight, EJunctionCapStyle.FullArrow, "purchases", "0..*", "1");
    	//    AddAssociation(clProduct, acProduct);
    	//    AddAssociation(clItem, acItem);
    	//}

    	#endregion

		public void DrawPIMPSMExample()
		{
			// prepare simple PIM diagram to start with
			RepresentationCollection ModelViewMap = xCaseDrawComponent.Canvas.ElementRepresentations;
			CreationResult<Class, ClassViewHelper> classCreationResult = DiagramController.NewClass("Region", 100, 10);
			Class clRegion = classCreationResult.ModelElement;
			PIM_Class clRegionView = (PIM_Class)ModelViewMap[clRegion];
			classCreationResult = DiagramController.NewClass("Address", 35, 189);
			Class clAddress = classCreationResult.ModelElement;
			PIM_Class clAddressView = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];

			Dictionary<PIM_Class, List<string>> properties = new Dictionary<PIM_Class, List<string>>();
			properties[clRegionView] = new List<string> { "name", "code" };
			properties[clAddressView] = new List<string> { "street", "postcode", "city" };

			foreach (KeyValuePair<PIM_Class, List<string>> keyValuePair in properties)
			{
				foreach (string attribute in keyValuePair.Value)
				{
					keyValuePair.Key.ClassController.AddNewAttribute(attribute);
				}
			}

			CreationResult<Association, AssociationViewHelper> associationCreationResult = DiagramController.NewAssociation("in", (Class)clRegionView.ModelElement, (Class)clAddressView.ModelElement);
			PIM_Association aRegionAdress = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
			aRegionAdress.Controller.ChangeMultiplicity(aRegionAdress.Ends[0], "1");
			aRegionAdress.Controller.ChangeMultiplicity(aRegionAdress.Ends[1], "0..*");

			// derive PSM diagram
			PSMClass psmRegion = clRegionView.ClassController.DerivePSMClassToNewDiagram();

            PanelWindow p = (PanelWindow)Manager.Documents.Last();
			XCaseCanvas psmDiagramView = p.xCaseDrawComponent.Canvas;
			//ManageAttributesMacroCommand c = (ManageAttributesMacroCommand)ManageAttributesMacroCommandFactory.Factory().Create(psmDiagramView.Controller);
			PSM_Class psmRegionView = (PSM_Class)psmDiagramView.ElementRepresentations[psmRegion];
			//((PSM_ClassController)psmRegionView.Controller).IncludeAttributes(new Dictionary<Property, string> { {clRegion.Attributes[0], "RegionName"}, {clRegion.Attributes[1], "RegionCode"} });

			ViewController.MoveElement(200, 20, psmRegionView.ViewHelper, DiagramController);

			// add an attribute container
			
			NewPSMAttributeContainerCommand attrib = (NewPSMAttributeContainerCommand)NewPSMAttributeContainerCommandFactory.Factory().Create(psmDiagramView.Controller);
			attrib.PSMAttributes.Add(psmRegion.PSMAttributes[0]);
			attrib.PSMClass = psmRegion;
			attrib.ViewHelper = new PSMElementViewHelper(Diagram) { X = 100, Y = 100};
			attrib.Execute();

			NewPSMAttributeContainerCommand attrib2 = (NewPSMAttributeContainerCommand)NewPSMAttributeContainerCommandFactory.Factory().Create(psmDiagramView.Controller);
			attrib2.PSMAttributes.Add(psmRegion.PSMAttributes[0]);
			attrib2.PSMClass = psmRegion;
			attrib2.ViewHelper = new PSMElementViewHelper(Diagram) { X = 280, Y = 100 };
			attrib2.Execute();

			AddPSMChildrenMacroCommand command = (AddPSMChildrenMacroCommand)AddPSMChildrenMacroCommandFactory.Factory().Create(psmDiagramView.Controller);
			command.Set(psmRegion);
			command.Execute();
		}

    	public void DrawPIMExample(int x, int y)
    	{
            RepresentationCollection ModelViewMap = xCaseDrawComponent.Canvas.ElementRepresentations;
            //clRegion = AddClass("Region", null, null, prRegion, null, 39 + x, 81 + y);
            CreationResult<Class, ClassViewHelper> classCreationResult = DiagramController.NewClass("Region", 39 + x, 81 + y);
            PIM_Class clRegion = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];
            //clSupplier = AddClass("Supplier", null, null, prSupplier, null, 192 + x, 78 + y);
            classCreationResult = DiagramController.NewClass("Supplier", 192 + x, 78 + y);
            PIM_Class clSupplier = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];
            //clPart = AddClass("Part", null, null, prPart, null, 489 + x, 88 + y);
            classCreationResult = DiagramController.NewClass("Part", 489 + x, 88 + y);
            PIM_Class clPart = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];
            //clAddress = AddClass("Address", null, null, prAddress, null, 35 + x, 189 + y);
            classCreationResult = DiagramController.NewClass("Address", 35 + x, 189 + y);
            PIM_Class clAddress = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];
            //clShop = AddClass("Shop", null, null, prShop, null, 127 + x, 224 + y);
            classCreationResult = DiagramController.NewClass("Shop", 127 + x, 189 + y);
            PIM_Class clShop = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];
            //clDealer = AddClass("Dealer", null, null, prDealer, null, 244 + x, 224 + y);
            classCreationResult = DiagramController.NewClass("Dealer", 244 + x, 224 + y);
            PIM_Class clDealer = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];
            //clSupply = AddClass("Supply", null, null, prSupply, null, 387 + x, 207 + y);
            classCreationResult = DiagramController.NewClass("Supply", 387 + x, 207 + y);
            PIM_Class clSupply = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];
            //clProductSet = AddClass("ProductSet", null, null, prProductSet, null, 537 + x, 215 + y);
            classCreationResult = DiagramController.NewClass("ProductSet", 537 + x, 215 + y);
            PIM_Class clProductSet = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];
            //clCustomer = AddClass("Customer", null, null, prCustomer, null, 14 + x, 335 + y);
            classCreationResult = DiagramController.NewClass("Customer", 14 + x, 335 + y);
            PIM_Class clCustomer = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];
            //clPurchase = AddClass("Purchase", null, null, prPurchase, null, 182 + x, 341 + y);
            classCreationResult = DiagramController.NewClass("Purchase", 182 + x, 341 + y);
            PIM_Class clPurchase = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];
            //clItem = AddClass("Item", null, null, prItem, null, 385 + x, 342 + y);
            classCreationResult = DiagramController.NewClass("Item", 385 + x, 342 + y);
            PIM_Class clItem = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];
            //clProduct = AddClass("Product", null, null, prProduct, null, 539 + x, 324 + y);
            classCreationResult = DiagramController.NewClass("Product", 529 + x, 324 + y);
            PIM_Class clProduct = (PIM_Class)ModelViewMap[classCreationResult.ModelElement];

            Dictionary<PIM_Class, List<string>> properties = new Dictionary<PIM_Class, List<string>>();
            properties[clRegion] = new List<string> { "name" };
            properties[clSupplier] = new List<string> { "supplier-number", "email", "phone" };
            properties[clPart] = new List<string> { "part-name", "number" };
            properties[clAddress] = new List<string> { "street", "postcode", "city" };
            properties[clShop] = new List<string> { "shop-number" };
            properties[clDealer] = new List<string> { "dealer-number" };
            properties[clSupply] = new List<string> { "amount", "supply-date", "unit-price" };
            properties[clProductSet] = new List<string> { "amount", "completion-date" };
            properties[clCustomer] = new List<string> { "customer-number", "name", "email" };
            properties[clPurchase] = new List<string> { "purchase-number", "date" };
            properties[clItem] = new List<string> { "amount", "position" };
            properties[clProduct] = new List<string> { "product-number", "title", "unit-price", "description" };

            foreach (KeyValuePair<PIM_Class, List<string>> keyValuePair in properties)
            {
                foreach (string attribute in keyValuePair.Value)
                {
                    keyValuePair.Key.ClassController.AddNewAttribute(attribute);
                }
            }

            //AddAssociation(clRegion, clAddress, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "in", "1", "0..*");
            CreationResult<Association, AssociationViewHelper> associationCreationResult = DiagramController.NewAssociation("in", (Class)clRegion.ModelElement, (Class)clAddress.ModelElement);
            PIM_Association aRegionAdress = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aRegionAdress.Controller.ChangeMultiplicity(aRegionAdress.Ends[0], "1");
            aRegionAdress.Controller.ChangeMultiplicity(aRegionAdress.Ends[1], "0..*");
            //AddAssociation(clAddress, clCustomer, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "deliver to", "0..1", "0..1");
            associationCreationResult = DiagramController.NewAssociation("deliver to", (Class)clAddress.ModelElement, (Class)clCustomer.ModelElement);
            PIM_Association aAddressCustomer = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aAddressCustomer.Controller.ChangeMultiplicity(aAddressCustomer.Ends[0], "0..1");
            aAddressCustomer.Controller.ChangeMultiplicity(aAddressCustomer.Ends[1], "0..1");
            //AddAssociation(clCustomer, clPurchase, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "buyer", "1", "0..*");
            associationCreationResult = DiagramController.NewAssociation("buyer", (Class)clCustomer.ModelElement, (Class)clPurchase.ModelElement);
            PIM_Association aCustomerPurchase = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aCustomerPurchase.Controller.ChangeMultiplicity(aCustomerPurchase.Ends[0], "1");
            aCustomerPurchase.Controller.ChangeMultiplicity(aCustomerPurchase.Ends[1], "0..*");
            //AddAssociation(clPurchase, clShop, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "made in", "0..*", "0..1");
            associationCreationResult = DiagramController.NewAssociation("made in", (Class)clPurchase.ModelElement, (Class)clShop.ModelElement);
            PIM_Association aPurchaseShop = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aPurchaseShop.Controller.ChangeMultiplicity(aPurchaseShop.Ends[0], "0..*");
            aPurchaseShop.Controller.ChangeMultiplicity(aPurchaseShop.Ends[1], "0..1");
            //AddAssociation(clPurchase, clDealer, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "made by", "0..*", "0..1");
            associationCreationResult = DiagramController.NewAssociation("made by", (Class)clPurchase.ModelElement, (Class)clDealer.ModelElement);
            PIM_Association aPurchaseDealer = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aPurchaseDealer.Controller.ChangeMultiplicity(aPurchaseDealer.Ends[0], "0..*");
            aPurchaseDealer.Controller.ChangeMultiplicity(aPurchaseDealer.Ends[1], "0..1");
            //AddAssociation(clPurchase, clItem, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "contained in", "1", "1..*");
            associationCreationResult = DiagramController.NewAssociation("contained in", (Class)clPurchase.ModelElement, (Class)clItem.ModelElement);
            PIM_Association aPurchaseItem = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aPurchaseItem.Controller.ChangeMultiplicity(aPurchaseItem.Ends[0], "1");
            aPurchaseItem.Controller.ChangeMultiplicity(aPurchaseItem.Ends[1], "1..*");
            //AddAssociation(clItem, clProduct, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "purchases", "0..*", "1");
            associationCreationResult = DiagramController.NewAssociation("purchases", (Class)clItem.ModelElement, (Class)clProduct.ModelElement);
            PIM_Association aItemProduct = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aItemProduct.Controller.ChangeMultiplicity(aItemProduct.Ends[0], "0..*");
            aItemProduct.Controller.ChangeMultiplicity(aItemProduct.Ends[1], "1");
            //AddAssociation(clProduct, clProduct, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "reffered", "0..*", "0..*");
            associationCreationResult = DiagramController.NewAssociation("reffered", (Class)clProduct.ModelElement);
            PIM_Association aProductProduct = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aProductProduct.Controller.ChangeMultiplicity(aProductProduct.Ends[0], "1");
            //aProductProduct.Controller.ChangeMultiplicity(aProductProduct.Ends[1], "0..*");
            //AddAssociation(clProduct, clProductSet, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "produces", "1", "0..*");
            associationCreationResult = DiagramController.NewAssociation("produces", (Class)clProduct.ModelElement, (Class)clProductSet.ModelElement);
            PIM_Association aProductProductSet = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aProductProductSet.Controller.ChangeMultiplicity(aProductProductSet.Ends[0], "1");
            aProductProductSet.Controller.ChangeMultiplicity(aProductProductSet.Ends[1], "0..*");
            //AddAssociation(clProductSet, clSupply, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "for", "1", "1..*");
            associationCreationResult = DiagramController.NewAssociation("for", (Class)clProductSet.ModelElement, (Class)clSupply.ModelElement);
            PIM_Association aProductSetSupply = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aProductSetSupply.Controller.ChangeMultiplicity(aProductSetSupply.Ends[0], "1");
            aProductSetSupply.Controller.ChangeMultiplicity(aProductSetSupply.Ends[1], "1..*");
            //AddAssociation(clSupply, clPart, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "supplies", "0..*", "1");
            associationCreationResult = DiagramController.NewAssociation("supplies", (Class)clSupply.ModelElement, (Class)clPart.ModelElement);
            PIM_Association aSupplyPart = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aSupplyPart.Controller.ChangeMultiplicity(aSupplyPart.Ends[0], "0..*");
            aSupplyPart.Controller.ChangeMultiplicity(aSupplyPart.Ends[1], "1");
            //AddAssociation(clSupply, clSupplier, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "supplied by", "0..*", "1");
            associationCreationResult = DiagramController.NewAssociation("supplied by", (Class)clSupply.ModelElement, (Class)clSupplier.ModelElement);
            PIM_Association aSupplySupplier = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aSupplySupplier.Controller.ChangeMultiplicity(aSupplySupplier.Ends[0], "0..*");
            aSupplySupplier.Controller.ChangeMultiplicity(aSupplySupplier.Ends[1], "1");
            //AddAssociation(clSupplier, clPart, EJunctionCapStyle.Straight, EJunctionCapStyle.Straight, "offers", "0..*", "1..*");
            associationCreationResult = DiagramController.NewAssociation("offers", (Class)clSupplier.ModelElement, (Class)clPart.ModelElement);
            PIM_Association aSupplierPart = (PIM_Association)ModelViewMap[associationCreationResult.ModelElement];
            aSupplierPart.Controller.ChangeMultiplicity(aSupplierPart.Ends[0], "0..*");
            aSupplierPart.Controller.ChangeMultiplicity(aSupplierPart.Ends[1], "1");
    	}

    	#endregion

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            ActivatePW();
        }

        private void ActivatePW()
        {
            if (!(this as AvalonDock.ManagedContent).IsActiveDocument)
            {
                (this as AvalonDock.ManagedContent).Activate();
                this.ContainerPane.GetManager().ActiveDocument = this;
            }
        }
    }
}


         

