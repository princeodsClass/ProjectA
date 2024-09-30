using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;

/** 플랫폼 빌더 - iOS */
public static partial class CPlatformBuilder
{
	#region 클래스 함수
	/** iOS 애플 플랫폼을 빌드한다 */
	public static void BuildiOSAppleStoreDist()
	{
		CPlatformBuilder.BuildiOSAppleStoreA();
	}

	/** iOS 플랫폼을 빌드한다 */
	private static void BuildiOSDebug(EiOSPlatformType a_eType, bool a_bIsAutoPlay, bool a_bIsEnableProfiler)
	{
		CPlatformBuilder.BuildMode = EBuildMode.DEBUG;
		EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Debug;

		// 빌드 옵션을 설정한다
		var oPlayerOpts = new BuildPlayerOptions();
		oPlayerOpts.options = BuildOptions.Development;
		oPlayerOpts.options |= a_bIsAutoPlay ? BuildOptions.AutoRunPlayer : BuildOptions.None;
		oPlayerOpts.options |= a_bIsEnableProfiler ? BuildOptions.ConnectWithProfiler : BuildOptions.None;

		// 프로비저닝 파일 정보를 설정한다
		PlayerSettings.iOS.iOSManualProvisioningProfileID = CPlatformOptsSetter.BuildInfoTable.iOSBuildInfo.m_oDevProfileID;
		PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Development;

		CPlatformBuilder.BuildiOS(a_eType, oPlayerOpts);
	}

	/** iOS 플랫폼을 빌드한다 */
	private static void BuildiOSDebugWithAutoPlay(EiOSPlatformType a_eType)
	{
		CPlatformBuilder.BuildiOSDebug(a_eType, true, false);
	}

	/** iOS 플랫폼을 빌드한다 */
	private static void BuildiOSDebugWithProfiler(EiOSPlatformType a_eType)
	{
		CPlatformBuilder.BuildiOSDebug(a_eType, true, true);
	}

	/** iOS 플랫폼을 빌드한다 */
	private static void BuildiOSRelease(EiOSPlatformType a_eType, bool a_bIsAutoPlay)
	{
		CPlatformBuilder.BuildMode = EBuildMode.RELEASE;
		EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Release;

		// 빌드 옵션을 설정한다
		var oPlayerOpts = new BuildPlayerOptions();
		oPlayerOpts.options = a_bIsAutoPlay ? BuildOptions.AutoRunPlayer : BuildOptions.None;

		// 프로비저닝 파일 정보를 설정한다
		PlayerSettings.iOS.iOSManualProvisioningProfileID = CPlatformOptsSetter.BuildInfoTable.iOSBuildInfo.m_oDevProfileID;
		PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Development;

		CPlatformBuilder.BuildiOS(a_eType, oPlayerOpts);
	}

	/** iOS 플랫폼을 빌드한다 */
	private static void BuildiOSReleaseWithAutoPlay(EiOSPlatformType a_eType)
	{
		CPlatformBuilder.BuildiOSRelease(a_eType, true);
	}

	/** iOS 플랫폼을 빌드한다 */
	private static void BuildiOSStoreA(EiOSPlatformType a_eType)
	{
		CPlatformBuilder.BuildMode = EBuildMode.STORE;
		EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Release;

		// 프로비저닝 파일 정보를 설정한다
		PlayerSettings.iOS.iOSManualProvisioningProfileID = CPlatformOptsSetter.BuildInfoTable.iOSBuildInfo.m_oDistProfileID;
		PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Distribution;

		CPlatformBuilder.BuildiOS(a_eType, new BuildPlayerOptions());
	}

	/** iOS 플랫폼을 빌드한다 */
	private static void BuildiOS(EiOSPlatformType a_eType, BuildPlayerOptions a_oPlayerOpts)
	{
		CPlatformBuilder.iOSPlatformType = a_eType;

		// 아키텍처를 설정한다 {
		PlayerSettings.iOS.appleDeveloperTeamID = "A9997B8HR5";
		PlayerSettings.iOS.targetOSVersionString = CPlatformOptsSetter.BuildInfoTable.iOSBuildInfo.m_oMinTargetOSVer;

		PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, (int)AppleMobileArchitecture.ARM64);
		// 아키텍처를 설정한다 }

		// 빌드 옵션을 설정한다
		a_oPlayerOpts.target = BuildTarget.iOS;
		a_oPlayerOpts.targetGroup = BuildTargetGroup.iOS;
		a_oPlayerOpts.locationPathName = string.Format("Builds/iOS/{0}", CPlatformBuilder.GetiOSPlatform(a_eType));

		// iOS 옵션을 설정한다
		PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
		PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
		
