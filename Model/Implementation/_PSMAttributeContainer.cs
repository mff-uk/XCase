using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the PSM Attribute Container construct.
    /// </summary>
    internal class _PSMAttributeContainer : _NamedElement<NUml.Uml2.Class>, PSMAttributeContainer, _ImplPSMSubordinateComponent
    {
        #region Constructors

        /// <summary>
        /// Creates a new attribute container.
        /// It is an UML Class with XSem.PSMAttributeContainer stereotype.
        /// </summary>
        /// <param name="_parent">
        /// Reference to a component that is the parent of this container.
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _PSMAttributeContainer(PSMSuperordinateComponent _parent, Schema _schema)
            : base(_schema, StereotypeTarget.Class)
        {
            parent = _parent;
            attributes = new ObservableCollection<PSMAttribute>();
            attributes.CollectionChanged += OnAttributesChanged;

            adaptedElement = NUml.Uml2.Create.Class();

            // Get the XSem.PSMAttributeContainer stereotype and apply it
            // The stereotype instance is then removed from the collection to hide it from the user.
            try
            {
                Stereotype xsem = Schema.Profiles.Get("XSem").Stereotypes.Get("PSMAttributeContainer");
                xsemStereotype = xsem.ApplyTo(this);
                AppliedStereotypes.Remove(xsemStereotype);
            }
            catch (NullReferenceException)
            {
                throw new Exception("Fatal error! Cannot find the XSem profile or " +
                    "the XSem.PSMAttributeContainer stereotype!");
            }

            // Initialize the stereotype instance
            xsemStereotype.Attributes.Get("Parent").Value = new ValueSpecification(parent);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Occurs whenever the Attributes collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnAttributesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PSMAttribute attribute in e.NewItems)
                {
                    _PSMAttribute implAttr = attribute as _PSMAttribute;
                    if (implAttr == null)
                    {
                        attributes.Remove(attribute);
                        throw new ArgumentException("Inserted attribute was created outside the model library!");
                    }

                    implAttr.Class = PSMClass;
                    implAttr.AttributeContainer = this;
                    adaptedElement.OwnedAttribute.Add(implAttr.AdaptedElement);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PSMAttribute attribute in e.OldItems)
                {
                    _PSMAttribute implAttr = attribute as _PSMAttribute;
                    implAttr.Class = null;
                    implAttr.AttributeContainer = null;
                    adaptedElement.OwnedAttribute.Remove(implAttr.AdaptedElement);
                }
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

            foreach (PSMAttribute attr in attributes)
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
            Debug.Assert(removedFromParent == null);

            removedFromParent = parent;
            removedFromParentIndex = parent.Components.IndexOf(this);
            parent.Components.Remove(this);

            foreach (PSMAttribute attr in attributes)
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
            PSMSuperordinateComponent copyParent = (PSMSuperordinateComponent)createdCopies[Parent];
            PSMAttributeContainer clone = (PSMAttributeContainer)PSMAttributeContainerFactory.Instance.Create(copyParent, targetModel.Schema);

            return clone;
        }

        public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyElement, targetModel, createdCopies);
            PSMAttributeContainer copyPSMAttributeContainer = (PSMAttributeContainer)copyElement;

            SubElementCopiesMap subElementCopiesMap = createdCopies.GetSubElementsList(this);

            foreach (PSMAttribute psmAttribute in PSMAttributes)
            {
                PSMAttribute copyAttribute = copyPSMAttributeContainer.AddAttribute();
                psmAttribute.FillCopy(copyAttribute, targetModel, createdCopies);
                subElementCopiesMap[psmAttribute] = copyAttribute;
            }

            if (Parent != null && createdCopies.ContainsKey(Parent))
            {
                PSMSuperordinateComponent copyParent = (PSMSuperordinateComponent)createdCopies[Parent];
                ((_ImplPSMSubordinateComponent)copyPSMAttributeContainer).Parent = copyParent;
                copyParent.Components.Add(copyPSMAttributeContainer);
            }

        }

        #endregion

        #endregion

        #region PSMAttributeContainer Members

        public PSMAttribute AddAttribute()
        {
            PSMAttribute attr = new _PSMAttribute(null, Schema);
            attributes.Add(attr);
            attr.Diagram = this.Diagram;
            return attr;
        }

        public PSMAttribute AddAttribute(string pimAttribName)
        {
            Property representedAttribute = null;

            PIMClass represented = RepresentedClass;
            if (represented != null)
            {
                foreach (Class cls in represented.MeAndAncestors)
                {
                    representedAttribute = cls.Attributes.Get(pimAttribName);
                    if (representedAttribute != null)
                        break;
                }
            }
            else
                throw new ArgumentException("The given attribute is not owned by the represented class!");

            return AddAttribute(representedAttribute);
        }

        public PSMAttribute AddAttribute(Property attribute)
        {
            if (attribute == null)
                return AddAttribute();

            PIMClass represented = RepresentedClass;
            if (attribute != null && !represented.MeAndAncestors.Contains(attribute.Class))
                throw new ArgumentException("The given attribute is not owned by the represented class!");

            PSMAttribute attr = new _PSMAttribute(attribute, Schema);
            attributes.Add(attr);
            attr.Diagram = this.Diagram;
            attribute.DerivedPSMAttributes.Add(attr);

            if (attribute.Class != represented)
            {
                List<Generalization> path = represented.GetPathToAncestor(attribute.Class);
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

        public override NamedElement GetChildByQualifiedName(string qName)
        {
            NamedElement ae;
            string sName;

            if (BasicGetByQualifiedName(qName, out sName, out ae))
            {
                return attributes.GetByQualifiedName(sName);
            }

            return ae;
        }

        public ObservableCollection<PSMAttribute> PSMAttributes
        {
            get { return attributes; }
        }

        public PSMClass PSMClass
        {
            get
            {
                PSMSuperordinateComponent tmpParent = parent;
                while (tmpParent != null)
                {
                    if (tmpParent is PSMClass)
                        return tmpParent as PSMClass;
                    else
                        tmpParent = (tmpParent as PSMSubordinateComponent).Parent;
                }
                return null;
            }
        }

        public PIMClass RepresentedClass
        {
            get
            {
                PSMClass C = PSMClass;
                if (C == null) return null;
                else return C.RepresentedClass;
            }
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
        /// Collection of the attributes held in this container.
        /// </summary>
        protected ObservableCollection<PSMAttribute> attributes;

        /// <summary>
        /// References a component that is the parent of this container.
        /// </summary>
        protected PSMSuperordinateComponent parent;

        /// <summary>
        /// Reference the instance of the XSem.PSMAttributeContainer stereotype.
        /// </summary>
        protected StereotypeInstance xsemStereotype;

        /// <summary>
        /// References the parent component that contained this container before
        /// it was removed from the model. If the container is currently in the model
        /// this attribute has a null value.
        /// </summary>
        protected PSMSuperordinateComponent removedFromParent;

        /// <summary>
        /// Holds the index in the parent components collection where
        /// this container was removed from. If the container is currently
        /// in the model the value is undefined.
        /// </summary>
        protected int removedFromParentIndex;

        /// <summary>
        /// References the diagram that this container belongs to.
        /// </summary>
        protected PSMDiagram diagram;

        #endregion

        public override string ToString()
        {
            if (Parent == null) return "Attribute Container with no parent";
            if (Parent.Components.Count > 0)
                return String.Format("Attribute Container in {0:F} (Component index: {1})", Parent, Parent.Components.IndexOf(this));
            else
                return String.Format("Attribute Container in {0:F}", Parent);
        }
    }
}
