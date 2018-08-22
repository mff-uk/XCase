using System.Collections.Generic;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Moves all the PSM Classes from the components of the source class union
    /// to the components of the target class union.
    /// </summary>
    public class MoveClassUnionContentCommand : DiagramCommandBase
    {
        /// <summary>
        /// Gets or sets the holder of the source class union.
        /// </summary>
        private Helpers.ElementHolder<PSMClassUnion> SourceUnion { get; set; }

        /// <summary>
        /// Gets or sets the holder of the target union.
        /// </summary>
        private Helpers.ElementHolder<PSMClassUnion> TargetUnion { get; set; }

        /// <summary>
        /// List of classes that were moved (for undo).
        /// </summary>
        private readonly List<PSMClass> movedClasses;

        /// <summary>
        /// Creates a new command instance.
        /// </summary>
        /// <param name="controller">Associated model controller</param>
        public MoveClassUnionContentCommand(DiagramController controller)
            : base(controller)
        {
            movedClasses = new List<PSMClass>();
        }

        /// <summary>
        /// Sets this command for use.
        /// </summary>
        /// <param name="sourceUnion">Reference to the source class union</param>
        /// <param name="targetUnion">Reference to the target class union</param>
        public void Set(PSMClassUnion sourceUnion, PSMClassUnion targetUnion)
        {
            if (SourceUnion == null)
                SourceUnion = new Helpers.ElementHolder<PSMClassUnion>();
            if (TargetUnion == null)
                TargetUnion = new Helpers.ElementHolder<PSMClassUnion>();

            SourceUnion.Element = sourceUnion;
            TargetUnion.Element = targetUnion;
        }

        /// <summary>
        /// Sets this command for use.
        /// </summary>
        /// <param name="sourceUnion">Reference to the source class union</param>
        /// <param name="targetUnion">Reference to the target class union holder</param>
        public void Set(PSMClassUnion sourceUnion, Helpers.ElementHolder<PSMClassUnion> targetUnion)
        {
            if (SourceUnion == null)
                SourceUnion = new Helpers.ElementHolder<PSMClassUnion>();

            SourceUnion.Element = sourceUnion;
            TargetUnion = targetUnion;
        }

        /// <summary>
        /// Sets this command for use.
        /// </summary>
        /// <param name="sourceUnion">Reference to the source class union holder</param>
        /// <param name="targetUnion">Reference to the target class union holder</param>
        public void Set(Helpers.ElementHolder<PSMClassUnion> sourceUnion, Helpers.ElementHolder<PSMClassUnion> targetUnion)
        {
            SourceUnion = sourceUnion;
            TargetUnion = targetUnion;
        }

        public override bool CanExecute()
        {
            return SourceUnion != null && TargetUnion != null && SourceUnion.HasValue && TargetUnion.HasValue;
        }

        internal override void CommandOperation()
        {
            foreach (PSMClass cls in SourceUnion.Element.Components)
            {
                movedClasses.Add(cls);
            }

            foreach (PSMClass cls in movedClasses)
            {
                SourceUnion.Element.Components.Remove(cls);
                TargetUnion.Element.Components.Add(cls);
            }
        }

        internal override OperationResult UndoOperation()
        {
            foreach (PSMClass cls in movedClasses)
                TargetUnion.Element.Components.Remove(cls);

            foreach (PSMClass cls in movedClasses)
            {
                SourceUnion.Element.Components.Add(cls);
            }

            movedClasses.Clear();

            return OperationResult.OK;
        }
    }

	#region MoveClassUnionContentCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="MoveClassUnionContentCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class MoveClassUnionContentCommandFactory : DiagramCommandFactory<MoveClassUnionContentCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private MoveClassUnionContentCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of MoveClassUnionContentCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new MoveClassUnionContentCommand(diagramController);
		}
	}

	#endregion
}
