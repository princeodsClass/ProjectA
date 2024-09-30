using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;

/** 플랫폼 빌더 - 독립 플랫폼 */
public static partial class CPlatformBuilder
{
	#region 클래스 함수
	/** 독립 플랫폼을 빌드한다 */
	private static void BuildStandaloneDebug(EStandalonePlatformType a_eType, bool a_bIsAutoPlay, bool a_bIsEnableProfiler)
	{
		CPlatformBuilder.BuildMode = EBuildMode.DEBUG;

		// 빌드 옵션을 설정한다
		var oPlayerOpts = new BuildPlayerOptions();
		oPlayerOpts.options = BuildOptions.Development;
		oPlayerOpts.options |= a_bIsAutoPlay ? BuildOptions.AutoRunPlayer : BuildOptions.None;
		oPlayerOpts.options |= a_bIsEnableProfiler ? BuildOptions.ConnectWithProfiler : BuildOptions.None;

		CPlatformBuilder.BuildStandalone(a_eType, oPlayerOpts);
	}

	/** 독립 플랫폼을 빌드한다 */
	private static void BuildStandaloneDebugWithAutoPlay(EStandalonePlatformType a_eType)
	{
		CPlatformBuilder.BuildStandaloneDebug(a_eType, true, false);
	}

	/** 독립 플랫폼을 빌드한다 */
	private static void BuildStandaloneDebugWithProfiler(EStandalonePlatformType a_eType)
	{
		CPlatformBuilder.BuildStandaloneDebug(a_eType, true, true);
	}

	/** 독립 플랫폼을 빌드한다 */
	private static void BuildStandaloneRelease(EStandalonePlatformType a_eType, bool a_bIsAutoPlay)
	{
		CPlatformBuilder.BuildMode = EBuildMode.RELEASE;

		// 빌드 옵션을 설정한다
		var oPlayerOpts = new BuildPlayerOptions();
		oPlayerOpts.options = a_bIsAutoPlay ? BuildOptions.AutoRunPlayer : BuildOptions.None;

		CPlatformBuilder.BuildStandalone(a_eType, oPlayerOpts);
	}

	/** 독립 플랫폼을 빌드한다 */
	private static void BuildStandaloneReleaseWithAutoPlay(EStandalonePlatformType a_eType)
	{
		CPlatformBuilder.BuildStandaloneRelease(a_eType, true);
	}

	/** 독립 플랫픔을 빌드한다 */
	private static void BuildStandaloneStoreA(EStandalonePlatformType a_eType)
	{
		CPlatformBuilder.BuildMode = EBuildMode.STORE;
		CPlatformBuilder.BuildStandalone(a_eType, new BuildPlayerOptions());
	}

	/** 독립 플랫폼을 빌드한다 */
	private static void BuildStandalone(EStandalonePlatformType a_eType, BuildPlayerOptions a_oPlayerOpts)
	{
		CPlatformBuilder.StandalonePlatformType = a_eType;

		// 빌드 옵션을 설정한다 {
		string oPlatform = CPlatformBuilder.GetStandalonePlatform(a_eType);
		string oBuildFileExtension = (a_eType == EStandalonePlatformType.WNDS_STEAM || a_eType == EStandalonePlatformType.WNDS_EDITOR) ? "exe" : "app";

		a_oPlayerOpts.target = (a_eType == EStandalonePlatformType.WNDS_STEAM || a_eType == EStandalonePlatformType.WNDS_EDITOR) ? BuildTarget.StandaloneWindows64 : BuildTarget.StandaloneOSX;
		a_oPlayerOpts.targetGroup = BuildTargetGroup.Standalone;
		a_oPlayerOpts.locationPathName = string.Format("Builds/Standalone/{0}/{1}", oPlatform, string.Format("{0}BuildOutput.{1}", oPlatform, oBuildFileExtension));
		// 빌드 옵션을 설정한다 }
		
		// 플랫폼을 빌드한다
		CPlatformBuilder.BuildPlatform(a_oPlayerOpts);
	}

