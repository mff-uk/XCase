using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the XSem PSM Class.
    /// </summary>
    internal class _PSMClass : _Class<NUml.Uml2.Class>, PSMClass, _ImplPSMAssociationChild, _ImplPSMSuperordinateComponent
    {
        #region Constructors

        /// <summary>
        /// Creates a new PSM class representing the given PIM class.
        /// </summary>
        /// <param name="representedClass">Reference to the represented PIM class</param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        /// <exception cref="ArgumentException">Given class is not a PIM class</exception>
        public _PSMClass(PIMClass representedClass, Schema _schema)
            : this(true, representedClass, _schema)
        {

        }

        /// <summary>
        /// Creates a new PSM class representing the given PIM class.
        /// </summary>
        /// <param name="applyXSem">
        /// If true, XSem.PSMClass stereotype is applied to the represented class,
        /// otherwise the class is not extended.
        /// </param>
        /// <param name="representedPIMClass">Reference to the represented PIM class</param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        /// <exception cref="ArgumentException">Given class is not a PIM class</exception>
        public _PSMClass(bool applyXSem, PIMClass representedPIMClass, Schema _schema)
            : base(_schema)
        {
            // Initialize the adapter
            psmAttributes = new ObservableCollection<PSMAttribute>();
            psmAttributes.CollectionChanged += OnPSMAttributesChanged;
            components = new ObservableCollection<PSMSubordinateComponent>();
            components.CollectionChanged += OnComponentsChanged;

            if (applyXSem)
            {
                // Get the XSem.PSMClass stereotype and apply it to this class
                // The stereotype instance is then removed from the collection to hide it from the user.
                try
                {
                    Stereotype stPSMClass = Schema.Profiles.Get("XSem").Stereotypes.Get("PSMClass");
                    xsemStereotype = stPSMClass.ApplyTo(this);
                    AppliedStereotypes.Remove(xsemStereotype);
                }
                catch (NullReferenceException)
                {
                    throw new Exception("Fatal error! Cannot find the XSem profile or XSem.PSMClass stereotype!");
                }

                // Initialize the stereotype instance
                xsemStereotype.Attributes.Get("RepresentedClass").Value = new ValueSpecification(representedPIMClass);
                xsemStereotype.Attributes.Get("ElementName").Value = new ValueSpecification(representedPIMClass.Name);
            }

            this.representedClass = representedPIMClass;
            this.representedClass.PropertyChanged +=
                delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
                {
                    if (e.PropertyName == "Name") NotifyPropertyChanged("RepresentedClassName");
                };
            this.representedClass.DerivedPSMClasses.CollectionChanged +=
                delegate(object sender, NotifyCollectionChangedEventArgs e)
                {
                    NotifyPropertyChanged("RepresentedClassRepresentants");
                };

            // Initialize the class
            Package = representedClass.Package;
            Visibility = representedClass.Visibility;
            Name = representedClass.Name;

            representedClass.PropertyChanged += OnRepresentedClassChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Occurs whenever the Attributes collection has changed.
        /// Synchronizes the content of the PSMAttributes collection.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        /// <exception cref="ArgumentException">
        /// Added some attributes that are not inherited from _PSMAttribute
        /// </exception>
        protected override void OnAttributesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Property prop in e.NewItems)
                {
                    _PSMAttribute implAttr = prop as _PSMAttribute;
                    if (implAttr == null)
                    {
                        attributes.Remove(prop);
                        throw new ArgumentException("A PIM attribute or a PSM attribute created outside " +
                            "the model library has been added to the collection!");
                    }

                    implAttr.Class = this;
                    if (!adaptedElement.OwnedAttribute.Contains(implAttr.Adaptee))
                        adaptedElement.OwnedAttribute.Add(implAttr.Adaptee);

                    if (!psmAttributes.Contains(implAttr))
                        psmAttributes.Add(implAttr);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Property prop in e.OldItems)
                {
                    _PSMAttribute implAttr = prop as _PSMAttribute;

                    implAttr.Class = null;
                    adaptedElement.OwnedAttribute.Remove(implAttr.Adaptee);

                    psmAttributes.Remove(implAttr);
                }
            }
        }

        /// <summary>
        /// Occurs whenever the PSMAttributes collection has changed.
        /// Synchronizes the content of the Attributes collection.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        /// <exception cref="ArgumentException">
        /// Added some attributes that are not inherited from _PSMAttribute
        /// </exception>
        protected void OnPSMAttributesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Property prop in e.NewItems)
                {
                    _PSMAttribute implAttr = prop as _PSMAttribute;
                    if (implAttr == null)
                    {
                        psmAttributes.Remove(implAttr);
                        throw new ArgumentException("A PIM attribute or a PSM attribute created outside " +
                            "the model library has been added to the collection!");
                    }

                    implAttr.Class = this;
                    if (!adaptedElement.OwnedAttribute.Contains(implAttr.Adaptee))
                        adaptedElement.OwnedAttribute.Add(implAttr.Adaptee);

                    if (!attributes.Contains(implAttr))
                        attributes.Add(implAttr);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Property prop in e.OldItems)
                {
                    _PSMAttribute implAttr = prop as _PSMAttribute;

                    implAttr.Class = null;
                    adaptedElement.OwnedAttribute.Remove(implAttr.Adaptee);

                    attributes.Remove(implAttr);
                }
            }
        }

        /// <summary>
        /// Occurs whenever the Components collection has changed.
        /// Sets the Parent attribute of the affected components appropriately.
        /// </summary>
        /// <param name="sender">Information about the change</param>
        /// <param name="e">Object that has raised the event</param>
        protected void OnComponentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PSMSubordinateComponent component in e.NewItems)
                {
                    _ImplPSMSubordinateComponent implComp = component as _ImplPSMSubordinateComponent;
                    if (implComp == null)
                        throw new ArgumentException("Component was created outside the model library!");

                    implComp.Parent = this;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PSMSubordinateComponent component in e.OldItems)
                {
                    ((_ImplPSMSubordinateComponent)component).Parent = null;
                }
            }
        }

        /// <summary>
        /// Occurs whenever some of the represented PIM class properties has changed.
        /// Sets the package property to the same as the represented PIM class has.
        /// </summary>
        /// <param name="sender">Information about the change</param>
        /// <param name="e">Object that has raised the event</param>
        protected void OnRepresentedClassChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Package")
            {
                if (RepresentedClass.Package != null)
                {
                    package.PSMClasses.Remove(this);
                    RepresentedClass.Package.PSMClasses.Add(this);
                }
            }
        }

        /// <summary>
        /// Changes the XSem stereotype of this PSM Class to either XSem.PSMClass
        /// if the RepresentedPSMClass property is null or to XSem.PSMStructuralRepresentative
        /// if the RepresentedPSMClass references another PSM class.
        /// </summary>
        protected void ChangeStereotype()
        {
            Profile xsem = Schema.Profiles.Get("XSem");
            Stereotype newStereotype = null;

            if (xsem == null)
                throw new Exception("Fatal error! Could not find the XSem profile!");

            if (representedPSMClass != null)
                newStereotype = xsem.Stereotypes.Get("PSMStructuralRepresentative");
            else
                newStereotype = xsem.Stereotypes.Get("PSMClass");

            if (newStereotype == null)
                throw new Exception("Fatal error! Could not find the XSem stereotype!");

            StereotypeInstance instance = new _StereotypeInstance(newStereotype as _Stereotype, Schema);
            instance.Attributes.Get("RepresentedClass").Value = xsemStereotype.Attributes.Get("RepresentedClass").Value;
            instance.Attributes.Get("ElementName").Value = xsemStereotype.Attributes.Get("ElementName").Value;

            if (representedPSMClass != null)
                instance.Attributes.Get("RepresentedPSMClass").Value = new ValueSpecification(representedPSMClass);

            xsemStereotype = instance;
        }

        /// <summary>
        /// Adds all the PSM attributes from the attribute containers subordinate 
        /// to root to the attributes list.
        /// </summary>
        /// <param name="attributes">List that will receive the attributes</param>
        /// <param name="root">Reference to the root of the searched tree</param>
        protected void AddAllAttributes(List<PSMAttribute> attributes, PSMSuperordinateComponent root)
        {
            foreach (PSMSubordinateComponent component in root.Components)
            {
                if (component is PSMAttributeContainer)
                    attributes.AddRange((component as PSMAttributeContainer).PSMAttributes);
                else if ((component is PSMContentContainer) || (component is PSMContentChoice))
                    AddAllAttributes(attributes, component as PSMSuperordinateComponent);
            }
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();
            
            if (removedFromUnion != null)
            {
                removedFromUnion.Components.Insert(removedFromUnionIndex, this);
                removedFromUnion = null;
            }
            if (removedFromRootsIndex >= 0)
            {
                Diagram.Roots.Insert(removedFromRootsIndex, this);
            }
            this.RepresentedClass.DerivedPSMClasses.Add(this);

            foreach (PSMAttribute attr in psmAttributes)
            {
                foreach (Generalization gen in attr.UsedGeneralizations)
                {
                    gen.ReferencingPSMAttributes.Add(attr);
                }
            }
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();

            Debug.Assert(removedFromUnion == null);

            if (parentUnion != null)
            {
                removedFromUnionIndex = parentUnion.Components.IndexOf(this);
                removedFromUnion = parentUnion;
                parentUnion.Components.Remove(this);
            }

            if (Diagram.Roots.Contains(this))
            {
                removedFromRootsIndex = Diagram.Roots.IndexOf(this);
                Diagram.Roots.Remove(this);
            }
            else removedFromRootsIndex = -1;

            this.RepresentedClass.DerivedPSMClasses.Remove(this);

            foreach (PSMAttribute attr in psmAttributes)
            {
                foreach (Generalization gen in attr.UsedGeneralizations)
                {
                    gen.ReferencingPSMAttributes.Remove(attr);
                }
            }
        }

        #region clone and copy

        public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
        {
            PIMClass representedClass;
            if (targetModel.Schema != this.Schema)
            {
                representedClass = (PIMClass)createdCopies[RepresentedClass];
            }
            else
            {
                representedClass = RepresentedClass;
            }

            PSMClass clone = representedClass.DerivePSMClass();

            return clone;
        }

        public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyElement, targetModel, createdCopies);
            PSMClass copyPSMClass = (PSMClass)copyElement;

            copyPSMClass.ElementName = ElementName;
            copyPSMClass.AllowAnyAttribute = AllowAnyAttribute;


            if (ParentAssociation != null)
            {
                if (createdCopies.ContainsKey(ParentAssociation))
                    ((_PSMClass)copyPSMClass).ParentAssociation = (PSMAssociation)createdCopies[ParentAssociation];
                // will be added later I suppose

                //else if (targetModel != Schema.Model)
                //{
                //    throw new InvalidOperationException("Missing parent association among created copies");
                //} 

            }
            if (ParentUnion != null)
            {
                if (createdCopies.ContainsKey(ParentUnion))
                {
                    ((PSMClassUnion)createdCopies[ParentUnion]).Components.Add(copyPSMClass);
                }
                // will be added later I suppose

                //else if (targetModel != Schema.Model)
                //{
                //    throw new InvalidOperationException("Missing parent union among created copies");
                //} 
            }


        }

        #endregion

        #endregion

        #region PSMClass Members

        public virtual List<PSMAttribute> AllPSMAttributes
        {
            get
            {
                List<PSMAttribute> list = new List<PSMAttribute>(psmAttributes);

                AddAllAttributes(list, this);

                return list;
            }
        }

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
                PSMElement parent = (PSMElement)ParentAssociation ?? ParentUnion;

                if (parent != null)
                {
                    string parentPath = !string.IsNullOrEmpty(parent.XPath) ? parent.XPath : "<virt-root>";
                    if (HasElementLabel)
                    {
                        return parentPath + "/" + ElementName;
                    }
                    else
                    {
                        return parentPath;
                    }
                }
                else if (!string.IsNullOrEmpty(ElementName))
                {
                    return "/" + ElementName;
                }
                else
                {
                    return "<virt-root>";
                }
            }
        }

        public string ElementName
        {
            get
            {
                return xsemStereotype.Attributes.Get("ElementName").Value.StringValue;
            }
            set
            {
                InstantiatedProperty attr = xsemStereotype.Attributes.Get("ElementName");
                if (attr.Value.StringValue != value)
                {
                    attr.Value.StringValue = value;
                    NotifyPropertyChanged("ElementName");
                }
            }
        }

        public bool HasElementLabel
        {
            get
            {
                return !string.IsNullOrEmpty(ElementName);
            }
        }

        public bool AllowAnyAttribute
        {
            get
            {
                InstantiatedProperty property = xsemStereotype.Attributes.Get("AllowAnyAttribute");
                return property.Value.BooleanValue;
            }
            set
            {
                InstantiatedProperty attr = xsemStereotype.Attributes.Get("AllowAnyAttribute");
                if (attr.Value.BooleanValue != value)
                {
                    attr.Value.BooleanValue = value;
                    NotifyPropertyChanged("AllowAnyAttribute");
                }
            }

        }

        public bool IsStructuralRepresentative
        {
            get { return RepresentedPSMClass != null;  }
        }

        public bool IsStructuralRepresentativeExternal
        {
            get { return IsStructuralRepresentative && RepresentedPSMClass.Diagram != this.Diagram; }
        }

        public PSMDiagramReference GetStructuralRepresentativeExternalDiagramReference()
        {
            if (!IsStructuralRepresentativeExternal)
                return null;
            return Diagram.DiagramReferences.First(r => r.ReferencedDiagram == this.RepresentedPSMClass.Diagram);
        }

        public PSMClass RepresentedPSMClass
        {
            get { return representedPSMClass; }
            set
            {
                if (representedPSMClass != value)
                {
                    representedPSMClass = value;
                    if (representedPSMClass != null)
                    {
                        representedPSMClass.PropertyChanged +=
                            delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
                            {
                                if (e.PropertyName == "Name")
                                    NotifyPropertyChanged("RepresentedPSMClassName");
                            };
                        ChangeStereotype();
                    }
                    NotifyPropertyChanged("RepresentedPSMClass");
                    NotifyPropertyChanged("RepresentedPSMClassName");
                    NotifyPropertyChanged("IsStructuralRepresentative");
                }
            }
        }

        public string RepresentedClassName
        {
            get
            {
                return RepresentedClass.Name;
            }
        }

        public string RepresentedPSMClassName
        {
            get
            {
                if (representedPSMClass != null)
                    return RepresentedPSMClass.Name;
                else return null;
            }
        }

        public ObservableCollection<PSMClass> RepresentedClassRepresentants
        {
            get { return RepresentedClass.DerivedPSMClasses; }
        }

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

        public ObservableCollection<PSMAttribute> PSMAttributes
        {
            get { return psmAttributes; }
        }

        public PIMClass RepresentedClass
        {
            get { return representedClass; }
        }

        public override Property AddAttribute()
        {
            PSMAttribute attr = new _PSMAttribute(null, Schema);
            psmAttributes.Add(attr);
            
            attr.Diagram = this.Diagram;
            attr.Name = "FreeAttribute" + psmAttributes.Count;
            attr.Alias = attr.Name;
            attr.Class = this; 
            return attr;
        }

        public PSMAttribute AddAttribute(string attrName)
        {
            Property representedAttribute = null;

            foreach (Class cls in RepresentedClass.MeAndAncestors)
            {
                representedAttribute = cls.Attributes.Get(attrName);
                if (representedAttribute != null)
                    break;
            }

            if (representedAttribute == null)
                throw new ArgumentException("The given attribute is not owned by the represented class!");

            return AddAttribute(representedAttribute);
        }

        public PSMAttribute AddAttribute(Property attribute)
        {
            if (attribute == null)
                return (PSMAttribute)AddAttribute();

            if (attribute != null && !representedClass.MeAndAncestors.Contains(attribute.Class))
                throw new ArgumentException("The given attribute is not owned by the represented class!");

            PSMAttribute attr = new _PSMAttribute(attribute, Schema);
            psmAttributes.Add(attr);
            attr.Diagram = this.Diagram;
            attribute.DerivedPSMAttributes.Add(attr);

            if (attribute.Class != representedClass)
            {
                List<Generalization> path = representedClass.GetPathToAncestor(attribute.Class);
                if (path != null)
                {
                    foreach (Generalization gen in path)
                    {
                        gen.ReferencingPSMAttributes.Add(attr);
                        attr.UsedGeneralizations.Add(gen);
                    }
                }
            }

            return attr;
        }

        #region clone and copy

        //public new virtual Element Clone(Model targetModel, IDictionary<Element, Element> createdCopies)
        //{
        //    PIMClass cRepresentedClass;
        //    if (targetModel == this.Schema.Model)
        //    {
        //        cRepresentedClass = this.RepresentedClass;
        //    }
        //    else
        //    {
        //        cRepresentedClass = (PIMClass) createdCopies[this.RepresentedClass];
        //    }

        //    PSMClass copy = cRepresentedClass.DerivePSMClass();

        //    return copy; 
        //}

        //public Element CreateCopy(Model targetModel, IDictionary<Element, Element> createdCopies, IList<Element> source)
        //{
        //    PSMClass element = (PSMClass) base.CreateCopy(targetModel, createdCopies, source);


        //    return element;
        //}

        #endregion

        #endregion

        #region PSMSuperordinateComponent Members

        public PSMSubordinateComponent AddComponent(PSMSubordinateComponentFactory factory)
        {
            PSMSubordinateComponent component = factory.Create(this, Schema);
            components.Add(component);

            return component;
        }

        public PSMSubordinateComponent AddComponent(PSMSubordinateComponentFactory factory, int index)
        {
            PSMSubordinateComponent component = factory.Create(this, Schema);
            components.Insert(index, component);

            return component;
        }

        public PSMClassUnion CreateClassUnion()
        {
            PSMClassUnion union = new _PSMClassUnion(Schema);

            return union;
        }

        public ObservableCollection<PSMSubordinateComponent> Components
        {
            get { return components; }
        }

        public bool SubtreeContains(object Object)
        {
            return PSMTree.SubtreeContains(this, Object);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Ordered collection of the components subordinate to this class.
        /// </summary>
        protected ObservableCollection<PSMSubordinateComponent> components;

        /// <summary>
        /// Collection of the PSM attributes.<br />
        /// This collection contains the same objects as the attributes collection
        /// inherited from the Class, but treats them as PSMAttribute instances
        /// rather than Property instances.
        /// </summary>
        protected ObservableCollection<PSMAttribute> psmAttributes;

        /// <summary>
        /// References the represented PIM class.
        /// </summary>
        protected PIMClass representedClass;

        /// <summary>
        /// References the represented PSM class if this class is a structural representative.
        /// Otherwise this value is null.
        /// </summary>
        protected PSMClass representedPSMClass;

        /// <summary>
        /// References the XSem PSMClass stereotype.<br />
        /// This is a derived property, reference to the stereotype is also
        /// in the AppliedStereotypes collection.
        /// </summary>
        protected StereotypeInstance xsemStereotype;

        /// <summary>
        /// References the diagram to which this class belongs.
        /// </summary>
        PSMDiagram diagram;

        /// <summary>
        /// References the PSM Association that owns this class if any.
        /// </summary>
        PSMAssociation parentAssociation;

        /// <summary>
        /// References the PSM Class Union that owns this class if any.
        /// </summary>
        PSMClassUnion parentUnion;

        /// <summary>
        /// References the class union that contained this class before
        /// it was removed from the model. If the class is currently in
        /// the model or it was not owned by a class union, the values is null.
        /// </summary>
        PSMClassUnion removedFromUnion;

        /// <summary>
        /// Holds the index of this class in the parent union components collection
        /// from whch it was removed. If the class is currently in the model, the
        /// value is undefined.
        /// </summary>
        int removedFromUnionIndex;

        /// <summary>
        /// Holds the index of this class in <see cref="PSMDiagram.Roots"/> if it was
        /// removed from Roots previously. Negative index indicates that the class was
        /// not in Roots before it was removed.
        /// <value>-1 when class was not in Roots, non-negative index otherwise</value>
        /// </summary>
        int removedFromRootsIndex = -1;

        #endregion

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) format = "R";

            if (formatProvider != null)
            {
                ICustomFormatter formatter = formatProvider.GetFormat(
                    this.GetType())
                    as ICustomFormatter;

                if (formatter != null)
                    return formatter.Format(format, this, formatProvider);
            }

            string normal = this.ToString();

            if (this.HasElementLabel)
                normal += string.Format(" - '{0}'", ElementName);

            switch (format)
            {
                case "r":
                case "R":
                    PSMSubordinateComponent as_subordinate = this as PSMSubordinateComponent;
                    if (as_subordinate != null)
                    {
                        if (as_subordinate.Parent.Components.Count == 1)
                            return string.Format("{0} in {1:f}", normal, as_subordinate.Parent);
                        else
                            return string.Format("{0} in {1:f} (Component index: {2})", normal, as_subordinate.Parent, as_subordinate.Parent.Components.IndexOf(as_subordinate));
                    }
                    else
                    {
                        return normal;
                    }
                case "f":
                case "F":
                    return normal;
                default: return base.ToString();
            }
        }
    }
}
