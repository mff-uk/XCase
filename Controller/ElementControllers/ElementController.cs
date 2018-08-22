using XCase.Model;
using XCase.Controller.Commands;
using System.Collections.Generic;

namespace XCase.Controller
{
    /// <summary>
    /// This is the base class for all Element controllers
    /// </summary>
    public abstract class ElementController
    {
        public DiagramController DiagramController { get; private set; }

		public Element Element { get; private set; }

        protected ElementController(Element element, DiagramController diagramController)
        {
            DiagramController = diagramController;
        	Element = element;
        }

        public virtual void Remove()
        {
            DeleteFromDiagramMacroCommand c =
                (DeleteFromDiagramMacroCommand)DeleteFromDiagramMacroCommandFactory.Factory().Create(DiagramController);
            List<Element> list = new List<Element>();
            list.Add(Element);
            if (c.InitializeCommand(list))
				c.Execute();
        }

    }
}