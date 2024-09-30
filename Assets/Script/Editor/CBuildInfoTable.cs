using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/** 빌드 버전 정보 */
[System.Serializable]
public struct STBuildVerInfo
{
	public int m_nNum;
	public string m_oVer;
}

/** iOS 빌드 정보 */
[System.Serializable]
public struct STiOSBuildInfo
{
	public string m_oDevProfileID;
	public string m_oDistProfileID;
	public string m_oMinTargetOSVer;
	
	public STBuildVerInfo m_stVerInfo;
}

/** 안드로이드 빌드 정보 */
[System.Serializable]
public struct STAndroidBuildInfo
{
	public string m_oKeystoreName;
	public string m_oKeyaliasName;

	public string m_oKeystorePassword;
	public string m_oKeyaliasPassword;

	public STBuildVerInfo m_stVerInfo;
}

/** 독립 플랫폼 빌드 정보 */
[System.Serializable]
public struct STStandaloneBuildInfo
{
	public STBuildVerInfo m_stVerInfo;
}

/** 빌드 정보 테이블 */
[CreateAssetMenu(menuName = "Utility/BuildInfoTable", fileName = "BuildInfoTable", order = 1)]
public class CBuildInfoTable : ScriptableObject
{
	#region 변수
	[SerializeField] private string m_oAppID;
	[SerializeField] private string m_oAppName;

	[SerializeField] private STiOSBuildInfo m_stiOSBuildInfo;
	[SerializeField] private STAndroidBuildInfo m_stAndroidBuildInfo;
	[SerializeField] private STStandaloneBuildInfo m_stStandaloneBuildInfo;
	#endregion // 변수

	#region 프로퍼티
	public string AppID => m_oAppID;
	public string AppName => m_oAppName;

	public STiOSBuildInfo iOSBuildInfo => m_stiOSBuildInfo;
	public STAndroidBuildInfo AndroidBuildInfo => m_stAndroidBuildInfo;
	public STStandaloneBuildInfo StandaloneBuildInfo => m_stStandaloneBuildInfo;
	#endregion // 프로퍼티
}
#endif // #if UNITY_EDITOR
