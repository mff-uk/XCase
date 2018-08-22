using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Schema;
using AvalonDock;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit;
using Microsoft.Win32;
using XCase.Model;
using XCase.Translation;
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

        public Dictionary<string, string> XMLSchemaTexts
        {
            get
            {
                Dictionary<string, string> lst = new Dictionary<string, string>();
                foreach (TabItem tab in tabControl.Items)
                {
                    TextEditor textEditor = (TextEditor)tab.Content;
                    lst.Add(tab.Header.ToString(), textEditor.Text);
                }
                return lst;
            }
            set
            {
                foreach (string fileName in value.Keys)
                {
                    string myschema = value[fileName];
                    if (myschema == null)
                        continue;

                    TextEditor textEditor = new TextEditor();
                    prepareTextEditor(textEditor);
                    textEditor.Text = myschema.Replace("utf-16", "utf-8");

                    TabItem newTab = new TabItem();
                    newTab.Header = fileName;
                    newTab.Content = textEditor;
                    newTab.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

                    tabControl.Items.Add(newTab);
                    tabControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

                    //foldingManager = FoldingManager.Install(textEditor.TextArea);
                    //foldingStrategy = new XmlFoldingStrategy();
                    //UpdateFolding(textEditor.Document);
                }
            }
        }

        private void prepareTextEditor(TextEditor textEditor)
        {
            FontFamilyConverter conv = new FontFamilyConverter();
            BrushConverter brconv = new BrushConverter();
            System.Windows.Markup.XmlLanguageConverter langconv = new System.Windows.Markup.XmlLanguageConverter();

            textEditor.FontFamily = (FontFamily)conv.ConvertFromString("Consolas");
            textEditor.FontSize = 12;
            textEditor.Background = (Brush)brconv.ConvertFromString("White");
            textEditor.Language = (System.Windows.Markup.XmlLanguage)langconv.ConvertFromString("XML");
            textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            textEditor.ShowLineNumbers = true;
            textEditor.IsReadOnly = false;
            textEditor.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
        }

        private string getSchemaFromCurrentTab()
        {
            TabItem currentTab = (TabItem)tabControl.SelectedItem;
            TextEditor textEditor = (TextEditor)currentTab.Content;
            return textEditor.Text;
        }

        private void UpdateFolding(ICSharpCode.AvalonEdit.Document.TextDocument doc)
	    {
            if (foldingStrategy != null)
	            foldingStrategy.UpdateFoldings(foldingManager, doc);
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

		    this.Icon = (ImageSource) FindResource("X");

            //tbSchema.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
		    //tbSchema.ShowLineNumbers = true;
		}

        public static void Show(DockingManager manager, PSMDiagram diagram, Dictionary<string,string> schemas, TranslationLog log)
        {
            XMLSchemaWindow w = new XMLSchemaWindow();
            w.Diagram = diagram;
            w.XMLSchemaTexts = schemas;
            w.LogMessages = log;
            w.MainWindow = (MainWindow)manager.ParentWindow;

            w.IsFloatingAllowed = true;
            w.IsCloseable = true;
            w.Title = "XML Schema"; //string.Format("{0}.xsd", diagram.Caption);
            DocumentFloatingWindow fw = new DocumentFloatingWindow(manager, w, manager.MainDocumentPane) { Topmost = true };
            w.MainWindow.DiagramTabManager.CreatedFloatingWindows.Add(fw);
            w.DocumentFloatingWindow = fw;
            fw.Show();
        }

	    protected DocumentFloatingWindow DocumentFloatingWindow { get; set; }

	    private MainWindow MainWindow { get; set; }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (MainWindow.DiagramTabManager.CreatedFloatingWindows.Contains(DocumentFloatingWindow))
            {
                MainWindow.DiagramTabManager.CreatedFloatingWindows.Remove(DocumentFloatingWindow);
            }
        }

	    private void button1_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

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
            /**/
		}

        public void SaveToFiles(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                //todo: File.WriteAllText(sfd.FileName, XMLSchemaText, Encoding.UTF8);
                foreach (string fileName in XMLSchemaTexts.Keys)
                {
                    File.WriteAllText(fbd.SelectedPath + "/" + fileName, XMLSchemaTexts[fileName], Encoding.UTF8);
                }
            }
        }

		private void validateSchema(object sender, RoutedEventArgs e)
		{
          XmlSchema schema;
          try
          {
              schema = XmlSchema.Read(new StringReader(getSchemaFromCurrentTab()), null);
              IsSchemaValid = true;            
          }
		  catch (XmlSchemaException ex)
		  {
			  string message = string.Format("{0} Position:[{1},{2}] object: {3}", ex.Message, ex.LineNumber, ex.LinePosition, ex.SourceSchemaObject);
			  System.Diagnostics.Debug.WriteLine(message);
			  System.Windows.MessageBox.Show(message);
			  return;
		  }
          catch (Exception ex)
          {
              System.Diagnostics.Debug.WriteLine(ex.Message);
              System.Windows.MessageBox.Show(ex.Message);
              return;
          }
         
          XmlSchemaSet schemaSet = new XmlSchemaSet();
          schemaSet.ValidationEventHandler += ValidationCallBack;
          schemaSet.Add(schema);
          schemaSet.Compile();

          if (IsSchemaValid)
          {
              System.Windows.MessageBox.Show("XML Schema schema valid");
          }
		}

        private void validateSchemas(object sender, RoutedEventArgs e)
        {
            foreach (string fileName in XMLSchemaTexts.Keys)
            {
                XmlSchema schema;
                try
                {
                    schema = XmlSchema.Read(new StringReader(XMLSchemaTexts[fileName]), null);
                    IsSchemaValid = true;
                }
                catch (XmlSchemaException ex)
                {
                    string message = string.Format("{0} Position:[{1},{2}] object: {3}", ex.Message, ex.LineNumber, ex.LinePosition, ex.SourceSchemaObject);
                    System.Diagnostics.Debug.WriteLine(message);
                    System.Windows.MessageBox.Show(message);
                    return;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    System.Windows.MessageBox.Show(ex.Message);
                    return;
                }

                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.ValidationEventHandler += ValidationCallBack;
                schemaSet.Add(schema);
                schemaSet.Compile();

                if (IsSchemaValid)
                {
                    System.Windows.MessageBox.Show("XML Schema schema valid");
                }
            }
        }

		private void ValidationCallBack(object sender, ValidationEventArgs e)
		{
            IsSchemaValid = false;
			System.Diagnostics.Debug.WriteLine(e.Message);
			System.Windows.MessageBox.Show(e.Message);
		}

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
