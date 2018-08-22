using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using XCase.Model;

namespace Tests.CodeTests
{
    [TestFixture]
	public class CloneCopyTests : CodeTestBase
	{
		private static bool isNotCloneable(Type t)
		{
			return t.GetCustomAttributes(typeof(NotCloneableAttribute), false).Length > 0;
		}

		[Test]
		public void TestCloneOverride()
		{
			Type elementInterfaceType = typeof(Element);
			foreach (Type type in ModelAssembly.GetTypes())
			{
				if (type.IsClass && elementInterfaceType.IsAssignableFrom(type))
				{
					MethodInfo method = type.GetMethod("Clone", new Type[] { typeof(Model), typeof(IDictionary<Element, Element>) });
					if (!type.IsAbstract && !isNotCloneable(type))
					{
						Assert.IsNotNull(method, string.Format("Element of type {0} should override Clone method. ", type.Name));
						Assert.AreEqual(method.DeclaringType, type, string.Format("Element of type {0} should override Clone method. ", type.Name));
					}
					else if (type.Name != "_Element`1")
					{
						Assert.IsFalse(method != null && type.IsAbstract && method.DeclaringType == type, string.Format("Element of type {0} should not override Clone method because it is abstract. ", type.Name));
					}
				}

			}
		}
	}
}