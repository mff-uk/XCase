using XCase.Model.Implementation;

namespace XCase.Model
{
    /// <summary>
    /// Interface for an abstract factory creating PSMSubordinate components.
    /// PSMSubordinateComponent is a common interface for PSM components
    /// that can form a content of other PSM components called superodrinate.
    /// <see cref="PSMSuperordinateComponent"/>
    /// </summary>
    public interface PSMSubordinateComponentFactory
    {
        /// <summary>
        /// Creates a concrete subordinate PSM component.
        /// </summary>
        /// <param name="_parent">
        /// References the PSM component that will be the parent of the new component
        /// in the XML hierarchy.
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        /// <returns></returns>
        PSMSubordinateComponent Create(PSMSuperordinateComponent _parent, Schema _schema);
    }

    /// <summary>
    /// A factory creating PSM attribute containers.
    /// </summary>
    public class PSMAttributeContainerFactory : PSMSubordinateComponentFactory
    {
    	/// <summary>
    	/// Initializes a new instance of the PSMAttributeContainerFactory class.
    	/// </summary>
    	protected PSMAttributeContainerFactory()
    	{
    		//protected constructor forbids creating instances
    	}

    	private static PSMAttributeContainerFactory _instance;

    	/// <summary>
    	/// Static singleton reference to PSMAttributeContainerFactory object
    	/// </summary>
    	public static PSMAttributeContainerFactory Instance
    	{
    		get
    		{
    			if (_instance == null)
    				_instance = new PSMAttributeContainerFactory();
    			return _instance;
    		}
    	}

        #region PSMSubordinateComponentFactory Members

        public PSMSubordinateComponent Create(PSMSuperordinateComponent _parent, Schema _schema)
        {
            return new _PSMAttributeContainer(_parent, _schema);
        }

        #endregion
    }

    /// <summary>
    /// A factory creating PSM content containers.
    /// </summary>
    public class PSMContentContainerFactory : PSMSubordinateComponentFactory
    {
    	/// <summary>
    	/// Initializes a new instance of the PSMContentContainerFactory class.
    	/// </summary>
    	protected PSMContentContainerFactory()
    	{
    		//protected constructor forbids creating instances
    	}

    	private static PSMContentContainerFactory _instance;

    	/// <summary>
    	/// Static singleton reference to PSMContentContainerFactory object
    	/// </summary>
    	public static PSMContentContainerFactory Instance
    	{
    		get
    		{
    			if (_instance == null)
    				_instance = new PSMContentContainerFactory();
    			return _instance;
    		}
    	}

        #region PSMSubordinateComponentFactory Members

        public PSMSubordinateComponent Create(PSMSuperordinateComponent _parent, Schema _schema)
        {
            return new _PSMContentContainer(_parent, _schema);
        }

        #endregion
    }

    /// <summary>
    /// A factory creating PSM content choices.
    /// </summary>
    public class PSMContentChoiceFactory : PSMSubordinateComponentFactory
    {
    	/// <summary>
    	/// Initializes a new instance of the PSMContentChoiceFactory class.
    	/// </summary>
    	protected PSMContentChoiceFactory()
    	{
    		//protected constructor forbids creating instances
    	}

    	private static PSMContentChoiceFactory _instance;

    	/// <summary>
    	/// Static singleton reference to PSMContentChoiceFactory object
    	/// </summary>
    	public static PSMContentChoiceFactory Instance
    	{
    		get
    		{
    			if (_instance == null)
    				_instance = new PSMContentChoiceFactory();
    			return _instance;
    		}
    	}

        #region PSMSubordinateComponentFactory Members

        public PSMSubordinateComponent Create(PSMSuperordinateComponent _parent, Schema _schema)
        {
            return new _PSMContentChoice(_parent, _schema);
        }

        #endregion
    }

    /// <summary>
    /// A factory creating PSM associations.
    /// </summary>
    public class PSMAssociationFactory : PSMSubordinateComponentFactory
    {
    	/// <summary>
		/// Initializes a new instance of the PSMAssociationFactory class.
    	/// </summary>
    	protected PSMAssociationFactory()
    	{
    	}
		
		private static PSMAssociationFactory _instance;

		/// <summary>
		/// Static singleton reference to PSMAssociationFactory object
		/// </summary>
		public static PSMAssociationFactory Instance
		{
			get
			{
				if (_instance == null)
					_instance = new PSMAssociationFactory();
				return _instance;
			}
		}

    	#region PSMSubordinateComponentFactory Members

        public PSMSubordinateComponent Create(PSMSuperordinateComponent _parent, Schema _schema)
        {
            return new _PSMAssociation(_parent, _schema);
        }

        #endregion
    }
}