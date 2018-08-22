using System;
using System.Collections.Generic;
using System.Linq;
using XCase.Model.Implementation;

namespace XCase.Model.Serialization
{
    public class SerializatorIdTable : Dictionary<Element, XCaseGuid>
    {
        public Version Version
        {
            get;
            set;
        }
           
        /// <exception cref="ArgumentNullException"><c>table</c> is null.</exception>
        public void AddFromTable(SerializatorIdTable table)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table", "Argument 'table' must not be null.");
            }
            foreach (KeyValuePair<Element, XCaseGuid> pair in table)
            {
                this.Add(pair.Key, pair.Value);
            }
        }

        //private int nextId = -1;

        //private int getNextId()
        //{
        //    if (nextId == -1)
        //        nextId = Math.Max(135, Math.Max(Count, this.Max(pair => pair.Value) + 1));
        //    return nextId++;
        //}

        //internal void SetNextId(int nextId)
        //{
        //    this.nextId = nextId;
        //}

        public XCaseGuid AddWithNewId(Element element)
        {
            XCaseGuid id = ((_ImplElement)element).Guid;
            this.Add(element, id);
            return id;
        }
    }

    public class DeserializatorIdTable : Dictionary<string, Element>
    {
        public SerializatorIdTable CreateSerializatorTable(DeserializatorIdTable deserializatorTable)
        {
            SerializatorIdTable result = new SerializatorIdTable();
            foreach (KeyValuePair<string, Element> pair in deserializatorTable)
            {
                result.Add(pair.Value, XCaseGuid.Parse(pair.Key));
            }
            return result;
        }

        public void AddFromTable(DeserializatorIdTable table)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table", "Argument 'table' must not be null.");
            }
            foreach (KeyValuePair<string, Element> pair in table)
            {
                Add(pair.Key, pair.Value);
            }
        }
    }
}