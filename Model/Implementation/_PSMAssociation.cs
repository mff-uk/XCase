using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the PSM Association construct.
    /// </summary>
	internal class _PSMAssociation : _NamedElement<NUml.Uml2.Association>, PSMAssociation, _ImplPSMSubordinateComponent
    {
        #region Constructors

        /// <summary>
        /// Creates a new PSM Association with the parent set.
        /// </summary>
        /// <param name="parent">
        /// References the parent component (the one the association starts in).
        /// </param>
        /// <param name="schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The parent component was not created in the model library
        /// </exception>
        public _PSMAssociation(PSMSuperordinateComponent parent, Schema schema)
            : base(schema, StereotypeTarget.Association)
        {
            nestingJoins = new ObservableCollection<NestingJoin>();
            nestingJoins.CollectionChanged += OnNestingJoinsChanged;
            this.parent = parent;

            adaptedElement = NUml.Uml2.Create.Association();
            parentEnd = NUml.Uml2.Create.Property();
            childEnd = NUml.Uml2.Create.Property();

            if (!(parent is _ImplPSMSuperordinateComponent))
                throw new ArgumentException("The parent component was not created in the model library!");

            parentEnd.Class = ((_ImplPSMSuperordinateComponent)parent).AdaptedClass;
            parentEnd.Association = adaptedElement;
            childEnd.Association = adaptedElement;

            adaptedElement.OwnedEnd.Add(parentEnd);
            adaptedElement.OwnedEnd.Add(childEnd);

            lower = 1;

            try
            {
                Stereotype stPSMAssoc = schema.Profiles.Get("XSem").Stereotypes.Get("PSMAssociation");
                xsemStereotype = stPSMAssoc.ApplyTo(this);
                AppliedStereotypes.Remove(xsemStereotype);
            }
            catch (NullReferenceException)
            {
                throw new Exception("Fatal error! Cannot find the XSem profile or XSem.PSMClass stereotype!");
            }

            referencedGeneralizations = new ObservableCollection<Generalization>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called whenever the NestingJoins collection changes.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnNestingJoinsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (NestingJoin nj in e.NewItems)
                {
                    if (!(nj is _NestingJoing))
                    {
                        nestingJoins.Remove(nj);
                        throw new ArgumentException("A nesting join created outside the model library added!");
                    }

                    ((_NestingJoing)nj).Association = this;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (NestingJoin nj in nestingJoins)
                    ((_NestingJoing)nj).Association = null;
            }
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();
            Debug.Assert(removedFromParent != null);
            
            removedFromParent.Components.Insert(removedFromParentIndex, this);
            removedFromParent = null;
            ((_ImplPSMAssociationChild)child).ParentAssociation = this;

            foreach (Generalization gen in referencedGeneralizations)
            {
                gen.ReferencingPSMAssociations.Add(this);
            }

            foreach (NestingJoin nj in nestingJoins)
            {
                foreach (PIMStep step in nj.Parent.Steps)
                    step.Association.ReferencingNestingJoins.Add(nj);
                foreach (PIMStep step in nj.Child.Steps)
                    step.Association.ReferencingNestingJoins.Add(nj);
                foreach (PIMPath path in nj.Context)
                {
                    foreach (PIMStep step in path.Steps)
                        step.Association.ReferencingNestingJoins.Add(nj);
                }
            }
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();
            Debug.Assert(removedFromParent == null);

            removedFromParent = parent;
            removedFromParentIndex = parent.Components.IndexOf(this);
            parent.Components.Remove(this);
            ((_ImplPSMAssociationChild)child).ParentAssociation = null;

            foreach (Generalization gen in referencedGeneralizations)
            {
                gen.ReferencingPSMAssociations.Remove(this);
            }

            foreach (NestingJoin nj in nestingJoins)
            {
                foreach (PIMStep step in nj.Parent.Steps)
                    step.Association.ReferencingNestingJoins.Remove(nj);
                foreach (PIMStep step in nj.Child.Steps)
                    step.Association.ReferencingNestingJoins.Remove(nj);
                foreach (PIMPath path in nj.Context)
                {
                    foreach (PIMStep step in path.Steps)
                        step.Association.ReferencingNestingJoins.Remove(nj);
                }
            }
        }

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		PSMSuperordinateComponent copyParent = (PSMSuperordinateComponent) createdCopies[Parent];
    		PSMAssociation clone = (PSMAssociation) PSMAssociationFactory.Instance.Create(copyParent, targetModel.Schema);

    		return clone;
    	}

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		base.FillCopy(copyElement, targetModel, createdCopies);
    		PSMAssociation copyPSMAssociation = (PSMAssociation) copyElement;

    		copyPSMAssociation.Lower = Lower;
    		copyPSMAssociation.Upper = Upper;
    		//copyPSMAssociation.IsUnique = IsUnique;
			//copyPSMAssociation.IsOrdered = IsOrdered;
    		copyPSMAssociation.Child = (PSMAssociationChild) createdCopies[Child];
    		foreach (NestingJoin nestingJoin in NestingJoins)
    		{
    			PIMClass coreClass;
				if (targetModel.Schema != this.Schema)
    			{
    				coreClass = (PIMClass) createdCopies[nestingJoin.CoreClass];
    			}
				else
    			{
    				coreClass = nestingJoin.CoreClass;
    			}
    			copyPSMAssociation.AddNestingJoin(coreClass);
    		}

    		if (Parent != null && createdCopies.ContainsKey(Parent))
    		{
    			PSMSuperordinateComponent copyParent = (PSMSuperordinateComponent)createdCopies[Parent];
    			((_ImplPSMSubordinateComponent)copyPSMAssociation).Parent = copyParent;
				copyParent.Components.Add(copyPSMAssociation);
    		}
    	}

    	#endregion 

        #endregion

        #region PSMAssociation Members

        public NestingJoin AddNestingJoin(PIMClass coreClass)
        {
            NestingJoin nj = new _NestingJoing(coreClass);
            nestingJoins.Add(nj);

            return nj;
        }

        public PSMAssociationChild Child
        {
            get
            {
                return child;
            }
            set
            {
                if (child != value)
                {
                    if (!(value is _ImplPSMAssociationChild))
                        throw new ArgumentException("Child component was created outside the model library!");

                    child = value;
                    childEnd.Class = ((_ImplPSMAssociationChild)child).AdaptedClass;
                    ((_ImplPSMAssociationChild)child).ParentAssociation = this;

                    NotifyPropertyChanged("Child");
                }
            }
        }

        public ObservableCollection<NestingJoin> NestingJoins
        {
            get { return nestingJoins; }
        }

        public uint? Lower
        {
            get
            {
                return lower;
            }
            set
            {
                if (lower != value)
                {
                    lower = value;
                    NotifyPropertyChanged("Lower");
					NotifyPropertyChanged("MultiplicityString");
                }
            }
        }

        public NUml.Uml2.UnlimitedNatural Upper
        {
            get
            {
                return upper;
            }
            set
            {
                if (upper != value)
                {
                    upper = value;
                    NotifyPropertyChanged("Upper");
					NotifyPropertyChanged("MultiplicityString");
                }
            }
        }

		public bool IsOrdered
		{
			get
			{
				throw new NotImplementedException("Method or operation is not implemented.");
			}
			set
			{
				throw new NotImplementedException("Method or operation is not implemented.");
			}
		}

		public bool IsUnique
		{
			get
			{
				throw new NotImplementedException("Method or operation is not implemented.");
			}
			set
			{
				throw new NotImplementedException("Method or operation is not implemented.");
			}
		}

    	public string MultiplicityString
    	{
			get
			{
				if (Lower != Upper)
					return String.Format("{0}..{1}", Lower, Upper);
				else
					return Lower.ToString();
			}
    	}

        public ObservableCollection<Generalization> UsedGeneralizations
        {
            get { return referencedGeneralizations; }
        }

    	#endregion

        #region PSMSubordinateComponent Members

        public PSMSuperordinateComponent Parent
        {
            get { return parent; }
            set
            {
                if (parent != value)
                {
                    parent = value;
                    NotifyPropertyChanged("Parent");
                }
            }
        }

        #endregion

        #region PSMElement Members

        public PSMDiagram Diagram
        {
            get { return diagram; }
            set
            {
                if (diagram != value)
                {
                    diagram = value;
                    NotifyPropertyChanged("Diagram");
                }
            }
        }

        public string XPath
        {
            get { return Parent.XPath; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// References the child component of the association 
        /// (the component the association ends in).
        /// </summary>
        protected PSMAssociationChild child;
        /// <summary>
        /// References the parent component of the association
        /// (the component the association starts in).
        /// </summary>
        protected PSMSuperordinateComponent parent;
        /// <summary>
        /// Collection of the nesting joins. Their union descibes the association semantics.
        /// </summary>
        protected ObservableCollection<NestingJoin> nestingJoins;
        /// <summary>
        /// Minimal number of occurrences of Child instances 
        /// in a Parent instance.
        /// Initiated by nesting joins.
        /// </summary>
        protected uint? lower;
        /// <summary>
        /// Maximal number of occurrences of Child instances 
        /// in a Parent instance.
        /// Initiated by nesting joins.
        /// </summary>
        protected NUml.Uml2.UnlimitedNatural upper;
        /// <summary>
        /// References the nUML Property instance representing the parent association end.
        /// </summary>
        protected NUml.Uml2.Property parentEnd;
        /// <summary>
        /// References the nUML Property instance representing the child association end.
        /// </summary>
        protected NUml.Uml2.Property childEnd;
        /// <summary>
        /// References the XSem.PSMAssociation stereotype instance.
        /// </summary>
        protected StereotypeInstance xsemStereotype;
        /// <summary>
        /// References the parent component that contained this association before
        /// it was removed fom the model. If the association is currently in the model
        /// this attribute is null.
        /// </summary>
        protected PSMSuperordinateComponent removedFromParent;
        /// <summary>
        /// Holds the index in the parent components collection
        /// where this association was removed from.
        /// If the association is currently in the model the value is undefined.
        /// </summary>
        protected int removedFromParentIndex;
        /// <summary>
        /// Collection of generalizations used to import this association.
        /// </summary>
        protected ObservableCollection<Generalization> referencedGeneralizations;
        /// <summary>
        /// References the diagram that this association belongs to.
        /// </summary>
        protected PSMDiagram diagram;

        #endregion

		public override string ToString()
		{
			return string.Format("Association {0}({1:F} -> {2:F})", Name, Parent, Child);
		}
	}
}
