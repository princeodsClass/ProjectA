using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.U2D;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using SRDebugger.Editor;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;

/** 에디터 기본 접근자 확장 클래스 */
public static partial class CEditorAccessExtension
{
	#region 클래스 함수
	/** 유효 여부를 검사한다 */
	public static bool ExIsValid(this PlistDocument a_oSender)
	{
		return a_oSender != null && a_oSender.root != null;
	}

	/** 포함 여부를 검사한다 */
	public static bool ExIsContains(this PlistElementArray a_oSender, string a_oStr)
	{
		return a_oSender.values.FindIndex((a_oElement) => a_oElement.AsString().Equals(a_oStr)) >= 0;
	}

	/** 포함 여부를 검사한다 */
	public static bool ExIsContains(this PlistElementDict a_oSender, string a_oStr)
	{
		return a_oSender.values.ContainsKey(a_oStr);
	}

	/** 포함 여부를 검사한다 */
	public static bool ExIsContainsAdsNetworkID(this PlistElementArray a_oSender, string a_oNetworkID)
	{
		for (int i = 0; i < a_oSender.values.Count; ++i)
		{
			var oValDict = a_oSender.values[i].AsDict();

			// 광고 네트워크 식별자가 존재 할 경우
			if (oValDict.values.TryGetValue("SKAdNetworkIdentifier", out PlistElement oElement) && oElement.AsString().Equals(a_oNetworkID))
			{
				return true;
			}
		}

		return false;
	}

	/** 배열을 반환한다 */
	public static PlistElementArray ExGetArray(this PlistDocument a_oSender, string a_oKey)
	{
		try
		{
			return a_oSender.root[a_oKey].AsArray();
		}
		catch
		{
			return a_oSender.root.CreateArray(a_oKey);
		}
	}

	/** 딕셔너리를 반환한다 */
	public static PlistElementDict ExGetDict(this PlistDocument a_oSender, string a_oKey)
	{
		try
		{
			return a_oSender.root[a_oKey].AsDict();
		}
		catch
		{
			return a_oSender.root.CreateDict(a_oKey);
		}
	}

	/** 문자열을 추가한다 */
	public static void ExAddStr(this PlistElementArray a_oSender, string a_oStr)
	{
		// 문자열 추가가 가능 할 경우
		if (a_oSender != null && !a_oSender.ExIsContains(a_oStr))
		{
			a_oSender.AddString(a_oStr);
		}
	}

	/** 값을 추가한다 */
	public static void ExAddStr(this PlistElementDict a_oSender, string a_oKey, bool a_bIsTrue)
	{
		// 값 추가가 가능 할 경우
		if (a_oSender != null && !a_oSender.values.ContainsKey(a_oKey))
		{
			a_oSender.SetBoolean(a_oKey, a_bIsTrue);
		}
	}

	/** 값을 추가한다 */
	public static void ExAddStr(this PlistElementDict a_oSender, string a_oKey, string a_oStr)
	{
		// 값 추가가 가능 할 경우
		if (a_oSender != null && !a_oSender.values.ContainsKey(a_oKey))
		{
			a_oSender.SetString(a_oKey, a_oStr);
		}
	}
	#endregion // 클래스 함수
}
#endif // #if UNITY_IOS

/** 플랫폼 빌더 */
public static partial class CPlatformBuilder
{
	/** 빌드 모드 */
	public enum EBuildMode
	{
		NONE = -1,
		DEBUG,
		RELEASE,
		STORE,
		[HideInInspector] MAX_VAL
	}

	/** iOS 플랫폼 타입 */
	public enum EiOSPlatformType
	{
		NONE = -1,
		APPLE,
		[HideInInspector] MAX_VAL
	}

	/** 안드로이드 플랫폼 타입 */
	public enum EAndroidPlatformType
	{
		NONE = -1,
		GOOGLE,
		AMAZON,
		[HideInInspector] MAX_VAL
	}

