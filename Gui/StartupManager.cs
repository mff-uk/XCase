//#define testingconsole

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using CommandLineParser.Arguments;
using CommandLineParser.Validation;
using XCase.Model;
using XCase.Translation.XmlSchema;
using XCase.View.Controls;

namespace XCase.Gui
{
	/// <summary>
	/// Starts the application, handles command line parameters
	/// </summary>
	[ArgumentGroupCertification("list,help,project", EArgumentGroupCondition.AtLeastOneUsed)]
	public class StartupManager
	{
		[FileArgument('p', "project", FileMustExist = true, Description = "Input XCase project file", FullDescription = "ARG_INPUT")]
		public FileInfo InputFile { get; set; }

		[DirectoryArgument('o', "outputDir", DirectoryMustExist = true, Description = "Output directory", FullDescription = "ARG_OUTPUTDIR")]
		public DirectoryInfo OutputDir { get; set; }

		[DirectoryArgument('n', "outputDirPng", DirectoryMustExist = true, Description = "PNG output directory ", FullDescription = "ARG_OUTPUTDIRPNG")]
		public DirectoryInfo OutputDirPng { get; set; }

		[DirectoryArgument('g', "outputDirXsd", DirectoryMustExist = true, Description = "XSD output directory", FullDescription = "ARG_OUTPUTDIRXSD")]
		public DirectoryInfo OutputDirXsd { get; set; }

		[SwitchArgument('i', "noImages", false, Description = "Flag, do not export diagrams as images", FullDescription = "ARG_NOIMAGES")]
		public bool ExcludeImages { get; set; }

		[SwitchArgument('s', "noSchemas", false, Description = "Flag, export PSM diagrams as schemas", FullDescription = "ARG_NOSCHEMAS")]
		public bool ExcludeSchemas { get; set; }

		[ValueArgument(typeof(string), 'd', "diagram", AllowMultiple = true, Description = "Identification of the diagrams to export", FullDescription = "ARG_DIAGRAM")]
		public string[] DiagramSpecifications { get; set; }

		[SwitchArgument('l', "list", false, Description = "Do nothing, only list diagrams in the project", FullDescription = "ARG_LIST")]
		public bool List { get; set; }

		[SwitchArgument('h', "help", false, Description = "Usage info", FullDescription = "ARG_HELP")]
		public bool Help { get; set; }

		/// <summary>
		/// Starts the application (according to arguments).
		/// </summary>
		/// <param name="args">Command line arguments</param>
		public void StartApp(string[] args)
		{
#if testingconsole
			if (args.Length == 1 && !Array.Exists(args, s => s.StartsWith("-")))
			{
				StartAppGui(args[0]);
			}
			else
				ProcessCommandLine(args);
#else
			if (args.Length == 0)
			{
				StartAppGui(null);
			}
			else if (args.Length == 1 && !Array.Exists(args, s => s.StartsWith("-")))
			{
				StartAppGui(args[0]);
			}
			else
			{
				ProcessCommandLine(args);
			}
#endif
		}

		/// <summary>
		/// Processes the command line.
		/// </summary>
		/// <param name="args">Command line arguments</param>
		private void ProcessCommandLine(string[] args)
		{
			CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
			parser.ShowUsageOnEmptyCommandline = false;
			parser.ExtractArgumentAttributes(this);
			parser.FillDescFromResource(new CommandlineArguments());

#if testingconsole
			args = new string[]
			       	{
			       		"--project",
			       		@"D:\Programování\XCase\Test\ABCCompany.XCase",
						//"--outputDir",
                        //@"D:\Programování\XCase\Test\cmdoutput\",
						//"-d",
                        //"0;1",
                        // "-d",
                        //"2(filename2)",
                        //"-d",
                        //"TransportDetailFormat(filename5)[I]"
						//"1-3(filename1,filename2,filename3)[I]"
						//"3"
			       	};
			//args = new string[] {"-i", "asdf"};
#endif 
			try
			{
				parser.ParseCommandLine(args);
			}
			catch (CommandLineParser.Exceptions.CommandLineException e)
			{
				Console.WriteLine("Error in command line parameters: ");
				Console.WriteLine(e.Message);
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine("Correct usage: ");
				parser.ShowUsage();
				ShutDown();
				return;
			}

			#region List and Help

			if (Help)
			{
				HelpAndShutdown(parser);
				return;
			}

			// povolena kombinace list + project
			if (List)
			{
				ListAndShutdown();
				return;
			}

			#endregion

			Export();
		}
	
