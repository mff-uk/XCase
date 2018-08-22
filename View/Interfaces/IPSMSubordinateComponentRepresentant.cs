using XCase.Model;
using XCase.View.Controls;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Extension of <see cref="IModelElementRepresentant"/> interface
	/// for representants of <see cref="PSMSubordinateComponent"/>s. 
	/// </summary>
	public interface IPSMSubordinateComponentRepresentant : IModelElementRepresentant
	{
		/// <summary>
		/// Returns underlying model element as <see cref="PSMSubordinateComponent"/>
		/// </summary>
		PSMSubordinateComponent ModelSubordinateComponent { get; }
	}
}