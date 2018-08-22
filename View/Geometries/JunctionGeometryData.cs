using System.Windows;
using XCase.WPFDraw.Controls;
using XCase.WPFDraw.Interfaces;

namespace XCase.WPFDraw.Geometries
{
	public class JunctionGeometryData
	{
		public XCaseJunction Junction { get; private set; }

		public JunctionGeometryData(XCaseJunction junction)
		{
			Junction = junction;
		}
	}
}