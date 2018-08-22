namespace XCase.Model.Serialization
{
	/// <summary>
	/// Memo struct for AssociationEnd class
	/// </summary>
	public struct AssociationEndMemo
	{
		/// <summary>
		/// ID
		/// </summary>
		public string id { get; set; }
		/// <summary>
		/// Name
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// Aggregation
		/// </summary>
		public string aggregation { get; set; }
		/// <summary>
		/// Deafult
		/// </summary>
		public string def { get; set; }
		/// <summary>
		/// Default value
		/// </summary>
		public string default_value { get; set; }
		/// <summary>
		/// IsComposite
		/// </summary>
		public string is_composite { get; set; }

		/// <summary>
		/// IsDerived.
		/// </summary>
		/// <value><see cref="System.String"/></value>
		public string is_derived { get; set; }
		
		/// <summary>
		/// IsOrdered
		/// </summary>
		/// <value><see cref="System.String"/></value>
		public string is_ordered { get; set; }
		
		/// <summary>
		/// IsReadonly
		/// </summary>
		/// <value><see cref="System.String"/></value>
		public string is_readonly { get; set; }
		
		/// <summary>
		/// Gets or sets the is_unique.
		/// </summary>
		/// <value><see cref="System.String"/></value>
		public string is_unique { get; set; }
		
		/// <summary>
		/// Lower
		/// </summary>
		/// <value><see cref="System.String"/></value>
		public string lower { get; set; }
		
		/// <summary>
		/// Upper
		/// </summary>
		/// <value><see cref="System.String"/></value>
		public string upper { get; set; }
		
		/// <summary>
		/// Cardinality
		/// </summary>
		/// <value><see cref="System.String"/></value>
		public string cardinality { get; set; }
		
		/// <summary>
		/// Type
		/// </summary>
		/// <value><see cref="System.String"/></value>
		public string type { get; set; }
		
		/// <summary>
		/// Visibility
		/// </summary>
		/// <value><see cref="System.String"/></value>
		public string visibility { get; set; }

		/// <summary>
		/// X coordinate
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Y coordinate
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Height
		/// </summary>
		public double Height { get; set; }

		/// <summary>
		/// Width
		/// </summary>
		public double Width { get; set; }
	}
}