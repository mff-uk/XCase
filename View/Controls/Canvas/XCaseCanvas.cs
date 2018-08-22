using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using Microsoft.Win32;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Controller.Dialogs;
using XCase.Model;
using XCase.View.Interfaces;
using ContextMenu=System.Windows.Controls.ContextMenu;
using Label=System.Windows.Controls.Label;
using MouseEventArgs=System.Windows.Input.MouseEventArgs;

namespace XCase.View.Controls
{
	/// <summary>
	/// XCaseCanvas provides a visualization for a single UML diagram
	/// </summary>
	public partial class XCaseCanvas : Canvas
	{
		/// <summary>
		/// Creates new instance of XCaseCanvas
		/// </summary>
		public XCaseCanvas()
		{
			selectedItems = new SelectedItemsCollection();

			normalState = new NormalState(this);
			draggingElementState = new DraggingElementState(this);
			draggingConnectionState = new DraggingConnectionState(this);
			State = ECanvasState.Normal; 

			#if DEBUG
			MouseLabel = new Label();
			this.Children.Add(MouseLabel);
			#endif

            InitializeContextMenu();
		}

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            return;
            //foreach (PSMClass c in Diagram.DiagramElements.Keys.OfType<PSMClass>())
            //{
            //    if (c.HasElementLabel && c.ElementName.Contains("_"))
            //    {
            //        c.ElementName = c.Name.Substring(0, c.Name.IndexOf("_"));
            //    }

            //    if (!c.HasElementLabel && c.IsStructuralRepresentative)
            //    {
            //        if (c.Components.Count == 0 && c.Attributes.Count == 0)
            //        {
                        
            //            if (!c.Name.Contains("_t") && !c.Name.Contains("_r"))
            //                c.ElementName = c.Name;
            //            else
            //                c.ElementName = c.Name.Substring(0, c.Name.IndexOf("_"));
            //        }
            //        else
            //        {

            //        }
            //    }

            //    int count = NewMethod(c);
            //    if (count == 1)
            //    {

            //    }
            //    if (count > 1)
            //    {

            //    }
            //}
        }

        private int NewMethod(PSMClass c)
        {
            if (c.IsReferencedFromStructuralRepresentative())
            {
                int count = Diagram.DiagramElements.Keys.OfType<PSMClass>().Where(rep => rep.RepresentedPSMClass == c).Count();
                return count;
            }
            return 0;
        }

        ContextMenuItem findUnreferencedRoots, findNotNecessaryClasses;