	/** 독립 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildStandaloneDebug(EStandalonePlatformType a_eType)
	{
		CPlatformBuilder.ExecuteStandaloneJenkinsBuild(a_eType, CPlatformBuilder.B_DEBUG_BUILD_FUNC_JENKINS, CPlatformBuilder.B_STANDALONE_DEBUG_PIPELINE_N_JENKINS, "zip");
	}

	/** 독립 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildStandaloneRelease(EStandalonePlatformType a_eType)
	{
		CPlatformBuilder.ExecuteStandaloneJenkinsBuild(a_eType, CPlatformBuilder.B_RELEASE_BUILD_FUNC_JENKINS, CPlatformBuilder.B_STANDALONE_RELEASE_PIPELINE_N_JENKINS, "zip");
	}

	/** 독립 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildStandaloneStoreA(EStandalonePlatformType a_eType)
	{
		CPlatformBuilder.ExecuteStandaloneJenkinsBuild(a_eType, CPlatformBuilder.B_STORE_A_BUILD_FUNC_JENKINS, CPlatformBuilder.B_STANDALONE_STORE_PIPELINE_N_JENKINS, "zip");
	}

	/** 독립 플랫폼을 원격 빌드한다 */
	private static void RemoteBuildStandaloneStoreDist(EStandalonePlatformType a_eType)
	{
		CPlatformBuilder.ExecuteStandaloneJenkinsBuild(a_eType, CPlatformBuilder.B_STORE_DIST_BUILD_FUNC_JENKINS, CPlatformBuilder.B_STANDALONE_STORE_PIPELINE_N_JENKINS, "zip");
	}
	#endregion // 클래스 함수

	#region 클래스 접근 함수
	/** 독립 플랫폼 프로젝트 이름을 반환한다 */
	public static string GetStandaloneProjName(EStandalonePlatformType a_eType)
	{
		switch (a_eType)
		{
			case EStandalonePlatformType.WNDS_STEAM: case EStandalonePlatformType.WNDS_EDITOR: return "51.StandaloneWndsSteam";
		}

		return "41.StandaloneMacSteam";
	}

	/** 독립 플랫폼을 반환한다 */
	private static string GetStandalonePlatform(EStandalonePlatformType a_eType)
	{
		switch (a_eType)
		{
			case EStandalonePlatformType.MAC_EDITOR: return CPlatformBuilder.B_PLATFORM_STANDALONE_MAC_EDITOR;
			case EStandalonePlatformType.WNDS_STEAM: return CPlatformBuilder.B_PLATFORM_STANDALONE_WNDS_STEAM;
			case EStandalonePlatformType.WNDS_EDITOR: return CPlatformBuilder.B_PLATFORM_STANDALONE_WNDS_EDITOR;
		}

		return CPlatformBuilder.B_PLATFORM_STANDALONE_MAC_STEAM;
	}

	/** 독립 플랫폼 빌드 결과 경로를 반환한다 */
	public static string GetStandaloneBuildOutputPath(EStandalonePlatformType a_eType, string a_oBuildFileExtension)
	{
		return string.Format("Builds/Standalone/{0}/{0}BuildOutput.{1}", CPlatformBuilder.GetStandalonePlatform(a_eType), a_oBuildFileExtension);
	}
	#endregion // 클래스 접근 함수
}

/** 플랫폼 빌더 - 에디터 */
public static partial class CPlatformBuilder
{
	#region 클래스 함수
	/** 맥 에디터를 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Editor/Mac/Debug", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 1)]
	public static void BuildMacEditorDebug()
	{
		CPlatformBuilder.BuildStandaloneDebug(EStandalonePlatformType.MAC_EDITOR, false, false);
	}

	/** 맥 에디터를 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Editor/Mac/Debug with Auto Play", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 1)]
	public static void BuildMacEditorDebugWithAutoPlay()
	{
		CPlatformBuilder.BuildStandaloneDebugWithAutoPlay(EStandalonePlatformType.MAC_EDITOR);
	}

	/** 윈도우즈 에디터를 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Editor/Windows/Debug", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 1)]
	public static void BuildWndsEditorDebug()
	{
		CPlatformBuilder.BuildStandaloneDebug(EStandalonePlatformType.WNDS_EDITOR, false, false);
	}

	/** 윈도우즈 에디터를 빌드한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_BUILD_BASE + "Local/Editor/Windows/Debug with Auto Play", false, CMenuHandler.B_SORTING_O_BUILD_MENU + 1)]
	public static void BuildWndsEditorDebugWithAutoPlay()
	{
		CPlatformBuilder.BuildStandaloneDebugWithAutoPlay(EStandalonePlatformType.WNDS_EDITOR);
	}
	#endregion // 클래스 함수
}
#endif // #if UNITY_EDITOR
