using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;
using XCase.Controller.Commands.Helpers;
using System.Diagnostics;

namespace XCase.Controller.Commands
{
    /// <summary>   
    /// Adds a nesting join to a PSM Association.
    /// </summary>
    /// <example>
    /// <list type="bullet">
    ///     <item>User selects the Product PSM Class and invokes the Add Children command</item>
    ///     <item>User selects the path Item - Purchase - Customer - Address - Region</item>
    ///     <item>
    ///         Resulting nesting join is Region[Address-Customer-Purchase-Item-Product -> .]<br />
    ///         i.e.
    ///         <list type="bullet">
    ///             <item>CoreClass = Region</item>
    ///             <item>Parent = The path that the user has selected but reversed</item>
    ///             <item>Child = empty path</item>
    ///             <item>Context = empty collection</item>
    ///         </list>
    ///     </item>
    /// </list>
    /// </example>
    public class AddSimpleNestingJoinCommand : ModelCommandBase
    {
        /// <summary>
        /// Holder for the association
        /// </summary>
        [MandatoryArgument]
        private ElementHolder<PSMAssociation> Association { get; set; }

        [MandatoryArgument]
        private List<NestingJoinStep> ParentPath { get; set; }

        [CommandResult]
        private NestingJoin CreatedNestingJoin { get; set; }

        public AddSimpleNestingJoinCommand(ModelController modelController)
            : base(modelController)
        {
            Description = CommandDescription.ADD_NESTING_JOIN;
        }

        public override bool CanExecute()
        {
            return (Association != null && ParentPath != null);
        }

        /// <summary>
        /// Sets this command for execution
        /// </summary>
        /// <param name="association">Holder that contains the PSM association</param>
		/// <param name="parentPath">List of NestingJoinSteps describing the ParentPath</param>
        public void Set(ElementHolder<PSMAssociation> association, List<NestingJoinStep> parentPath)
        {
            Association = association;
            ParentPath = parentPath;
        }

        /// <summary>
        /// Sets this command for execution
        /// </summary>
        /// <param name="association">The PSM association</param>
        /// <param name="parentPath">List of NestingJoinSteps describing the ParentPath</param>
        public void Set(PSMAssociation association, List<NestingJoinStep> parentPath)
        {
            Association = new ElementHolder<PSMAssociation>() { Element = association };
            ParentPath = parentPath;
        }
        
        internal override void CommandOperation()
        {
            CreatedNestingJoin = Association.Element.AddNestingJoin((Association.Element.Child as PSMClass).RepresentedClass);
            for (int i = 0; i < ParentPath.Count; i++)
            {
                CreatedNestingJoin.Parent.AddStep(ParentPath[i].End, ParentPath[i].Start, ParentPath[i].Association);
            }
			AssociatedElements.Add(Association.Element);
        }

        internal override OperationResult UndoOperation()
        {
            Association.Element.NestingJoins.Remove(CreatedNestingJoin);
            return OperationResult.OK;
        }

        internal override void RedoOperation()
        {
            Association.Element.NestingJoins.Add(CreatedNestingJoin);
        }
    }

    #region AddSimpleNestingJoinCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="AddSimpleNestingJoinCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class AddSimpleNestingJoinCommandFactory : ModelCommandFactory<AddSimpleNestingJoinCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private AddSimpleNestingJoinCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of AddSimpleNestingJoinCommand
        /// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new AddSimpleNestingJoinCommand(modelController);
        }
    }

    #endregion

    public class NestingJoinStep
    {
        public Association Association;
        public PIMClass Start, End;
    }
}