		/// <summary>
		/// Initializes the context menu, creates context menu items.
		/// </summary>
		private void InitializeContextMenu()
		{
		    ContextMenu = new ContextMenu();
		    ContextMenuItem includeDependent = new ContextMenuItem("Include hidden elements");
		    includeDependent.Icon = ContextMenuIcon.GetContextIcon("branch");
		    includeDependent.Click += delegate { IncludeDependentElements(); };
		    ContextMenu.Items.Add(includeDependent);

		    ContextMenuItem exportPng = new ContextMenuItem("Export diagram to PNG...");
		    exportPng.Icon = ContextMenuIcon.GetContextIcon("Palette");
		    exportPng.Click += delegate { ExportToImage(EExportToImageMethod.PNG); };
		    ContextMenu.Items.Add(exportPng);

		    ContextMenuItem exportPng2 = new ContextMenuItem("Export diagram to PNG (no frame)...");
		    exportPng2.Icon = ContextMenuIcon.GetContextIcon("Palette");
		    exportPng2.Click += delegate { ExportToImage(EExportToImageMethod.PNG, false); };
		    ContextMenu.Items.Add(exportPng2);

		    ContextMenuItem clipboardPng = new ContextMenuItem("Copy as PNG to clipboard");
		    clipboardPng.Icon = ContextMenuIcon.GetContextIcon("Copy");
		    clipboardPng.Click += delegate { ExportToImage(EExportToImageMethod.PNGClipBoard); };
		    ContextMenu.Items.Add(clipboardPng);

		    ContextMenuItem clipboardPng2 = new ContextMenuItem("Copy as PNG to clipboard (no frame)");
		    clipboardPng2.Icon = ContextMenuIcon.GetContextIcon("Copy");
		    clipboardPng2.Click += delegate { ExportToImage(EExportToImageMethod.PNGClipBoard, false); };
		    ContextMenu.Items.Add(clipboardPng2);

            ContextMenuItem exportXPS = new ContextMenuItem("Export diagram to XPS...");
            exportXPS.Icon = ContextMenuIcon.GetContextIcon("Palette");
            exportXPS.Click += delegate { ExportToImage(EExportToImageMethod.XPS); };
            ContextMenu.Items.Add(exportXPS);

		    ContextMenuItem duplicate = new ContextMenuItem("Duplicate diagram");
		    duplicate.Click += delegate
		                           {
		                               DuplicateDiagramCommand command =
		                                   (DuplicateDiagramCommand)
		                                   DuplicateDiagramCommandFactory.Factory().Create(Controller.ModelController);
		                               command.Diagram = this.Diagram;
		                               if (this.Diagram is PIMDiagram)
		                               {
		                                   command.PIMElementsOrder = ElementRepresentations.ElementRepresentationOrder;
		                               }
		                               command.Execute();
		                           };
		    ContextMenu.Items.Add(duplicate);
		    
            findUnreferencedRoots = new ContextMenuItem("Find unreferenced root classes");
		    findUnreferencedRoots.Click += delegate { SelectUnreferencedRootClasses(); };
		    ContextMenu.Items.Add(findUnreferencedRoots);

            findNotNecessaryClasses = new ContextMenuItem("Find subtrees not required for selected roots");
		    findNotNecessaryClasses.Click += delegate { SelectNotRequiredSubtrees(); };
		    ContextMenu.Items.Add(findNotNecessaryClasses);

            ContextMenuItem showHideComments = new ContextMenuItem("Show comments");
		    showHideComments.IsCheckable = true;
		    showHideComments.IsChecked = true; 
            showHideComments.Click += delegate { ShowHideComments(showHideComments.IsChecked); };
            ContextMenu.Items.Add(showHideComments);
		}

	    
	    private void ShowHideComments(bool show)
	    {
	        foreach (Element element in ElementRepresentations.PresentElements)
	        {
                IModelElementRepresentant modelElementRepresentant = ElementRepresentations[element];
                if (modelElementRepresentant != null && modelElementRepresentant is XCaseComment)
                {
                    XCaseComment comment = (XCaseComment)modelElementRepresentant;

                    if (show)
                    {
                        comment.Show();
                    }
                    else
                    {
                        comment.Hide();
                    }
                }
	        }
	    }


	    /// <summary>
		/// Prepares the command that include hidden elements in the diagram
		/// </summary>
		private void IncludeDependentElements()
		{
			List <Element> candidates = ElementDependencies.FindHiddenElements(ElementRepresentations.PresentElements, Controller.ModelController.Model);
			
			IncludeElementsDialog dialog = new IncludeElementsDialog { NoElementsContent = "There are no hidden dependent elements on the diagram", Items = candidates };

			if (dialog.ShowDialog() == true && dialog.SelectedElements.Count > 0)
			{
				IncludeElementsCommand command = (IncludeElementsCommand)IncludeElementsCommandFactory.Factory().Create(Controller);
				foreach (Element element in dialog.SelectedElements)
				{
					if (ElementRepresentations.CanRepresentElement(element))
					{
						command.IncludedElements.Add(element, ElementRepresentations.CreateNewViewHelper(element, Diagram));
					}
				}
				command.Execute();
			}
		}

		/// <summary>
		/// Initializes the registration set for the canvas. Registration set is 
		/// used to find a represeentation for a model element when it is 
		/// added to the diagram
		/// </summary>
		/// <param name="registrationSet">The registration set.</param>
		public void InitializeRegistrationSet(RegistrationSet registrationSet)
        {
            ElementRepresentations = new RepresentationCollection(registrationSet);
        }

	    private readonly SelectedItemsCollection selectedItems;

		/// <summary>
		/// Gets currently selected items on the canvas
		/// </summary>
		public SelectedItemsCollection SelectedItems
		{
			get { return selectedItems; }
		}

