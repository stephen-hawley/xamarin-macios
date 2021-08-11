using System;
using System.Drawing;
using Foundation;
using CoreAnimation;
using ObjCRuntime;

namespace BindAs1050ProtocolErrorTests {

	[Protocol]
	interface MyFooClass {

		[return: BindAs (typeof (CAScroll? []))]
		[Export ("getScrollArrayEnum2:")]
		NSNumber [] GetScrollArrayFooEnumArray2 ([BindAs (typeof (CAScroll []))] NSNumber [] arg1);
		
	}
}
