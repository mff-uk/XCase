using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the NestingJoin construct.
    /// </summary>
    internal class _NestingJoing : NestingJoin
    {
        #region Constructors

        /// <summary>
        /// Creates a new nesting join with the core class and parent set and empty
        /// child and context paths.
        /// </summary>
        /// <param name="CoreClass">References the core class</param>
        public _NestingJoing(PIMClass CoreClass)
        {
            coreClass = CoreClass;

            _PIMPath tmp = new _PIMPath();
            tmp.Join = this;
            parent = tmp;
            
            tmp = new _PIMPath();
            tmp.Join = this;
            child = tmp;
            
            context = new ObservableCollection<PIMPath>();
            context.CollectionChanged += OnContextChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called whenever the context collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnContextChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PIMPath path in e.NewItems)
                {
                    if (!(path is _PIMPath))
                    {
                        context.Remove(path);
                        throw new ArgumentException("A path created outside the model library added!");
                    }

                    ((_PIMPath)path).Join = this;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PIMPath path in e.OldItems)
                {
                    ((_PIMPath)path).Join = null;
                }
            }
        }

        #endregion

        #region NestingJoin Members

        public PSMAssociation Association
        {
            get { return association; }
            internal set
            {
                association = value;
            }
        }

        public PIMClass CoreClass
        {
            get { return coreClass; }
        }

        public PIMPath Parent
        {
            get { return parent; }
        }

        public PIMPath Child
        {
            get { return child; }
        }

        public ObservableCollection<PIMPath> Context
        {
            get { return context; }
        }

        public PIMPath AddContextPath()
        {
            PIMPath path = new _PIMPath();
            context.Add(path);

            return path;
        }

        public PIMPath AddContextPath(int index)
        {
            PIMPath path = new _PIMPath();
            context.Insert(index, path);

            return path;
        }

        #endregion

        #region Object Members

        public override string ToString()
        {
            string contextRepresentation = "";

            foreach (PIMPath path in context)
                contextRepresentation += path.ToString() + ";";

            return String.Format("{0}^{{{1}}}[{2} -> {3}]", CoreClass.Name, contextRepresentation, Parent, Child);
        }

        #endregion

        #region Fields

        /// <summary>
        /// References the PSM association that owns this nesting join.
        /// </summary>
        protected PSMAssociation association;
        /// <summary>
        /// References the core class.
        /// </summary>
        protected PIMClass coreClass;
        /// <summary>
        /// References the parent class.
        /// </summary>
        protected PIMPath parent;
        /// <summary>
        /// References the PIM path defining the child.
        /// </summary>
        protected PIMPath child;
        /// <summary>
        /// References the PIM path defining the context.
        /// </summary>
        protected ObservableCollection<PIMPath> context;

        #endregion
    }
}
