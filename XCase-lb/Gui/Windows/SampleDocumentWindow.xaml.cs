using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Schema;
using AvalonDock;
using ICSharpCode.AvalonEdit.Folding;
using XCase.Model;
using XCase.Translation;
using XCase.Translation.XmlSchema;
using XCase.View;
using System.Linq;
using Microsoft.Win32;

namespace XCase.Gui
{
    /// <summary>
    /// Interaction logic for SampleDocumentWindow.xaml
    /// </summary>
    public partial class SampleDocumentWindow
    {
        public SampleDocumentWindow()
        {
            InitializeComponent();

            tbDocument.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            tbDocument.ShowLineNumbers = true;
        }

        private XmlFoldingStrategy foldingStrategy;
        private FoldingManager foldingManager;

        private string documentText;

        private IList<LogMessage> logMessages;

        public IList<LogMessage> LogMessages
        {
            get
            {
                return logMessages;
            }
            set
            {
                logMessages = value;
                LogMessage.ImageGetter = ImageGetter;
                gridLog.ItemsSource = logMessages.OrderBy(message => message.Severity == LogMessage.ESeverity.Error ? 0 : 1);
                int countw = logMessages.Count(e => e.Severity == LogMessage.ESeverity.Warning);
                int counte = logMessages.Count(e => e.Severity == LogMessage.ESeverity.Error);
                if (countw > 0 && counte > 0)
                    expander1.Header = String.Format("Document created with {0} errors and {1} warnings", counte, countw);
                else if (countw > 0)
                    expander1.Header = String.Format("Document created with {0} warnings", countw);
                else if (counte > 0)
                    expander1.Header = String.Format("Document created with {0} errors", counte);
                else
                    expander1.Header = "Document created successfully";
            }
        }

        public PSMDiagram Diagram { get; set; }

        private static object ImageGetter(LogMessage message)
        {
            if (message.Severity == LogMessage.ESeverity.Error)
                return ContextMenuIcon.GetContextIcon("error_button").Source;
            else
                return ContextMenuIcon.GetContextIcon("Warning").Source;
        }

        //public static void Show(string documentText, PSMDiagram diagram)
        public static void Show(DockingManager manager, string documentText, PSMDiagram diagram, TranslationLog log)
        {
            documentText = documentText.Replace("utf-16", "utf-8");

            SampleDocumentWindow w = new SampleDocumentWindow();
            w.Diagram = diagram;
            w.documentText = documentText;
            w.tbDocument.Text = documentText;
            w.LogMessages = log;
            w.MainWindow = (MainWindow) manager.ParentWindow;
            
            
            w.foldingManager = FoldingManager.Install(w.tbDocument.TextArea);
            w.foldingStrategy = new XmlFoldingStrategy();
            if (!String.IsNullOrEmpty(documentText))
                w.UpdateFolding();
            
            w.IsFloatingAllowed = true;
            w.IsCloseable = true;
            w.Title = string.Format("{0}.xml", diagram.Caption);
            DocumentFloatingWindow fw = new DocumentFloatingWindow(manager, w, manager.MainDocumentPane) { Topmost = true };
            w.MainWindow.DiagramTabManager.CreatedFloatingWindows.Add(fw);
            w.DocumentFloatingWindow = fw; 
            fw.Show();
            
        }

        private DocumentFloatingWindow DocumentFloatingWindow { get; set; }

        private MainWindow MainWindow
        {
            get; set;
        }

        private void UpdateFolding()
        {
            if (foldingStrategy != null)
                foldingStrategy.UpdateFoldings(foldingManager, tbDocument.Document);
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (MainWindow.DiagramTabManager.CreatedFloatingWindows.Contains(DocumentFloatingWindow))
            {
                MainWindow.DiagramTabManager.CreatedFloatingWindows.Remove(DocumentFloatingWindow);
            }
        }

        private void bValidate_Click(object sender, RoutedEventArgs e)
        {
            // show start dialog of translation
            StartTranslation st = new StartTranslation();
            System.Windows.Forms.DialogResult dr = st.ShowDialog();
            if (dr != System.Windows.Forms.DialogResult.OK)
                return;

            Configuration config = new Configuration();
            if (!st.isDefConfigChecked())
            {
                config.Load(st.getConfigFileName());
            }

            if (config == null)
                return;

            XmlSchemaTranslator t = new XmlSchemaTranslator(config,"projectN");  //todo:...

            string xmlSchemaText = t.Translate(Diagram);
            XmlReader xmlfile = null;
            XmlReader schemaReader = null;
            MemoryStream _msSchemaText = null;
            isValid = true;
            abort = false; 
            try
            {
                byte[] b = Encoding.Unicode.GetBytes(xmlSchemaText);
                _msSchemaText = new MemoryStream(b);
                schemaReader = new XmlTextReader(_msSchemaText);
                XmlSchema schema = XmlSchema.Read(schemaReader, schemaSettings_ValidationEventHandler);
                schema.TargetNamespace = Diagram.Project.Schema.XMLNamespaceOrDefaultNamespace;
                
                XmlReaderSettings schemaSettings = new XmlReaderSettings();
                schemaSettings.Schemas.Add(schema);
                schemaSettings.ValidationType = ValidationType.Schema;
                schemaSettings.ValidationEventHandler += schemaSettings_ValidationEventHandler;

                try
                {
                    xmlfile = XmlReader.Create(new StringReader(tbDocument.Text), schemaSettings);
                }
                catch (XmlSchemaValidationException ex)
                {
                    isValid = false;
                    MessageBox.Show(string.Format("Validation can not continue - schema is invalid. \r\n\r\n{0}", ex.Message), "Invalid schema", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (isValid)
                {
                    while (xmlfile.Read() && !abort)
                    {
                    }
                }
            }
            catch (XmlSchemaValidationException ex)
            {
                isValid = false;
                MessageBox.Show(string.Format("{0} \r\n\r\nValidation can not continue.", ex.Message), "Invalid document", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                isValid = false;
                MessageBox.Show(string.Format("{0} \r\n\r\nValidation can not continue.", ex.Message), "Invalid document", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                if (xmlfile != null) xmlfile.Close();
                if (schemaReader != null) schemaReader.Close();
                if (_msSchemaText != null) _msSchemaText.Dispose();
            }

            if (isValid)
            {
                MessageBox.Show("Document is valid", "Document valid", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool isValid;
        private bool abort; 

        void schemaSettings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            string location = string.Empty;
            if (e.Exception != null)
            {
                location = string.Format("\r\n\rLine number: {0} position {1}", e.Exception.LineNumber, e.Exception.LinePosition);
            }

            MessageBoxResult result = MessageBox.Show(string.Format("{0}{1}\r\n\rContinue validation?", e.Message, location), "Invalid document", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

            if (result == MessageBoxResult.No)
                abort = true; 
            isValid = false; 
        }

        private void tbDocument_TextChanged(object sender, EventArgs e)
        {
            UpdateFolding();
        }

        protected override void OnKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if ((e.Key == Key.F4 && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
                || e.Key == Key.Escape)
                Close();
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "xml",
                FileName = Diagram.Caption,
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*||"
            };
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, tbDocument.Text, Encoding.UTF8);
            }
        }
        
    }
}
