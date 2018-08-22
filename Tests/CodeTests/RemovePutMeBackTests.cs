using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using XCase.Model;

namespace Tests.CodeTests
{
    [TestFixture]
	public class RemovePutMeBackTests : CodeTestBase
	{
		private static bool isNoDeleteUndoRedoSupportClass(Type t)
		{
			return t.GetCustomAttributes(typeof (NoDeleteUndoRedoSupportAttribute), false).Length > 0;
		}

        [Test]
        public void TestBaseCall()
        {
            DirectoryInfo modelImplDiectory = new DirectoryInfo(new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "\\Model\\Implementation");

            FileInfo[] files = modelImplDiectory.GetFiles(@"*.cs");

            foreach (FileInfo file in files)
            {
                string fileText = System.IO.File.ReadAllText(file.FullName, System.Text.Encoding.ASCII);

                int pos = 0;
                while ((pos = fileText.IndexOf(" class _", pos)) > 0)
                {
                    int methodDef = fileText.IndexOf("void RemoveMeFromModel()");

                    int p = fileText.IndexOf("{", methodDef) + 1;

                    while (fileText[p] == ' ' || fileText[p] == '\n' || fileText[p] == '\r' || fileText[p] == '\t')
                    {
                        p++;
                    }

                    if (!fileText.Substring(p, 20).StartsWith("base.RemoveMeFromModel()"))
                    {
                        Assert.Fail("RemoveMeFromModel in type {0} does not call base.RemoveMeFromModel(). ", fileText.Substring(pos + " class _".Length, fileText.IndexOfAny(new char[] { '\n', ':' }, pos) - pos - " class _".Length));
                    }
                    pos++;
                }

                pos = 0;
                while ((pos = fileText.IndexOf(" class _", pos)) > 0)
                {
                    int methodDef = fileText.IndexOf("void PutMeBackToModel()");

                    int p = fileText.IndexOf("{", methodDef) + 1;

                    while (fileText[p] == ' ' || fileText[p] == '\n' || fileText[p] == '\r' || fileText[p] == '\t')
                    {
                        p++;
                    }

                    if (!fileText.Substring(p, 20).StartsWith("base.PutMeBackToModel()"))
                    {
                        Assert.Fail("PutMeBackToModel in type {0} does not call base.PutMeBackToModel(). ", fileText.Substring(pos + " class _".Length, fileText.IndexOfAny(new char[] { '\n', ':' }, pos) - pos - " class _".Length));
                    }
                    pos++;
                }
            } 
        }

		private void methodLookupTest(string methodName)
		{
			Type elementInterfaceType = typeof(Element);

			foreach (Type type in ModelAssembly.GetTypes())
			{
				if (type.IsClass && !type.IsAbstract && elementInterfaceType.IsAssignableFrom(type)
					&& !isNoDeleteUndoRedoSupportClass(type))
				{
					MethodInfo method = type.GetMethod(methodName, new Type[] { }, null);
					Assert.IsNotNull(method, string.Format("Element of type {0} should override {1} method. ", type.Name, methodName));
					Assert.AreEqual(method.DeclaringType, type, string.Format("Element of type {0} should override {1} method. ", type.Name, methodName));
				}
			}
		}

		[Test]
		public void TestRemoveMeFromModelPresent()
		{
			methodLookupTest("RemoveMeFromModel");
		}

		[Test]
		public void TestPutMeBackToModelPresent()
		{
			methodLookupTest("PutMeBackToModel");
		}

		/// <summary>
		/// Both or none of the methods should be defined or none of them 
		/// </summary>
		[Test]
		public void TestRemovePutMeBackPairs()
		{
			Type elementInterfaceType = typeof(Element);

			foreach (Type type in ModelAssembly.GetTypes())
			{
				if (type.IsClass && !type.IsAbstract && elementInterfaceType.IsAssignableFrom(type))
				{
					bool no = isNoDeleteUndoRedoSupportClass(type);
					MethodInfo method1 = type.GetMethod("PutMeBackToModel", new Type[] { }, null);
					MethodInfo method2 = type.GetMethod("RemoveMeFromModel", new Type[] { }, null);					
					Assert.AreEqual(method1.DeclaringType, method2.DeclaringType, string.Format("Methods RemoveMeFromModel and PutMeBackToModel should be defined both in type {0}. ", type.Name));
					if (method1.DeclaringType == type)
						Assert.IsFalse(no && (method1 != null || method2 != null), string.Format("PutMeBackToModel or RemoveMeFromModel are defined on type {0} which is marked with NoDeleteUndoRedoSupport attribute.", type.Name));
					Assert.IsTrue((method1 == null && method2 == null) || (method1 != null && method2 != null), string.Format("Methods RemoveMeFromModel and PutMeBackToModel should be defined both in type {0}. ", type.Name));
				}
			}
		}
	}
}   