	/** 독립 플랫폼 타입 */
	public enum EStandalonePlatformType
	{
		NONE = -1,
		MAC_STEAM,
		MAC_EDITOR,
		WNDS_STEAM,
		WNDS_EDITOR,
		[HideInInspector] MAX_VAL
	}

	/** 젠킨스 매개 변수 */
	private struct STJenkinsParams
	{
		public string m_oSrc;
		public string m_oPipeline;
		public string m_oProjName;
		public string m_oBuildOutputPath;
		public string m_oBuildFileExtension;
		public string m_oPlatform;
		public string m_oProjPlatform;
		public string m_oBuildVer;
		public string m_oBuildFunc;
		public string m_oPipelineName;

		public Dictionary<string, string> m_oDataDict;
	}

	#region 상수
	public const string B_PLATFORM_IOS_APPLE = "iOSApple";

	public const string B_PLATFORM_ANDROID_GOOGLE = "AndroidGoogle";
	public const string B_PLATFORM_ANDROID_AMAZON = "AndroidAmazon";

	public const string B_PLATFORM_STANDALONE_MAC_STEAM = "StandaloneMacSteam";
	public const string B_PLATFORM_STANDALONE_MAC_EDITOR = "StandaloneMacEditor";

	public const string B_PLATFORM_STANDALONE_WNDS_STEAM = "StandaloneWndsSteam";
	public const string B_PLATFORM_STANDALONE_WNDS_EDITOR = "StandaloneWndsEditor";