		/// <summary>
		/// Gets currently selected items on the canvas that represent model elements. 
		/// </summary>
		public IEnumerable<IModelElementRepresentant> SelectedRepresentants
		{
			get
			{
				return SelectedItems.OfType<IModelElementRepresentant>().Union(
					from selectable in SelectedItems
					where selectable is IRepresentsIndirect
					select ((IRepresentsIndirect)selectable).RepresentedElement);
			}
		}

	    #region Versioned element events

	    public delegate void VersionedElementEvent(object sender, VersionedElementEventArgs eventArgs);
        
	    public class VersionedElementEventArgs: EventArgs
	    {
	        public IVersionedElement Element { get; set; }

	        public VersionedElementEventArgs()
	        {
	        }

	        public VersionedElementEventArgs(IVersionedElement element)
	        {
	            Element = element;
	        }
	    }
	    
	    public event VersionedElementEvent VersionedElementMouseEnter;

	    public void InvokeVersionedElementMouseEnter(object sender, IVersionedElement element)
	    {
	        VersionedElementEvent handler = VersionedElementMouseEnter;
	        if (handler != null) handler(sender, new VersionedElementEventArgs(element));
	        Debug.WriteLine(string.Format("Over: {0}", element));
	    }

	    public event VersionedElementEvent VersionedElementMouseLeave;

	    public void InvokeVersionedElementMouseLeave(object sender, IVersionedElement element)
	    {
	        VersionedElementEvent handler = VersionedElementMouseLeave;
	        if (handler != null) handler(sender, new VersionedElementEventArgs(element));
	        Debug.WriteLine(string.Format("Leave: {0}", element));
	    }

	    #endregion

#if DEBUG
		private readonly Label MouseLabel;
		#endif 

		#region state handling

		/// <summary>
		/// Instance of <see cref="DraggingConnectionState"/> for this XCaseCanvas.
		/// </summary>
		public DraggingConnectionState draggingConnectionState { get; private set; }

		/// <summary>
		/// Instance of <see cref="DraggingElementState"/> for this XCaseCanvas.
		/// </summary>
		public DraggingElementState draggingElementState { get; private set; }

		/// <summary>
		/// Instance of <see cref="NormalState"/> for this XCaseCanvas.
		/// </summary>
		public NormalState normalState { get; private set; }

		/// <summary>
		/// Current canvas state
		/// </summary>
		private XCaseCanvasState CurrentState;

		private ECanvasState state;

		/// <summary>
		/// Sets <see cref="XCaseCanvasState">state</see> of the canvas
		/// </summary>
		public ECanvasState State
		{
			get
			{
				return state;
			}
			set
			{
				if (CurrentState != null)
					CurrentState.StateLeft();
				state = value;
				switch (state)
				{
					case ECanvasState.DraggingConnection:
						CurrentState = draggingConnectionState;
						break;
					case ECanvasState.Normal:
						CurrentState = normalState;
						break;
					case ECanvasState.DraggingElement:
						CurrentState = draggingElementState;
						break;
				}
				if (CurrentState != null)
					CurrentState.StateActivated();
			}
		}

		#endregion

		protected override void OnKeyUp(KeyEventArgs e)
		{
			Debug.WriteLine("KeyUPCanvas");
			base.OnKeyUp(e);
		}

		#region events delegated to current state

