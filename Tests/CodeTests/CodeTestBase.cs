using NUnit.Framework;
using System.Reflection;
using XCase.Model;

namespace Tests.CodeTests
{
	public abstract class CodeTestBase
	{
		public Assembly ModelAssembly { get; set; }

		[TestFixtureSetUp]
		public virtual void InitializeFixture()
		{
			ModelAssembly = Assembly.GetAssembly(typeof(Element));
			
		}

		[TestFixtureTearDown]
		public virtual void FinalizeFixture()
		{
			
		}
	}
}