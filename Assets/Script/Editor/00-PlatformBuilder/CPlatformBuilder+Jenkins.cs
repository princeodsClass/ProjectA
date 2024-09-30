using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;

/** 플랫폼 빌더 - 젠킨스 */
public static partial class CPlatformBuilder
{
	#region 상수
	public const string B_KEY_JENKINS_PROJ_ROOT = "ProjRoot";
	public const string B_KEY_JENKINS_MODULE_VER = "ModuleVer";
	public const string B_KEY_JENKINS_BRANCH = "Branch";
	public const string B_KEY_JENKINS_SRC = "Src";
	public const string B_KEY_JENKINS_ANALYTICS_SRC = "AnalyticsSrc";
	public const string B_KEY_JENKINS_PROJ_NAME = "ProjName";
	public const string B_KEY_JENKINS_PROJ_PATH = "ProjPath";
	public const string B_KEY_JENKINS_BUILD_OUTPUT_PATH = "BuildOutputPath";
	public const string B_KEY_JENKINS_BUNDLE_ID = "BundleID";
	public const string B_KEY_JENKINS_PROFILE_ID = "ProfileID";
	public const string B_KEY_JENKINS_PLATFORM = "Platform";
	public const string B_KEY_JENKINS_PROJ_PLATFORM = "ProjPlatform";
	public const string B_KEY_JENKINS_BUILD_VER = "BuildVer";
	public const string B_KEY_JENKINS_BUILD_FUNC = "BuildFunc";
	public const string B_KEY_JENKINS_PIPELINE_NAME = "PipelineName";
	public const string B_KEY_JENKINS_IPA_EXPORT_METHOD = "IPAExportMethod";
	public const string B_KEY_JENKINS_BUILD_FILE_EXTENSION = "BuildFileExtension";

	public const string B_DEBUG_BUILD_FUNC_JENKINS = "Debug";
	public const string B_RELEASE_BUILD_FUNC_JENKINS = "Release";
	public const string B_STORE_A_BUILD_FUNC_JENKINS = "StoreA";
	public const string B_STORE_B_BUILD_FUNC_JENKINS = "StoreB";
	public const string B_STORE_DIST_BUILD_FUNC_JENKINS = "StoreDist";

	public const string B_IOS_DEBUG_PIPELINE_N_JENKINS = "01.iOSDebug";
	public const string B_IOS_RELEASE_PIPELINE_N_JENKINS = "02.iOSRelease";
	public const string B_IOS_STORE_PIPELINE_N_JENKINS = "03.iOSStore";

	public const string B_ANDROID_DEBUG_PIPELINE_N_JENKINS = "11.AndroidDebug";
	public const string B_ANDROID_RELEASE_PIPELINE_N_JENKINS = "12.AndroidRelease";
	public const string B_ANDROID_STORE_PIPELINE_N_JENKINS = "13.AndroidStore";

	public const string B_STANDALONE_DEBUG_PIPELINE_N_JENKINS = "41.StandaloneDebug";
	public const string B_STANDALONE_RELEASE_PIPELINE_N_JENKINS = "42.StandaloneRelease";
	public const string B_STANDALONE_STORE_PIPELINE_N_JENKINS = "43.StandaloneStore";

	public const string B_URL_FMT_JENKINS = "http://59.11.2.140:18081/{0}/buildWithParameters";
	public const string B_BUILD_CMD_FMT_JENKINS = "curl -X POST {0} --user {1}:{2} --data token={3}";
	public const string B_PIPELINE_GROUP_NAME_FMT_JENKINS = "job/000000.Common/job/{0}/job/01.Pipelines/job";

	public static readonly string B_JENKINS_IOS_PIPELINE = string.Format($"{CPlatformBuilder.B_PIPELINE_GROUP_NAME_FMT_JENKINS}/01.iOS", "0.0.1");
	public static readonly string B_JENKINS_ANDROID_PIPELINE = string.Format($"{CPlatformBuilder.B_PIPELINE_GROUP_NAME_FMT_JENKINS}/11.Android", "0.0.1");
	public static readonly string B_JENKINS_STANDALONE_PIPELINE = string.Format($"{CPlatformBuilder.B_PIPELINE_GROUP_NAME_FMT_JENKINS}/41.Standalone", "0.0.1");

