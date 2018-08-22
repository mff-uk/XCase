using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the PSM Class Union construct.
    /// </summary>
    internal class _PSMClassUnion : _NamedElement<NUml.Uml2.Class>, PSMClassUnion, _ImplPSMAssociationChild
    {
        #region Constructors

        /// <summary>
        /// Creates a new PSM Class union.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _PSMClassUnion(Schema _schema) : base(_schema, StereotypeTarget.Class)
        {
            adaptedElement = NUml.Uml2.Create.Class();
            components = new ObservableCollection<PSMAssociationChild>();

            components.CollectionChanged += OnComponentsChanged;

            // Get the XSem.PSMClassUnion stereotype and apply it to this class
            try
            {
                Stereotype xsem = Schema.Profiles.Get("XSem").Stereotypes.Get("PSMClassUnion");
                xsemStereotype = xsem.ApplyTo(this);
                AppliedStereotypes.Remove(xsemStereotype);
            }
            catch (NullReferenceException)
            {
                throw new Exception("Fatal error! XSem profile or XSem.PSMClassUnion stereotype cannot be found!");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Occurs when the Components collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnComponentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PSMAssociationChild child in e.NewItems)
                {
                    if (child is PSMClassUnion)
                        throw new XCaseException("Class union can not be a component of another class union.");

                    if (child.ParentAssociation != null)
                    {
                        //child.ParentAssociation.RemoveMeFromModel();
                    }

                    _PSMClass implClass = child as _PSMClass;
                    if (implClass != null)
                        implClass.ParentUnion = this;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PSMAssociationChild child in e.OldItems)
                {
                    if (child is _PSMClass)
                        (child as _PSMClass).ParentUnion = null;
					_PSMClassUnion childUnion = child as _PSMClassUnion;
					if (childUnion != null)
						childUnion.ParentUnion = null; 
                }
            }
        }

        #endregion

        #region Element Members

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();
        }

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		if (ParentAssociation == null)
    		{
    			throw new InvalidOperationException("Parent association of class union can not be null");
    		}
			PSMClassUnion clone = new _PSMClassUnion(targetModel.Schema); 
    		return clone;
    	}

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		base.FillCopy(copyElement, targetModel, createdCopies);
    		_PSMClassUnion copyPSMClassUnion = (_PSMClassUnion) copyElement;
			
			if (ParentAssociation != null && createdCopies.ContainsKey(ParentAssociation))
			{
				copyPSMClassUnion.ParentAssociation = createdCopies[ParentAssociation] as PSMAssociation;
			}
			
			if (ParentUnion != null && createdCopies.ContainsKey(ParentUnion))
			{
				copyPSMClassUnion.ParentUnion = createdCopies[ParentUnion] as PSMClassUnion;
			}
    	}

    	#endregion 

        #endregion

        #region PSMClassUnion Members

        public PSMAssociation ParentAssociation
        {
            get { return parentAssociation; }
            set
            {
                if (parentAssociation != value)
                {
                    parentAssociation = value;
                    if (parentAssociation != null)
                        parentUnion = null;
                    NotifyPropertyChanged("ParentAssociation");
                }
            }
        }

		public PSMClassUnion ParentUnion
		{
			get { return parentUnion; }
			internal set
			{
				if (parentUnion != value)
				{
					parentUnion = value;
                    if (parentUnion != null)
                        parentAssociation = null;
					NotifyPropertyChanged("ParentUnion");
				}
			}
		}

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();
        }

        public ObservableCollection<PSMAssociationChild> Components
        {
            get { return components; }
        }

        #endregion

        #region PSMElement Members

        public string XPath
        {
            get
            {
                PSMElement parent = (PSMElement) ParentAssociation ?? ParentUnion;
                if (parent == null)
                {
                    return string.Empty;
                }
                else
                {
                    return parent.XPath;
                }

            }
        }

        #endregion

        #region _ImplPSMAssociationChild Members

        public NUml.Uml2.Class AdaptedClass
        {
            get { return adaptedElement; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Collection of the classes in this union.
        /// </summary>
        protected ObservableCollection<PSMAssociationChild> components;

        /// <summary>
        /// References the XSem PSMClass stereotype.<br />
        /// This is a derived property, reference to the stereotype is also
        /// in the AppliedStereotypes collection.
        /// </summary>
        protected StereotypeInstance xsemStereotype;

        /// <summary>
        /// References the PSM Association that owns this union.
        /// </summary>
        protected PSMAssociation parentAssociation;

		/// <summary>
		/// References the PSM Class Union that owns this union if any.
		/// </summary>
		PSMClassUnion parentUnion;

        /// <summary>
        /// References the diagram that this class union belongs to.
        /// </summary>
        PSMDiagram diagram;

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

        #endregion

    	public string ToString(string format, IFormatProvider formatProvider)
    	{
    		return ToString();
    	}

    	public override string ToString()
		{
    		string name = String.IsNullOrEmpty(Name) ? String.Format("Class union {0}", Name) : "Class union";
			
			if (components.Count > 0)
			{
				StringBuilder s = new StringBuilder();
				foreach (PSMAssociationChild component in components)
				{
					s.AppendFormat("{0:F}, ", component);
				}
				s.Remove(s.Length - 2, 2);
				
				return String.Format("{0} ({1})", name, s);
			}
			else
			{
				return name;	
			}
		}
	}
}
