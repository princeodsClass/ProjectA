using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;

/** 플랫폼 빌더 - 안드로이드 */
public static partial class CPlatformBuilder
{
	#region 클래스 함수
	/** 안드로이드 구글 플랫폼을 빌드한다 */
	public static void BuildAndroidGoogleStoreDist()
	{
		CPlatformBuilder.BuildAndroidGoogleStoreB();
	}

	/** 안드로이드 아마존 플랫폼을 빌드한다 */
	public static void BuildAndroidAmazonStoreDist()
	{
		CPlatformBuilder.BuildAndroidAmazonStoreA();
	}

	/** 안드로이드 플랫폼을 빌드한다 */
	private static void BuildAndroidDebug(EAndroidPlatformType a_eType, bool a_bIsAutoPlay, bool a_bIsEnableProfiler)
	{
		CPlatformBuilder.BuildMode = EBuildMode.DEBUG;
		EditorUserBuildSettings.buildAppBundle = false;

		// 빌드 옵션을 설정한다
		var oPlayerOpts = new BuildPlayerOptions();
		oPlayerOpts.options = BuildOptions.Development;
		oPlayerOpts.options |= a_bIsAutoPlay ? BuildOptions.AutoRunPlayer : BuildOptions.None;
		oPlayerOpts.options |= a_bIsEnableProfiler ? BuildOptions.ConnectWithProfiler : BuildOptions.None;

		CPlatformBuilder.BuildAndroid(a_eType, oPlayerOpts);
	}

	/** 안드로이드 플랫폼을 빌드한다 */
	private static void BuildAndroidDebugWithAutoPlay(EAndroidPlatformType a_eType)
	{
		CPlatformBuilder.BuildAndroidDebug(a_eType, true, false);
	}

	/** 안드로이드 플랫폼을 빌드한다 */
	private static void BuildAndroidDebugWithRoboTest(EAndroidPlatformType a_eType)
	{
		CPlatformBuilder.BuildAndroidDebug(a_eType, false, false);
	}

	/** 안드로이드 플랫폼을 빌드한다 */
	private static void BuildAndroidDebugWithProfiler(EAndroidPlatformType a_eType)
	{
		CPlatformBuilder.BuildAndroidDebug(a_eType, true, true);
	}

	/** 안드로이드 플랫폼을 빌드한다 */
	private static void BuildAndroidRelease(EAndroidPlatformType a_eType, bool a_bIsAutoPlay)
	{
		CPlatformBuilder.BuildMode = EBuildMode.RELEASE;
		EditorUserBuildSettings.buildAppBundle = false;

		// 빌드 옵션을 설정한다
		var oPlayerOpts = new BuildPlayerOptions();
		oPlayerOpts.options = a_bIsAutoPlay ? BuildOptions.AutoRunPlayer : BuildOptions.None;

		CPlatformBuilder.BuildAndroid(a_eType, oPlayerOpts);
	}

	/** 안드로이드 플랫폼을 빌드한다 */
	private static void BuildAndroidReleaseWithAutoPlay(EAndroidPlatformType a_eType)
	{
		CPlatformBuilder.BuildAndroidRelease(a_eType, true);
	}

	/** 안드로이드 플랫폼을 빌드한다 */
	private static void BuildAndroidStoreA(EAndroidPlatformType a_eType)
	{
		CPlatformBuilder.BuildAndroidStore(a_eType, false);
	}

	/** 안드로이드 플랫폼을 빌드한다 */
	private static void BuildAndroidStoreB(EAndroidPlatformType a_eType)
	{
		CPlatformBuilder.BuildAndroidStore(a_eType, true);
	}

	/** 안드로이드 플랫폼을 빌드한다 */
	private static void BuildAndroidStore(EAndroidPlatformType a_eType, bool a_bIsBuildAppBundle)
	{
		CPlatformBuilder.BuildMode = EBuildMode.STORE;
		EditorUserBuildSettings.buildAppBundle = a_bIsBuildAppBundle;

		CPlatformBuilder.BuildAndroid(a_eType, new BuildPlayerOptions());
	}

