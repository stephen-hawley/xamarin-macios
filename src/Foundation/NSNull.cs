using System;

// Disable until we get around to enable + fix any issues.
#nullable disable

namespace Foundation {
	public partial class NSNull {

		static NSNull _null;

		// helper to avoid all, but one, native calls when called repeatedly
		static public NSNull Null {
			get {
				if (_null is null)
					_null = _Null;
				return _null;
			}
		}
	}
}
