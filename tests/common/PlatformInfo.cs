//
// PlatformInfo.cs: info about the host platform
// and AvailabilityBaseAttribute extensions for tests
//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright 2015 Xamarin Inc. All rights reserved.

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Versioning;

using ObjCRuntime;
using Foundation;
#if !(MONOMAC || __MACOS__)
using UIKit;
#endif

using Xamarin.Utils;

namespace Xamarin.Tests 
{
	public sealed class PlatformInfo
	{
		static PlatformInfo GetHostPlatformInfo ()
		{
			string name;
			string version;
#if __MACCATALYST__
			name = "MacCatalyst";
			version = UIDevice.CurrentDevice.SystemVersion;
#elif __TVOS__ || __IOS__
			name = UIDevice.CurrentDevice.SystemName;
			version = UIDevice.CurrentDevice.SystemVersion;
#elif __WATCHOS__
			name = WatchKit.WKInterfaceDevice.CurrentDevice.SystemName;
			version = WatchKit.WKInterfaceDevice.CurrentDevice.SystemVersion;
#elif MONOMAC || __MACOS__
			using (var plist = NSDictionary.FromFile ("/System/Library/CoreServices/SystemVersion.plist")) {
				name = (NSString)plist ["ProductName"];
				version = (NSString)plist ["ProductVersion"];
			}
#else
#error Unknown platform
#endif
			name = name?.Replace (" ", String.Empty)?.ToLowerInvariant ();
			if (name == null)
				throw new FormatException ("Product name is `null`");

			var platformInfo = new PlatformInfo ();

#if NET
			if (name.StartsWith ("maccatalyst", StringComparison.Ordinal))
				platformInfo.Name = ApplePlatform.MacCatalyst;
			else if (name.StartsWith ("mac", StringComparison.Ordinal))
				platformInfo.Name = ApplePlatform.MacOSX;
			else if (name.StartsWith ("ios", StringComparison.Ordinal) || name.StartsWith ("iphoneos", StringComparison.Ordinal) || name.StartsWith ("ipados", StringComparison.Ordinal))
				platformInfo.Name = ApplePlatform.iOS;
			else if (name.StartsWith ("tvos", StringComparison.Ordinal))
				platformInfo.Name = ApplePlatform.TVOS;
			else if (name.StartsWith ("watchos", StringComparison.Ordinal))
				platformInfo.Name = ApplePlatform.WatchOS;
			else
				throw new FormatException ($"Unknown product name: {name}");
#else
			if (name.StartsWith ("maccatalyst", StringComparison.Ordinal))
				platformInfo.Name = PlatformName.MacCatalyst;
			else if (name.StartsWith ("mac", StringComparison.Ordinal))
				platformInfo.Name = PlatformName.MacOSX;
			else if (name.StartsWith ("ios", StringComparison.Ordinal) || name.StartsWith ("iphoneos", StringComparison.Ordinal))
				platformInfo.Name = PlatformName.iOS;
			else if (name.StartsWith ("tvos", StringComparison.Ordinal))
				platformInfo.Name = PlatformName.TvOS;
			else if (name.StartsWith ("watchos", StringComparison.Ordinal))
				platformInfo.Name = PlatformName.WatchOS;
			else
				throw new FormatException ($"Unknown product name: {name}");
#endif

			platformInfo.Version = Version.Parse (version);

#if !NET
			if (IntPtr.Size == 4)
				platformInfo.Architecture = PlatformArchitecture.Arch32;
			else if (IntPtr.Size == 8)
				platformInfo.Architecture = PlatformArchitecture.Arch64;
#endif

			return platformInfo;
		}

		public static readonly PlatformInfo Host = GetHostPlatformInfo ();

#if NET
		public ApplePlatform Name { get; private set; }
#else
		public PlatformName Name { get; private set; }
		public PlatformArchitecture Architecture { get; private set; }
#endif
		public Version Version { get; private set; }

#if NET
		public bool IsMac => Name == ApplePlatform.MacOSX;
		public bool IsIos => Name == ApplePlatform.iOS;
#else
		public bool IsMac => Name == PlatformName.MacOSX;
		public bool IsIos => Name == PlatformName.iOS;
		public bool IsArch32 => Architecture.HasFlag (PlatformArchitecture.Arch32);
		public bool IsArch64 => Architecture.HasFlag (PlatformArchitecture.Arch64);
#endif

