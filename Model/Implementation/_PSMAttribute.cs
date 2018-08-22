using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the XSem PSM Attribute construct.
    /// </summary>
	[NotCloneable]
	internal class _PSMAttribute : _Property, PSMAttribute
    {
        #region Constructors

        /// <summary>
        /// Creates a PSM attribute representing the given PIM attribute.
        /// </summary>
        /// <param name="representedAttribute">
        /// Reference to the represented PIM attribute
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the given property reference is not a PIM attribute.
        /// </exception>
        public _PSMAttribute(Property representedAttribute, Schema _schema) : base(_schema)
        {
            if (representedAttribute != null && 
                ((representedAttribute is AssociationEnd) || (representedAttribute is PSMAttribute))
                )
                    throw new ArgumentException("The PSM attribute can only represent a PIM attribute");

            this.representedAttribute = representedAttribute;
            if (representedAttribute != null)
                representedAttribute.PropertyChanged += OnRepresentedAttributeChanged;

            // Get the XSem.PSMAttribute stereotype and apply it
            try
            {
                Stereotype xsemSt = Schema.Profiles.Get("XSem").Stereotypes.Get("PSMAttribute");
                xsemStereotype = xsemSt.ApplyTo(this);
                AppliedStereotypes.Remove(xsemStereotype);
            }
            catch (NullReferenceException)
            {
                throw new Exception("Fatal error! Cannot find the XSem profile or the XSem.PSMAttribute stereotype!");
            }

            // Initialize the stereotype instance
            Property ra;
            string elname;

            if (representedAttribute != null)
            {
                elname = representedAttribute.Name;
                ra = representedAttribute;

                // Initialize the attribute properties
                Name = representedAttribute.Name;
                Aggregation = representedAttribute.Aggregation;
                Default = representedAttribute.Default;
                DefaultValue = representedAttribute.DefaultValue;
                IsComposite = representedAttribute.IsComposite;
                IsDerived = representedAttribute.IsDerived;
                IsOrdered = representedAttribute.IsOrdered;
                IsReadOnly = representedAttribute.IsReadOnly;
                IsUnique = representedAttribute.IsUnique;
                Lower = representedAttribute.Lower;
                Type = representedAttribute.Type;
                Upper = representedAttribute.Upper;
                Visibility = representedAttribute.Visibility;
            }
            else
            {
                elname = Name;
                ra = null;
            }
            
            xsemStereotype.Attributes.Get("RepresentedAttribute").Value = new ValueSpecification(ra);
            xsemStereotype.Attributes.Get("Alias").Value = new ValueSpecification(elname);

            referencedGeneralizations = new ObservableCollection<Generalization>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called whenever some of the properties of the represented attribute changes.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnRepresentedAttributeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
                Name = representedAttribute.Name;
            else if (e.PropertyName == "Type")
                Type = representedAttribute.Type;
            else if (e.PropertyName == "Default")
                Default = representedAttribute.Default;
        }

        #endregion

        #region Element Members

        protected override System.Collections.IList ContainingCollection
        {
            get
            {
                if (AttributeContainer != null)
                    return AttributeContainer.PSMAttributes;
                else
                    return Class.Attributes;
            }
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();
            
            if (representedAttribute != null)
                representedAttribute.DerivedPSMAttributes.Remove(this);

            foreach (Generalization gen in referencedGeneralizations)
            {
                gen.ReferencingPSMAttributes.Remove(this);
            }
        }

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();

            if (representedAttribute != null)
                representedAttribute.DerivedPSMAttributes.Add(this);

            foreach (Generalization gen in referencedGeneralizations)
            {
                gen.ReferencingPSMAttributes.Add(this);
            }
            System.Diagnostics.Debug.Assert(Class != null);
        }

        #region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		throw new InvalidOperationException("Clone should not be called on objects of type type PSMAttribute");
    	}

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		base.FillCopy(copyElement, targetModel, createdCopies);
    		PSMAttribute copyPSMAttribute = (PSMAttribute) copyElement;

    		copyPSMAttribute.Alias = Alias;
    		copyPSMAttribute.XSDImplementation = XSDImplementation;

    		if (UsedGeneralizations.Count == 0)
    		{
    			if (this.RepresentedAttribute != null)
    			{
    				int attributeIndex = this.Class.RepresentedClass.Attributes.IndexOf(this.RepresentedAttribute);

    				if (attributeIndex >= 0)
    					copyPSMAttribute.RepresentedAttribute = copyPSMAttribute.Class.RepresentedClass.Attributes[attributeIndex];
    			}
    		}
			else
    		{
    			foreach (Generalization generalization in this.UsedGeneralizations)
    			{
    				Generalization copyGeneralization;
    				if (targetModel != Schema.Model || createdCopies.ContainsKey(generalization))
    				{
    					copyGeneralization = (Generalization) createdCopies[generalization];
    				}
    				else
    				{
    					copyGeneralization = generalization;
    				}
    				copyPSMAttribute.UsedGeneralizations.Add(copyGeneralization);

    				if (this.RepresentedAttribute != null)
    				{
    					int attributeIndex = generalization.General.Attributes.IndexOf(this.RepresentedAttribute);
    					if (attributeIndex >= 0)
    					{
    						copyPSMAttribute.RepresentedAttribute = copyGeneralization.General.Attributes[attributeIndex];
    					}
    				}
    			}
    		}
    		

			if (this.RepresentedAttribute != null && copyPSMAttribute.RepresentedAttribute == null)
			{
				throw new InvalidOperationException("Faild to copy psm class. ");
			}
    	}

    	#endregion 

        #endregion

        #region PSMAttribute Members

        public new PSMClass Class
        {
            get { return (PSMClass)parentClass; }
            set 
            {
                if (parentClass != value)
                {
                    parentClass = value;

                    if (value == null)
                        adaptedElement.Class = null;
                    else
                        adaptedElement.Class = (value as _PSMClass).Adaptee;

                    NotifyPropertyChanged("Class");
                }
            }
        }

        public PSMAttributeContainer AttributeContainer
        {
            get { return attributeContainer; }
            internal set
            {
                if (attributeContainer != value)
                {
                    attributeContainer = value;
                    NotifyPropertyChanged("AttributeContainer");
                }
            }
        }

    	public Property RepresentedAttribute
    	{
    		get { return representedAttribute; }
    		set 
    		{ 
    			representedAttribute = value;
    			if (representedAttribute != null)
    				representedAttribute.PropertyChanged += OnRepresentedAttributeChanged;
    			NotifyPropertyChanged("RepresentedAttribute");
    		}
    	}

    	public string Alias
        {
            get
            {
                return xsemStereotype.Attributes.Get("Alias").Value.StringValue;
            }
            set
            {
                InstantiatedProperty attr = xsemStereotype.Attributes.Get("Alias");
                string val = string.IsNullOrEmpty(value) ? null : value;
                if (attr.Value.StringValue != val)
                {
                    attr.Value.StringValue = val;
                    NotifyPropertyChanged("Alias");
                }
            }
        }

        public string AliasOrName
        {
            get 
            {
                if (!string.IsNullOrEmpty(Alias))
                {
                    return Alias;
                }
                else
                {
                    return Name;
                }
            }
        }

        public string XSDImplementation
        {
            get { return null; }
            set { }
        }

        public ObservableCollection<Generalization> UsedGeneralizations
        {
            get { return referencedGeneralizations; }
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
            get
            {
                if (AttributeContainer != null)
                {
                    return AttributeContainer.XPath + "/" + AliasOrName;
                }
                else if (Class != null)
                {
                    return Class.XPath + "/@" + AliasOrName;
                }
                else return String.Empty;
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// References the attribute container that owns this attribute.
        /// Null if this attribute is not inside an attribute container.
        /// </summary>
        protected PSMAttributeContainer attributeContainer;

        /// <summary>
        /// References the attribute represented by this PSM attribute.
        /// </summary>
        protected Property representedAttribute;

        /// <summary>
        /// References the XSem PSMClass stereotype.<br />
        /// This is a derived property, reference to the stereotype is also
        /// in the AppliedStereotypes collection.
        /// </summary>
        protected StereotypeInstance xsemStereotype;

        /// <summary>
        /// Collection of generalizations used to import this attribute.
        /// </summary>
        protected ObservableCollection<Generalization> referencedGeneralizations;

        /// <summary>
        /// References the diagram that this attribute belongs to.
        /// </summary>
        protected PSMDiagram diagram;

        #endregion
    }
}
