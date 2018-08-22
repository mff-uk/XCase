using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XCase.Model;

namespace XCase.Controller.Commands
{
    public class MoveSubordinateToSuperordinateCommand<SUPERORDINATE_TYPE> : DiagramCommandBase
        where SUPERORDINATE_TYPE : PSMElement, PSMSuperordinateComponent
    {
        /// <summary>
        /// Creates new instance of <see cref="MoveSubordinateToSuperordinateCommand{SUPERORDINATE_TYPE}">MoveSubordinateToSuperordinateCommand</see>. 
        /// </summary>
        /// <param name="diagramController">command controller</param>
        public MoveSubordinateToSuperordinateCommand(DiagramController diagramController)
            : base(diagramController)
        {
            Description = CommandDescription.PSM_COMPONENTS_MOVED;
            components = new List<PSMSubordinateComponent>();
        }

        private readonly List<PSMSubordinateComponent> components;

        public List<PSMSubordinateComponent> Components
        {
            get
            {
                return components;
            }
        }

        [MandatoryArgument]
        public SUPERORDINATE_TYPE Container { get; set; }

        public override bool CanExecute()
        {
            PSMClass dummy;
            return Components.Count > 0 && CheckCommonParentClass(Components, out dummy);
        }

        private readonly Dictionary<PSMSubordinateComponent, KeyValuePair<PSMSuperordinateComponent, int>> parents = 
            new Dictionary<PSMSubordinateComponent, KeyValuePair<PSMSuperordinateComponent, int>>();


        internal override void CommandOperation()
        {
            foreach (PSMSubordinateComponent psmSubordinateComponent in Components.Where(comp => !(comp is SUPERORDINATE_TYPE) || comp != (object)Container))
            {
                parents[psmSubordinateComponent] = new KeyValuePair<PSMSuperordinateComponent, int>(psmSubordinateComponent.Parent, psmSubordinateComponent.ComponentIndex());
                parents[psmSubordinateComponent].Key.Components.Remove(psmSubordinateComponent);
                Container.Components.Add(psmSubordinateComponent);
            }
        }

        internal override OperationResult UndoOperation()
        {
            foreach (PSMSubordinateComponent psmSubordinateComponent in Components.Where(comp => !(comp is SUPERORDINATE_TYPE) || comp != (object)Container))
            {
                Container.Components.Remove(psmSubordinateComponent);
                parents[psmSubordinateComponent].Key.Components.Insert(parents[psmSubordinateComponent].Value, psmSubordinateComponent);
                Debug.Assert(parents[psmSubordinateComponent].Key == psmSubordinateComponent.Parent);
            }
            return OperationResult.OK;
        }

        public static bool CheckCommonParentClass(IEnumerable<PSMSubordinateComponent> components, out PSMClass parentClass)
        {
            parentClass = null; 
            foreach (PSMSubordinateComponent component in components)
            {
                PSMSuperordinateComponent p = component.Parent;
                while (!(p is PSMClass) && (p is PSMSubordinateComponent) &&
                       ((PSMSubordinateComponent) p).Parent != null)
                {
                    p = ((PSMSubordinateComponent) p).Parent;
                }

                if (p is PSMClass)
                {
                    if (parentClass == null)
                    {
                        parentClass = (PSMClass) p;
                    }
                    else
                    {
                        if (parentClass != p)
                        {
                            parentClass = null;
                            return false; 
                        }
                    }
                }
            }

            return true; 
        }
    }


	#region MoveSubordinateToSuperordinateCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="MoveSubordinateToSuperordinateCommand{SUPERORDINATE_TYPE}"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands receive reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	/// <typeparam name="SUPERORDINATE_TYPE">type of container to move to</typeparam>
    public class MoveSubordinateToSuperordinateCommandFactory<SUPERORDINATE_TYPE> : DiagramCommandFactory<MoveSubordinateToSuperordinateCommandFactory<SUPERORDINATE_TYPE>>
        where SUPERORDINATE_TYPE : PSMElement, PSMSuperordinateComponent
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private MoveSubordinateToSuperordinateCommandFactory()
		{
		}

		/// <summary>
        /// Creates new instance of <see cref="MoveSubordinateToSuperordinateCommand{SUPERORDINATE_TYPE}"/>
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new MoveSubordinateToSuperordinateCommand<SUPERORDINATE_TYPE> (diagramController);
		}
	}

	#endregion
}