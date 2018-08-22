using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Schema;
using AvalonDock;
using ICSharpCode.AvalonEdit.Folding;
using Microsoft.Win32;
using XCase.Model;
using XCase.Translation;
using XCase.Translation.XmlSchema;
using XCase.View;

namespace XCase.Gui.Windows
{
    /// <summary>
    /// Interaction logic for XMLSchemaWindow.xaml
    /// </summary>
    public partial class XMLSchemaWindow
    {
        private bool IsSchemaValid = true;

        private XmlFoldingStrategy foldingStrategy;
        private FoldingManager foldingManager;

        public string XMLSchemaText
        {
            get
            {
                return tbSchema.Text;
            }
            set
            {
                tbSchema.Text = value.Replace("utf-16", "utf-8");
                foldingManager = FoldingManager.Install(tbSchema.TextArea);
                foldingStrategy = new XmlFoldingStrategy();
                UpdateFolding();
            }
        }

        private void UpdateFolding()
        {
            if (foldingStrategy != null)
                foldingStrategy.UpdateFoldings(foldingManager, tbSchema.Document);
        }

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
                    expander1.Header = String.Format("Schema translated with {0} errors and {1} warnings", counte, countw);
                else if (countw > 0)
                    expander1.Header = String.Format("Schema translated with {0} warnings", countw);
                else if (counte > 0)
                    expander1.Header = String.Format("Schema translated with {0} errors", counte);
                else
                    expander1.Header = "Translation successful";
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

        public XMLSchemaWindow()
        {
            InitializeComponent();

            this.Icon = (ImageSource)FindResource("X");

            tbSchema.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            tbSchema.ShowLineNumbers = true;
        }

        public static void Show(DockingManager manager, PSMDiagram diagram, string schema, TranslationLog log)
        {
            XMLSchemaWindow w = new XMLSchemaWindow();
            w.Diagram = diagram;
            w.XMLSchemaText = schema;
            w.LogMessages = log;
            w.MainWindow = manager.GetMainWindow();

            w.IsFloatingAllowed = true;
            w.IsCloseable = true;
            w.Title = string.Format("{0}.xsd", diagram.Caption);
            w.Show(manager, true);
            //DocumentFloatingWindow fw = new DocumentFloatingWindow(manager, w, manager.MainDocumentPane) { Topmost = true };
            //w.MainWindow.DiagramTabManager.CreatedFloatingWindows.Add(fw);
            //w.DocumentFloatingWindow = fw;
            //fw.Show();
        }

        protected DocumentFloatingWindow DocumentFloatingWindow { get; set; }

        private MainWindow MainWindow { get; set; }

        // Code For OpenWithDialog Box
        [DllImport("shell32.dll", SetLastError = true)]
        extern public static bool
               ShellExecuteEx(ref ShellExecuteInfo lpExecInfo);

        public const uint SW_NORMAL = 1;

        private static void OpenAs(string file)
        {
            ShellExecuteInfo sei = new ShellExecuteInfo();
            sei.Size = Marshal.SizeOf(sei);
            sei.Verb = "openas";
            sei.File = file;
            sei.Show = SW_NORMAL;
            if (!ShellExecuteEx(ref sei))
                throw new System.ComponentModel.Win32Exception();
        }

        private void openAs_click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tmpFilePath = string.Format("{0}{1}.jpg", Path.GetTempPath(), Guid.NewGuid());
                //File.WriteAllText(tmpFilePath, XMLSchemaText, Encoding.UTF8);
                OpenAs(tmpFilePath);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void SaveToFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
                                    {
                                        AddExtension = true,
                                        DefaultExt = "xsd",
                                        FileName = Diagram.Caption,
                                        Filter = "XML Schema files (*.xsd)|*.xsd|All files (*.*)|*.*||"
                                    };
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, XMLSchemaText, Encoding.UTF8);
            }
        }

        private void validateSchema(object sender, RoutedEventArgs e)
        {
            XmlSchema schema;
            /*ProjectTranslator projectTranslator = null;
            bool diagramWithReferences = Diagram.DiagramReferences.Count() > 0;
            if (diagramWithReferences)
            {
                projectTranslator = new ProjectTranslator { Project = MainWindow.CurrentProject };
                projectTranslator.SaveSchemas(null);
            }*/
            try
            {
                /*if (diagramWithReferences)
                {
                    FileStream f = new FileStream(projectTranslator.SchemaFiles[Diagram], FileMode.Open);
                    schema = XmlSchema.Read(f, ValidationCallBack);
                    f.Close();
                }
                else
                {*/
                    schema = XmlSchema.Read(new StringReader(XMLSchemaText), null);  
                //}
                IsSchemaValid = true;

            }
            catch (XmlSchemaException ex)
            {
                string message = string.Format("{0} Position:[{1},{2}] object: {3}", ex.Message, ex.LineNumber, ex.LinePosition, ex.SourceSchemaObject);
                System.Diagnostics.Debug.WriteLine(message);
                MessageBox.Show(message);
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
                return;
            }

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += ValidationCallBack;
            schemaSet.Add(schema);
            schemaSet.Compile();

            if (IsSchemaValid)
            {
                MessageBox.Show("XML Schema schema valid");
            }
        }

        private void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            IsSchemaValid = false;
            System.Diagnostics.Debug.WriteLine(e.Message);
            MessageBox.Show(e.Message);
        }

        private void tbSchema_TextChanged(object sender, EventArgs e)
        {
            UpdateFolding();
        }

        //private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.AddedItems.Contains(tbBrowser))
        //    {
        //        if (tmpFileCreated == null)
        //        {
        //            tmpFileCreated = string.Format("{0}XCase-{1}.xml", Path.GetTempPath(), Guid.NewGuid());
        //        }
        //        File.WriteAllText(tmpFileCreated, XMLSchemaText, Encoding.UTF8);
        //        Uri uri = new Uri("file://" + tmpFileCreated);	
        //        wbSchema.Source = uri;
        //    }
        //}

        protected override void OnKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if ((e.Key == Key.F4 && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
                || e.Key == Key.Escape)
                Close();
        }
    }

    /// <summary>
    /// Structure wrapping ShellExecute winapi call.
    /// </summary>
    [Serializable]
    public struct ShellExecuteInfo
    {
        public int Size;
        public uint Mask;
        public IntPtr hwnd;
        public string Verb;
        public string File;
        public string Parameters;
        public string Directory;
        public uint Show;
        public IntPtr InstApp;
        public IntPtr IDList;
        public string Class;
        public IntPtr hkeyClass;
        public uint HotKey;
        public IntPtr Icon;
        public IntPtr Monitor;
    }
}
