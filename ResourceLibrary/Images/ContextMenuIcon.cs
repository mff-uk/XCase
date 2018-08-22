using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace XCase.View
{
	/// <summary>
	/// Provides access to resource images. 
	/// </summary>
    public static class ContextMenuIcon
    {
		/// <summary>
		/// Returns resource image as an icon
		/// </summary>
		/// <param name="Res">key in resources</param>
		/// <returns>small icon (16x16)</returns>
		public static Image GetContextIcon(string Res)
        {
            return new Image() { Source = Application.Current.Resources[Res] as ImageSource, Height = 18, Width = 18 };
        }
    }
}
