using System;
using NUnit.Framework;

namespace Tests
{
	public static class TestUtils
	{
		public static void AssertException<CustomException> (Action action)
			where CustomException : System.Exception
		{
			try
			{
				action();
			}
			catch (CustomException)
			{
				return;
			}
			catch (Exception e)
			{
				Assert.Fail(string.Format("Expected exception of type: {0}. Exception of type {1} occured instead.", 
					typeof(CustomException).Name, e.GetType().Name));
				return; 
			}
			Assert.Fail(string.Format("Expected exception of type: {0}.",
					typeof(CustomException).Name));
		}
	}
}