	public static readonly Dictionary<EiOSPlatformType, Dictionary<string, string>> B_JENKINS_IOS_SOURCES = new Dictionary<EiOSPlatformType, Dictionary<string, string>>()
	{
		[EiOSPlatformType.APPLE] = new Dictionary<string, string>()
		{
			[CPlatformBuilder.B_DEBUG_BUILD_FUNC_JENKINS] = "01.iOSAppleDebug",
			[CPlatformBuilder.B_RELEASE_BUILD_FUNC_JENKINS] = "11.iOSAppleRelease",
			[CPlatformBuilder.B_STORE_A_BUILD_FUNC_JENKINS] = "21.iOSAppleStoreA",
			[CPlatformBuilder.B_STORE_DIST_BUILD_FUNC_JENKINS] = "31.iOSAppleStoreDist"
		}
	};

	public static readonly Dictionary<EAndroidPlatformType, Dictionary<string, string>> B_JENKINS_ANDROID_SOURCES = new Dictionary<EAndroidPlatformType, Dictionary<string, string>>()
	{
		[EAndroidPlatformType.GOOGLE] = new Dictionary<string, string>()
		{
			[CPlatformBuilder.B_DEBUG_BUILD_FUNC_JENKINS] = "01.AndroidGoogleDebug",
			[CPlatformBuilder.B_RELEASE_BUILD_FUNC_JENKINS] = "11.AndroidGoogleRelease",
			[CPlatformBuilder.B_STORE_A_BUILD_FUNC_JENKINS] = "21.AndroidGoogleStoreA",
			[CPlatformBuilder.B_STORE_B_BUILD_FUNC_JENKINS] = "22.AndroidGoogleStoreB",
			[CPlatformBuilder.B_STORE_DIST_BUILD_FUNC_JENKINS] = "31.AndroidGoogleStoreDist"
		},

		[EAndroidPlatformType.AMAZON] = new Dictionary<string, string>()
		{
			[CPlatformBuilder.B_DEBUG_BUILD_FUNC_JENKINS] = "01.AndroidAmazonDebug",
			[CPlatformBuilder.B_RELEASE_BUILD_FUNC_JENKINS] = "11.AndroidAmazonRelease",
			[CPlatformBuilder.B_STORE_A_BUILD_FUNC_JENKINS] = "21.AndroidAmazonStoreA",
			[CPlatformBuilder.B_STORE_B_BUILD_FUNC_JENKINS] = "22.AndroidAmazonStoreB",
			[CPlatformBuilder.B_STORE_DIST_BUILD_FUNC_JENKINS] = "31.AndroidAmazonStoreDist"
		}
	};

	public static readonly Dictionary<EStandalonePlatformType, Dictionary<string, string>> B_JENKINS_STANDALONE_SOURCES = new Dictionary<EStandalonePlatformType, Dictionary<string, string>>()
	{
		[EStandalonePlatformType.MAC_STEAM] = new Dictionary<string, string>()
		{
			[CPlatformBuilder.B_DEBUG_BUILD_FUNC_JENKINS] = "01.StandaloneMacSteamDebug",
			[CPlatformBuilder.B_RELEASE_BUILD_FUNC_JENKINS] = "11.StandaloneMacSteamRelease",
			[CPlatformBuilder.B_STORE_A_BUILD_FUNC_JENKINS] = "21.StandaloneMacSteamStoreA",
			[CPlatformBuilder.B_STORE_DIST_BUILD_FUNC_JENKINS] = "31.StandaloneMacSteamStoreDist"
		},

		[EStandalonePlatformType.WNDS_STEAM] = new Dictionary<string, string>()
		{
			[CPlatformBuilder.B_DEBUG_BUILD_FUNC_JENKINS] = "01.StandaloneWndsSteamDebug",
			[CPlatformBuilder.B_RELEASE_BUILD_FUNC_JENKINS] = "11.StandaloneWndsSteamRelease",
			[CPlatformBuilder.B_STORE_A_BUILD_FUNC_JENKINS] = "21.StandaloneWndsSteamStoreA",
			[CPlatformBuilder.B_STORE_DIST_BUILD_FUNC_JENKINS] = "31.StandaloneWndsSteamStoreDist"
		}
	};
	#endregion // 상수

