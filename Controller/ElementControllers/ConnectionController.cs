using System.Collections.Generic;
using System.Windows;
using XCase.Model;

namespace XCase.Controller
{
    /// <summary>
    /// This is a Controller for a Connection
    /// </summary>
    public class ConnectionController : ElementController
	{
		public ConnectionController(Element element, DiagramController diagramController)
			: base(element, diagramController)
		{
		}

		public void AddBreakPoint(Point point, int orderInJunction, ObservablePointCollection viewHelperPointCollection)
		{
			ViewController.BreakLine(point, orderInJunction, viewHelperPointCollection, DiagramController);
		}

		public void RemoveBreakPoint(int orderInJunction, ObservablePointCollection viewHelperPointCollection)
		{
			ViewController.StraightenLine(orderInJunction, viewHelperPointCollection, DiagramController);
		}
	}
}