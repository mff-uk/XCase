using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML Association metaclass adapter.
    /// </summary>
    /// <typeparam name="NUmlType">
    /// Type of the adpated nUML element, must inherit from NUml.Uml2.Association
    /// </typeparam>
    internal class _Association<NUmlType> : _NamedElement<NUmlType>, _ImplAssociation
                                    where NUmlType : NUml.Uml2.Association
    {
        #region Constructors

        /// <summary>
        /// Creates a new association relating the given classes.
        /// The association ends are stored in the same order as the classes 
        /// are passed to the association.
        /// </summary>
        /// <param name="classes">
        /// References to the classes to be associated.
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Association(IEnumerable<Class> classes, Schema _schema) 
            : this(classes, _schema, true, StereotypeTarget.Association)
        {
            
        }

        /// <summary>
        /// Creates a new association relating given classes.
        /// The associations ends are stored in the same order as the classes
        /// are passed to the association.
        /// </summary>
        /// <param name="classes">
        /// References to the classes to be associated
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        /// <param name="createAdaptee">
        /// If true, adapted nUML Association is also created, otherwise it is left null.
        /// </param>
        /// <param name="_stTarget">
        /// UML metaclass to be targeted by the stereotype
        /// </param>
        public _Association(IEnumerable<Class> classes, Schema _schema, bool createAdaptee,
            StereotypeTarget _stTarget)
            : base(_schema, _stTarget)
        {
            if (createAdaptee)
                adaptedElement = (NUmlType)NUml.Uml2.Create.Association();

            associationEnds = new ObservableCollection<AssociationEnd>();
            referencingJoins = new ObservableCollection<NestingJoin>();

            // Create an association end adapter for each class in the association
            foreach (Class end in classes)
            {
                associationEnds.Add(new _AssociationEnd(this, end, Schema));
            }

            // If the association is self-referencing and the classes collection
            // contains only one class, create a second end referencing the same class.
            if (associationEnds.Count == 1)
            {
                foreach (Class cls in classes)
                    associationEnds.Add(new _AssociationEnd(this, cls, Schema));
            }
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();
            
            foreach (AssociationEnd end in associationEnds)
            {
                if (!end.Class.Assocations.Contains(this))
                    end.Class.Assocations.Add(this);
            }

            Schema.Model.Associations.Add(this);
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();
            
            foreach (AssociationEnd end in associationEnds)
            {
                end.Class.Assocations.Remove(this);
            }

            Schema.Model.Associations.Remove(this);
        }

		public override bool CanBePutToDiagram(Diagram targetDiagram, IEnumerable<Element> ignoredElements)
    	{
			return Ends.All(end => targetDiagram.IsElementPresent(end.Class) || ignoredElements.Contains(end.Class));
    	}

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		IEnumerable<Class> classes;
    		if (targetModel == Schema.Model)
    		{
    			classes = this.Ends.Select(end => end.Class);
    		}
			else
    		{
    			classes = this.Ends.Select(end => (Class) createdCopies[end.Class]);
    		}
    		Association clone = targetModel.Schema.AssociateClasses(classes);
    		return clone;
    	}

		public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
		{
			base.FillCopy(copyElement, targetModel, createdCopies);

			Association copyAssociation = (Association) copyElement;

            SubElementCopiesMap subElementCopiesMap = createdCopies.GetSubElementsList(this);

    		for (int i = 0; i < this.Ends.Count; i++)
    		{
    			AssociationEnd end = this.Ends[i];
    			AssociationEnd copyEnd = copyAssociation.Ends[i];

    			end.FillCopy(copyEnd, targetModel, createdCopies);
    		    subElementCopiesMap[end] = copyEnd;
    		}

		}

    	#endregion 

    	#endregion

        #region Association Members

        public ObservableCollection<NestingJoin> ReferencingNestingJoins
        {
            get { return referencingJoins; }
        }
        
        public override NamedElement GetChildByQualifiedName(string qName)
        {
            NamedElement ae;
            string sName;

            if (BasicGetByQualifiedName(qName, out sName, out ae))
                return associationEnds.GetByQualifiedName(sName);
            
            return ae;
        }

        public AssociationEnd CreateEnd(int index, Class end)
        {
            _AssociationEnd newEnd = new _AssociationEnd(this, end, Schema);
            associationEnds.Insert(index, newEnd);

            return newEnd;
        }

        public AssociationEnd CreateEnd(Class end)
        {
            return CreateEnd(associationEnds.Count, end);
        }

    	public void RemoveEnd(AssociationEnd end)
    	{
    		associationEnds.Remove(end);
			if (!associationEnds.Contains(end))
			{
				end.Class.Assocations.Remove(this);
			}
    	}

    	public ObservableCollection<AssociationEnd> Ends
        {
            get { return associationEnds; }
        }

        #endregion

        #region _ImplAssociation Members

        public NUml.Uml2.Association AdaptedAssociation
        {
            get { return Adaptee; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Collection of nesting joins that reference this association.
        /// </summary>
        protected ObservableCollection<NestingJoin> referencingJoins;
        /// <summary>
        /// Collection of the association ends.
        /// </summary>
        protected ObservableCollection<AssociationEnd> associationEnds;
        
        #endregion

		public override string ToString()
		{
			String name = "";
			foreach (AssociationEnd end in associationEnds)
			{
				name += end + " - ";
			}
			name = name.Substring(0, name.Length - 3);
			return string.Format("Association {0}( {1} )", Name, name);
		}
    }
}
