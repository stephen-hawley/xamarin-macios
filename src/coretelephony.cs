using Foundation;
using ObjCRuntime;
using System;

namespace CoreTelephony {
	[Obsoleted (PlatformName.iOS, 14,0, message: Constants.UseCallKitInstead)]
	[Deprecated (PlatformName.iOS, 10,0, message: Constants.UseCallKitInstead)]
	[BaseType (typeof (NSObject))]
	interface CTCall {
		[Export ("callID")]
		string CallID { get;  }

		[Export ("callState")]
		string CallState { get; }

	}

	[Obsoleted (PlatformName.iOS, 14,0, message: Constants.UseCallKitInstead)]
	[iOS (9,0)]
	[BaseType (typeof (NSObject))]
	interface CTCellularData {
		[NullAllowed, Export ("cellularDataRestrictionDidUpdateNotifier", ArgumentSemantic.Copy)]
		Action<CTCellularDataRestrictedState> RestrictionDidUpdateNotifier { get; set; }
	
		[Export ("restrictedState")]
		CTCellularDataRestrictedState RestrictedState { get; }
	}
	

	[Obsoleted (PlatformName.iOS, 14,0, message: Constants.UseCallKitInstead)]
	[Static]
	[iOS (7,0)]
	interface CTRadioAccessTechnology {
		[Field ("CTRadioAccessTechnologyGPRS")]
		NSString GPRS { get; }

		[Field ("CTRadioAccessTechnologyEdge")]
		NSString Edge { get; }

		[Field ("CTRadioAccessTechnologyWCDMA")]
		NSString WCDMA { get; }

		[Field ("CTRadioAccessTechnologyHSDPA")]
		NSString HSDPA { get; }

		[Field ("CTRadioAccessTechnologyHSUPA")]
		NSString HSUPA { get; }

		[Field ("CTRadioAccessTechnologyCDMA1x")]
		NSString CDMA1x { get; }

		[Field ("CTRadioAccessTechnologyCDMAEVDORev0")]
		NSString CDMAEVDORev0 { get; }

		[Field ("CTRadioAccessTechnologyCDMAEVDORevA")]
		NSString CDMAEVDORevA { get; }

		[Field ("CTRadioAccessTechnologyCDMAEVDORevB")]
		NSString CDMAEVDORevB { get; }

		[Field ("CTRadioAccessTechnologyeHRPD")]
		NSString EHRPD { get; }

		[Field ("CTRadioAccessTechnologyLTE")]
		NSString LTE { get; }

		[iOS (14,1)]
		[Field ("CTRadioAccessTechnologyNRNSA")]
		NSString NRNsa { get; }

		[iOS (14,1)]
		[Field ("CTRadioAccessTechnologyNR")]
		NSString NR { get; }
	}

	interface ICTTelephonyNetworkInfoDelegate {}

	[Obsoleted (PlatformName.iOS, 14,0, message: Constants.UseCallKitInstead)]
	[iOS (13,0)]
#if NET
	[Protocol, Model]
#else
	[Protocol, Model (AutoGeneratedName = true)]
#endif
	[BaseType (typeof (NSObject))]
	interface CTTelephonyNetworkInfoDelegate {

		[Export ("dataServiceIdentifierDidChange:")]
		void DataServiceIdentifierDidChange (string identifier);
	}

	[Obsoleted (PlatformName.iOS, 14,0, message: Constants.UseCallKitInstead)]
	[BaseType (typeof (NSObject))]
	interface CTTelephonyNetworkInfo {
		[Deprecated (PlatformName.iOS, 12,0, message: "Use 'ServiceSubscriberCellularProviders' instead.")]
		[Export ("subscriberCellularProvider", ArgumentSemantic.Retain)]
		[NullAllowed]
		CTCarrier SubscriberCellularProvider { get; }

		[Deprecated (PlatformName.iOS, 12,0, message: "Use 'ServiceSubscriberCellularProvidersDidUpdateNotifier' instead.")]
		[NullAllowed] // by default this property is null
		[Export ("subscriberCellularProviderDidUpdateNotifier")]
		Action<CTCarrier> CellularProviderUpdatedEventHandler { get; set; }

		[Deprecated (PlatformName.iOS, 12,0, message: "Use 'ServiceCurrentRadioAccessTechnology' instead.")]
		[iOS (7,0), Export ("currentRadioAccessTechnology")]
		[NullAllowed]
		NSString CurrentRadioAccessTechnology { get; }

		[iOS (12,0)]
		[NullAllowed]
		[Export ("serviceSubscriberCellularProviders", ArgumentSemantic.Retain)]
		NSDictionary<NSString, CTCarrier> ServiceSubscriberCellularProviders { get; }

		[iOS (12,0)]
		[NullAllowed]
		[Export ("serviceCurrentRadioAccessTechnology", ArgumentSemantic.Retain)]
		NSDictionary<NSString, NSString> ServiceCurrentRadioAccessTechnology { get; }

