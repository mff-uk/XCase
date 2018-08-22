using XCase.Controller;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Interface for controls that can be removed from canvas (almost everything).
	/// </summary>
    public interface IDeletable: IBindable
    {
		/// <summary>
		/// Removes the control from canvas completely.
		/// </summary>
    	void DeleteFromCanvas();
    }

	
}
