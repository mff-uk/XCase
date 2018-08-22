using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Schema;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML NamedElement metaclass adapter.
    /// </summary>
    /// <typeparam name="NUmlType">
    /// Type of the adapted nUML element, has to inherit from NUml.Uml2.NamedElement
    /// </typeparam>
    internal abstract class _NamedElement<NUmlType> : _Element<NUmlType>, NamedElement
                                            where NUmlType : NUml.Uml2.NamedElement
    {
        #region Constructors

        /// <summary>
        /// Creates a new NamedElement adapter but not the adaptee.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        /// <param name="_metaclass">
        /// UML metaclass(es) that this element is instance of.
        /// Bitwise combination of StereotypeTarget values.
        /// </param>
        public _NamedElement(Schema _schema, StereotypeTarget _metaclass) : base(_schema, _metaclass)
        {
            
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs a basic routine for GetChldByQualifiedName.
        /// </summary>
        /// <param name="qName">Qualified name of the element being searched</param>
        /// <param name="remaining">
        /// The remaining part of the qualified name after removing its root part
        /// and the separator. (i.e. Package.Class.Attribute -> Class.Attribute)
        /// </param>
        /// <param name="elem">
        /// Set to a reference to this element if it is the element that was searched
        /// or null if it is not the searched element or the root name does not correspond
        /// to the name of this element.
        /// </param>
        /// <returns>
        /// True if the search should continue to the subordinate collections (elem is null)<br />
        /// False if the search should stop at this level:<br />
        /// - If elem is null, the root of the name is not in this element<br />
        /// - If elem is set to a reference to this element, it is the element being searched
        /// </returns>
        protected bool BasicGetByQualifiedName(string qName, out string remaining, out NamedElement elem)
        {
            string root = qName.Substring(0, qName.IndexOf('.'));
            remaining = qName.Substring(qName.IndexOf('.') + 1);

            if (root != Name)
            {
                elem = null;
                return false;
            }
            else if (qName == Name)
            {
                elem = this;
                return false;
            }
            else
            {
                elem = null;
                return true;
            }
        }

		

    	#endregion

    	#region Element members

    	#region copy

		public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
		{
			base.FillCopy(copyElement, targetModel, createdCopies);

			_NamedElement<NUmlType> copyNamedElement = (_NamedElement<NUmlType>)copyElement;

			copyNamedElement.Name = Name;
			copyNamedElement.Visibility = Visibility;
		}

    	#endregion 

    	#endregion

    	#region NamedElement Members

		public virtual NamedElement GetChildByQualifiedName(string qName)
        {
            NamedElement ae;
            string sName;

            BasicGetByQualifiedName(qName, out sName, out ae);

            return ae;
        }

        public string Name
        {
            get
            {
                return adaptedElement.Name;
            }
            set
            {
                if (adaptedElement.Name != value)
                {
                    adaptedElement.Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public string QualifiedName
        {
            get
            {
                return adaptedElement.QualifiedName;
            }
        }

        private string ontologyEquivalent;
        public string OntologyEquivalent
        {
            get
            {
                return ontologyEquivalent;
            }
            set
            {
                ontologyEquivalent = value;
                NotifyPropertyChanged("OntologyEquivalent");
            }
        }

        public NUml.Uml2.VisibilityKind Visibility
        {
            get
            {
                return adaptedElement.Visibility;
            }
            set
            {
                if (adaptedElement.Visibility != value)
                {
                    adaptedElement.Visibility = value;
                    NotifyPropertyChanged("Visibility");
                }
            }
        }

        #endregion

    	///<summary>
    	///Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    	///</summary>
    	///
    	///<returns>
    	///A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    	///</returns>
    	///<filterpriority>2</filterpriority>
    	public override string ToString()
    	{
    		return !string.IsNullOrEmpty(Name) ? Name : base.ToString();
    	}


    }
}
