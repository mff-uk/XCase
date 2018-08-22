using System.Windows;
using XCase.UMLModel;
using XCase.WPFDraw.Controls;

namespace XCase.WPFDraw.Interfaces
{
    public interface ISnapPointHookProvider
    {
		SnapPointHook CreateHook();
		SnapPointHook CreateHook(Point preferedPosition);
        Rect GetBounds();
    }
}
