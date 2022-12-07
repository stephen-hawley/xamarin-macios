using System;
using System.IO;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
_REPLACE_USING_REPLACE_

namespace Xamarin.iOS.UnitTests {
	// autogenerated class that uses types from the test assemblies so that
	// they are aot and not linked away.
	public static class RegisterType {
		public static bool IsXUnit = _REPLACE_IS_XUNIT_REPLACE_;
		public static Dictionary<string, Type> TypesToRegister = new Dictionary<string, Type> {
_REPLACE_KEY_VALUES_REPLACE_
		};

		public static void RegisterTypes ()
		{
			// line used to ensure that the runner is not remove by the linker :/
#if MONOMAC
			Console.WriteLine ($"Got the runner for the linker {typeof(Xunit.Sdk.AllException)}");
#else
			Console.WriteLine ($"Got the runner for the linker {typeof (Xunit.Sdk.TypeUtility)}");
#endif
			foreach (var a in TypesToRegister.Keys) {
				// do something with the type, so that it is not removed
				var assemblyPath = Path.GetFileName (TypesToRegister [a].Assembly.Location);
				Console.WriteLine ($"We are using type {TypesToRegister [a]} from assembly {assemblyPath}");
			}
		}
	}
}
