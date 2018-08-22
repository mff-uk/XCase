using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Interface for resizable control
	/// </summary>
	/// <seealso cref="ResizeElementCommand"/>
    public interface IResizable
    {
		/// <summary>
		/// ViewHelper of the control, stores visualization information.
		/// </summary>
		/// <value></value>
		PositionableElementViewHelper ViewHelper { get; }
    }

	/// <summary>
	/// Defines extension methods for <see cref="IResizable"/> interface
	/// </summary>
	public static class ResizableExtensions
	{
		/// <summary>
		/// Sets the size of <paramref name="resizable"/> to autosize.
		/// </summary>
		/// <param name="resizable">The resizable.</param>
		public static void ResetSize(this IResizable resizable)
		{
			resizable.ViewHelper.Width = double.NaN;
			resizable.ViewHelper.Height = double.NaN;
		}
	}
}