		/// <summary>
		/// Information needed to export a diagram
		/// </summary>
		struct ExportInfo
		{
			Diagram diagram;

			internal Diagram Diagram
			{
				get { return diagram; }
				set
				{
					diagram = value;
					Filename = Diagram.Caption;
					ExportType = EExportType.Unspec;
				}
			}

			internal string Filename { get; set; }

			/// <summary>
			/// Type of export (PNG or XSD or both)
			/// </summary>
			[Flags]
			internal enum EExportType
			{
				/// <summary>
				/// Value is not specified
				/// </summary>
				Unspec = 0,
				/// <summary>
				/// Nothing is exported
				/// </summary>
				None = 1,
				/// <summary>
				/// PNG export
				/// </summary>
				Png = 2,
				/// <summary>
				/// XML schema export
				/// </summary>
				Xsd = 4,
				/// <summary>
				/// Both exports
				/// </summary>
				Both = 6
			}

			internal EExportType ExportType;

			/// <summary>
			/// Initializes a new instance of the <see cref="T:System.Object"/> class.
			/// </summary>
			/// <param name="stringInfo">string from command line</param>
			/// <param name="project">The project from which files are exported.</param>
			public ExportInfo(string stringInfo, Project project) : 
				this()
			{
				Regex r = new Regex("(\\w+)(\\((\\w+)\\)){0,1}(\\[(\\w+)\\]){0,1}");
				Match match = r.Match(stringInfo);
				if (match != null)
				{
					if (!String.IsNullOrEmpty(match.Groups[1].Value))
					{
						string d = match.Groups[1].Value;
						Diagram nameMatch = project.Diagrams.Where(item => item.Caption == d).SingleOrDefault();
						if (nameMatch != null)
							Diagram = nameMatch;
						else
						{
							int numMatch;
							if (int.TryParse(d, out numMatch) 
							    && project.Diagrams.Count > numMatch)
							{
								Diagram = project.Diagrams[numMatch];
							} 
						}
					}

					if (Diagram == null)
					{
						Console.WriteLine("Error: Could not found diagram {0}.", stringInfo);
						return;
					}

					string g3 = match.Groups[3].Value;
					if (!String.IsNullOrEmpty(g3))
					{
						Filename = g3;
					}

					string g4 = match.Groups[4].Value;
					if (!String.IsNullOrEmpty(g4))
					{
						ExportType = EExportType.None;
						if (g4.Contains('I') || g4.Contains('i'))
						{
							ExportType |= EExportType.Png;
						}
						if (g4.Contains('S') || g4.Contains('s'))
						{
							ExportType |= EExportType.Xsd;
						}
					}
				}
			}
		}

		private List<ExportInfo> ExportedDiagrams;

		private void Export()
		{
			CreateHiddenMainWindow();
			Project project = mainWindow.OpenProject(InputFile.FullName);
			
			string outputDirPng;
			string outputDirXsd;
			
			#region init output directories
	
			if (OutputDirPng != null)
				outputDirPng = OutputDirPng.FullName;
			else if (OutputDir != null)
				outputDirPng = OutputDir.FullName;
			else
				outputDirPng = InputFile.DirectoryName;

			if (OutputDirXsd != null)
				outputDirXsd = OutputDirXsd.FullName;
			else if (OutputDir != null)
				outputDirXsd = OutputDir.FullName;
			else
				outputDirXsd = InputFile.DirectoryName;

			#endregion

			ExportedDiagrams = new List<ExportInfo>();
			if (DiagramSpecifications != null)
			{
				foreach (string diagramSpecification in DiagramSpecifications)
				{
					string[] splitted = diagramSpecification.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string s in splitted)
					{
						ExportedDiagrams.Add(new ExportInfo(s, project));
					}
				}
			}

