using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the PSM Content Container construct.
    /// </summary>
    internal class _PSMContentContainer : _PSMSuperordinateComponent, PSMContentContainer, _ImplPSMSubordinateComponent
    {
        #region Constructors

        /// <summary>
        /// Creates a new PSM Content Container.
        /// It is an UML Class with XSem.PSMContentContainer stereotype.
        /// </summary>
        /// <param name="_parent">
        /// Reference to a component that is the parent of this container.
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _PSMContentContainer(PSMSuperordinateComponent _parent, Schema _schema) : base(_schema)
        {
            parent = _parent;

            // Get the XSem stereotype and apply it
            try
            {
                Stereotype xsem = Schema.Profiles.Get("XSem").Stereotypes.Get("PSMContentContainer");
                xsemStereotype = xsem.ApplyTo(this);
                AppliedStereotypes.Remove(xsemStereotype);
            }
            catch (NullReferenceException)
            {
                throw new Exception("Fatal error! Cannot find the XSem profile or " +
                    "the XSem.PSMContentContainer!");
            }

            // Initialize the stereotype instance
            xsemStereotype.Attributes.Get("Parent").Value = new ValueSpecification(parent);
            xsemStereotype.Attributes.Get("ElementLabel").Value = new ValueSpecification("NewContentContainer");
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();

            if (parent == null)
            {
                if (removedFromRootsIndex >= 0)
                {
                    Diagram.Roots.Insert(removedFromRootsIndex, this);
                }
            }
            if (removedFromParent != null)
            {
                removedFromParent.Components.Insert(removedFromParentIndex, this);
                removedFromParent = null;
            }
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();
            
            if (parent != null)
            {
                removedFromParent = parent;
                removedFromParentIndex = parent.Components.IndexOf(this);
                parent.Components.Remove(this);
            }
            else
            {
                if (Diagram.Roots.Contains(this))
                {
                    removedFromRootsIndex = Diagram.Roots.IndexOf(this);
                    Diagram.Roots.Remove(this);
                }
                else removedFromRootsIndex = -1;
            }
        }

        public override string XPath
        {
            get
            {
                if (Parent != null)
                    return Parent.XPath + "/" + Name;
                else
                    return Name;
            }
        }

        #region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
			PSMSuperordinateComponent copyParent = (PSMSuperordinateComponent)createdCopies[Parent];
    		PSMContentContainer clone = new _PSMContentContainer(copyParent, targetModel.Schema);

    		return clone;
    	}

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		base.FillCopy(copyElement, targetModel, createdCopies);
    		PSMContentContainer copyPSMContentContainer = (PSMContentContainer) copyElement;
    		copyPSMContentContainer.ElementLabel = ElementLabel;

			if (Parent != null && createdCopies.ContainsKey(Parent))
			{
				PSMSuperordinateComponent copyParent = (PSMSuperordinateComponent)createdCopies[Parent];
				((_ImplPSMSubordinateComponent)copyPSMContentContainer).Parent = (PSMSuperordinateComponent)createdCopies[Parent];
				copyParent.Components.Add(copyPSMContentContainer);
			}
    	}

    	#endregion 

        #endregion

        #region PSMContentContainer Members

        public string ElementLabel
        {
            get
            {
                return xsemStereotype.Attributes.Get("ElementLabel").Value.StringValue;
            }
            set
            {
                InstantiatedProperty attr = xsemStereotype.Attributes.Get("ElementLabel");
                if (attr.Value.StringValue != value)
                {
                    attr.Value.StringValue = value;
                    NotifyPropertyChanged("ElementLabel");
                }
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

        #region Fields

        /// <summary>
        /// References the component that is the parent of this container.
        /// </summary>
        protected PSMSuperordinateComponent parent;

        /// <summary>
        /// References the instance of the XSem.PSMContentContainer stereotype.
        /// </summary>
        protected StereotypeInstance xsemStereotype;

        /// <summary>
        /// References the parent component that this container was removed from.
        /// If the container is currently in the model the value is null.
        /// </summary>
        protected PSMSuperordinateComponent removedFromParent;

        /// <summary>
        /// Holds the index in the parent components collection where
        /// this container was removed from. If the container is currently
        /// in the model the value is undefined.
        /// </summary>
        protected int removedFromParentIndex;

        /// <summary>
        /// Holds the index of this class in <see cref="PSMDiagram.Roots"/> if it was
        /// removed from Roots previously. Negative index indicates that the class was
        /// not in Roots before it was removed.
        /// <value>-1 when class was not in Roots, non-negative index otherwise</value>
        /// </summary>
        int removedFromRootsIndex = -1;

        #endregion

    	protected override string ElementName
    	{
    		get
    		{
                if (!String.IsNullOrEmpty(Name))
                    return string.Format("Content container {0}", Name);
                else 
    			    return "Content container";
    		}
    	}
    }
}