		[iOS (12,0)]
		[NullAllowed]
		[Export ("serviceSubscriberCellularProvidersDidUpdateNotifier", ArgumentSemantic.Copy)]
		Action<NSString> ServiceSubscriberCellularProvidersDidUpdateNotifier { get; set; }

		[Obsoleted (PlatformName.iOS, 14,0, message: "Use the 'CallKit' API instead.")]
		[iOS (12,0)]
		[Notification]
		[Field ("CTServiceRadioAccessTechnologyDidChangeNotification")]
		NSString ServiceRadioAccessTechnologyDidChangeNotification { get; }

		[iOS (13,0)]
		[NullAllowed, Export ("dataServiceIdentifier")]
		string DataServiceIdentifier { get; }

		[iOS (13,0)]
		[Wrap ("WeakDelegate")]
		[NullAllowed]
		ICTTelephonyNetworkInfoDelegate Delegate { get; set; }

		[iOS (13,0)]
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }
	}

	[Obsoleted (PlatformName.iOS, 14,0, message: "Replaced by 'CXCallObserver' from 'CallKit'.")]
	[Deprecated (PlatformName.iOS, 10, 0, message: "Replaced by 'CXCallObserver' from 'CallKit'.")]
	[BaseType (typeof (NSObject))]
	interface CTCallCenter {
		[NullAllowed] // by default this property is null
		[Export ("callEventHandler")]
		Action<CTCall> CallEventHandler { get; set; }

		[Export ("currentCalls")]
		[NullAllowed]
		NSSet CurrentCalls { get; }

	}

	[Obsoleted (PlatformName.iOS, 14,0, message: Constants.UseCallKitInstead)]
	[BaseType (typeof (NSObject))]
	interface CTCarrier {
		[NullAllowed]
		[Export ("mobileCountryCode")]
		string MobileCountryCode { get;  }

		[NullAllowed]
		[Export ("mobileNetworkCode")]
		string MobileNetworkCode { get;  }

		[NullAllowed]
		[Export ("isoCountryCode")]
		string IsoCountryCode { get;  }

		[Export ("allowsVOIP")]
		bool AllowsVoip { get;  }

		[NullAllowed]
		[Export ("carrierName")]
		string CarrierName { get; }
	}

	interface ICTSubscriberDelegate {}

	[Obsoleted (PlatformName.iOS, 14,0, message: Constants.UseCallKitInstead)]
	[Protocol]
	[iOS (12,1)]
	interface CTSubscriberDelegate {
		[Abstract]
		[Export ("subscriberTokenRefreshed:")]
		void SubscriberTokenRefreshed (CTSubscriber subscriber);
	}

	[Obsoleted (PlatformName.iOS, 14,0, message: Constants.UseCallKitInstead)]
	[BaseType (typeof (NSObject))]
	[iOS (7,0)]
	partial interface CTSubscriber {
		[Export ("carrierToken")]
		[NullAllowed]
		[Deprecated (PlatformName.iOS, 11, 0)]
		NSData CarrierToken { get; }

		[iOS (12,1)]
		[Export ("identifier")]
		string Identifier { get; }

		[iOS (12,1)]
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		[iOS (12,1)]
		[Wrap ("WeakDelegate")]
		[NullAllowed]
		ICTSubscriberDelegate Delegate { get; set; }
	}

	[Obsoleted (PlatformName.iOS, 14,0, message: Constants.UseCallKitInstead)]
	[BaseType (typeof (NSObject))]
	partial interface CTSubscriberInfo {
		[Deprecated (PlatformName.iOS, 12, 1, message : "Use 'Subscribers' instead.")]
		[Static]
		[Export ("subscriber")]
		CTSubscriber Subscriber { get; }

		[iOS (12,1)]
		[Static]
		[Export ("subscribers")]
		CTSubscriber[] Subscribers { get; }
	}

	[Obsoleted (PlatformName.iOS, 14,0, message: Constants.UseCallKitInstead)]
	[iOS (12,0)]
	[BaseType (typeof (NSObject))]
	interface CTCellularPlanProvisioningRequest : NSSecureCoding {
		[Export ("address")]
		string Address { get; set; }

		[NullAllowed, Export ("matchingID")]
		string MatchingId { get; set; }

		[NullAllowed, Export ("OID")]
		string Oid { get; set; }

		[NullAllowed, Export ("confirmationCode")]
		string ConfirmationCode { get; set; }

		[NullAllowed, Export ("ICCID")]
		string Iccid { get; set; }

		[NullAllowed, Export ("EID")]
		string Eid { get; set; }
	}

	[Obsoleted (PlatformName.iOS, 14,0, message: Constants.UseCallKitInstead)]
	[iOS (12,0)]
	[BaseType (typeof (NSObject))]
	interface CTCellularPlanProvisioning {
		[Export ("supportsCellularPlan")]
		bool SupportsCellularPlan { get; }

		[Async]
		[Export ("addPlanWith:completionHandler:")]
		void AddPlan (CTCellularPlanProvisioningRequest request, Action<CTCellularPlanProvisioningAddPlanResult> completionHandler);
	}
}
