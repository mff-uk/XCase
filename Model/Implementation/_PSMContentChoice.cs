using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the PSM Content Choice construct.
    /// </summary>
    internal class _PSMContentChoice : _PSMSuperordinateComponent, PSMContentChoice, _ImplPSMSubordinateComponent
    {
        #region Constructors

        /// <summary>
        /// Creates a new PSM Content choice component.
        /// It is an UML class with XSem.PSMContentChoice stereotype.
        /// </summary>
        /// <param name="_parent">
        /// Reference to a component that is the parent of this one
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _PSMContentChoice(PSMSuperordinateComponent _parent, Schema _schema) : base(_schema)
        {
            parent = _parent;

            // Get the PSMContentChoice stereotype and apply it to this class
            try
            {
                Stereotype xsem = Schema.Profiles.Get("XSem").Stereotypes.Get("PSMContentChoice");
                xsemStereotype = xsem.ApplyTo(this);
                AppliedStereotypes.Remove(xsemStereotype);
            }
            catch (NullReferenceException)
            {
                throw new Exception("Fatal error! Cannot find the XSem profile or " +
                    "the XSem.PSMContentChoice stereotype!");
            }

            // Initialize the stereotype instance
            xsemStereotype.Attributes.Get("Parent").Value = new ValueSpecification(parent);
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();
            Debug.Assert(removedFromParent != null);
            
            removedFromParent.Components.Insert(removedFromParentIndex, this);
            removedFromParent = null;
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();
            Debug.Assert(removedFromParent == null);
            removedFromParent = parent;
            removedFromParentIndex = parent.Components.IndexOf(this);
            parent.Components.Remove(this);
        }

        public override string XPath
        {
            get { return Parent.XPath; }
        }

        #region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
			PSMSuperordinateComponent copyParent = (PSMSuperordinateComponent)createdCopies[Parent];
    		PSMContentChoice clone = new _PSMContentChoice(copyParent, targetModel.Schema);

    		return clone;
    	}

		public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
		{
			base.FillCopy(copyElement, targetModel, createdCopies);
			PSMContentChoice copyPSMContentChoice = (PSMContentChoice)copyElement;

			if (Parent != null && createdCopies.ContainsKey(Parent))
			{
				PSMSuperordinateComponent copyParent = (PSMSuperordinateComponent)createdCopies[Parent];
				((_ImplPSMSubordinateComponent)copyPSMContentChoice).Parent = (PSMSuperordinateComponent)createdCopies[Parent];
				copyParent.Components.Add(copyPSMContentChoice);
			}
		}

		// FillCopy overide not neccessary

    	#endregion 

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

    	protected override string ElementName
    	{
    		get
    		{
    			return "Content choice";
    		}
    	}

    	#endregion

        #region Fields

        /// <summary>
        /// References the component that isa parent of this one.
        /// </summary>
        protected PSMSuperordinateComponent parent;

        /// <summary>
        /// References the xsem stereotype applied to this component.
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

        #endregion
	}
}
