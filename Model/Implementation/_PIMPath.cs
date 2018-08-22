using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the PIM path.
    /// </summary>
    internal class _PIMPath : PIMPath
    {
        #region Constructors

        /// <summary>
        /// Creates a new empty PIM path.
        /// </summary>
        public _PIMPath()
        {
            steps = new ObservableCollection<PIMStep>();
            steps.CollectionChanged += OnStepsChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called whenever the steps collection changes.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnStepsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PIMStep step in e.NewItems)
                {
                    if (!(step is _PIMStep))
                    {
                        steps.Remove(step);
                        throw new ArgumentException("A step created outside the model library added!");
                    }

                    ((_PIMStep)step).Path = this;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PIMStep step in e.OldItems)
                {
                    ((_PIMStep)step).Path = null;
                }
            }
        }

        #endregion

        #region Object Members

        public override string ToString()
        {
            string representation = "";

            if (steps.Count == 0)
                return ".";

            foreach (PIMStep step in steps)
                representation += step.ToString() + ",";

            return representation;
        }

        #endregion

        #region PIMPath Members

        public NestingJoin Join
        {
            get { return join; }
            internal set
            {
                join = value;
            }
        }

        public void AddStep(PIMClass start, PIMClass end, Association association)
        {
            steps.Add(new _PIMStep(start, end, association));
        }

        public ObservableCollection<PIMStep> Steps
        {
            get { return steps; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Collection of the path steps.
        /// </summary>
        readonly ObservableCollection<PIMStep> steps;
        /// <summary>
        /// References the nesting join that owns this path.
        /// </summary>
        NestingJoin join;

        #endregion
    }
}
