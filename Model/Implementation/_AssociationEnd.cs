using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of an association end adapter class.
    /// </summary>
    [NotCloneable]
    internal class _AssociationEnd : _Property, AssociationEnd
    {
        #region Properties

        /// <summary>
        /// Returns reference of the association that owns this end.
        /// </summary>
        public Association Association
        {
            get
            {
                return association;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new association bound to the given class and inserts
        /// a reference to the association into the class associations collection.
        /// </summary>
        /// <param name="parentAssociation">Reference to the association that owns this end</param>
        /// <param name="end">Reference to the class that should be bound to this end</param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _AssociationEnd(_ImplAssociation parentAssociation, Class end, Schema _schema)
            : base(_schema)
        {
            adaptedElement = NUml.Uml2.Create.Property();

            association = parentAssociation;
            adaptedElement.Association = parentAssociation.AdaptedAssociation;
            try
            {
                parentClass = end;
                if (!end.Assocations.Contains(parentAssociation))
                    end.Assocations.Add(parentAssociation);
                adaptedElement.Type = (end as _ImplClass).AdaptedClass;
            }
            catch (NullReferenceException)
            {
                throw new ArgumentException("Reference to a class element created outside the model" +
                " library has been passed to the model!");
            }
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();

            Debug.Assert(removedFromAssociation != null);
            if (!removedFromAssociation.Ends.Contains(this))
                removedFromAssociation.Ends.Add(this);

            removedFromAssociation = null;
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();

            Debug.Assert(removedFromAssociation == null);
            removedFromAssociation = association;
            association.Ends.Remove(this);
        }

        #region clone

        public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
        {
            throw new InvalidOperationException("Clone should not be called on objects of type type AssociationEnd");
        }

        #endregion

        #endregion

        #region Fields

        /// <summary>
        /// References the association that owns this end.
        /// </summary>
        protected _ImplAssociation association;
        /// <summary>
        /// References the association that this end was removed from.
        /// Null if the end is currently present in the model.
        /// </summary>
        protected Association removedFromAssociation;

        #endregion

        public override string ToString()
        {
            return !String.IsNullOrEmpty(Name) ? String.Format("{0} ({1})", Name, Class) : String.Format("({0})", Class);
        }
    }
}