		PlatformInfo ()
		{
		}
	}

	public static class AvailabilityExtensions
	{
		public static bool IsAvailableOnHostPlatform (this ICustomAttributeProvider attributeProvider)
		{
			return attributeProvider.IsAvailable (PlatformInfo.Host);
		}

		public static bool IsAvailable (this ICustomAttributeProvider attributeProvider, PlatformInfo targetPlatform)
		{
			var customAttributes = attributeProvider.GetCustomAttributes (true);

#if NET
			customAttributes = customAttributes.ToArray (); // don't iterate twice
			if (customAttributes.Any (v => v is ObsoleteAttribute))
				return false;
#endif

			return customAttributes
#if NET
				.OfType<OSPlatformAttribute> ()
#else
				.OfType<AvailabilityBaseAttribute> ()
#endif
				.IsAvailable (targetPlatform);
		}

#if NET
		public static bool IsAvailableOnHostPlatform (this IEnumerable<OSPlatformAttribute> attributes)
#else
		public static bool IsAvailableOnHostPlatform (this IEnumerable<AvailabilityBaseAttribute> attributes)
#endif
		{
			return attributes.IsAvailable (PlatformInfo.Host);
		}

#if NET
		public static bool IsAvailable (this IEnumerable<OSPlatformAttribute> attributes, PlatformInfo targetPlatform)
		{
			// Sort the attributes so that maccatalyst attributes are processed before ios attributes
			// This way we can easily fall back to ios for maccatalyst
			attributes = attributes.OrderBy (v => v.PlatformName.ToLowerInvariant ()).Reverse ();

			foreach (var attr in attributes) {
				if (!attr.TryParse (out ApplePlatform? platform, out var version))
					continue;

				if (targetPlatform.Name == ApplePlatform.MacCatalyst) {
					if (platform != ApplePlatform.iOS && platform != ApplePlatform.MacCatalyst)
						continue;
				} else if (platform != targetPlatform.Name) {
					continue;
				}

				if (attr is UnsupportedOSPlatformAttribute)
					return version is not null && targetPlatform.Version < version;

				if (attr is SupportedOSPlatformAttribute)
					return version is null || targetPlatform.Version >= version;
			}

			// Our current attribute logic assumes that no attribute means that an API is available on all versions of that platform.
			// This is not correct: the correct logic is that an API is available on a platform if there are no availability attributes
			// for any other platforms. However, enforcing this here would make a few of our tests fail, so we must keep the incorrect
			// logic until we've got all the right attributes implemented.
			return true;

			// Correct logic:
			// Only available if there aren't any attributes for other platforms
			// return attributes.Count () == 0;
		}
#else
		public static bool IsAvailable (this IEnumerable<AvailabilityBaseAttribute> attributes, PlatformInfo targetPlatform)
		{
			// always "available" from a binding perspective if
			// there are no explicit annotations saying otherwise
			var available = true;

			foreach (var attr in attributes) {
				if (attr.Platform != targetPlatform.Name)
					continue;

				switch (attr.AvailabilityKind) {
				case AvailabilityKind.Introduced:
					if (attr.Version != null)
						available &= targetPlatform.Version >= attr.Version;

					if (attr.Architecture != PlatformArchitecture.None &&
						attr.Architecture != PlatformArchitecture.All)
						available &= attr.Architecture.HasFlag (targetPlatform.Architecture);
					break;
				case AvailabilityKind.Deprecated:
				case AvailabilityKind.Obsoleted:
					if (attr.Version != null)
						available &= targetPlatform.Version < attr.Version;
					// FIXME: handle architecture-level _un_availability?
					// we didn't do this with the old AvailabilityAttribute...
					break;
				case AvailabilityKind.Unavailable:
					available = false;
					break;
				}

				if (!available)
					return false;
			}

			return available;
		}
#endif
	}
}