	public static readonly List<string> B_IOS_ADS_NETWORK_ID_LIST = new List<string>() {
		"22mmun2rn5.skadnetwork",
		"238da6jt44.skadnetwork",
		"24t9a8vw3c.skadnetwork",
		"2u9pt9hc89.skadnetwork",
		"3qy4746246.skadnetwork",
		"3rd42ekr43.skadnetwork",
		"3sh42y64q3.skadnetwork",
		"424m5254lk.skadnetwork",
		"4468km3ulz.skadnetwork",
		"44jx6755aq.skadnetwork",
		"44n7hlldy6.skadnetwork",
		"488r3q3dtq.skadnetwork",
		"4dzt52r2t5.skadnetwork",
		"4fzdc2evr5.skadnetwork",
		"4pfyvq9l8r.skadnetwork",
		"578prtvx9j.skadnetwork",
		"5a6flpkh64.skadnetwork",
		"5lm9lj6jb7.skadnetwork",
		"5tjdwbrq8w.skadnetwork",
		"7rz58n8ntl.skadnetwork",
		"7ug5zh24hu.skadnetwork",
		"8s468mfl3y.skadnetwork",
		"9rd848q2bz.skadnetwork",
		"9t245vhmpl.skadnetwork",
		"av6w8kgt66.skadnetwork",
		"bvpn9ufa9b.skadnetwork",
		"c6k4g5qg8m.skadnetwork",
		"cstr6suwn9.skadnetwork",
		"ejvt5qm6ak.skadnetwork",
		"f38h382jlk.skadnetwork",
		"f73kdq92p3.skadnetwork",
		"g28c52eehv.skadnetwork",
		"glqzh8vgby.skadnetwork",
		"gta9lk7p23.skadnetwork",
		"hs6bdukanm.skadnetwork",
		"kbd757ywx3.skadnetwork",
		"klf5c3l5u5.skadnetwork",
		"lr83yxwka7.skadnetwork",
		"ludvb6z3bs.skadnetwork",
		"m8dbw4sv7c.skadnetwork",
		"mlmmfzh3r3.skadnetwork",
		"mtkv5xtk9e.skadnetwork",
		"n38lu8286q.skadnetwork",
		"n9x2a789qt.skadnetwork",
		"ppxm28t8ap.skadnetwork",
		"prcb7njmu6.skadnetwork",
		"s39g8k73mm.skadnetwork",
		"su67r6k2v3.skadnetwork",
		"t38b2kh725.skadnetwork",
		"tl55sbb4fm.skadnetwork",
		"v72qych5uu.skadnetwork",
		"v79kvwwj4g.skadnetwork",
		"v9wttpbfk9.skadnetwork",
		"wg4vff78zm.skadnetwork",
		"wzmmz9fp6w.skadnetwork",
		"yclnxrl5pm.skadnetwork",
		"ydx93a7ass.skadnetwork",
		"zmvfpc5aq8.skadnetwork",
		"mp6xlyr22a.skadnetwork",
		"275upjj5gd.skadnetwork",
		"6g9af3uyq4.skadnetwork",
		"9nlqeag3gk.skadnetwork",
		"cg4yq2srnc.skadnetwork",
		"qqp299437r.skadnetwork",
		"rx5hdcabgc.skadnetwork",
		"u679fj5vs4.skadnetwork",
		"uw77j35x4d.skadnetwork",
		"2fnua5tdw4.skadnetwork",
		"3qcr597p9d.skadnetwork",
		"e5fvkxwrpn.skadnetwork",
		"ecpz2srf59.skadnetwork",
		"hjevpa356n.skadnetwork",
		"k674qkevps.skadnetwork",
		"n6fk4nfna4.skadnetwork",
		"p78axxw29g.skadnetwork",
		"y2ed4ez56y.skadnetwork",
		"zq492l623r.skadnetwork",
		"32z4fx6l9h.skadnetwork",
		"523jb4fst2.skadnetwork",
		"54nzkqm89y.skadnetwork",
		"5l3tpt7t6e.skadnetwork",
		"6xzpu9s2p8.skadnetwork",
		"79pbpufp6p.skadnetwork",
		"9b89h5y424.skadnetwork",
		"cj5566h2ga.skadnetwork",
		"feyaarzu9v.skadnetwork",
		"ggvn48r87g.skadnetwork",
		"pwa73g5rt2.skadnetwork",
		"xy9t38ct57.skadnetwork",
		"4w7y6s5ca2.skadnetwork",
		"737z793b9f.skadnetwork",
		"dzg6xy7pwj.skadnetwork",
		"hdw39hrw9y.skadnetwork",
		"mls7yz5dvl.skadnetwork",
		"w9q455wk68.skadnetwork",
		"x44k69ngh6.skadnetwork",
		"y45688jllp.skadnetwork",
		"252b5q8x7y.skadnetwork",
		"9g2aggbj52.skadnetwork",
		"nu4557a4je.skadnetwork",
		"v4nxqhlyqp.skadnetwork",
		"r26jy69rpl.skadnetwork",
		"eh6m2bh4zr.skadnetwork",
		"8m87ys6875.skadnetwork",
		"97r2b46745.skadnetwork",
		"52fl2v3hgk.skadnetwork",
		"9yg77x724h.skadnetwork",
		"gvmwg8q7h5.skadnetwork",
		"n66cz3y3bx.skadnetwork",
		"nzq8sh4pbs.skadnetwork",
		"pu4na253f3.skadnetwork",
		"yrqqpx2mcb.skadnetwork",
		"z4gj7hsk7h.skadnetwork",
		"f7s53z58qe.skadnetwork",
		"7953jerfzd.skadnetwork"
	};

	public static readonly List<string> B_IOS_EXTRA_FRAMEWORK_LIST = new List<string>() {
		"libz.tbd",
		"libsqlite3.0.tbd",

		"iAd.framework",
		"WebKit.framework",
		"GameKit.framework",
		"Security.framework",
		"StoreKit.framework",
		"MessageUI.framework",
		"AdSupport.framework",
		"UserNotifications.framework",
		"SystemConfiguration.framework",
		"AuthenticationServices.framework"
	};

	public static readonly List<string> B_IOS_REMOVE_FRAMEWORK_LIST = new List<string>() {
		"AppTrackingTransparency.framework"
	};
	#endregion // 상수

