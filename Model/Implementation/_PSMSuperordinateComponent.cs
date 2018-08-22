using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the PSMSuperordinateComponent interface.
    /// </summary>
    internal abstract class _PSMSuperordinateComponent : _NamedElement<NUml.Uml2.Class>, 
        _ImplPSMSuperordinateComponent
    {
        #region Constructors

        /// <summary>
        /// Creates a new component. An adapted nUML Class is created.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        protected _PSMSuperordinateComponent(Schema _schema) : this(true, _schema)
        {
        }

        /// <summary>
        /// Creates a new component.
        /// </summary>
        /// <param name="createAdaptee">
        /// If true, an adapted nUML Class is also created, otherwise it is left null.
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        protected _PSMSuperordinateComponent(bool createAdaptee, Schema _schema) : base(_schema, StereotypeTarget.Class)
        {
            if (createAdaptee)
                adaptedElement = NUml.Uml2.Create.Class();

            components = new ObservableCollection<PSMSubordinateComponent>();
            components.CollectionChanged += OnComponentsChanged;
        }

        #endregion

        #region Methods

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

        public abstract string XPath { get; }
        
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

        #region _ImplPSMSuperordinateComponent Members

        public NUml.Uml2.Class AdaptedClass
        {
            get { return adaptedElement; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Ordered collection of the components subordinate to this component.
        /// </summary>
        protected ObservableCollection<PSMSubordinateComponent> components;

        /// <summary>
        /// References the diagram that this component belongs to.
        /// </summary>
        protected PSMDiagram diagram;

        #endregion

		protected virtual string ElementName
		{
			get
			{
				return "Superordinate component";
			}
		}

		public override string ToString()
		{
			if (this is PSMSubordinateComponent)
			{
				PSMSuperordinateComponent parent = ((PSMSubordinateComponent)this).Parent;
				if (parent.Components.Count > 1)
					return String.Format("{0} in {1:F} (Component index: {2})", ElementName, parent, parent.Components.IndexOf((PSMSubordinateComponent)this));
				else
					return String.Format("{0} in {1:F}", ElementName, parent);
			}
			else return base.ToString();
		}

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

    		string normal = ElementName;

			switch (format)
			{
				case "r":
				case "R":
					PSMSubordinateComponent as_subordinate = this as PSMSubordinateComponent;
					if (as_subordinate != null )
					{
						System.Diagnostics.Debug.Assert(as_subordinate.Parent != null);
						if (as_subordinate.Parent != null)
						{
							if (as_subordinate.Parent.Components.Count == 1)
								return string.Format("{0} in {1:f}", normal, as_subordinate.Parent);
							else
								return string.Format("{0} in {1:f} (Component index: {2})", normal, as_subordinate.Parent,
													 as_subordinate.Parent.Components.IndexOf(as_subordinate));
						}
						
					}
					return normal;
				case "f":
				case "F":
					return normal;
				default: return base.ToString();
			}
    	}
    }
}
