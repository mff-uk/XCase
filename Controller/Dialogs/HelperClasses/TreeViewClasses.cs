using XCase.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace XCase.Controller.Dialogs
{
    /// <summary>
    /// This class represents one item of the TreeView in the SelectPIMPathDialog used in AddPSMChildrenMacroCommand
    /// It is used as the DataContext of the TreeView and stores all useful data for the PIM -> PSM derivation
    /// </summary>
    public class TreeClasses : INotifyPropertyChanged
    {
        /// <summary>
        /// The PIMClass of the current item (the one to be derived from)
        /// </summary>
        public PIMClass PIMClass { get; set; }

        /// <summary>
        /// The PIM Association through which this PIMClass was discovered
        /// </summary>
        public Association Association { get; set; }

        /// <summary>
        /// Parent TreeClasses structure (usually parent as in PIMPath)
        /// </summary>
        public TreeClasses ParentTC { get; set; }

        /// <summary>
        /// Lower multiplicity of the AssociationEnd belonging to this PIMClass
        /// </summary>
        public uint? Lower { get; set; }

        /// <summary>
        /// Upper multiplicity of the AssociationEnd belonging to this PIMClass
        /// </summary>
        public NUml.Uml2.UnlimitedNatural Upper { get; set; }
        
        private bool _selected;

        /// <summary>
        /// Stores the information whether this item was selected in the TreeView for derivation
        /// Works as two-way binding, setting this property causes the TreeViewItem to become selected
        /// </summary>
        public bool selected {
            get 
            { 
                return _selected; 
            } 
            set 
            { 
                _selected = value;
                NotifyPropertyChanged("selected");
            } 
        }

        private ObservableCollection<TreeClasses> _children;

        /// <summary>
        /// Children of this PIMClass as in all possible ways to continue the PIMPath
        /// </summary>
        public ObservableCollection<TreeClasses> Children
        {
            get
            {
                return _children;
            }
            set
            {
                _children = value;
                NotifyPropertyChanged("Children");
            }
        }

        /// <summary>
        /// All generalizations used to get to this PIMClass
        /// </summary>
        public List<Generalization> UsedGeneralizations { get; set; }
        
        /// <summary>
        /// The PSMClass to be represented. Not null indicates that this item will become a Structural Representative
        /// </summary>
        public PSMClass Represented { get; set; }

        /// <summary>
        /// A root PSMClass to be connected instead of creating a new PSMClass
        /// </summary>
        public PSMClass RootClass { get; set; }

        /// <summary>
        /// Tells whether the children of the PIMClass represented by this TreeClasses were already added or not.
        /// Used in lazy discovery of possible PSM children
        /// </summary>
        public bool ChildrenAdded;

        /// <summary>
        /// Saves the recursion status for the lazy discovery of possible PSM children
        /// </summary>
        public RecursionStatus RecursionStatus;

        /// <summary>
        /// Notifies that a property has been changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    /// <summary>
    /// Saves the recursion status for the lazy discovery of possible PSM children
    /// </summary>
    public class RecursionStatus
    {
        /// <summary>
        /// Visited PIM classes for avoiding cycle
        /// </summary>
        public List<PIMClass> Paths;

        /// <summary>
        /// Original PIM class where the recursion started
        /// </summary>
        public PIMClass Original;

        /// <summary>
        /// Generalizations used in process of discovering current PIMClass
        /// </summary>
        public List<Generalization> UsedGeneralizations;
    }

}