	#region 클래스 프로퍼티
	private static EBuildMode BuildMode { get; set; } = EBuildMode.NONE;
	private static EiOSPlatformType iOSPlatformType { get; set; } = EiOSPlatformType.NONE;
	private static EAndroidPlatformType AndroidPlatformType { get; set; } = EAndroidPlatformType.NONE;
	private static EStandalonePlatformType StandalonePlatformType { get; set; } = EStandalonePlatformType.NONE;
	#endregion // 클래스 프로퍼티

	#region 클래스 함수
	/** 플랫폼 빌더를 설정한다 */
	public static void SetupPlatformBuilder()
	{
		// Do Something
	}

	/** 빌드가 완료 되었을 경우 */
	[PostProcessBuild(byte.MaxValue)]
	public static void OnPostProcessBuild(BuildTarget a_eTarget, string a_oPath)
	{
		// 배치 모드가 아닐 경우
		if (!Application.isBatchMode)
		{
			EditorUtility.RevealInFinder(a_oPath);
		}

#if UNITY_IOS
		CPlatformBuilder.HandleOnPostProcessBuildiOS(a_eTarget, a_oPath);
		CPlatformBuilder.HandleOnLatePostProcessBuildiOS(a_eTarget, a_oPath);
#endif // #if UNITY_IOS
	}

	/** 플랫폼을 빌드한다 */
	private static void BuildPlatform(BuildPlayerOptions a_oPlayerOpts)
	{
		// 빌드가 진행 중 일 경우
		if (BuildPipeline.isBuildingPlayer)
		{
			return;
		}
		
		// 버전을 설정한다
		CMenuHandler.SetupVersion();

		// 빌드 옵션을 설정한다 {
		var eBuildOptsMask = a_oPlayerOpts.options | BuildOptions.CleanBuildCache;
		var eFilterBuildOptsMask = BuildOptions.SymlinkSources | BuildOptions.CompressWithLz4 | BuildOptions.CompressWithLz4HC;

		a_oPlayerOpts.options = eBuildOptsMask & ~eFilterBuildOptsMask;
		PlayerSettings.Android.useAPKExpansionFiles = EditorUserBuildSettings.buildAppBundle;

		PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.iOS, Il2CppCodeGeneration.OptimizeSpeed);
		PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Android, Il2CppCodeGeneration.OptimizeSpeed);
		PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Standalone, Il2CppCodeGeneration.OptimizeSpeed);

#if DEBUG || DEVELOPMENT_BUILD
		a_oPlayerOpts.options |= BuildOptions.CompressWithLz4;
#else
		a_oPlayerOpts.options |= BuildOptions.CompressWithLz4HC;
#endif // #if DEBUG || DEVELOPMENT_BUILD
		// 빌드 옵션을 설정한다 }

		// 스플래시 옵션을 설정한다
		PlayerSettings.SplashScreen.show = false;
		PlayerSettings.SplashScreen.showUnityLogo = false;
		PlayerSettings.SplashScreen.blurBackgroundImage = false;