		/// <summary>
		/// This operation depends on current state. 
		/// </summary>
		/// <param name="e">event args</param>
		/// <seealso cref="State"/>
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			CurrentState.OnMouseDown(e);
		}

		/// <summary>
		/// This operation depends on current state. 
		/// </summary>
		/// <param name="e">event args</param>
		/// <seealso cref="State"/>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			#if DEBUG
			MouseLabel.Content = string.Format("{0}, {1}", Math.Round(e.GetPosition(this).X), Math.Round(e.GetPosition(this).Y));

			#endif

			CurrentState.OnMouseMove(e);
		}

		/// <summary>
		/// This operation depends on current state. 
		/// </summary>
		/// <param name="e">event args</param>
		/// <seealso cref="State"/>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			CurrentState.OnMouseUp(e);
			
			base.OnMouseUp(e);
		}

		/// <summary>
		/// This operation depends on current state. 
		/// </summary>
		/// <param name="item">clicked item</param>
		/// <param name="e">event args</param>
		/// <seealso cref="State"/>
		internal void SelectableItemPreviewMouseDown(ISelectable item, MouseButtonEventArgs e)
		{
			CurrentState.SelectableItemPreviewMouseDown(item, e);
            InvokeActivated();
		}

		#endregion

		#region MVC interaction

		private DiagramController controller;
		
		/// <summary>
		/// Diagram controller
		/// </summary>
		public DiagramController Controller
		{
			get { return controller; }
			set
			{
				Debug.Assert(controller == null, "Controller should be assigned only once");
				Debug.Assert(value != null, "Controller can be assigned only with not null value.");
				controller = value;

				//Register diagram level event handlers
				controller.Diagram.ElementRemoved += Diagram_ElementRemoved;
				controller.Diagram.ElementAdded += Diagram_ElementAdded;
				controller.ExecutedCommand += updateSelectionAfterCommand;
				controller.ModelController.ExecutedCommand += updateSelectionAfterCommand;
				controller.ModelController.UndoCommand.UndoExecuted += updateSelectionAfterCommand;
				controller.ModelController.RedoCommand.RedoExecuted += updateSelectionAfterCommand;

				if (controller.Diagram is PSMDiagram)
				{
					// hide include dependent elements command for PSM diagrams
					((ContextMenuItem)ContextMenu.Items[0]).Visibility = Visibility.Collapsed;
                    controller.ExecutedCommand += updateLayout;
                    controller.ModelController.ExecutedCommand += updateLayout;
                    controller.ModelController.UndoCommand.UndoExecuted += updateLayout;
                    controller.ModelController.RedoCommand.RedoExecuted += updateLayout;
                    ElementSizeChanged += updateLayout;
				    findUnreferencedRoots.Visibility = Visibility.Visible;
                }
                else
				{
                    findUnreferencedRoots.Visibility = Visibility.Collapsed;
				}
			}
        }

        #region UpdateLayout

        void updateLayout(DragThumb element, SizeChangedEventArgs e)
        {
            updateLayout();
        }
        
        void updateLayout(IStackedCommand command)
        {
            updateLayout();
        }

        void updateLayout(CommandBase command, bool isPartOfMacro, CommandBase macroCommand)
        {
            updateLayout();
        }

        void updateLayout()
        {
            TreeLayout.LayoutDiagram(this);
        }

        #endregion

        /// <summary>
		/// Represented diagram
		/// </summary>
		public Diagram Diagram
		{
			get { return Controller != null ? Controller.Diagram : null; }
		}

		private void updateSelectionAfterCommand(IStackedCommand command)
		{
			updateSelectionAfterCommand((CommandBase)command);
		}

		private void updateSelectionAfterCommand(CommandBase command, bool macro, CommandBase macroCommand)
		{
			updateSelectionAfterCommand(command);
		}

		private void updateSelectionAfterCommand(CommandBase command)
		{
			try
			{
                if (command.AssociatedElements.Count > 0 && Diagram != null &&
					command.AssociatedElements.Any(element => Diagram.IsElementPresent(element)))
				{
					IEnumerable<ISelectable> newSelection =
						from element in command.AssociatedElements
						where ElementRepresentations.IsElementPresent(element)
						      && ElementRepresentations[element] is ISelectable
						select (ISelectable)ElementRepresentations[element];
					SelectedItems.SetSelection(newSelection);
				}
				else
				{
					Debug.WriteLine("WARNING: no associated elements for command " + command);
				}
			}
			catch (Exception)
			{
				Debug.WriteLine("ERROR: selected items update failed after command was executed.");
			}
		}

		/// <summary>
		/// Unbinds the canvas from the model events
		/// </summary>
		public void Unbind()
		{
			if (Controller != null)
			{
				Controller.Diagram.ElementRemoved -= Diagram_ElementRemoved;
				Controller.Diagram.ElementAdded -= Diagram_ElementAdded;


			}
			controller = null;
		}

		/// <summary>
		/// Map bettween model elements and their representatins in the diagram. 
		/// Also provides other functions - see <see cref="RepresentationCollection"/>.
		/// </summary>
		public RepresentationCollection ElementRepresentations { get; set; }

		/// <summary>
		/// Handles the ElementRemoved event of the Diagram control.
		/// Removes representation of the removed element
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="XCase.Model.ElementRemovedEventArgs"/> instance containing the event data.</param>
		private void Diagram_ElementRemoved(object sender, ElementRemovedEventArgs e)
		{
			if (ElementRepresentations.CanDeleteElement(e.Element))
			{
				IModelElementRepresentant representant = ElementRepresentations[e.Element];
				ISelectable selectable = representant as ISelectable;
				if (selectable != null)
				{
					selectable.IsSelected = false;
					SelectedItems.Remove(selectable);
				}
				ElementRepresentations.RemoveElement(e.Element);
				representant.DeleteFromCanvas();
				InvalidateMeasure();
			}
		}

		/// <summary>
		/// Handles the ElementAdded event of the Diagram control.
		/// Creates representation of the added element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="XCase.Model.ElementAddedEventArgs"/> instance containing the event data.</param>
		private void Diagram_ElementAdded(object sender, ElementAddedEventArgs e)
		{
			if (ElementRepresentations.CanRepresentElement(e.Element))
			{
				Element modelElement = e.Element;
				ViewHelper visualization = e.Visualization;

				ElementController elementController; 
				IModelElementRepresentant representant = ElementRepresentations.CreateRepresentant(this, e.Element, out elementController);
				representant.InitializeRepresentant(modelElement, visualization, elementController);
				ElementRepresentations.AddElement(modelElement, representant);
				InvalidateMeasure();
			}
		}

		#endregion

		#region image export

		/// <summary>
		/// Available image formats for exports
		/// </summary>
		public enum EExportToImageMethod
		{
			PNG,
			PNGClipBoard,
            XPS
		}

        /// <summary>
        /// Exports the diagram to an image (with frame and caption), 
        /// uses interactive dialogs to select filename. 
        /// </summary>
        /// <param name="method">image format</param>
        public void ExportToImage(EExportToImageMethod method)
        {
            ExportToImage(method, true);
        }

        /// <summary>
        /// Exports the diagram to an image, uses interactive dialogs to select filename.
        /// </summary>
        /// <param name="method">image format</param>
        /// <param name="useFrameAndCaption">if set to <c>true</c> frame and caption is added to the image</param>
        public void ExportToImage(EExportToImageMethod method, bool useFrameAndCaption)
		{
			if (method == EExportToImageMethod.PNG)
			{
				SaveFileDialog dialog = new SaveFileDialog
				                        	{
				                        		Title = "Export to file...",
				                        		Filter = "PNG images (*.png)|*.png|All files|*.*"
				                        	};
				if (dialog.ShowDialog() == true)
				{
					ExportToImage(method, dialog.FileName, Diagram.Caption, true);
				}
			}
            if (method == EExportToImageMethod.XPS)
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    Title = "Export to file...",
                    Filter = "XPS images (*.xps)|*.xps|All files|*.*"
                };
                if (dialog.ShowDialog() == true)
                {
                    ExportToImage(method, dialog.FileName, Diagram.Caption, true);
                }
            }
            if (method == EExportToImageMethod.PNGClipBoard)
			{
                ExportToImage(EExportToImageMethod.PNGClipBoard, null, Diagram.Caption, useFrameAndCaption);
			}
		}

	    /// <summary>
	    /// Exports the diagram to an image.
	    /// </summary>
	    /// <param name="method">image format</param>
	    /// <param name="filename">file name</param>
	    /// <param name="title">diagram title</param>
	    /// <param name="useFrameAndCaption"></param>
	    public void ExportToImage(EExportToImageMethod method, string filename, string title, bool useFrameAndCaption)
		{
			const int bounds = 10;
			const int textoffset = 20;

            #if DEBUG
	        Visibility labelVisible = MouseLabel.Visibility;
            MouseLabel.Visibility = Visibility.Collapsed;
            #endif

			if (method == EExportToImageMethod.PNG || method == EExportToImageMethod.PNGClipBoard)
			{
				FormattedText titleText = new FormattedText(title, new CultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 20, Brushes.Gray);

				double canvasWidth;
				double canvasHeight;
				GetCanvasWidthAndHeight(out canvasWidth, out canvasHeight);

				RenderTargetBitmap rtb;
			    
                if (useFrameAndCaption)
                    rtb = new RenderTargetBitmap((int)(Math.Max(bounds + canvasWidth + bounds, textoffset + titleText.Width + textoffset)), (int)(textoffset + titleText.Height + textoffset + canvasHeight + bounds), 96, 96, PixelFormats.Pbgra32);
                else
                    rtb = new RenderTargetBitmap((int)(canvasWidth), (int)(canvasHeight), 96, 96, PixelFormats.Pbgra32);
			    
				this.InvalidateVisual();
				DrawingVisual drawingVisual = new DrawingVisual();
				DrawingContext drawingContext = drawingVisual.RenderOpen();
				drawingContext.DrawRectangle(this.Background, null, new Rect(0, 0, rtb.Width, rtb.Height));
                VisualBrush canvasBrush = new VisualBrush(this);
				canvasBrush.Stretch = Stretch.None;
				canvasBrush.AlignmentX = 0;
				canvasBrush.AlignmentY = 0;
                if (useFrameAndCaption)
                {
                    Rect rect = new Rect(bounds, textoffset + titleText.Height + textoffset, rtb.Width - 2 * bounds, rtb.Height - bounds - textoffset - titleText.Height - textoffset);
                    drawingContext.DrawRectangle(canvasBrush, new Pen(Brushes.LightGray, 1), rect);
                    drawingContext.DrawText(titleText, new Point(rtb.Width / 2 - titleText.Width / 2, textoffset));
                }
                else
                {
                    drawingContext.DrawRectangle(canvasBrush, null, new Rect(0, 0, (canvasWidth), (canvasHeight)));
                }
				drawingContext.Close();

				rtb.Render(drawingVisual);
				PngBitmapEncoder png = new PngBitmapEncoder();
				png.Frames.Add(BitmapFrame.Create(rtb));
				if (method == EExportToImageMethod.PNG)
				{
					using (Stream stm = File.Create(filename))
					{
						png.Save(stm);
					}
				}
				if (method == EExportToImageMethod.PNGClipBoard)
				{
					Clipboard.SetImage(rtb);
				}
			}
			else if (method == EExportToImageMethod.XPS)
			{
                {
                   double canvasWidth;
                   double canvasHeight;
                   GetCanvasWidthAndHeight(out canvasWidth, out canvasHeight);


                  // Save current canvas transorm
                  Transform transform = this.LayoutTransform;
                  // Temporarily reset the layout transform before saving
                  this.LayoutTransform = null;

                  
                  // Get the size of the canvas
                  Size size = new Size(canvasWidth, canvasHeight);
                  // Measure and arrange elements
                  this.Measure(size);
                  this.Arrange(new Rect(size));

                  // Open new package
                  System.IO.Packaging.Package package = System.IO.Packaging.Package.Open(filename, FileMode.Create);
                  // Create new xps document based on the package opened
                  XpsDocument doc = new XpsDocument(package);
                  // Create an instance of XpsDocumentWriter for the document
                  XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);
                  // Write the canvas (as Visual) to the document
                  writer.Write(this);
                  // Close document
                  doc.Close();
                  // Close package
                  package.Close();

                  // Restore previously saved layout
                  this.LayoutTransform = transform;
                }
			}

            #if DEBUG
	        MouseLabel.Visibility = labelVisible;
            #endif
		}

		public void GetCanvasWidthAndHeight(out double canvasWidth, out double canvasHeight)
		{
			if (this.Children.OfType<DragThumb>().Count() > 0)
			{
				canvasWidth = this.Children.OfType<DragThumb>().Max(thumb => thumb.Right) + 20;
				canvasHeight = this.Children.OfType<DragThumb>().Max(thumb => thumb.Bottom) + 20;
			}
			else
			{
				canvasWidth = 40;
				canvasHeight = 40;
			}
            Rect r = new Rect(0, 0, canvasWidth, canvasHeight);
		    Rect transformed = this.LayoutTransform.TransformBounds(r);
		    canvasWidth = transformed.Width;
		    canvasHeight = transformed.Height;
		}

		#endregion

		/// <summary>
		/// This is called by ScrollView, returns desired Canvas size
		/// </summary>
		/// <param name="constraint"></param>
		/// <returns>Desired Canvas size</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			Size size = new Size();
			foreach (UIElement element in Children)
			{
				double left = GetLeft(element);
				double top = GetTop(element);
				left = double.IsNaN(left) ? 0 : left;
				top = double.IsNaN(top) ? 0 : top;

				//measure desired size for each child
				element.Measure(constraint);

				Size desiredSize = element.DesiredSize;
				if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
				{
					size.Width = Math.Max(size.Width, left + desiredSize.Width);
					size.Height = Math.Max(size.Height, top + desiredSize.Height);
				}
			}
			//for aesthetic reasons add extra points
			size.Width += 10;
			size.Height += 10;
			return size;
		}

        public event ElementSizeChangedHandler ElementSizeChanged;

        public void InvokeElementSizeChanged(DragThumb element, SizeChangedEventArgs e)
        {
            if (ElementSizeChanged != null) ElementSizeChanged(element, e);
        }

	    public event Action Activated;

	    private void InvokeActivated()
	    {
	        Action activated = Activated;
	        if (activated != null) activated();
	    }

        private void SelectNotRequiredSubtrees()
        {
            PSMDiagram psmDiagram = ((PSMDiagram)Diagram);
            List<PSMClass> startClasses = SelectedItems.OfType<IModelElementRepresentant>().Select(
                r => ElementRepresentations.GetElementRepresentedBy(r)).OfType<PSMClass>().Where(
                    r => psmDiagram.Roots.Contains(r)).ToList();
            
            List<PSMClass> visitedClasses = new List<PSMClass>(startClasses);
            Queue<Element> queue = new Queue<Element>(startClasses);

            while (!queue.IsEmpty())
            {
                Element element = queue.Dequeue();
                PSMClass psmClass = element as PSMClass;
                if (psmClass != null)
                {
                    visitedClasses.Add(psmClass);
                    // new subtree added when structural representative found. 
                    if (psmClass.IsStructuralRepresentative && !psmClass.IsStructuralRepresentativeExternal)
                    {
                        queue.Enqueue(psmClass.RepresentedPSMClass);
                    }
                }

                foreach (Element child in PSMTree.GetChildrenOfElement(element))
                {
                    queue.Enqueue(child);
                }
            }

            IEnumerable<IModelElementRepresentant> representants = 
                psmDiagram.Roots.Where(r => !visitedClasses.Contains(r)).Select(element => ElementRepresentations[element]);

            SelectedItems.SetSelection(representants);
        }

	    private void SelectUnreferencedRootClasses()
        {
            List<IModelElementRepresentant> toSelect = new List<IModelElementRepresentant>();
            foreach (PSMClass root in ((PSMDiagram)Diagram).Roots.OfType<PSMClass>().Where(c => !c.HasElementLabel && !c.IsReferencedFromStructuralRepresentative()))
            {
                toSelect.Add(ElementRepresentations[root]);
            }
            SelectedItems.SetSelection(toSelect);
        }

        public void SelectElement(Element element)
        {
            if (Diagram.DiagramElements.ContainsKey(element))
            {
                var r = this.ElementRepresentations[element];
                if (r != null && r is ISelectable)
                {
                    if (r is FrameworkElement)
                    {
                        if (r is PSM_Association)
                        {
                            SelectElement(((PSM_Association)r).Association.Parent);
                        }
                        if (r is PIM_Association)
                        {
                            SelectElement(((PIM_Association)r).Association.Ends[0].Class);
                        }
                        if (r is Generalization)
                        {
                            SelectElement(((Generalization)r).General);
                        }
                        ((FrameworkElement)r).BringIntoView();
                    }
                    SelectedItems.SetSelection((ISelectable)r);
                }
            }
            else
            {
                if (element is PSMAttribute && ((PSMAttribute)element).AttributeContainer != null)
                {
                    SelectElement(((PSMAttribute)element).AttributeContainer);
                }
                else if (element is Property)
                {
                    SelectElement(((Property)element).Class);
                }
            }
        }
	}

    public delegate void ElementSizeChangedHandler(DragThumb element, SizeChangedEventArgs e);
}
