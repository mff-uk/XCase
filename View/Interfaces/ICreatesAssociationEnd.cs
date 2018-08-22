using XCase.Model;
using XCase.View.Controls;
using System.Windows;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Interface for those controls that can create association ends (these will be representants 
	/// of association and association classes).
	/// </summary>
    public interface ICreatesAssociationEnd
    {
		/// <summary>
		/// Creates visualization for an association end.
		/// </summary>
		/// <param name="preferedPosition">The prefered position of the created point.</param>
		/// <param name="viewHelper">association end's view helper.</param>
		/// <param name="associationEnd">model association end.</param>
		/// <returns>new control representing association end</returns>
        PIM_AssociationEnd CreateAssociationEnd(Point preferedPosition, AssociationEndViewHelper viewHelper, AssociationEnd associationEnd);
    }
}
