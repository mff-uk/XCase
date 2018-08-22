using Tests.CodeTests;

namespace Tests
{
	public class Program
	{
		public static void Main()
		{
			RemovePutMeBackTests t = new RemovePutMeBackTests();

			t.InitializeFixture();

			t.FinalizeFixture();
		}
	}
}