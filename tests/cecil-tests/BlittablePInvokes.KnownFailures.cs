using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;

using ObjCRuntime;

using Xamarin.Tests;
using Xamarin.Utils;

#nullable enable

namespace Cecil.Tests {
	public partial class BlittablePInvokes {
		static HashSet<string> knownFailuresPInvokes = new HashSet<string> {
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioComponentInstanceNew(System.IntPtr,System.IntPtr&)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitGetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,AudioToolbox.AudioStreamBasicDescription&,System.UInt32&)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitGetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,AudioUnit.AudioUnitParameterInfoNative&,System.UInt32&)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitGetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,System.Double&,System.UInt32&)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitGetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,System.IntPtr&,System.Int32&)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitGetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,System.UInt32*,System.UInt32&)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitGetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,System.UInt32&,System.Int32&)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitGetPropertyInfo(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,System.UInt32&,System.Boolean&)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitRender(System.IntPtr,AudioUnit.AudioUnitRenderActionFlags&,AudioToolbox.AudioTimeStamp&,System.UInt32,System.UInt32,System.IntPtr)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitSetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,AudioToolbox.AudioTimeStamp&,System.Int32)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitSetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,AudioUnit.AudioUnitConnection&,System.Int32)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitSetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,AudioUnit.AURenderCallbackStruct&,System.Int32)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitSetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,AudioUnit.AUSamplerInstrumentData&,System.Int32)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitSetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,AudioUnit.AUScheduledAudioFileRegion/ScheduledAudioFileRegion&,System.Int32)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitSetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,ObjCRuntime.NativeHandle&,System.Int32)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitSetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,System.Double&,System.UInt32)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitSetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,System.IntPtr&,System.Int32)",
			"AudioUnit.AudioUnitStatus AudioUnit.AudioUnit::AudioUnitSetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,System.UInt32&,System.UInt32)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphAddNode(System.IntPtr,AudioUnit.AudioComponentDescription&,System.Int32&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphCountNodeInteractions(System.IntPtr,System.Int32,System.UInt32&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphGetCPULoad(System.IntPtr,System.Single&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphGetIndNode(System.IntPtr,System.UInt32,System.Int32&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphGetMaxCPULoad(System.IntPtr,System.Single&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphGetNodeCount(System.IntPtr,System.Int32&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphGetNumberOfInteractions(System.IntPtr,System.UInt32&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphIsInitialized(System.IntPtr,System.Boolean&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphIsOpen(System.IntPtr,System.Boolean&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphIsRunning(System.IntPtr,System.Boolean&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphNodeInfo(System.IntPtr,System.Int32,AudioUnit.AudioComponentDescription&,System.IntPtr&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphNodeInfo(System.IntPtr,System.Int32,System.IntPtr,System.IntPtr&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphSetNodeInputCallback(System.IntPtr,System.Int32,System.UInt32,AudioUnit.AURenderCallbackStruct&)",
			"AudioUnit.AUGraphError AudioUnit.AUGraph::AUGraphUpdate(System.IntPtr,System.Boolean&)",
			"AVFoundation.AVSampleCursorAudioDependencyInfo ObjCRuntime.Messaging::AVSampleCursorAudioDependencyInfo_objc_msgSend(System.IntPtr,System.IntPtr)",
			"AVFoundation.AVSampleCursorAudioDependencyInfo ObjCRuntime.Messaging::AVSampleCursorAudioDependencyInfo_objc_msgSendSuper(System.IntPtr,System.IntPtr)",
			"AVFoundation.AVSampleCursorChunkInfo ObjCRuntime.Messaging::AVSampleCursorChunkInfo_objc_msgSend_stret(System.IntPtr,System.IntPtr)",
			"AVFoundation.AVSampleCursorChunkInfo ObjCRuntime.Messaging::AVSampleCursorChunkInfo_objc_msgSend(System.IntPtr,System.IntPtr)",
			"AVFoundation.AVSampleCursorChunkInfo ObjCRuntime.Messaging::AVSampleCursorChunkInfo_objc_msgSendSuper_stret(System.IntPtr,System.IntPtr)",
			"AVFoundation.AVSampleCursorChunkInfo ObjCRuntime.Messaging::AVSampleCursorChunkInfo_objc_msgSendSuper(System.IntPtr,System.IntPtr)",
			"AVFoundation.AVSampleCursorSyncInfo ObjCRuntime.Messaging::AVSampleCursorSyncInfo_objc_msgSend_stret(System.IntPtr,System.IntPtr)",
			"AVFoundation.AVSampleCursorSyncInfo ObjCRuntime.Messaging::AVSampleCursorSyncInfo_objc_msgSend(System.IntPtr,System.IntPtr)",
			"AVFoundation.AVSampleCursorSyncInfo ObjCRuntime.Messaging::AVSampleCursorSyncInfo_objc_msgSendSuper_stret(System.IntPtr,System.IntPtr)",
			"AVFoundation.AVSampleCursorSyncInfo ObjCRuntime.Messaging::AVSampleCursorSyncInfo_objc_msgSendSuper(System.IntPtr,System.IntPtr)",
			"MediaToolbox.MTAudioProcessingTapError MediaToolbox.MTAudioProcessingTap::MTAudioProcessingTapCreate(System.IntPtr,MediaToolbox.MTAudioProcessingTap/Callbacks&,MediaToolbox.MTAudioProcessingTapCreationFlags,System.IntPtr&)",
			"MediaToolbox.MTAudioProcessingTapError MediaToolbox.MTAudioProcessingTap::MTAudioProcessingTapGetSourceAudio(System.IntPtr,System.IntPtr,System.IntPtr,MediaToolbox.MTAudioProcessingTapFlags&,CoreMedia.CMTimeRange&,System.IntPtr&)",
			"Security.SecStatusCode Security.SecIdentity::SecIdentityCopyPrivateKey(System.IntPtr,System.IntPtr&)",
			"Security.SecStatusCode Security.SecImportExport::SecPKCS12Import(System.IntPtr,System.IntPtr,System.IntPtr&)",
			"Security.SecStatusCode Security.SecItem::SecItemCopyMatching(System.IntPtr,System.IntPtr&)",
			"Security.SecStatusCode Security.SecKeyChain::SecKeychainFindGenericPassword(System.IntPtr,System.Int32,System.Byte[],System.Int32,System.Byte[],System.Int32&,System.IntPtr&,System.IntPtr)",
			"Security.SecStatusCode Security.SecKeyChain::SecKeychainFindInternetPassword(System.IntPtr,System.Int32,System.Byte[],System.Int32,System.Byte[],System.Int32,System.Byte[],System.Int32,System.Byte[],System.Int16,System.IntPtr,System.IntPtr,System.Int32&,System.IntPtr&,System.IntPtr)",
			"Security.SecStatusCode Security.SecTrust::SecTrustCopyCustomAnchorCertificates(System.IntPtr,System.IntPtr&)",
			"Security.SecStatusCode Security.SecTrust::SecTrustCopyPolicies(System.IntPtr,System.IntPtr&)",
			"Security.SecStatusCode Security.SecTrust::SecTrustCreateWithCertificates(System.IntPtr,System.IntPtr,System.IntPtr&)",
			"Security.SecStatusCode Security.SecTrust::SecTrustEvaluate(System.IntPtr,Security.SecTrustResult&)",
			"Security.SecStatusCode Security.SecTrust::SecTrustGetNetworkFetchAllowed(System.IntPtr,System.Boolean&)",
			"Security.SecStatusCode Security.SecTrust::SecTrustGetTrustResult(System.IntPtr,Security.SecTrustResult&)",
			"Security.SecStatusCode Security.SecTrust::SecTrustSetAnchorCertificatesOnly(System.IntPtr,System.Boolean)",
			"Security.SecStatusCode Security.SecTrust::SecTrustSetNetworkFetchAllowed(System.IntPtr,System.Boolean)",
			"Security.SslStatus Security.SslContext::SSLCopyPeerTrust(System.IntPtr,System.IntPtr&)",
			"Security.SslStatus Security.SslContext::SSLCopyRequestedPeerName(System.IntPtr,System.Byte[],System.UIntPtr&)",
			"Security.SslStatus Security.SslContext::SSLCopyRequestedPeerNameLength(System.IntPtr,System.UIntPtr&)",
			"Security.SslStatus Security.SslContext::SSLGetBufferedReadSize(System.IntPtr,System.IntPtr&)",
			"Security.SslStatus Security.SslContext::SSLGetClientCertificateState(System.IntPtr,Security.SslClientCertificateState&)",
			"Security.SslStatus Security.SslContext::SSLGetConnection(System.IntPtr,System.IntPtr&)",
			"Security.SslStatus Security.SslContext::SSLGetDatagramWriteSize(System.IntPtr,System.IntPtr&)",
			"Security.SslStatus Security.SslContext::SSLGetMaxDatagramRecordSize(System.IntPtr,System.IntPtr&)",
			"Security.SslStatus Security.SslContext::SSLGetNegotiatedProtocolVersion(System.IntPtr,Security.SslProtocol&)",
			"Security.SslStatus Security.SslContext::SSLGetPeerDomainName(System.IntPtr,System.Byte[],System.IntPtr&)",
			"Security.SslStatus Security.SslContext::SSLGetPeerDomainNameLength(System.IntPtr,System.IntPtr&)",
			"Security.SslStatus Security.SslContext::SSLGetPeerID(System.IntPtr,System.IntPtr&,System.IntPtr&)",
			"Security.SslStatus Security.SslContext::SSLGetProtocolVersionMax(System.IntPtr,Security.SslProtocol&)",
			"Security.SslStatus Security.SslContext::SSLGetProtocolVersionMin(System.IntPtr,Security.SslProtocol&)",
			"Security.SslStatus Security.SslContext::SSLGetSessionOption(System.IntPtr,Security.SslSessionOption,System.Boolean&)",
			"Security.SslStatus Security.SslContext::SSLGetSessionState(System.IntPtr,Security.SslSessionState&)",
			"Security.SslStatus Security.SslContext::SSLRead(System.IntPtr,System.Byte*,System.IntPtr,System.IntPtr&)",
			"Security.SslStatus Security.SslContext::SSLSetSessionOption(System.IntPtr,Security.SslSessionOption,System.Boolean)",
			"Security.SslStatus Security.SslContext::SSLWrite(System.IntPtr,System.Byte*,System.IntPtr,System.IntPtr&)",
			"System.Boolean Network.NWAdvertiseDescriptor::nw_advertise_descriptor_get_no_auto_rename(System.IntPtr)",
			"System.Boolean Network.NWBrowserDescriptor::nw_browse_descriptor_get_include_txt_record(System.IntPtr)",
			"System.Boolean Network.NWConnectionGroup::nw_connection_group_reinsert_extracted_connection(System.IntPtr,System.IntPtr)",
			"System.Boolean Network.NWContentContext::nw_content_context_get_is_final(System.IntPtr)",
			"System.Boolean Network.NWEstablishmentReport::nw_establishment_report_get_proxy_configured(System.IntPtr)",
			"System.Boolean Network.NWEstablishmentReport::nw_establishment_report_get_used_proxy(System.IntPtr)",
			"System.Boolean Network.NWFramer::nw_framer_deliver_input_no_copy(System.IntPtr,System.UIntPtr,System.IntPtr,System.Boolean)",
			"System.Boolean Network.NWFramer::nw_framer_parse_input(System.IntPtr,System.UIntPtr,System.UIntPtr,System.Byte*,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Network.NWFramer::nw_framer_parse_output(System.IntPtr,System.UIntPtr,System.UIntPtr,System.Byte*,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Network.NWFramer::nw_framer_prepend_application_protocol(System.IntPtr,System.IntPtr)",
			"System.Boolean Network.NWFramer::nw_framer_write_output_no_copy(System.IntPtr,System.UIntPtr)",
			"System.Boolean Network.NWFramerMessage::nw_framer_message_access_value(System.IntPtr,System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Network.NWMulticastGroup::nw_group_descriptor_add_endpoint(System.IntPtr,System.IntPtr)",
			"System.Boolean Network.NWMulticastGroup::nw_multicast_group_descriptor_get_disable_unicast_traffic(System.IntPtr)",
			"System.Boolean Network.NWParameters::nw_parameters_get_fast_open_enabled(System.IntPtr)",
			"System.Boolean Network.NWParameters::nw_parameters_get_include_peer_to_peer(System.IntPtr)",
			"System.Boolean Network.NWParameters::nw_parameters_get_local_only(System.IntPtr)",
			"System.Boolean Network.NWParameters::nw_parameters_get_prefer_no_proxy(System.IntPtr)",
			"System.Boolean Network.NWParameters::nw_parameters_get_prohibit_constrained(System.IntPtr)",
			"System.Boolean Network.NWParameters::nw_parameters_get_prohibit_expensive(System.IntPtr)",
			"System.Boolean Network.NWParameters::nw_parameters_get_reuse_local_address(System.IntPtr)",
			"System.Boolean Network.NWParameters::nw_parameters_requires_dnssec_validation(System.IntPtr)",
			"System.Boolean Network.NWPath::nw_path_has_dns(System.IntPtr)",
			"System.Boolean Network.NWPath::nw_path_has_ipv4(System.IntPtr)",
			"System.Boolean Network.NWPath::nw_path_has_ipv6(System.IntPtr)",
			"System.Boolean Network.NWPath::nw_path_is_constrained(System.IntPtr)",
			"System.Boolean Network.NWPath::nw_path_is_equal(System.IntPtr,System.IntPtr)",
			"System.Boolean Network.NWPath::nw_path_is_expensive(System.IntPtr)",
			"System.Boolean Network.NWPath::nw_path_uses_interface_type(System.IntPtr,Network.NWInterfaceType)",
			"System.Boolean Network.NWProtocolDefinition::nw_protocol_definition_is_equal(System.IntPtr,System.IntPtr)",
			"System.Boolean Network.NWProtocolMetadata::nw_protocol_metadata_is_framer_message(System.IntPtr)",
			"System.Boolean Network.NWProtocolMetadata::nw_protocol_metadata_is_ip(System.IntPtr)",
			"System.Boolean Network.NWProtocolMetadata::nw_protocol_metadata_is_quic(System.IntPtr)",
			"System.Boolean Network.NWProtocolMetadata::nw_protocol_metadata_is_tcp(System.IntPtr)",
			"System.Boolean Network.NWProtocolMetadata::nw_protocol_metadata_is_tls(System.IntPtr)",
			"System.Boolean Network.NWProtocolMetadata::nw_protocol_metadata_is_udp(System.IntPtr)",
			"System.Boolean Network.NWProtocolMetadata::nw_protocol_metadata_is_ws(System.IntPtr)",
			"System.Boolean Network.NWProtocolOptions::nw_protocol_options_is_quic(System.IntPtr)",
			"System.Boolean Network.NWProtocolQuicOptions::nw_quic_get_stream_is_datagram(System.IntPtr)",
			"System.Boolean Network.NWProtocolQuicOptions::nw_quic_get_stream_is_unidirectional(System.IntPtr)",
			"System.Boolean Network.NWTxtRecord::nw_txt_record_access_bytes(System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Network.NWTxtRecord::nw_txt_record_access_key(System.IntPtr,System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Network.NWTxtRecord::nw_txt_record_apply(System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Network.NWTxtRecord::nw_txt_record_is_equal(System.IntPtr,System.IntPtr)",
			"System.Boolean Network.NWWebSocketRequest::nw_ws_request_enumerate_additional_headers(System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Network.NWWebSocketRequest::nw_ws_request_enumerate_subprotocols(System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Network.NWWebSocketResponse::nw_ws_response_enumerate_additional_headers(System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Security.SecIdentity2::sec_identity_access_certificates(System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Security.SecProtocolMetadata::sec_protocol_metadata_access_distinguished_names(System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Security.SecProtocolMetadata::sec_protocol_metadata_access_ocsp_response(System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Security.SecProtocolMetadata::sec_protocol_metadata_access_peer_certificate_chain(System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Security.SecProtocolMetadata::sec_protocol_metadata_access_pre_shared_keys(System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Boolean Security.SecProtocolMetadata::sec_protocol_metadata_challenge_parameters_are_equal(System.IntPtr,System.IntPtr)",
			"System.Boolean Security.SecProtocolMetadata::sec_protocol_metadata_peers_are_equal(System.IntPtr,System.IntPtr)",
			"System.Boolean Security.SecProtocolOptions::sec_protocol_options_are_equal(System.IntPtr,System.IntPtr)",
			"System.Boolean Security.SecTrust::SecTrustEvaluateWithError(System.IntPtr,System.IntPtr&)",
			"System.Boolean Security.SecTrust::SecTrustSetExceptions(System.IntPtr,System.IntPtr)",
			"System.Byte Security.SecProtocolMetadata::sec_protocol_metadata_access_supported_signature_algorithms(System.IntPtr,ObjCRuntime.BlockLiteral*)",
			"System.Byte* Network.NWEndpoint::nw_endpoint_get_signature(System.IntPtr,System.UIntPtr&)",
			"System.Int32 AudioUnit.AudioUnit::AudioObjectGetPropertyData(System.UInt32,AudioUnit.AudioObjectPropertyAddress&,System.UInt32&,System.IntPtr&,System.UInt32&,System.UInt32&)",
			"System.Int32 AudioUnit.AudioUnit::AudioUnitGetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,System.UInt32&,System.UInt32&)",
			"System.Int32 AudioUnit.AudioUnit::AudioUnitSetProperty(System.IntPtr,AudioUnit.AudioUnitPropertyIDType,AudioUnit.AudioUnitScopeType,System.UInt32,AudioToolbox.AudioStreamBasicDescription&,System.UInt32)",
			"System.Int32 AudioUnit.AUGraph::NewAUGraph(System.IntPtr&)",
			"System.Int32 Security.Authorization::AuthorizationCreate(Security.AuthorizationItemSet*,Security.AuthorizationItemSet*,Security.AuthorizationFlags,System.IntPtr&)",
			"System.Int32 Security.SslContext::SSLCopyALPNProtocols(System.IntPtr,System.IntPtr&)",
			"System.Int32 Security.SslContext::SSLSetSessionTicketsEnabled(System.IntPtr,System.Boolean)",
			"System.IntPtr ObjCRuntime.Selector::GetHandle(System.String)",
			"System.IntPtr Security.SecAccessControl::SecAccessControlCreateWithFlags(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr&)",
			"System.IntPtr Security.SecKey::SecKeyCreateEncryptedData(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr&)",
			"System.IntPtr Security.SecPolicy::SecPolicyCreateSSL(System.Boolean,System.IntPtr)",
			"System.Void Network.NWAdvertiseDescriptor::nw_advertise_descriptor_set_no_auto_rename(System.IntPtr,System.Boolean)",
			"System.Void Network.NWBrowserDescriptor::nw_browse_descriptor_set_include_txt_record(System.IntPtr,System.Boolean)",
			"System.Void Network.NWConnection::nw_connection_send(System.IntPtr,System.IntPtr,System.IntPtr,System.Boolean,ObjCRuntime.BlockLiteral*)",
			"System.Void Network.NWConnectionGroup::nw_connection_group_set_receive_handler(System.IntPtr,System.UInt32,System.Boolean,ObjCRuntime.BlockLiteral*)",
			"System.Void Network.NWContentContext::nw_content_context_set_is_final(System.IntPtr,System.Boolean)",
			"System.Void Network.NWFramer::nw_framer_deliver_input(System.IntPtr,System.Byte*,System.UIntPtr,System.IntPtr,System.Boolean)",
			"System.Void Network.NWMulticastGroup::nw_multicast_group_descriptor_set_disable_unicast_traffic(System.IntPtr,System.Boolean)",
			"System.Void Network.NWParameters::nw_parameters_set_fast_open_enabled(System.IntPtr,System.Boolean)",
			"System.Void Network.NWParameters::nw_parameters_set_include_peer_to_peer(System.IntPtr,System.Boolean)",
			"System.Void Network.NWParameters::nw_parameters_set_local_only(System.IntPtr,System.Boolean)",
			"System.Void Network.NWParameters::nw_parameters_set_prefer_no_proxy(System.IntPtr,System.Boolean)",
			"System.Void Network.NWParameters::nw_parameters_set_prohibit_constrained(System.IntPtr,System.Boolean)",
			"System.Void Network.NWParameters::nw_parameters_set_prohibit_expensive(System.IntPtr,System.Boolean)",
			"System.Void Network.NWParameters::nw_parameters_set_requires_dnssec_validation(System.IntPtr,System.Boolean)",
			"System.Void Network.NWParameters::nw_parameters_set_reuse_local_address(System.IntPtr,System.Boolean)",
			"System.Void Network.NWPrivacyContext::nw_privacy_context_require_encrypted_name_resolution(System.IntPtr,System.Boolean,System.IntPtr)",
			"System.Void Network.NWProtocolIPOptions::nw_ip_options_set_disable_multicast_loopback(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolOptions::nw_ip_options_set_calculate_receive_time(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolOptions::nw_ip_options_set_disable_fragmentation(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolOptions::nw_ip_options_set_use_minimum_mtu(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolOptions::nw_tcp_options_set_disable_ack_stretching(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolOptions::nw_tcp_options_set_disable_ecn(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolOptions::nw_tcp_options_set_enable_fast_open(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolOptions::nw_tcp_options_set_enable_keepalive(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolOptions::nw_tcp_options_set_no_delay(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolOptions::nw_tcp_options_set_no_options(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolOptions::nw_tcp_options_set_no_push(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolOptions::nw_tcp_options_set_retransmit_fin_drop(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolOptions::nw_udp_options_set_prefer_no_checksum(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolQuicOptions::nw_quic_set_stream_is_datagram(System.IntPtr,System.Boolean)",
			"System.Void Network.NWProtocolQuicOptions::nw_quic_set_stream_is_unidirectional(System.IntPtr,System.Boolean)",
			"System.Void Network.NWWebSocketOptions::nw_ws_options_set_auto_reply_ping(System.IntPtr,System.Boolean)",
			"System.Void Network.NWWebSocketOptions::nw_ws_options_set_skip_handshake(System.IntPtr,System.Boolean)",
			"System.Void ObjCRuntime.Messaging::void_objc_msgSend_GCDualSenseAdaptiveTriggerPositionalAmplitudes_float(System.IntPtr,System.IntPtr,GameController.GCDualSenseAdaptiveTriggerPositionalAmplitudes,System.Single)",
			"System.Void ObjCRuntime.Messaging::void_objc_msgSend_GCDualSenseAdaptiveTriggerPositionalResistiveStrengths(System.IntPtr,System.IntPtr,GameController.GCDualSenseAdaptiveTriggerPositionalResistiveStrengths)",
			"System.Void ObjCRuntime.Messaging::void_objc_msgSendSuper_GCDualSenseAdaptiveTriggerPositionalAmplitudes_float(System.IntPtr,System.IntPtr,GameController.GCDualSenseAdaptiveTriggerPositionalAmplitudes,System.Single)",
			"System.Void ObjCRuntime.Messaging::void_objc_msgSendSuper_GCDualSenseAdaptiveTriggerPositionalResistiveStrengths(System.IntPtr,System.IntPtr,GameController.GCDualSenseAdaptiveTriggerPositionalResistiveStrengths)",
		};
	}
}
