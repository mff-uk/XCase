using System;
namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the PIMStep construct.
    /// </summary>
    internal class _PIMStep : PIMStep
    {
        #region Constructors

        /// <summary>
        /// Creates a new PIM step begining in start, ending in end and using association.
        /// </summary>
        /// <param name="start">Reference to the starting PIM class</param>
        /// <param name="end">Reference to the ending PIM class</param>
        /// <param name="association">Association used to get from start to end.</param>
        public _PIMStep(PIMClass start, PIMClass end, Association association)
        {
            if ((start == null) || (end == null) || (association == null))
                throw new ArgumentException("All the members of the step have to be set to non-null values!");

            this.start = start;
            this.end = end;
            this.association = association;
        }

        #endregion

        #region Object Members

        public override string ToString()
        {
            return String.Format("{0}-({1})-{2}", start.Name, association.Name, end.Name);
        }

        #endregion

        #region PIMStep Members

        public PIMPath Path
        {
            get { return path; }
            internal set
            {
                if (path != value)
                {
                    if (path != null)
                        association.ReferencingNestingJoins.Remove(path.Join);
                    
                    path = value;

                    if (path != null)
                        association.ReferencingNestingJoins.Add(path.Join);
                }
            }
        }

        public Association Association
        {
            get { return association; }
        }

        public PIMClass Start
        {
            get { return start; }
        }

        public PIMClass End
        {
            get { return end; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// References the PIM class in which the step starts.
        /// </summary>
        readonly PIMClass start;
        /// <summary>
        /// References the PIM class in which the step ends.
        /// </summary>
        readonly PIMClass end;
        /// <summary>
        /// References the association (in PIM) used to get from start to end.
        /// </summary>
        readonly Association association;
        /// <summary>
        /// References the path that owns this step.
        /// </summary>
        PIMPath path;

        #endregion
    }
}
