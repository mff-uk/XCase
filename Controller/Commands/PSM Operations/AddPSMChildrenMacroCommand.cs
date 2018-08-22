using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;
using XCase.Controller.Dialogs;
using XCase.Controller.Commands.Helpers;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// This command manages addition of PSM Children. It asks the user which children to add
    /// and then performs all the necessary steps.
    /// </summary>
    public class AddPSMChildrenMacroCommand : MacroCommand<DiagramController>
    {
        private ObservableCollection<TreeClasses> Selected;
        private TreeClasses Structure;
        private PSMClass SourcePSMClass;

        public AddPSMChildrenMacroCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.ADD_PSM_CHILDREN_MACRO;
        }

        /// <summary>
        /// Recursively adds all paths existing in the PIM to the TreeClasses structure
        /// including associations inherited through generalizations
        /// </summary>
        /// <param name="TC">Pre-filled TreeClasses structure</param>
        /// <param name="Paths">List of visited Classes</param>
        /// <param name="Original">Original source class of this command</param>
        /// <param name="UsedGeneralizations">Generalizations used during discovery of current PIM Class</param>
        /// <param name="depth">Depth of recursion. 0 means recursion will stop to be continued on demand</param>
        /// <returns>TreeClasses TC with chidren added or ChildrenAdded=false in case of depth=0</returns>
        private TreeClasses AddChildren(TreeClasses TC, List<PIMClass> Paths, PIMClass Original, List<Generalization> UsedGeneralizations, int depth)
        {
            /*Debug.WriteLine("AddChildren now in PIMClass " + TC.PIMClass);
            Debug.WriteLine("Paths: " + Paths.Count);
            foreach (PIMClass p in Paths) Debug.Write(p + "-");*/

            TC.Children = new ObservableCollection<TreeClasses>();

            if (depth == 0) //Recursion depth maximum reached, save status for lazy discovery
            {
                TC.ChildrenAdded = false;
                TC.RecursionStatus = new RecursionStatus() { Original = Original, Paths = Paths, UsedGeneralizations = UsedGeneralizations };
                return TC;
            }
            else TC.ChildrenAdded = true;
            
            #region Discovering Associations to go through
            List<Association> Associations = new List<Association>();

            //Tells us source class of each processed association
            Dictionary<Association, PIMClass> Sources = new Dictionary<Association, PIMClass>();

            #region Processing Generalizations
            //Add all associations inherited through generalizations
            foreach (PIMClass Current in TC.PIMClass.MeAndAncestors)
            {
                Associations.AddRange(Current.Assocations);                             // Add to processed associations
                foreach (Association A in Current.Assocations) if (!Sources.ContainsKey(A)) Sources.Add(A, Current); // Set their sources
            }
            #endregion
            #endregion

            #region AssociationClass processing
            //ASSOCIATIONCLASS - need to go through all ends of the association, which is association class (and TC.PIMCLASS)
            if (TC.PIMClass is AssociationClass)
            {
                List<PIMClass> newPaths = new List<PIMClass>(Paths);
                newPaths.Add(TC.PIMClass);
                foreach (AssociationEnd E in (TC.PIMClass as AssociationClass).Ends)
                {
                    TC.Children.Add(AddChildren(
                        new TreeClasses()
                        {
                            Association = TC.PIMClass as AssociationClass,
                            Upper = E.Upper,
                            Lower = E.Lower,
                            ParentTC = TC,
                            PIMClass = E.Class as PIMClass,
                            Represented = null,
                            UsedGeneralizations = UsedGeneralizations,
                            Children = new ObservableCollection<TreeClasses>()
                        },
                        newPaths, Original, UsedGeneralizations, --depth));
                }
            }
            #endregion

            //Go through all found associations
            foreach (Association A in Associations)
            {
                List<Generalization> NewUsedGeneralizations = UsedGeneralizations;

                if (TC.PIMClass != Sources[A])
                {
                    NewUsedGeneralizations = new List<Generalization>(UsedGeneralizations);
                    NewUsedGeneralizations.AddRange(TC.PIMClass.GetPathToAncestor(Sources[A]));
                }
                ProcessAssociation(A, TC, Paths, Original, Sources, NewUsedGeneralizations, depth);
            }

            return TC;
        }

        /// <summary>
        /// Rescursive function generating a list of selected items from the TreeView
        /// </summary>
        /// <param name="TC">Current position in the structure</param>
        private void GetSelected(TreeClasses TC)
        {
            if (TC.selected) Selected.Add(TC);
            foreach (TreeClasses T in TC.Children)
                GetSelected(T);
        }

        /// <summary>
        /// Goes through the association and recursively discovers PIMClasses and creates TreeClasses structure
        /// </summary>
        /// <param name="A">Association to process</param>
        /// <param name="T">Current TreeClasses structure</param>
        /// <param name="Paths">Current visited PIMClasses list</param>
        /// <param name="Original">PIMClass in which this command started</param>
        /// <param name="Sources">Source classes of associations</param>
        /// <param name="UsedGeneralizations">Generalizations used during discovery of current PIM Class</param>
        /// <param name="depth">Depth of recursion. 0 means recursion will stop to be continued on demand</param>
        private void ProcessAssociation(Association A, TreeClasses T, List<PIMClass> Paths, PIMClass Original, Dictionary<Association, PIMClass> Sources, List<Generalization> UsedGeneralizations, int depth)
        {
            Debug.WriteLine("AddChidren: Processing association " + A);
            
            #region AssociationClass discovered
            if (A is AssociationClass)
            {
                TreeClasses ACTreeClass;
                if (Paths.Contains(A as AssociationClass)) //Do not go to AssociationClass Associations, it is already visited
                {
                    ACTreeClass =
                        new TreeClasses()
                        {
                            Association = A,
                            Upper = 1,
                            Lower = 1,
                            Represented = null,
                            ParentTC = T,
                            PIMClass = A as AssociationClass,
                            UsedGeneralizations = UsedGeneralizations,
                            Children = new ObservableCollection<TreeClasses>(),
                            ChildrenAdded = true
                        };
                }
                else    //Not visited yet, also go through all associations of AssociationClass
                {
                    Paths = new List<PIMClass>(Paths);
                    Paths.Add(A as AssociationClass);
                    ACTreeClass = AddChildren(
                        new TreeClasses()
                        {
                            Association = A,
                            Upper = 1,
                            Lower = 1,
                            Represented = null,
                            ParentTC = T,
                            UsedGeneralizations = UsedGeneralizations,
                            PIMClass = A as AssociationClass
                        }, Paths, Original, UsedGeneralizations, --depth);
                }
                T.Children.Add(ACTreeClass);
                T = ACTreeClass;
            }
            #endregion
            
            #region Regular association
            else        //Normal Association
            {

                if (A.Ends.All<AssociationEnd>(E => E.Class == Sources[A])) // Self-reference - End of recursion
                {
                    foreach (AssociationEnd E in A.Ends)
                    {
                        T.Children.Add(
                            new TreeClasses()
                            {
                                Association = A,
                                Upper = E.Upper,
                                Lower = E.Lower,
                                Represented = null,
                                ParentTC = T,
                                PIMClass = T.PIMClass,
                                UsedGeneralizations = UsedGeneralizations,
                                Children = new ObservableCollection<TreeClasses>(),
                                ChildrenAdded = true
                            }
                        );
                    }
                    return;
                }

                //Update the list of already visited classes
                List<PIMClass> NewPaths = new List<PIMClass>(Paths);
                NewPaths.Add(Sources[A]);

                foreach (AssociationEnd E in A.Ends)
                {
                    if (!NewPaths.Contains(E.Class as PIMClass) && E.Class != Original) //New class discovered - process recursively
                        T.Children.Add(AddChildren(
                            new TreeClasses()
                            {
                                Association = A,
                                Upper = E.Upper,
                                Lower = E.Lower,
                                Represented = null,
                                UsedGeneralizations = UsedGeneralizations,
                                PIMClass = E.Class as PIMClass,
                                ParentTC = T
                            },
                            NewPaths, Original, UsedGeneralizations, --depth));
                    else if (E.Class != T.PIMClass && (!(A is AssociationClass) || E.Class != Sources[A]) && E.Class == Original) //Path leading back to original source - end of recursion
                        T.Children.Add(
                            new TreeClasses()
                            {
                                Association = A,
                                Upper = E.Upper,
                                Lower = E.Lower,
                                UsedGeneralizations = UsedGeneralizations,
                                Represented = null,
                                PIMClass = E.Class as PIMClass,
                                ParentTC = T,
                                Children = new ObservableCollection<TreeClasses>(),
                                ChildrenAdded = true
                            }
                            );
                }
            }
            #endregion
        }
        /// <summary>
        /// Processes one selected TreeView item. Generates commands for addition of PSMClasses, PSMStructuralRepresentatives and PSMAssociations
        /// </summary>
        /// <param name="T">The item to be processed</param>
        private void GenerateCommands(TreeClasses T)
        {
            #region Holders
            ElementHolder<PSMClass> Child = new ElementHolder<PSMClass>();
            ElementHolder<PSMAssociationChild> ChildAssociation = new ElementHolder<PSMAssociationChild>();
            ElementHolder<PSMAssociation> AssociationHolder = new ElementHolder<PSMAssociation>();
            #endregion

            #region Create new element

            if (T.RootClass == null)
            {
                NewPSMClassCommand c1 = NewPSMClassCommandFactory.Factory().Create(Controller.ModelController) as NewPSMClassCommand;
                c1.RepresentedClass = T.PIMClass;
                c1.CreatedClass = Child;
                Commands.Add(c1);

                if (T.Represented != null) //Creating PSM Structural Representative
                {
                    SetRepresentedPSMClassCommand c1a = SetRepresentedPSMClassCommandFactory.Factory().Create(Controller) as SetRepresentedPSMClassCommand;
                    c1a.Set(T.Represented, Child);
                    Commands.Add(c1a);
                }

                //Put the newly created PSMClass to the diagram
                ElementToDiagramCommand<PSMClass, PSMElementViewHelper> c2 = ElementToDiagramCommandFactory<PSMClass, PSMElementViewHelper>.Factory().Create(Controller) as ElementToDiagramCommand<PSMClass, PSMElementViewHelper>;
                c2.IncludedElement = Child;
                Commands.Add(c2);
            }
            else //Connecting existing root
            {
                Child.Element = T.RootClass;
                RemovePSMClassFromRootsCommand c = RemovePSMClassFromRootsCommandFactory.Factory().Create(Controller) as RemovePSMClassFromRootsCommand;
                c.Set(Child);
                Commands.Add(c);
            }

            #endregion

            #region PSM Association

            #region Path generation
            //Prepare path for NestingJoin - Path from child to parent consisting of AssociationEnds (PIMSteps)
            List<NestingJoinStep> Path = new List<NestingJoinStep>();
            uint? Lower = 1;
            NUml.Uml2.UnlimitedNatural Upper = 1;
            TreeClasses t = T;
            while (t.ParentTC != null)
            {
                Path.Add(new NestingJoinStep() { Association = t.Association, End = t.PIMClass, Start = t.ParentTC.PIMClass });
                Lower *= t.Lower;
                if (t.Upper.IsInfinity) Upper = NUml.Uml2.UnlimitedNatural.Infinity;
                else if (!Upper.IsInfinity) Upper = Upper.Value * t.Upper.Value;
                t = t.ParentTC;
            }

            #endregion

            //Add PSMAssociation connecting the parent to the PSMClass
            Commands.Add(new HolderConvertorCommand<PSMClass, PSMAssociationChild>(Child, ChildAssociation));
            NewPSMAssociationCommand c3 = NewPSMAssociationCommandFactory.Factory().Create(Controller.ModelController) as NewPSMAssociationCommand;
            c3.Set(new ElementHolder<PSMSuperordinateComponent>() { Element = SourcePSMClass }, ChildAssociation, AssociationHolder, null);

            //Set PSM association multiplicity
            c3.Upper = Upper;
            c3.Lower = Lower;
            c3.UsedGeneralizations = T.UsedGeneralizations;
            Commands.Add(c3);
            #endregion

            #region Nesting join
            //Set the nestingjoin
            AddSimpleNestingJoinCommand c4 = AddSimpleNestingJoinCommandFactory.Factory().Create(Controller.ModelController) as AddSimpleNestingJoinCommand;
            c4.Set(AssociationHolder, Path);
            Commands.Add(c4);
            #endregion

            #region Association to diagram
            //Add created PSMAssociations to the diagram
			ElementToDiagramCommand<PSMAssociation, PSMAssociationViewHelper> c5 = (ElementToDiagramCommand<PSMAssociation, PSMAssociationViewHelper>)ElementToDiagramCommandFactory<PSMAssociation, PSMAssociationViewHelper>.Factory().Create(Controller);
            c5.IncludedElement = AssociationHolder;
            Commands.Add(c5);
            #endregion
        }
        
        public void Set(PSMClass sourcePSMClass)
        {
            #region Init recursion
            SourcePSMClass = sourcePSMClass;
            SelectPIMPathDialog dialog = new SelectPIMPathDialog();
            List<Generalization> UsedGeneralizations = new List<Generalization>();
            Structure = AddChildren(
                new TreeClasses() { 
                    Association = null,
                    Lower = 1,
                    Upper = 1,
                    UsedGeneralizations = UsedGeneralizations,
                    ParentTC = null,
                    Represented = null,
                    PIMClass = SourcePSMClass.RepresentedClass },
                new List<PIMClass>(),
                SourcePSMClass.RepresentedClass, UsedGeneralizations, 2);
            dialog.treeView.ItemsSource = Structure.Children;
            dialog.AddChildren = AddChildren;
            #endregion

            if (dialog.ShowDialog() == true) //Show the dialog and let the user choose
            {
                Selected = new ObservableCollection<TreeClasses>();
                GetSelected(Structure);
                ResolveRepresentants();
                foreach (TreeClasses T in Selected)
                    GenerateCommands(T);
            }
        }

        /// <summary>
        /// Processes selected items (from TreeView) and shows a dialog, where the user
        /// can choose whether to create a new PSMClass or 
        /// create a Structural Representative and select the PSMClass to represent
        /// or connect an existing root of a PSM Diagram
        /// </summary>
        public void ResolveRepresentants()
        {
            List<StructuralRepresentativeSelectorData> List = new List<StructuralRepresentativeSelectorData>();
            StructuralRepresentativeSelectorData D;
            //Look for PSM Classes to represent
            foreach (TreeClasses T in Selected)
            {
                D = new StructuralRepresentativeSelectorData();
                D.Representative = false;
                D.ExistingRootClass = false;
                D.TreeClass = T;
                List<PSMClass> possibleRepresented = T.PIMClass.DerivedPSMClasses.Where(PSMClass => PSMClass.Diagram == Controller.Diagram).ToList();

                /* also allow to reference classes from referenced diagrams */
                foreach (PSMDiagramReference diagramReference in ((PSMDiagram)Controller.Diagram).DiagramReferences)
                {
                    PSMDiagramReference reference = diagramReference;
                    possibleRepresented.AddRange(T.PIMClass.DerivedPSMClasses.Where(PSMClass => PSMClass.Diagram == reference.ReferencedDiagram));
                }

                D.PossibleRepresented = possibleRepresented;
                
                D.PossibleRoots = T.PIMClass.DerivedPSMClasses.Where(
                    PSMClass => PSMClass.Diagram == Controller.Diagram && (Controller.Diagram as PSMDiagram).Roots.Contains(PSMClass) && !PSMClass.SubtreeContains(SourcePSMClass)
                    ).ToList();

                D.RootsSelectionEnabled = D.PossibleRoots.Count > 0;

                if (D.PossibleRepresented.Count > 0) //Add only PSMClasses that are candidates for StructuralRepresentative
                {                                    //Their corresponding PIM classes already have a PSMClass/Representative derived in current diagram
                    D.SelectedRepresentative = D.PossibleRepresented[0];
                    if (D.RootsSelectionEnabled)     //Candidate for Root to child transition
                        D.SelectedRootClass = D.PossibleRoots[0];
                    List.Add(D);
                }
            }

            if (List.Count > 0)
            {
                SelectRepresentantDialog dialog = new SelectRepresentantDialog();
                dialog.List.ItemsSource = List;
                dialog.ShowDialog();
            }

            foreach (StructuralRepresentativeSelectorData data in List)
            {
                if (data.Representative) data.TreeClass.Represented = data.SelectedRepresentative;
                if (data.ExistingRootClass) data.TreeClass.RootClass = data.SelectedRootClass;
            }
        }
    }

    #region AddPSMChildrenMacroCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="AddPSMChildrenMacroCommand"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class AddPSMChildrenMacroCommandFactory : DiagramCommandFactory<AddPSMChildrenMacroCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private AddPSMChildrenMacroCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of AddPSMChildrenMacroCommand
        /// <param name="diagramController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController diagramController)
        {
            return new AddPSMChildrenMacroCommand(diagramController);
        }
    }

    #endregion
}

