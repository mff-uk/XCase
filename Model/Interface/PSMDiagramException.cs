using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace XCase.Model
{
    public class PSMDiagramException: Exception
    {
        public PSMDiagram Diagram { get; private set; }

        public PSMDiagramException(string message, PSMDiagram diagram) : base(message)
        {
            Diagram = diagram;
        }

        public PSMDiagramException(string message, Exception innerException, PSMDiagram diagram) : base(message, innerException)
        {
            Diagram = diagram;
        }

        protected PSMDiagramException(SerializationInfo info, StreamingContext context, PSMDiagram diagram) : base(info, context)
        {
            Diagram = diagram;
        }

        public PSMDiagramException(PSMDiagram diagram)
        {
            Diagram = diagram;
        }
    }
}