	/** 안드로이드 플랫폼을 빌드한다 */
	private static void BuildAndroid(EAndroidPlatformType a_eType, BuildPlayerOptions a_oPlayerOpts)
	{
		CPlatformBuilder.AndroidPlatformType = a_eType;
		
		// 아키텍처를 설정한다
		PlayerSettings.Android.buildApkPerCpuArchitecture = false;
		PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

		// 빌드 옵션을 설정한다 {
		string oPlatform = CPlatformBuilder.GetAndroidPlatform(a_eType);
		string oBuildFileExtension = EditorUserBuildSettings.buildAppBundle ? "aab" : "apk";

		a_oPlayerOpts.target = BuildTarget.Android;
		a_oPlayerOpts.targetGroup = BuildTargetGroup.Android;
		a_oPlayerOpts.locationPathName = string.Format("Builds/Android/{0}/{1}", oPlatform, string.Format("{0}BuildOutput.{1}", oPlatform, oBuildFileExtension));
		// 빌드 옵션을 설정한다 }

		// 안드로이드 옵션을 설정한다 {
		EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

		EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
		EditorUserBuildSettings.androidETC2Fallback = AndroidETC2Fallback.Quality32Bit;
		EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
		EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.Generic;
		// 안드로이드 옵션을 설정한다 }

		// 빌드 정보 테이블이 존재 할 경우
		if (CPlatformOptsSetter.BuildInfoTable != null)
		{
			PlayerSettings.Android.keystoreName = CPlatformOptsSetter.BuildInfoTable.AndroidBuildInfo.m_oKeystoreName;
			PlayerSettings.Android.keyaliasName = CPlatformOptsSetter.BuildInfoTable.AndroidBuildInfo.m_oKeyaliasName;
			PlayerSettings.Android.keystorePass = CPlatformOptsSetter.BuildInfoTable.AndroidBuildInfo.m_oKeystorePassword;
			PlayerSettings.Android.keyaliasPass = CPlatformOptsSetter.BuildInfoTable.AndroidBuildInfo.m_oKeyaliasPassword;
			
			PlayerSettings.Android.useCustomKeystore = true;
		}

		// 안드로이드 옵션을 설정한다
		PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
		PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
		PlayerSettings.Android.androidTargetDevices = AndroidTargetDevices.AllDevices;

		// 플랫폼을 빌드한다
		CPlatformBuilder.BuildPlatform(a_oPlayerOpts);
	}

	/** 안드로이드 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildAndroidDebug(EAndroidPlatformType a_eType)
	{
		CPlatformBuilder.ExecuteAndroidJenkinsBuild(a_eType, CPlatformBuilder.B_DEBUG_BUILD_FUNC_JENKINS, CPlatformBuilder.B_ANDROID_DEBUG_PIPELINE_N_JENKINS, "apk");
	}

	/** 안드로이드 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildAndroidRelease(EAndroidPlatformType a_eType)
	{
		CPlatformBuilder.ExecuteAndroidJenkinsBuild(a_eType, CPlatformBuilder.B_RELEASE_BUILD_FUNC_JENKINS, CPlatformBuilder.B_ANDROID_RELEASE_PIPELINE_N_JENKINS, "apk");
	}

	/** 안드로이드 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildAndroidStoreA(EAndroidPlatformType a_eType)
	{
		CPlatformBuilder.ExecuteAndroidJenkinsBuild(a_eType, CPlatformBuilder.B_STORE_A_BUILD_FUNC_JENKINS, CPlatformBuilder.B_ANDROID_STORE_PIPELINE_N_JENKINS, "apk");
	}

	/** 안드로이드 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildAndroidStoreB(EAndroidPlatformType a_eType)
	{
		CPlatformBuilder.ExecuteAndroidJenkinsBuild(a_eType, CPlatformBuilder.B_STORE_B_BUILD_FUNC_JENKINS, CPlatformBuilder.B_ANDROID_STORE_PIPELINE_N_JENKINS, "aab");
	}

	/** 안드로이드 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildAndroidStoreDistA(EAndroidPlatformType a_eType)
	{
		CPlatformBuilder.ExecuteAndroidJenkinsBuild(a_eType, CPlatformBuilder.B_STORE_DIST_BUILD_FUNC_JENKINS, CPlatformBuilder.B_ANDROID_STORE_PIPELINE_N_JENKINS, "apk");
	}

	/** 안드로이드 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildAndroidStoreDistB(EAndroidPlatformType a_eType)
	{
		CPlatformBuilder.ExecuteAndroidJenkinsBuild(a_eType, CPlatformBuilder.B_STORE_DIST_BUILD_FUNC_JENKINS, CPlatformBuilder.B_ANDROID_STORE_PIPELINE_N_JENKINS, "aab");
	}
	#endregion // 클래스 함수

	#region 클래스 접근 함수
	/** 안드로이드 프로젝트 이름을 반환한다 */
	public static string GetAndroidProjName(EAndroidPlatformType a_eType)
	{
		switch (a_eType)
		{
			case EAndroidPlatformType.AMAZON: return "12.AndroidAmazon";
		}

		return "11.AndroidGoogle";
	}