	#region 클래스 함수
	/** iOS 플랫폼 젠킨스 빌드를 실행한다 */
	public static void ExecuteiOSJenkinsBuild(EiOSPlatformType a_eType, string a_oBuildFunc, string a_oPipelineName, string a_oBuildFileExtension, string a_oProfileID, string a_oIPAExportMethod)
	{
		var oDataDict = new Dictionary<string, string>()
		{
			["BundleID"] = Application.identifier,
			["ProfileID"] = a_oProfileID,
			["IPAExportMethod"] = a_oIPAExportMethod
		};

		CPlatformBuilder.ExecuteJenkinsBuild(new STJenkinsParams()
		{
			m_oSrc = string.Format("{0}/{1}/{2}", "200000.Casual/203811.Casual_ProjectA", CPlatformBuilder.GetiOSProjName(a_eType), CPlatformBuilder.B_JENKINS_IOS_SOURCES[a_eType][a_oBuildFunc]),
			m_oPipeline = CPlatformBuilder.B_JENKINS_IOS_PIPELINE,
			m_oProjName = CPlatformBuilder.GetiOSProjName(a_eType),
			m_oBuildOutputPath = CPlatformBuilder.GetiOSBuildOutputPath(a_eType, a_oBuildFileExtension),
			m_oBuildFileExtension = a_oBuildFileExtension,
			m_oPlatform = CPlatformBuilder.GetiOSPlatform(a_eType),
			m_oProjPlatform = "iOS",
			m_oBuildVer = string.Format("{0}_{1}", CPlatformOptsSetter.BuildInfoTable.iOSBuildInfo.m_stVerInfo.m_oVer, CPlatformOptsSetter.BuildInfoTable.iOSBuildInfo.m_stVerInfo.m_nNum),
			m_oBuildFunc = a_oBuildFunc,
			m_oPipelineName = a_oPipelineName,
			m_oDataDict = oDataDict
		});
	}

	/** 안드로이드 플랫폼 젠킨스 빌드를 실행한다 */
	public static void ExecuteAndroidJenkinsBuild(EAndroidPlatformType a_eType, string a_oBuildFunc, string a_oPipelineName, string a_oBuildFileExtension)
	{
		CPlatformBuilder.ExecuteJenkinsBuild(new STJenkinsParams()
		{
			m_oSrc = string.Format("{0}/{1}/{2}", "200000.Casual/203811.Casual_ProjectA", CPlatformBuilder.GetAndroidProjName(a_eType), CPlatformBuilder.B_JENKINS_ANDROID_SOURCES[a_eType][a_oBuildFunc]),
			m_oPipeline = CPlatformBuilder.B_JENKINS_ANDROID_PIPELINE,
			m_oProjName = CPlatformBuilder.GetAndroidProjName(a_eType),
			m_oBuildOutputPath = CPlatformBuilder.GetAndroidBuildOutputPath(a_eType, a_oBuildFileExtension),
			m_oBuildFileExtension = a_oBuildFileExtension,
			m_oPlatform = CPlatformBuilder.GetAndroidPlatform(a_eType),
			m_oProjPlatform = "Android",
			m_oBuildVer = string.Format("{0}_{1}", CPlatformOptsSetter.BuildInfoTable.AndroidBuildInfo.m_stVerInfo.m_oVer, CPlatformOptsSetter.BuildInfoTable.AndroidBuildInfo.m_stVerInfo.m_nNum),
			m_oBuildFunc = a_oBuildFunc,
			m_oPipelineName = a_oPipelineName,
			m_oDataDict = null
		});
	}

