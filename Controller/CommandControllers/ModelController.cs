using System.Linq;
using XCase.Controller.Commands;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller
{
    /// <summary>
    /// Controller for a model. All Commands that do not relate to any Diagram or View are processed here.
    /// </summary>
    public class ModelController : CommandControllerBase
    {
        /// <summary>
        /// Accesses the command called when Undo is clicked on
        /// </summary>
        public UndoCommand UndoCommand { get; private set; }

        /// <summary>
        /// Accesses the command called when Redo is clicked on
        /// </summary>
        public RedoCommand RedoCommand { get; private set; }
        
        /// <summary>
		/// Reference of the model that the controller is working with
		/// </summary>
        public Model.Model Model { get; private set; }

        /// <summary>
        /// Gets a project to wich this model belongs
        /// </summary>
        public Project Project {get; private set; }

		/// <summary>
		/// Creates controller for a model
		/// </summary>
		/// <param name="model">associated model</param>
		/// <param name="project">associated project</param>
        public ModelController(Model.Model model, Project project)
        {
            Model = model;
            Project = project;

            UndoCommand = new UndoCommand(getUndoStack(), this);
            RedoCommand = new RedoCommand(getRedoStack(), this);
        }

        /// <summary>
        /// The actual Undo Stack accessed by getUndoStack method
        /// </summary>
        private readonly CommandStack undoStack = new CommandStack();

        /// <summary>
        /// The actual Redo Stack accessed by getRedoStack method
        /// </summary>
        private readonly CommandStack redoStack = new CommandStack();

        /// <summary>
        /// This is the accessor of the UndoStack
        /// </summary>
        /// <returns>The Undo Stack</returns>
        public override CommandStack getUndoStack()
        {
            return undoStack;
        }

        /// <summary>
        /// This is the accessor of the RedoStack.
        /// </summary>
        /// <returns>The Redo Stack</returns>
        public override CommandStack getRedoStack()
        {
            return redoStack;
        }

        /// <summary>
        /// All commands executed after this call will be stored in a queue and executed when CommitMacro is called
        /// </summary>
        /// <returns>The MacroCommand created</returns>
        public override IMacroCommand BeginMacro()
		{
			CreatedMacro = MacroCommandFactory<ModelController>.Factory().Create(this);
			return CreatedMacro;
		}

		/// <summary>
		/// Returns true if <paramref name="element">element</paramref>
		/// is used in some diagram.
		/// </summary>
		/// <param name="element">searched element</param>
		/// <returns>true if <paramref name="element">element</paramref> is used in some diagram, false otherwise</returns>
    	public bool IsElementUsedInDiagrams(Element element)
    	{
			return Project.PIMDiagrams.Any(diagram => diagram.IsElementPresent(element))
			       || Project.PSMDiagrams.Any(diagram => diagram.IsElementPresent(element));
    	}

		/// <summary>
		/// Returns true if <paramref name="element">element</paramref>
		/// is used in some diagram except from <paramref name="excludedDiagram"/>.
		/// </summary>
		/// <param name="element">searched element</param>
		/// <param name="excludedDiagram">occurrences in this diagram are ignored</param>
		/// <returns>true if <paramref name="element">element</paramref> is used in some diagram, false otherwise</returns>
		public bool IsElementUsedInDiagrams(Element element, Diagram excludedDiagram)
		{
			return Project.PIMDiagrams.Any(diagram => diagram != excludedDiagram && diagram.IsElementPresent(element))
				   || Project.PSMDiagrams.Any(diagram => diagram != excludedDiagram && diagram.IsElementPresent(element));
		}

        /// <summary>
        /// Returns true if <paramref name="element">element</paramref>
        /// has some PSM dependencies (like derived PSM Classes)
        /// </summary>
        /// <param name="element">searched element</param>
        /// <returns>true if <paramref name="element">element</paramref> has PSM dependencies, false otherwise</returns>
        public bool HasElementPSMDependencies(Element element)
        {
            if (element is PIMClass)
            {
                if ((element as PIMClass).DerivedPSMClasses.Count > 0)
                    return true;
            }
            else if (element is Association)
            {
                if ((element as Association).ReferencingNestingJoins.Count > 0)
                    return true;
            }
            else if (element is Generalization)
            {
                if ((element as Generalization).ReferencingPSMAttributes.Count > 0)
                    return true;
                if ((element as Generalization).ReferencingPSMAssociations.Count > 0)
                    return true;
            }
            return false;
        }

		/* not needed any more, but maybe some day...
		public DataType TypeLookupPackageOnly(string typeAsString, Package package)
		{
			IEnumerable<DataType> types = from ownedType in package.OwnedTypes
										  where ownedType.ToString() == typeAsString
										  select ownedType;

			return types.FirstOrDefault();
		}

		public DataType TypeLookup(string typeAsString, Package startPackage)
		{
			Queue<Package> lookupQueue = new Queue<Package>();
			lookupQueue.Enqueue(startPackage);

			Package parentPackage = startPackage.NestingPackage;
			while (parentPackage != null)
			{
				lookupQueue.Enqueue(parentPackage);
				parentPackage = parentPackage.NestingPackage;
			}

			return TypeLookup(typeAsString, lookupQueue);
		}

    	private DataType TypeLookup(string typeAsString, Queue<Package> lookupQueue)
    	{
    		while (lookupQueue.Count != 0)
    		{
    			Package package = lookupQueue.Dequeue();
    			DataType type = TypeLookupPackageOnly(typeAsString, package);
				if (type != null)
					return type;
    			foreach (var nestedPackage in package.NestedPackages)
    			{
    				lookupQueue.Enqueue(nestedPackage);
    			}
    		}
    		return null;
    	}
		*/

		public void RemoveSimpleType(SimpleDataType type)
		{
			
		}

		public void AlterSimpleType(SimpleDataType type, string name, string xsdImplementation)
		{
			AlterSimpleTypeCommand alterSimpleTypeCommand = (AlterSimpleTypeCommand)AlterSimpleTypeCommandFactory.Factory().Create(this);
			alterSimpleTypeCommand.SimpleDataType = type;
			alterSimpleTypeCommand.Name = name;
			alterSimpleTypeCommand.XSDImplementation = xsdImplementation;
			alterSimpleTypeCommand.Execute();
		}

    	public void CreateSimpleType(string name, Package package, SimpleDataType parent, string xsdImplementation, ElementHolder<DataType> holder)
    	{
			AddSimpleTypeCommand simpleTypeCommand = (AddSimpleTypeCommand)AddSimpleTypeCommandFactory.Factory().Create(this);
			simpleTypeCommand.XSDefinition = xsdImplementation;
			simpleTypeCommand.CreatedSimpleType = holder;
			simpleTypeCommand.TypeName = name;
    		simpleTypeCommand.Parent = parent;
			simpleTypeCommand.Package = package;
			simpleTypeCommand.Execute();
    	}
    }
}