#if !(DEBUG || DEVELOPMENT_BUILD)
		// 스프라이트 아틀라스를 설정한다 {
		var oSpriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(CPlatformOptsSetter.B_ASSET_P_SPRITE_ATLAS);
		var oPackables = oSpriteAtlas.GetPackables();

		var oClonePackables = new Object[oPackables.Length];

		for (int i = 0; i < oPackables.Length; ++i)
		{
			oClonePackables[i] = oPackables[i];
			oSpriteAtlas.Remove(new Object[] { oPackables[i] });
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
		// 스프라이트 아틀라스를 설정한다 }
#endif // #if !(DEBUG || DEVELOPMENT_BUILD)

		// 씬을 설정한다 {
		var oScenePathList = new List<string>();

		for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
		{
#if DEBUG || DEVELOPMENT_BUILD
			oScenePathList.ExAddVal(EditorBuildSettings.scenes[i].path);
#else
			// 에디터 씬 일 경우
			if(EditorBuildSettings.scenes[i].path.Contains("MapEditor")) {
				continue;
			}

			oScenePathList.ExAddVal(EditorBuildSettings.scenes[i].path);
#endif // #if DEBUG || DEVELOPMENT_BUILD
		}

		a_oPlayerOpts.scenes = oScenePathList.ToArray();
		// 씬을 설정한다 }

		// 빌드 정보 테이블이 존재 할 경우
		if (CPlatformOptsSetter.BuildInfoTable != null)
		{
			PlayerSettings.companyName = "9tap";
			PlayerSettings.productName = CPlatformOptsSetter.BuildInfoTable.AppName;

			PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, CPlatformOptsSetter.BuildInfoTable.AppID);
			PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, CPlatformOptsSetter.BuildInfoTable.AppID);
			PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, CPlatformOptsSetter.BuildInfoTable.AppID);
		}

		BuildPipeline.BuildPlayer(a_oPlayerOpts);

#if !(DEBUG || DEVELOPMENT_BUILD)
		// 스프라이트 아틀라스를 설정한다 {
		oSpriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(CPlatformOptsSetter.B_ASSET_P_SPRITE_ATLAS);
		oSpriteAtlas.Add(oClonePackables);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
		// 스프라이트 아틀라스를 설정한다 }
#endif // #if !(DEBUG || DEVELOPMENT_BUILD)
	}