	/** 안드로이드 이름을 반환한다 */
	private static string GetAndroidPlatform(EAndroidPlatformType a_eType)
	{
		switch (a_eType)
		{
			case EAndroidPlatformType.AMAZON: return CPlatformBuilder.B_PLATFORM_ANDROID_AMAZON;
		}

		return CPlatformBuilder.B_PLATFORM_ANDROID_GOOGLE;
	}

	/** 안드로이드 빌드 결과 경로를 반환한다 */
	public static string GetAndroidBuildOutputPath(EAndroidPlatformType a_eType, string a_oBuildFileExtension)
	{
		return string.Format("Builds/Android/{0}/{0}BuildOutput.{1}", CPlatformBuilder.GetAndroidPlatform(a_eType), a_oBuildFileExtension);
	}
	#endregion // 클래스 접근 함수
}

/** 플랫폼 빌드 - 크리에이티브 */
public static partial class CPlatformBuilder
{
	#region 클래스 함수
	/** 구글 크리에이티브를 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Creative/Google/Release", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 2)]
	public static void BuildGoogleCreativeRelease()
	{
		CPlatformBuilder.BuildAndroidRelease(EAndroidPlatformType.GOOGLE, false);
	}

	/** 구글 크리에이티브를 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Creative/Google/Release with Auto Play", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 2)]
	public static void BuildGoogleCreativeReleaseWithAutoPlay()
	{
		CPlatformBuilder.BuildAndroidReleaseWithAutoPlay(EAndroidPlatformType.GOOGLE);
	}

	/** 아마존 크리에이티브를 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Creative/Amazon/Release", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 2)]
	public static void BuildAmazonCreativeRelease()
	{
		CPlatformBuilder.BuildAndroidRelease(EAndroidPlatformType.AMAZON, false);
	}

	/** 아마존 크리에이티브를 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Creative/Amazon/Release with Auto Play", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 2)]
	public static void BuildAmazonCreativeReleaseWithAutoPlay()
	{
		CPlatformBuilder.BuildAndroidReleaseWithAutoPlay(EAndroidPlatformType.AMAZON);
	}
	#endregion // 클래스 함수
}

/** 플랫폼 빌더 - 안드로이드 구글 */
public static partial class CPlatformBuilder
{
	#region 클래스 함수
	/** 안드로이드 구글 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Google/Debug", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void BuildAndroidGoogleDebug()
	{
		CPlatformBuilder.BuildAndroidDebug(EAndroidPlatformType.GOOGLE, false, false);
	}

	/** 안드로이드 구글 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Google/Debug with Auto Play", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void BuildAndroidGoogleDebugWithAutoPlay()
	{
		CPlatformBuilder.BuildAndroidDebugWithAutoPlay(EAndroidPlatformType.GOOGLE);
	}

	/** 안드로이드 구글 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Google/Debug with Robo Test", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void BuildAndroidGoogleDebugWithRoboTest()
	{
		CPlatformBuilder.BuildAndroidDebugWithRoboTest(EAndroidPlatformType.GOOGLE);
	}

	/** 안드로이드 구글 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Google/Debug with Profiler", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void BuildAndroidGoogleDebugWithProfiler()
	{
		CPlatformBuilder.BuildAndroidDebugWithProfiler(EAndroidPlatformType.GOOGLE);
	}

	/** 안드로이드 구글 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Google/Release", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 201)]
	public static void BuildAndroidGoogleRelease()
	{
		CPlatformBuilder.BuildAndroidRelease(EAndroidPlatformType.GOOGLE, false);
	}
	
	/** 안드로이드 구글 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Google/Release with Auto Play", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 201)]
	public static void BuildAndroidGoogleReleaseWithAutoPlay()
	{
		CPlatformBuilder.BuildAndroidReleaseWithAutoPlay(EAndroidPlatformType.GOOGLE);
	}

	/** 안드로이드 구글 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Google/Distribution (APK)", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 301)]
	public static void BuildAndroidGoogleStoreA()
	{
		CPlatformBuilder.BuildAndroidStoreA(EAndroidPlatformType.GOOGLE);
	}

	/** 안드로이드 구글 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Google/Distribution (AAB)", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 301)]
	public static void BuildAndroidGoogleStoreB()
	{
		CPlatformBuilder.BuildAndroidStoreB(EAndroidPlatformType.GOOGLE);
	}

	/** 안드로이드 구글 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/Android/Google/Debug", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void RemoteBuildAndroidGoogleDebug()
	{
		CPlatformBuilder.RemoteBuildAndroidDebug(EAndroidPlatformType.GOOGLE);
	}

	/** 안드로이드 구글 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/Android/Google/Release", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 201)]
	public static void RemoteBuildAndroidGoogleRelease()
	{
		CPlatformBuilder.RemoteBuildAndroidRelease(EAndroidPlatformType.GOOGLE);
	}

	/** 안드로이드 구글 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/Android/Google/Distribution (APK)", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 301)]
	public static void RemoteBuildAndroidGoogleStoreA()
	{
		CPlatformBuilder.RemoteBuildAndroidStoreA(EAndroidPlatformType.GOOGLE);
	}

	/** 안드로이드 구글 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/Android/Google/Distribution (AAB)", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 301)]
	public static void RemoteBuildAndroidGoogleStoreB()
	{
		CPlatformBuilder.RemoteBuildAndroidStoreB(EAndroidPlatformType.GOOGLE);
	}

	/** 안드로이드 구글 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/Android/Google/Distribution (Store)", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 301)]
	public static void RemoteBuildAndroidGoogleStoreDist()
	{
		CPlatformBuilder.RemoteBuildAndroidStoreDistB(EAndroidPlatformType.GOOGLE);
	}
	#endregion // 클래스 함수
}

/** 플랫폼 빌더 - 안드로이드 아마존 */
public static partial class CPlatformBuilder
{
	#region 클래스 함수
	/** 안드로이드 아마존 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Amazon/Debug", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void BuildAndroidAmazonDebug()
	{
		CPlatformBuilder.BuildAndroidDebug(EAndroidPlatformType.AMAZON, false, false);
	}

	/** 안드로이드 아마존 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Amazon/Debug with Auto Play", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void BuildAndroidAmazonDebugWithAutoPlay()
	{
		CPlatformBuilder.BuildAndroidDebugWithAutoPlay(EAndroidPlatformType.AMAZON);
	}

	/** 안드로이드 아마존 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Amazon/Debug with Robo Test", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void BuildAndroidAmazonDebugWithRoboTest()
	{
		CPlatformBuilder.BuildAndroidDebugWithRoboTest(EAndroidPlatformType.AMAZON);
	}

	/** 안드로이드 아마존 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Amazon/Debug with Profiler", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void BuildAndroidAmazonDebugWithProfiler()
	{
		CPlatformBuilder.BuildAndroidDebugWithProfiler(EAndroidPlatformType.AMAZON);
	}

	/** 안드로이드 아마존 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Amazon/Release", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 201)]
	public static void BuildAndroidAmazonRelease()
	{
		CPlatformBuilder.BuildAndroidRelease(EAndroidPlatformType.AMAZON, false);
	}

	/** 안드로이드 아마존 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Amazon/Release with Auto Play", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 201)]
	public static void BuildAndroidAmazonReleaseWithAutoPlay()
	{
		CPlatformBuilder.BuildAndroidReleaseWithAutoPlay(EAndroidPlatformType.AMAZON);
	}

	/** 안드로이드 아마존 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Android/Amazon/Distribution (APK)", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 301)]
	public static void BuildAndroidAmazonStoreA()
	{
		CPlatformBuilder.BuildAndroidStoreA(EAndroidPlatformType.AMAZON);
	}

	/** 안드로이드 아마존 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/Android/Amazon/Debug", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void RemoteBuildAndroidAmazonDebug()
	{
		CPlatformBuilder.RemoteBuildAndroidDebug(EAndroidPlatformType.AMAZON);
	}

	/** 안드로이드 아마존 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/Android/Amazon/Release", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 201)]
	public static void RemoteBuildAndroidAmazonRelease()
	{
		CPlatformBuilder.RemoteBuildAndroidRelease(EAndroidPlatformType.AMAZON);
	}

	/** 안드로이드 아마존 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/Android/Amazon/Distribution (APK)", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 301)]
	public static void RemoteBuildAndroidAmazonStoreA()
	{
		CPlatformBuilder.RemoteBuildAndroidStoreA(EAndroidPlatformType.AMAZON);
	}

	/** 안드로이드 아마존 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/Android/Amazon/Distribution (Store)", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 301)]
	public static void RemoteBuildAndroidAmazonStoreDist()
	{
		CPlatformBuilder.RemoteBuildAndroidStoreDistA(EAndroidPlatformType.AMAZON);
	}
	#endregion // 클래스 함수
}
#endif // #if UNITY_EDITOR
