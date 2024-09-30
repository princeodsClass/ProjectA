using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/** 메뉴 처리자 */
public static partial class CMenuHandler
{
	#region 상수
	public const int B_SORTING_O_BUILD_MENU = 10000;
	public const int B_SORTING_O_SETUP_MENU = 20000;

	public const string B_MENU_TOOLS_BASE = "Tools/Utility/";
	public const string B_MENU_TOOLS_BUILD_BASE = CMenuHandler.B_MENU_TOOLS_BASE + "Build/";
	public const string B_MENU_TOOLS_SETUP_BASE = CMenuHandler.B_MENU_TOOLS_BASE + "Setup/";
	#endregion // 상수

	#region 클래스 함수
	/** 버전을 설정한다 */
	[MenuItem(CMenuHandler.B_MENU_TOOLS_SETUP_BASE + "Build Version", false, CMenuHandler.B_SORTING_O_SETUP_MENU)]
	public static void SetupVersion()
	{
		// 빌드 정보 테이블이 없을 경우
		if (CPlatformOptsSetter.BuildInfoTable == null)
		{
			return;
		}

#if UNITY_IOS
		PlayerSettings.bundleVersion = CPlatformOptsSetter.BuildInfoTable.iOSBuildInfo.m_stVerInfo.m_oVer;
		PlayerSettings.iOS.buildNumber = $"{CPlatformOptsSetter.BuildInfoTable.iOSBuildInfo.m_stVerInfo.m_nNum}";
#elif UNITY_ANDROID
		PlayerSettings.bundleVersion = CPlatformOptsSetter.BuildInfoTable.AndroidBuildInfo.m_stVerInfo.m_oVer;
		PlayerSettings.Android.bundleVersionCode = CPlatformOptsSetter.BuildInfoTable.AndroidBuildInfo.m_stVerInfo.m_nNum;
#else
		PlayerSettings.bundleVersion = CPlatformOptsSetter.BuildInfoTable.StandaloneBuildInfo.m_stVerInfo.m_oVer;
		PlayerSettings.macOS.buildNumber = $"{CPlatformOptsSetter.BuildInfoTable.StandaloneBuildInfo.m_stVerInfo.m_nNum}";
#endif // #if UNITY_IOS
	}
	#endregion // 클래스 함수
}
#endif // #if UNITY_EDITOR
