using System.Collections.Generic;
using System.Linq;
using XCase.Controller.Dialogs;
using XCase.Model;
using System;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Implements the group-by operation.
    /// </summary>
    /// <remarks>
	/// Group by is defined as follows...<br /><br />
    /// <b>Input:</b><br /><br />
    /// <list type="bullet">
    ///     <item>C<sub>psm</sub> - A PSM class representing a PIM class C that will be grouped</item>
    ///     <item>
    ///         An ordered set of PSM associations A<sup>1</sup> ... A<sup>n</sup> going from C<sub>psm</sub>
    ///         (directly or from a container assigned to C<sub>psm</sub>). For each i, 1 &lt;= i &lt;=n:
    ///         <list type="bullet">
    ///             <item>
    ///                 A<sup>i</sup> goes to a PSM class C<sub>psm</sub><sup>i</sup> that represents
    ///                 a PIM class C<sup>i</sup>.
    ///             </item>
    ///             <item>
    ///                 A<sup>i</sup> represents a nesting join in a form C<sup>i</sup>[P<sup>i</sup> -&gt; .],
    ///                 where P<sup>i</sup> is a PIM path such that C<sup>i</sup> - P<sup>i</sup> goes to C.
    ///             </item>
    ///         </list>
    ///         <b>Any other type of association is not allowed for the operation!</b>
    /// </item>
    /// </list>
    /// As a result the C<sub>psm</sub> class will be grouped respectively by the classes 
    /// C<sub>psm</sub><sup>i</sup>.<br /><br />
    /// <b>Output:</b><br /><br />
    /// <para>
    /// For each i, 1 &lt;= i &lt;=n, let P'<sup>i</sup> be a PIM path constructed from P<sup>i</sup>
    /// by reversion of all its steps. In other words, if P<sup>i</sup> = S<sub>1</sub> - ... - S<sub>m</sub>
    /// then P'<sup>i</sup> = S<sub>m</sub> - ... - S<sub>1</sub>. Remind that S<sub>i</sub> ends with C<sup>i</sup>.
    /// </para>
    /// <para>
    /// For each association A<sup>i</sup>, 1 &lt;= i &lt;=n a new PSM association A'<sup>i</sup>
    /// will be created as follows:
    /// </para>
    /// <list type="bullet">
    ///     <item>
    ///         A'<sup>i</sup> goes from C<sub>psm</sub><sup>i</sup> to C<sub>psm</sub><sup>i + 1</sup>
    ///         for i &lt; n.<br />
    ///         A'<sup>n</sup> goes from C<sub>psm</sub><sup>i</sup> to C<sub>psm</sub>
    ///     </item>
    ///     <item>
    ///         The nesting join of A'<sup>i</sup> has the following form:
    ///         <list type="bullet">
    ///             <item>CoreClass is always set to C<sub>psm</sub></item>
    ///             <item>Parent path is P'<sup>i</sup></item>
    ///             <item>Child Path is P'<sup>i+1</sup></item>
    ///             <item>Context is an orderd set of paths P'<sup>1</sup>, ..., P'<sup>i-1</sup></item>
    ///         </list>
    ///         shortly C<sub>psm</sub><sup>P'<sup>1</sup>, ..., P'<sup>i-1</sup></sup>
    ///         [P'<sup>i</sup> -&gt; P'<sup>i+1</sup>].
    ///     </item>
    /// </list>
    /// <para>
    /// A<sup>i</sup> is removed from the model
    /// </para>
    /// <para>
    /// If C<sub>psm</sub> was previously a root of the diagram, C<sub>psm</sub><sup>1</sup> is made a root
    /// instead of C<sub>psm</sub>.
    /// </para>
	/// </remarks>
    /// <example>
    /// Let's start with the following situation:
    /// <list type="bullet">
    ///     <item>Purchase class is made a root of the diagram</item>
    ///     <item>Region is inserted as a child of Purchase via Region-Address-Customer-Purchase path.</item>
    ///     <item>Product is inserted as a child of Purchase via Product-Item-Purchase path.</item>
    ///     <item>
    ///         Thus two PSM associations are created:
    ///         <list type="bullet">
    ///             <item>Purchase -&gt; Region with join Region[Region-Address-Customer-Purchase -&gt; .]</item>
    ///             <item>Purchase -&gt; Product with join Product[Product-Item-Purchase -&gt; .]</item>
    ///         </list>
    ///     </item>
    /// </list>
    /// <para>
    /// Now the user executes the GroupBy command on Purchase and selects the Purchase -&gt; Region association
    /// as A<sup>1</sup> and Purchase -&gt; Product association as A<sup>2</sup>.
    /// </para>
    /// The result will be a linear structure having a root in Region with associations as follows:
    /// <list type="bullet">
    ///     <item>
    ///         Association Region -&gt; Product with join 
    ///         Purchase[Purchase-Customer-Address-Region -&gt; Purchase-Item-Product]
    ///     </item>
    ///     <item>
    ///         Association Product -&gt; Purchase with join
    ///         Purchase<sup>Purchase-Customer-Address-Region</sup>[Purchase-Item-Product -&gt; .]
    ///     </item>
    /// </list>
    /// </example>
    public class GroupByCommand : DiagramCommandBase
    {
        public GroupByCommand(DiagramController controller)
            : base(controller)
        {
            Associations = new List<PSMAssociation>();
            createdAssociations = new List<NewPSMAssociationCommand>();
            deletedAssociations = new List<DeleteFromPSMDiagramCommand>();
            visualizedAssociations = new List<ElementToDiagramCommand<PSMAssociation, PSMAssociationViewHelper>>();
            Description = CommandDescription.GROUP_BY;
        }
        
        /// <summary>
        /// List of input associations A<sup>i</sup>, ..., A<sup>n</sup>.
        /// </summary>
        private List<PSMAssociation> Associations { get; set; }

        /// <summary>
        /// List of commands that have created associations A'<sup>1</sup>, ..., A'<sup>n</sup>.
        /// </summary>
        private List<NewPSMAssociationCommand> createdAssociations;

        /// <summary>
        /// List of commands that have put A'<sup>i</sup> to the PSM diagram.
        /// </summary>
        private List<ElementToDiagramCommand<PSMAssociation, PSMAssociationViewHelper>> visualizedAssociations;

        /// <summary>
        /// List of commands that have deleted A<sup>i</sup>.
        /// </summary>
        private List<DeleteFromPSMDiagramCommand> deletedAssociations;

        /// <summary>
        /// Index in the diagram roots collections where the GropedClass resides.
        /// </summary>
        private int rootIndex;

        /// <summary>
        /// Reference to the class that replaced the GroupedClass in the diagram roots collection.
        /// Null if the class was not replaced or was not in the roots collection.
        /// </summary>
        private PSMClass newRoot;

        /// <summary>
        /// Input PSM class C<sub>psm</sub> that will be grouped.
        /// </summary>
        private PSMClass GroupedClass { get; set; }

        public void Set(PSMClass groupedClass)
        {
            if (groupedClass == null) return;

            List<GroupBySelectorData> List = new List<GroupBySelectorData>();
            foreach (PSMSubordinateComponent comp in groupedClass.Components)
            {
                if (comp is PSMAssociation)
                {
                    PSMAssociation A = comp as PSMAssociation;
                    if (A.Child is PSMClass)
                        List.Add(new GroupBySelectorData() { IsSelected = true, PSMAssociation = A, PSMClass = A.Child as PSMClass });
                }
            }

            if (List.Count == 0) return;

            GroupByDialog D = new GroupByDialog();
            D.List.ItemsSource = List;
            if (D.ShowDialog() == true)
            {
                foreach (GroupBySelectorData Data in List)
                    if (Data.IsSelected)
                        Associations.Add(Data.PSMAssociation);
       
                GroupedClass = groupedClass;
            }
        }
        
        public override bool CanExecute()
        {
            // Verify that the input is set
            bool canExecute = (Associations != null) && (Associations.Count >= 1) && GroupedClass != null;

            if (!canExecute)
            {
                ErrorDescription = CommandError.CMDERR_GROUP_BY_NOTSET;
                return false;
            }
            
            // Verify that the grouped class is among the diagram roots
            canExecute &= GroupedClass.Diagram.Roots.Contains(GroupedClass);

            if (canExecute)
            {
                // Verify if all the input associations meet the requirements
                foreach (PSMAssociation assoc in Associations)
                {
                    // Verify that the association represents only one nesting join without context
                    // having the core class set to the association child, child path set to . and
                    // parent path ending with the grouped class
                    canExecute &= (assoc.NestingJoins.Count == 1) && (assoc.NestingJoins[0].Context.Count == 0)
                        && (assoc.NestingJoins[0].CoreClass == ((PSMClass)assoc.Child).RepresentedClass)
                        && (assoc.NestingJoins[0].Child.Steps.Count == 0)
                        && (assoc.NestingJoins[0].Parent.Steps[assoc.NestingJoins[0].Parent.Steps.Count - 1].End
                                == GroupedClass.RepresentedClass);

                    if (!canExecute)
                    {
                        ErrorDescription = String.Format(CommandError.CMDERR_GROUP_BY_ALREADYGROUPED, assoc.Child.Name);
                        break;
                    }

                    // Verify that the association goes directly or indirectly from the grouped class.
                    PSMSuperordinateComponent parent = assoc.Parent;
                    while (!(parent is PSMClass))
                        parent = ((PSMSubordinateComponent)parent).Parent;
                    canExecute &= (parent == GroupedClass);
                }
            }
            else
                ErrorDescription = CommandError.CMDERR_GROUP_BY_NOTROOT;

            return canExecute;
        }

        internal override void CommandOperation()
        {          
            // The context of each new association includes the entire context of the previous
            // created association + Parent path of the nesting join of the previous association
            // Therefore we progressively cumulate it in this collection.
            List<PIMPath> context = new List<PIMPath>();
            
            for (int i = 0; i < Associations.Count; ++i)
            {
                // Get a reference to Ai and Ai+1 if defined.
                PSMAssociation association = Associations[i];
                PSMAssociation nextAssociation = null;
                if (i < Associations.Count - 1)
                    nextAssociation = Associations[i + 1];

                // Remove Ai from the model
                DeleteFromPSMDiagramCommand cmdDel = DeleteFromPSMDiagramCommandFactory.Factory().Create(Controller) as DeleteFromPSMDiagramCommand;
                cmdDel.DeletedElements = new List<Element>();
                cmdDel.DeletedElements.Add(association);
                cmdDel.CommandOperation();
                deletedAssociations.Add(cmdDel);

                // Create a new PSM association A'i
                NewPSMAssociationCommand cmdNew = NewPSMAssociationCommandFactory.Factory().Create(
                    Controller.ModelController) as NewPSMAssociationCommand;
                PSMAssociationChild child = (nextAssociation == null ? GroupedClass : nextAssociation.Child);
                cmdNew.Set((PSMClass)association.Child, child, null, null);
                cmdNew.CommandOperation();
                PSMAssociation newAssociation = cmdNew.CreatedAssociation.Element;
                createdAssociations.Add(cmdNew);

                // Create a nesting join for the new association A'i, all the nesting joins
                // of the associations A1..n will have the GroupedClass as their CoreClass.
                NestingJoin nj = newAssociation.AddNestingJoin(GroupedClass.RepresentedClass);

                // Copy the parent path of the nesting join of Ai to the parent
                // path of A'i and reverse it.
                IEnumerable<PIMStep> revPath = association.NestingJoins[0].Parent.Steps.Reverse();
                foreach (PIMStep step in revPath)
                {
                    nj.Parent.AddStep(step.End, step.Start, step.Association);
                }
                                

                // If Ai+1 is defined then copy its Parent path to the Child path of A'i
                // and reverse it. Set the A'i ending to the child of Ai+1.
                // Thus, the A'i association will look as follows
                //  - A'i: Ai.Child -> Ai+1.Child
                //  - Nesting join: GroupedClass[Ai.ParentPath(reversed) -> Ai+1.ParentPath(reversed)]
                // 
                // If Ai+1 is not defined, A'i will end at the GroupedClass and will look as follows
                //  - A'i: Ai.Child -> GroupedClass
                //  - Nesting join: GroupedClass[Ai.ParentPath(reversed) -> .]
                if (nextAssociation != null)
                {
                    revPath = nextAssociation.NestingJoins[0].Parent.Steps.Reverse();
                    foreach (PIMStep step in revPath)
                    {
                        nj.Child.AddStep(step.End, step.Start, step.Association);
                    }
                }

                // Set the context of A'i as a collection of all the parent paths of A'1 .. A'i-1
                foreach (PIMPath path in context)
                {
                    PIMPath newPath = nj.AddContextPath();
                    foreach (PIMStep step in path.Steps)
                    {
                        newPath.AddStep(step.Start, step.End, step.Association);
                    }
                }

                // Add the parent path of A'i to the temporal collection to enable it for A'i+1
                context.Add(nj.Parent);

                // Put A'i on the diagram
                ElementToDiagramCommand<PSMAssociation, PSMAssociationViewHelper> cmdToDiag =
                    ElementToDiagramCommandFactory<PSMAssociation, PSMAssociationViewHelper>.Factory().Create(Controller)
                    as ElementToDiagramCommand<PSMAssociation, PSMAssociationViewHelper>;
                cmdToDiag.IncludedElement = new Helpers.ElementHolder<PSMAssociation>(newAssociation);
                cmdToDiag.CommandOperation();
                visualizedAssociations.Add(cmdToDiag);
            }

            // If the grouped class was previously a root of the PSM diagram
            // replace it with the topmost grouping class.
            rootIndex = GroupedClass.Diagram.Roots.IndexOf(GroupedClass);
            if (rootIndex != -1)
            {
                newRoot = Associations[0].Child as PSMClass;
                GroupedClass.Diagram.Roots[rootIndex] = newRoot;
            }
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            for (int i = visualizedAssociations.Count - 1; i >= 0; --i)
            {
                visualizedAssociations[i].UndoOperation();
                createdAssociations[i].UndoOperation();
            }

            for (int i = deletedAssociations.Count - 1; i >= 0; --i)
                deletedAssociations[i].UndoOperation();

            if (newRoot != null)
                newRoot.Diagram.Roots[rootIndex] = GroupedClass;

            return OperationResult.OK;
        }

        internal override void RedoOperation()
        {
            for (int i = 0; i < visualizedAssociations.Count; ++i)
            {
                deletedAssociations[i].RedoOperation();
                createdAssociations[i].RedoOperation();
                visualizedAssociations[i].RedoOperation();
            }

            if (newRoot != null)
                newRoot.Diagram.Roots[rootIndex] = newRoot;
        }
    }

    #region GroupByCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="GroupByCommand"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class GroupByCommandFactory : DiagramCommandFactory<GroupByCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private GroupByCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of GroupByCommand
        /// <param name="controller">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController controller)
        {
            return new GroupByCommand(controller);
        }
    }

    #endregion
}
