using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace XCase.Model.Implementation
{
	/// <summary>
	/// Implementation of the UML Generailization metaclass adapter.
	/// </summary>
	internal class _Generalization : _Element<NUml.Uml2.Generalization>, Generalization
	{
		#region Constructors

		/// <summary>
		/// Creates a new generalization that does not connect any classes.
		/// The adapee is created.
		/// </summary>
		/// <param name="_schema">
		/// Reference to the Schema instance that is the top
		/// of this model hierarchy.
		/// </param>
		public _Generalization(Schema _schema)
			: this(true, _schema)
		{
		}

		/// <summary>
		/// Creates a new generalization that does not connect any classes.
		/// </summary>
		/// <param name="createAdaptee">
		/// If true, the adaptee is created, otherwise it is left null.
		/// </param>
		/// <param name="_schema">
		/// Reference to the Schema instance that is the top
		/// of this model hierarchy.
		/// </param>
		public _Generalization(bool createAdaptee, Schema _schema)
			: base(_schema, StereotypeTarget.Generalization)
		{
			adaptedElement = NUml.Uml2.Create.Generalization();
			referencingPSMAssociations = new ObservableCollection<PSMAssociation>();
			referencingPSMAttributes = new ObservableCollection<PSMAttribute>();
		}

		#endregion

		#region Element Members

		public override void PutMeBackToModel()
		{
            base.PutMeBackToModel();

			if (!Schema.Model.Generalizations.Contains(this))
				Schema.Model.Generalizations.Add(this);

			if (!specific.Generalizations.Contains(this))
				specific.Generalizations.Add(this);

			if (!general.Specifications.Contains(this))
				general.Specifications.Add(this);
		}

		/// <summary>
		/// Removes the generalization from the model.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// The generalization was already removed or some PSM attributes or associations still reference it
		/// </exception>
		public override void RemoveMeFromModel()
		{
            base.RemoveMeFromModel();

			if ((referencingPSMAssociations.Count > 0) || (referencingPSMAttributes.Count > 0))
				throw new NotSupportedException("The generalization cannot be removed, because there are attributes" +
					"or associations that refence it in the platform-specific model!");

			Schema.Model.Generalizations.Remove(this);
			specific.Generalizations.Remove(this);
			general.Specifications.Remove(this);
		}

		#region clone and copy

		public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
			Generalization clone;
			if (this.General is PIMClass)
			{
				if (targetModel.Schema == this.Schema)
				{
					throw new InvalidOperationException("Cannot create clones of pim generalization in the same model.");
				}
				else
				{
					clone = targetModel.Schema.SetGeneralization(null, null);
				}
			}
			else
			{
				clone = targetModel.Schema.SetGeneralization(null, null);
			}
	
    		return clone;
    	}

		public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
		{
			if (this.general is PIMClass)
			{
				if (targetModel.Schema == this.Schema)
				{
					throw new InvalidOperationException("Cannot create copy of pim generalization in the same model.");
				}
			}
			base.FillCopy(copyElement, targetModel, createdCopies);
			Generalization copyGeneralization = (Generalization) copyElement;
			copyGeneralization.General = (Class) createdCopies[General];
			copyGeneralization.Specific = (Class) createdCopies[Specific];
			copyGeneralization.IsSubstituable = IsSubstituable;
		}

		#endregion

		#endregion

		#region Generalization Members

		public bool IsSubstituable
		{
			get
			{
				return adaptedElement.IsSubstitutable;
			}
			set
			{
				if (adaptedElement.IsSubstitutable != value)
				{
					adaptedElement.IsSubstitutable = value;
					NotifyPropertyChanged("IsSubstituable");
				}
			}
		}

		public Class General
		{
			get
			{
				return general;
			}
			set
			{
				if (general != value)
				{
					if (Specific != null && value != null && (Specific is PSMClass != value is PSMClass))
					{
						throw new ArgumentException("Cannot set generalization between PIM and PSM class!");
					}

					_ImplClass implClass = value as _ImplClass;
					if (implClass == null)
						throw new ArgumentException("A class element created outside the model library has " +
							"been passed!");

					// If the generalization previously referenced another class
					// remove this generalization from its specifications collection
					if (general != null)
						general.Specifications.Remove(this);

					// Set the new general class reference
					general = implClass;
					if (general != null)
					{
						adaptedElement.General = general.AdaptedClass;
						general.Specifications.Add(this);
					}
					else
						adaptedElement.General = null;

					NotifyPropertyChanged("General");
				}
			}
		}

		public Class Specific
		{
			get
			{
				return specific;
			}
			set
			{
				if (specific != value)
				{
					if (General != null && value != null && (General is PSMClass != value is PSMClass))
					{
						throw new ArgumentException("Cannot set generalization between PIM and PSM class!");
					}

					_ImplClass implClass = value as _ImplClass;
					if (implClass == null)
						throw new ArgumentException("A class element created outside the model library has " +
							"been passed!");

					// If the generalization previously referenced another class
					// remove this generalization from its generalizations collection
					if (specific != null)
						specific.Generalizations.Remove(this);

					// Set the new specific class
					specific = implClass;
					if (specific != null)
					{
						adaptedElement.Specific = specific.AdaptedClass;
						specific.Generalizations.Add(this);
					}
					else
						adaptedElement.Specific = null;

					NotifyPropertyChanged("Specific");
				}
			}
		}

		public ObservableCollection<PSMAttribute> ReferencingPSMAttributes
		{
			get { return referencingPSMAttributes; }
		}

		public ObservableCollection<PSMAssociation> ReferencingPSMAssociations
		{
			get { return referencingPSMAssociations; }
		}

		#endregion

		#region Fields

		/// <summary>
		/// References the general class in the relation.
		/// </summary>
		protected _ImplClass general;
		/// <summary>
		/// References the specific class in the relation.
		/// </summary>
		protected _ImplClass specific;
		/// <summary>
		/// Collection of PSM attributes that reference this generalization.
		/// </summary>
		private readonly ObservableCollection<PSMAttribute> referencingPSMAttributes;
		/// <summary>
		/// Collection of PSM associations that reference this generalization.
		/// </summary>
		private readonly ObservableCollection<PSMAssociation> referencingPSMAssociations;

		#endregion

		public override bool CanBePutToDiagram(Diagram targetDiagram, IEnumerable<Element> ignoredElements)
		{
			return (targetDiagram.IsElementPresent(Specific) || ignoredElements.Contains(Specific))
				&& (targetDiagram.IsElementPresent(General) || ignoredElements.Contains(General));
		}

		public override string ToString()
		{
			return String.Format("Generalization {0} -> {1}", Specific, General);
		}
	}
}
