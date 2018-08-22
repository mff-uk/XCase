using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using XCase.Model;
using XCase.Evolution;

namespace Tests.CodeTests
{
    [TestFixture]
    public class Evolution
    {
        [Test]
        public void TestDetectMethodDefinedInChanges()
        {
            Assembly assembly = Assembly.GetAssembly(typeof (EvolutionChange));
            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(EvolutionChange)))
                {
                    if (!type.IsAbstract)
                    {
                        MethodInfo methodInfo = type.GetMethod("Detect");
                        if (methodInfo == null)
                            Assert.Fail(string.Format("Detect method not defined on type {0}", type.Name));
                    }
                }

            }
        }

        [Test]
        public void TestConsistentChangeAndScope()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(EvolutionChange));
            Type[] types = assembly.GetTypes();

            Dictionary<EEditType, List<EvolutionChange>> byEditType = new Dictionary<EEditType, List<EvolutionChange>>();
            Dictionary<EChangeScope, List<EvolutionChange>> byScope = new Dictionary<EChangeScope, List<EvolutionChange>>();

            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(EvolutionChange)))
                {
                    if (!type.IsAbstract)
                    {
                        object[] customAttributes = type.GetCustomAttributes(typeof(ChangePropertiesAttribute), true);

                        if (customAttributes.Length == 0)
                            Assert.Fail(string.Format("Scope attribute not defined on type {0}", type.Name));

                        ConstructorInfo[] constructorInfos = type.GetConstructors();

                        foreach (ConstructorInfo constructorInfo in constructorInfos)
                        {
                            ParameterInfo[] parameterInfos = constructorInfo.GetParameters();

                            object[] parameters = new object[parameterInfos.Length];

                            for (int index = 0; index < parameterInfos.Length; index++)
                            {
                                ParameterInfo parameterInfo = parameterInfos[index];
                                parameters[index] = null;
                            }

                            EvolutionChange createdObject = (EvolutionChange)constructorInfo.Invoke(parameters);

                            if (!byEditType.ContainsKey(createdObject.EditType))
                                byEditType[createdObject.EditType] = new List<EvolutionChange>();
                            byEditType[createdObject.EditType].Add(createdObject);

                            if (!byScope.ContainsKey(createdObject.Scope))
                                byScope[createdObject.Scope] = new List<EvolutionChange>();
                            byScope[createdObject.Scope].Add(createdObject);

                            Assert.AreEqual(createdObject.Scope, ((ChangePropertiesAttribute)customAttributes[0]).Scope, string.Format("Inconsistent scope in type {0}.", type.Name));
                            Assert.AreEqual(createdObject.EditType, ((ChangePropertiesAttribute)customAttributes[0]).EditType, string.Format("Inconsistent edit type in type {0}.", type.Name));
                        }
                    }
                }

            }

            foreach (KeyValuePair<EChangeScope, List<EvolutionChange>> keyValuePair in byScope)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Scope: {0}", keyValuePair.Key));

                foreach (EvolutionChange evolutionChange in keyValuePair.Value)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format(" {0}", evolutionChange.GetType().Name));                    
                }
                System.Diagnostics.Debug.WriteLine("");
            }

            foreach (KeyValuePair<EEditType, List<EvolutionChange>> keyValuePair in byEditType)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Edit type: {0}", keyValuePair.Key));

                foreach (EvolutionChange evolutionChange in keyValuePair.Value)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format(" {0}", evolutionChange.GetType().Name));
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }
    }
}