#if UNITY_IOS
	/** iOS 빌드 완료를 처리한다 */
	private static void HandleOnPostProcessBuildiOS(BuildTarget a_eTarget, string a_oPath)
	{
		string oPlistPath = string.Format("{0}/Info.plist", a_oPath);
		string oPBXProjPath = PBXProject.GetPBXProjectPath(a_oPath);

		// Plist 파일이 존재 할 경우
		if (File.Exists(oPlistPath))
		{
			var oDoc = new PlistDocument();
			oDoc.ReadFromFile(oPlistPath);
			oDoc.root.SetBoolean("FirebaseAppStoreReceiptURLCheckEnabled", false);

			oDoc.root.SetString("ITSAppUsesNonExemptEncryption", "NO");
			oDoc.root.SetString("NSUserTrackingUsageDescription", "Special offers and promotions just for you\nAdvertisements that match your interests\nAn improved personalized experience over time");
			oDoc.root.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com");

			var oDeviceCapabilityList = oDoc.ExGetArray("UIRequiredDeviceCapabilities");
			oDeviceCapabilityList.values.Clear();

			var oAppTransportSecurityDict = oDoc.ExGetArray("NSAppTransportSecurity");
			oAppTransportSecurityDict.values.Clear();

			for (int i = 0; i < CPlatformBuilder.B_IOS_ADS_NETWORK_ID_LIST.Count; ++i)
			{
				var oAdsNetworkItemList = oDoc.ExGetArray("SKAdNetworkItems");

				// 광고 네트워크 식별자가 없을 경우
				if (!oAdsNetworkItemList.ExIsContainsAdsNetworkID(CPlatformBuilder.B_IOS_ADS_NETWORK_ID_LIST[i]))
				{
					var oAdsNetworkIDInfoDict = oAdsNetworkItemList.AddDict();
					oAdsNetworkIDInfoDict.SetString("SKAdNetworkIdentifier", CPlatformBuilder.B_IOS_ADS_NETWORK_ID_LIST[i]);
				}
			}

			oDoc.WriteToFile(oPlistPath);
		}

		// 프로젝트 파일이 존재 할 경우
		if (File.Exists(oPBXProjPath))
		{
			var oPBXProj = new PBXProject();
			oPBXProj.ReadFromFile(oPBXProjPath);

			string oMainGUID = oPBXProj.GetUnityMainTargetGuid();
			string oFrameworkGUID = oPBXProj.GetUnityFrameworkTargetGuid();

			oPBXProj.SetBuildProperty(oMainGUID, "ENABLE_BITCODE", "NO");
			oPBXProj.SetBuildProperty(oFrameworkGUID, "ENABLE_BITCODE", "NO");

			oPBXProj.SetBuildProperty(oMainGUID, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
			oPBXProj.SetBuildProperty(oFrameworkGUID, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

			for (int i = 0; i < CPlatformBuilder.B_IOS_EXTRA_FRAMEWORK_LIST.Count; ++i)
			{
				oPBXProj.AddFrameworkToProject(oMainGUID, CPlatformBuilder.B_IOS_EXTRA_FRAMEWORK_LIST[i], false);
				oPBXProj.AddFrameworkToProject(oFrameworkGUID, CPlatformBuilder.B_IOS_EXTRA_FRAMEWORK_LIST[i], false);
			}

			oPBXProj.WriteToFile(oPBXProjPath);
		}
	}

	/** 빌드가 완료 되었을 경우 */
	public static void HandleOnLatePostProcessBuildiOS(BuildTarget a_eTarget, string a_oPath)
	{
		string oPodsPath = string.Format("{0}/Podfile", a_oPath);
		string oPlistPath = string.Format("{0}/Info.plist", a_oPath);
		string oPBXProjPath = string.Format("{0}/Pods/Pods.xcodeproj/project.pbxproj", a_oPath);

		// 코코아 포드 파일이 존재 할 경우
		if (File.Exists(oPodsPath))
		{
			ComUtil.ExecuteCmdLine(string.Format("pod update --clean-install --project-directory={0}", a_oPath), false);
		}

		// Plist 파일이 존재 할 경우
		if (File.Exists(oPlistPath))
		{
			var oDoc = new PlistDocument();
			oDoc.ReadFromFile(oPlistPath);

			var oDeviceCapabilityList = oDoc.ExGetArray("UIRequiredDeviceCapabilities");
			oDeviceCapabilityList.ExAddStr("metal");
			oDeviceCapabilityList.ExAddStr("arm64");

			var oAppTransportSecurityDict = oDoc.ExGetDict("NSAppTransportSecurity");
			oAppTransportSecurityDict.ExAddStr("NSAllowsArbitraryLoads", true);

			oDoc.WriteToFile(oPlistPath);
		}

		// 프로젝트 파일이 존재 할 경우
		if (File.Exists(oPBXProjPath))
		{
			var oPBXProj = new PBXProject();
			oPBXProj.ReadFromFile(oPBXProjPath);

			string oMainGUID = oPBXProj.GetUnityMainTargetGuid();
			string oFrameworkGUID = oPBXProj.GetUnityFrameworkTargetGuid();

			oPBXProj.SetBuildProperty(oPBXProj.ProjectGuid(), "ENABLE_BITCODE", "NO");
			oPBXProj.SetBuildProperty(oPBXProj.ProjectGuid(), "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

			oPBXProj.AddBuildProperty(oPBXProj.ProjectGuid(), "USER_HEADER_SEARCH_PATHS", "$(SRCROOT)/**");

			for (int i = 0; i < CPlatformBuilder.B_IOS_REMOVE_FRAMEWORK_LIST.Count; ++i)
			{
				oPBXProj.RemoveFrameworkFromProject(oMainGUID, CPlatformBuilder.B_IOS_REMOVE_FRAMEWORK_LIST[i]);
				oPBXProj.RemoveFrameworkFromProject(oFrameworkGUID, CPlatformBuilder.B_IOS_REMOVE_FRAMEWORK_LIST[i]);
			}

			oPBXProj.WriteToFile(oPBXProjPath);
		}
	}
#endif // #if UNITY_IOS
	#endregion // 클래스 함수
}
#endif // #if UNITY_EDITOR
