namespace XCase.View.Interfaces
{
	/// <summary>
	/// Use <see cref="IRepresentsIndirect"/> for controls that are themselves selectable, 
	/// but in fact are part of another control (like AssociationDiamond is part of PIM_Association)
	/// </summary>
	public interface IRepresentsIndirect
	{
		/// <summary>
		/// The element that is inderectly reprsented by this control
		/// </summary>
		IModelElementRepresentant RepresentedElement { get; }
	}
}