	/** 독립 플랫폼 젠킨스 빌드를 실행한다 */
	public static void ExecuteStandaloneJenkinsBuild(EStandalonePlatformType a_eType, string a_oBuildFunc, string a_oPipelineName, string a_oBuildFileExtension)
	{
		CPlatformBuilder.ExecuteJenkinsBuild(new STJenkinsParams()
		{
			m_oSrc = string.Format("{0}/{1}/{2}", "200000.Casual/203811.Casual_ProjectA", CPlatformBuilder.GetStandaloneProjName(a_eType), CPlatformBuilder.B_JENKINS_STANDALONE_SOURCES[a_eType][a_oBuildFunc]),
			m_oPipeline = CPlatformBuilder.B_JENKINS_STANDALONE_PIPELINE,
			m_oProjName = CPlatformBuilder.GetStandaloneProjName(a_eType),
			m_oBuildOutputPath = CPlatformBuilder.GetStandaloneBuildOutputPath(a_eType, a_oBuildFileExtension),
			m_oBuildFileExtension = a_oBuildFileExtension,
			m_oPlatform = CPlatformBuilder.GetStandalonePlatform(a_eType),
			m_oProjPlatform = (a_eType == EStandalonePlatformType.WNDS_STEAM) ? "Win64" : "OSXUniversal",
			m_oBuildVer = string.Format("{0}_{1}", CPlatformOptsSetter.BuildInfoTable.StandaloneBuildInfo.m_stVerInfo.m_oVer, CPlatformOptsSetter.BuildInfoTable.StandaloneBuildInfo.m_stVerInfo.m_nNum),
			m_oBuildFunc = a_oBuildFunc,
			m_oPipelineName = a_oPipelineName,
			m_oDataDict = null
		});
	}

	/** 젠킨스 빌드를 실행한다 */
	private static void ExecuteJenkinsBuild(STJenkinsParams a_stParams)
	{
		string oURL = string.Format(CPlatformBuilder.B_URL_FMT_JENKINS, a_stParams.m_oPipeline);
		string oUserID = "9tap";
		string oAccessToken = "111e9bf7eda996ab2ad9620e0dd0e15220";
		string oBuildToken = "JenkinsBuild";
		string oProjRoot = string.Empty;

		// 매개 변수를 설정한다 {
		var oDataDict = a_stParams.m_oDataDict ?? new Dictionary<string, string>();
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_PROJ_ROOT, "Client");
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_MODULE_VER, "0.0.1");
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_BRANCH, string.Format("origin/{0}", "9tap"));
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_SRC, a_stParams.m_oSrc);
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_ANALYTICS_SRC, string.Format("{0}/00.Analytics", "200000.Casual/203811.Casual_ProjectA"));
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_PROJ_NAME, "203811.Casual_ProjectA_Ver02");
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_PROJ_PATH, oProjRoot.ExIsValid() ? string.Format("{0}/{1}/{2}", "/Volumes/LaCie/Workspace/Jenkins/workspace", a_stParams.m_oSrc, oProjRoot) : string.Format("{0}/{1}", "/Volumes/LaCie/Workspace/Jenkins/workspace", a_stParams.m_oSrc));
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_BUILD_OUTPUT_PATH, a_stParams.m_oBuildOutputPath);
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_PLATFORM, a_stParams.m_oPlatform);
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_PROJ_PLATFORM, a_stParams.m_oProjPlatform);
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_BUILD_VER, a_stParams.m_oBuildVer);
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_BUILD_FUNC, a_stParams.m_oBuildFunc);
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_PIPELINE_NAME, a_stParams.m_oPipelineName);
		oDataDict.TryAdd(CPlatformBuilder.B_KEY_JENKINS_BUILD_FILE_EXTENSION, a_stParams.m_oBuildFileExtension);

		var oStrBuilder = new System.Text.StringBuilder();
		oStrBuilder.AppendFormat(CPlatformBuilder.B_BUILD_CMD_FMT_JENKINS, oURL, oUserID, oAccessToken, oBuildToken);

		foreach (var stKeyVal in oDataDict)
		{
			oStrBuilder.AppendFormat(string.Format("{0}{1}", " ", "--data {0}={1}"), stKeyVal.Key, stKeyVal.Value);
		}
		// 매개 변수를 설정한다 }

		ComUtil.ExecuteCmdLine(oStrBuilder.ToString());
	}
	#endregion // 클래스 함수
}
#endif // #if UNITY_EDITOR