			if (ExportedDiagrams.Count == 0) // all diagrams from project are processed
			{
				foreach (Diagram diagram in project.Diagrams)
				{
					#region export XSD

					if (diagram is PSMDiagram && !ExcludeSchemas)
					{
						ExportXsd(diagram, Path.Combine(outputDirXsd, diagram.Caption + ".xsd"));
					}

					#endregion

					#region export PNG

					if (!ExcludeImages)
					{
						ExportPng(diagram, Path.Combine(outputDirPng, diagram.Caption + ".png"), diagram.Caption);
					}

					#endregion
				}
			}
			else // only explicitly named diagrams are processed
			{
				foreach (ExportInfo exportInfo in ExportedDiagrams)
				{
					if (exportInfo.Diagram == null)
						continue;

					#region export XSD
					if (exportInfo.Diagram is PSMDiagram && 
							(
								(exportInfo.ExportType & ExportInfo.EExportType.Xsd) == ExportInfo.EExportType.Xsd
								|| (exportInfo.ExportType == ExportInfo.EExportType.Unspec && !ExcludeSchemas)
							)
						)
					{
						ExportXsd(exportInfo.Diagram, Path.Combine(outputDirXsd, exportInfo.Filename + ".xsd"));
					}

					#endregion 

					#region export PNG
					if (	
							(exportInfo.ExportType & ExportInfo.EExportType.Png) == ExportInfo.EExportType.Png
							|| (exportInfo.ExportType == ExportInfo.EExportType.Unspec && !ExcludeSchemas)	
						)
					{
						ExportPng(exportInfo.Diagram, Path.Combine(outputDirPng, exportInfo.Filename + ".png"), exportInfo.Diagram.Caption);
					}

					#endregion 
				}
			}
			ShutDown();
		}

		private void ExportPng(Diagram diagram, string path, string caption)
		{
			Console.Write("Exporting diagram {0} to {1}.... ", diagram.Caption, path);
			mainWindow.DiagramTabManager.ActivateDiagram(diagram);
			mainWindow.UpdateLayout();
			mainWindow.ActiveDiagram.ExportToImage(XCaseCanvas.EExportToImageMethod.PNG, path, caption, true);
			Console.WriteLine("OK");
		}

		private static XmlSchemaTranslator translator;

		/// <summary>
		/// Exports the xml schema into a file. 
		/// </summary>
		/// <param name="diagram">The exported diagram.</param>
		/// <param name="path">The file path.</param>
		private static void ExportXsd(Diagram diagram, string path)
		{
			Console.Write("Exporting diagram {0} to {1}.... ", diagram.Caption, path);
			if (translator == null)
			{
				translator = new XmlSchemaTranslator();
			}
			string xsdString = translator.Translate((PSMDiagram) diagram);
			xsdString = xsdString.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>",
			                              "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			File.WriteAllText(path, xsdString, Encoding.UTF8);
			Console.WriteLine("OK");
		}

		/// <summary>
		/// Prints help to console and shuts down.
		/// </summary>
		/// <param name="parser">The parser.</param>
		private void HelpAndShutdown(CommandLineParser.CommandLineParser parser)
		{
			parser.ShowUsage();
			ShutDown();
		}

		/// <summary>
		/// Lists the diagrams in input projects and shuts down.
		/// </summary>
		private void ListAndShutdown()
		{
			if (InputFile == null)
			{
				Console.WriteLine("Error > input project was not specified");
			}
			else
			{
				CreateHiddenMainWindow();
				Project project = mainWindow.OpenProject(InputFile.FullName);
				Console.WriteLine("Diagrams in project {0}", project.Caption);
				for (int i = 0; i < project.Diagrams.Count; i++)
				{
					Diagram diagram = project.Diagrams[i];
					Console.WriteLine("  {2}: {0} ({1})", diagram.Caption, diagram is PIMDiagram ? "PIM" : "PSM", i);
				}
				Console.WriteLine("Total: {0} diagrams.", project.Diagrams.Count);
			}
			ShutDown();
		}

		private MainWindow mainWindow;

		/// <summary>
		/// Creates the hidden main window. 
		/// </summary>
		private void CreateHiddenMainWindow()
		{
			mainWindow = new MainWindow(null);
			mainWindow.Visibility = Visibility.Hidden;
			//mainWindow.ShowInTaskbar = false;
			mainWindow.WindowState = WindowState.Minimized;
            mainWindow.WindowStyle = WindowStyle.None;
			mainWindow.Topmost = false; 
			mainWindow.Show();
		}

		/// <summary>
		/// Starts the app with GUI interface
		/// </summary>
		/// <param name="filename">The filename.</param>
		public void StartAppGui(string filename)
		{
			mainWindow = new MainWindow(filename);
			mainWindow.Show();
		}

		/// <summary>
		/// Shuts down the application. 
		/// </summary>
		private void ShutDown()
		{
			if (mainWindow != null)
			{
				mainWindow.Close();
			}
			Application.Current.Shutdown();
		}
	}
}