		// 플랫폼을 빌드한다
		CPlatformBuilder.BuildPlatform(a_oPlayerOpts);
	}

	/** iOS 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildiOSDebug(EiOSPlatformType a_eType)
	{
		CPlatformBuilder.ExecuteiOSJenkinsBuild(a_eType, CPlatformBuilder.B_DEBUG_BUILD_FUNC_JENKINS, CPlatformBuilder.B_IOS_DEBUG_PIPELINE_N_JENKINS, "ipa", CPlatformOptsSetter.BuildInfoTable.iOSBuildInfo.m_oDevProfileID, "development");
	}

	/** iOS 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildiOSRelease(EiOSPlatformType a_eType)
	{
		CPlatformBuilder.ExecuteiOSJenkinsBuild(a_eType, CPlatformBuilder.B_RELEASE_BUILD_FUNC_JENKINS, CPlatformBuilder.B_IOS_RELEASE_PIPELINE_N_JENKINS, "ipa", CPlatformOptsSetter.BuildInfoTable.iOSBuildInfo.m_oDevProfileID, "development");
	}

	/** iOS 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildiOSStoreA(EiOSPlatformType a_eType)
	{
		CPlatformBuilder.ExecuteiOSJenkinsBuild(a_eType, CPlatformBuilder.B_STORE_A_BUILD_FUNC_JENKINS, CPlatformBuilder.B_IOS_STORE_PIPELINE_N_JENKINS, "ipa", CPlatformOptsSetter.BuildInfoTable.iOSBuildInfo.m_oDistProfileID, "app-store");
	}

	/** iOS 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildiOSStoreDist(EiOSPlatformType a_eType)
	{
		CPlatformBuilder.ExecuteiOSJenkinsBuild(a_eType, CPlatformBuilder.B_STORE_DIST_BUILD_FUNC_JENKINS, CPlatformBuilder.B_IOS_STORE_PIPELINE_N_JENKINS, "ipa", CPlatformOptsSetter.BuildInfoTable.iOSBuildInfo.m_oDistProfileID, "app-store");
	}
	#endregion // 클래스 함수

	#region 클래스 접근 함수
	/** iOS 프로젝트 이름을 반환한다 */
	private static string GetiOSProjName(EiOSPlatformType a_eType)
	{
		return "01.iOSApple";
	}

	/** iOS 플랫폼을 반환한다 */
	private static string GetiOSPlatform(EiOSPlatformType a_eType)
	{
		switch (a_eType)
		{
			// Do Something
		}

		return CPlatformBuilder.B_PLATFORM_IOS_APPLE;
	}

	/** iOS 빌드 결과 경로를 반환한다 */
	private static string GetiOSBuildOutputPath(EiOSPlatformType a_eType, string a_oBuildFileExtension)
	{
		return string.Format("Builds/iOS/{0}/BuildOutput/Export/{0}BuildOutput.{1}", CPlatformBuilder.GetiOSPlatform(a_eType), a_oBuildFileExtension);
	}
	#endregion // 클래스 접근 함수
}

/** 플랫폼 빌더 - iOS 애플 */
public static partial class CPlatformBuilder
{
	#region 클래스 함수
	/** iOS 애플 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/iOS/Apple/Debug", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void BuildiOSAppleDebug()
	{
		CPlatformBuilder.BuildiOSDebug(EiOSPlatformType.APPLE, false, false);
	}

	/** iOS 애플 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/iOS/Apple/Debug with Auto Play", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void BuildiOSAppleDebugWithAutoPlay()
	{
		CPlatformBuilder.BuildiOSDebugWithAutoPlay(EiOSPlatformType.APPLE);
	}

	/** iOS 애플 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/iOS/Apple/Debug with Profiler", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void BuildiOSAppleDebugWithProfiler()
	{
		CPlatformBuilder.BuildiOSDebugWithProfiler(EiOSPlatformType.APPLE);
	}

	/** iOS 애플 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/iOS/Apple/Release", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 201)]
	public static void BuildiOSAppleRelease()
	{
		CPlatformBuilder.BuildiOSRelease(EiOSPlatformType.APPLE, false);
	}

	/** iOS 애플 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/iOS/Apple/Release with Auto Play", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 201)]
	public static void BuildiOSAppleReleaseWithAutoPlay()
	{
		CPlatformBuilder.BuildiOSReleaseWithAutoPlay(EiOSPlatformType.APPLE);
	}

	/** iOS 애플 플랫폼을 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/iOS/Apple/Distribution", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 301)]
	public static void BuildiOSAppleStoreA()
	{
		CPlatformBuilder.BuildiOSStoreA(EiOSPlatformType.APPLE);
	}

	/** iOS 애플 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/iOS/Apple/Debug", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 101)]
	public static void RemoteBuildiOSAppleDebug()
	{
		CPlatformBuilder.RemoteBuildiOSDebug(EiOSPlatformType.APPLE);
	}

	/** iOS 애플 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/iOS/Apple/Release", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 201)]
	public static void RemoteBuildiOSAppleRelease()
	{
		CPlatformBuilder.RemoteBuildiOSRelease(EiOSPlatformType.APPLE);
	}

	/** iOS 애플 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/iOS/Apple/Distribution", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 301)]
	public static void RemoteBuildiOSAppleStoreA()
	{
		CPlatformBuilder.RemoteBuildiOSStoreA(EiOSPlatformType.APPLE);
	}

	/** iOS 애플 플랫폼을 원격 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Remote (Jenkins)/iOS/Apple/Distribution (Store)", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 301)]
	public static void RemoteBuildiOSAppleStoreDist()
	{
		CPlatformBuilder.RemoteBuildiOSStoreDist(EiOSPlatformType.APPLE);
	}
	#endregion // 클래스 함수
}
#endif // #if UNITY_EDITOR
