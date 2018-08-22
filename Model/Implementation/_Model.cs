using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML Model metaclass adapter.
    /// </summary>
	[NotCloneable]
	internal class _Model : _Package<NUml.Uml2.Model>, Model
    {
        #region Constructors

        /// <summary>
        /// Creates a new empty model.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Model(Schema _schema) : this(true, _schema)
        {
            
        }

        /// <summary>
        /// Creates a new empty model.
        /// </summary>
        /// <param name="createAdaptee">
        /// If true, the adaptee is also created otherwise, it is left null
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Model(bool createAdaptee, Schema _schema) : base(false, _schema)
        {
            if (createAdaptee)
                adaptedElement = NUml.Uml2.Create.Model();

            associations = new ObservableCollection<Association>();
            associations.CollectionChanged += OnAssociationsChanged;

            generalizations = new ObservableCollection<Generalization>();
            generalizations.CollectionChanged += OnGeneralizationsChanged;   
        }

        #endregion

        #region Methods

        /// <summary>
        /// Occurs when the Assocations collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnAssociationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Association assoc in e.NewItems)
                {
                    _ImplAssociation implAssoc = (assoc as _ImplAssociation);
                    if (implAssoc == null)
                    {
                        associations.Remove(assoc);
                        throw new ArgumentException("An Association element created outside the model library " + 
                            "was added to the Associations colletion!");
                    }

                    adaptedElement.OwnedMember.Add(implAssoc.AdaptedAssociation);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Association assoc in e.OldItems)
                {
                    adaptedElement.OwnedMember.Remove((assoc as _ImplAssociation).AdaptedAssociation);
                }
            }
        }

        /// <summary>
        /// Occurs when the Generalizations collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnGeneralizationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Generalization gen in e.NewItems)
                {
                    _Generalization implGen = gen as _Generalization;
                    if (implGen == null)
                    {
                        generalizations.Remove(gen);
                        throw new ArgumentException("A Generalization element created outside the model library " + 
                            "was added to the Generalizations collection!");
                    }

                    adaptedElement.OwnedMember.Add(implGen.Adaptee);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Generalization gen in e.OldItems)
                {
                    adaptedElement.OwnedMember.Remove((gen as _Generalization).Adaptee);
                }
            }
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            throw new NotSupportedException("Model cannot be removed / put back from / to the model!");
        }

        public override void RemoveMeFromModel()
        {
            throw new NotSupportedException("Model cannot be removed / put back from / to model!");
        }

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		throw new InvalidOperationException("Clone should not be called on objects of type type _Model");
    	}

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		throw new InvalidOperationException("CreateCopy should not be called on objects of type type _Model");
    	}

    	#endregion 

        #endregion

        #region Model Members

        public ObservableCollection<Association> Associations
        {
            get { return associations; }
        }

        public ObservableCollection<Generalization> Generalizations
        {
            get { return generalizations; }
        }

        public string ViewPoint
        {
            get
            {
                return adaptedElement.Viewpoint;
            }
            set
            {
                if (adaptedElement.Viewpoint != value)
                {
                    adaptedElement.Viewpoint = value;
                    NotifyPropertyChanged("ViewPoint");
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Collection of associations present in this model.
        /// </summary>
        protected ObservableCollection<Association> associations;
        /// <summary>
        /// Collection of generalizations present in this model.
        /// </summary>
        protected ObservableCollection<Generalization> generalizations;

        #endregion
    }
}
