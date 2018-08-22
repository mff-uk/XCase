using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML profile metaclass adapter.
    /// </summary>
	[NotCloneable]
	internal class _Profile : _Package<NUml.Uml2.Profile>, Profile
    {
        #region Constructors

        /// <summary>
        /// Creates a new empty profile.
        /// The adapted nUML profile object is created but has no attributes nor operations.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Profile(Schema _schema) : this(true, _schema)
        {
            
        }

        /// <summary>
        /// Creates a new empty profile.
        /// </summary>
        /// <param name="createAdaptee">
        /// If true, the adaptee (NUml.Uml2.Profile is created), otherwise it is left null.
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Profile(bool createAdaptee, Schema _schema) : base(false, _schema)
        {
            if (createAdaptee)
                adaptedElement = NUml.Uml2.Create.Profile();

            stereotypes = new ObservableCollection<Stereotype>();
            stereotypes.CollectionChanged += OnStereotypesChanged;

            metamodels = new ObservableCollection<Model>();
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();
            
            if (!Schema.Profiles.Contains(this))
                Schema.Profiles.Add(this);
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();

            Schema.Profiles.Remove(this);
        }

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		if (targetModel.Schema == this.Schema)
    		{
				throw new InvalidOperationException("Clones of Profile should not be created in the same model.");
    		}

    		return targetModel.Schema.AddProfile();
    	}

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
			if (targetModel.Schema == this.Schema)
			{
				throw new InvalidOperationException("Copies of Profile should not be created in the same model.");
			}

			base.FillCopy(copyElement, targetModel, createdCopies);
    		Profile copyProfile = (Profile) copyElement;
    		createdCopies[this] = copyProfile;
    		
			foreach (Stereotype stereotype in this.Stereotypes)
    		{
    			Stereotype addStereotype = copyProfile.AddStereotype();
				stereotype.FillCopy(addStereotype, targetModel, createdCopies);
    		}
    	}

    	#endregion 

        #endregion

        #region Methods

        /// <summary>
        /// Called whenever the stereotypes collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnStereotypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Stereotype st in e.NewItems)
                {
                    _Stereotype implSt = st as _Stereotype;
                    if (st == null)
                        throw new ArgumentException("A stereotype element created outside the model library " + 
                            "has been added to the collection!");

                    adaptedElement.OwnedStereotype.Add(implSt.Adaptee);
                    implSt.Package = this;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Stereotype st in e.OldItems)
                {
                    _Stereotype implSt = st as _Stereotype;

                    adaptedElement.OwnedStereotype.Remove(implSt.Adaptee);
                    implSt.Package = null;
                }
            }
        }

        #endregion

        #region Profile Members

        public Stereotype AddStereotype()
        {
            _Stereotype st = new _Stereotype(Schema);
            stereotypes.Add(st);
            st.Name = "Stereotype" + stereotypes.Count;

            return st;
        }

        public ObservableCollection<Model> MetamodelReference
        {
            get { return metamodels; }
        }

        public ObservableCollection<Stereotype> Stereotypes
        {
            get { return stereotypes; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// References the model containing (directly or indirectly) the metaclasses
        /// that may be extended.
        /// </summary>
        protected ObservableCollection<Model> metamodels;
        
        /// <summary>
        /// Collection of stereotypes owned by this profile.
        /// </summary>
        protected ObservableCollection<Stereotype> stereotypes;

        #endregion
    }
}
