using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/** 플랫폼 옵션 설정자 */
[InitializeOnLoad]
public static partial class CPlatformOptsSetter
{
	#region 상수
	public const string B_ASSET_P_SPRITE_ATLAS = "Assets/Resources/SpriteAtlas/MapEditor.spriteatlas";
	public const string B_ASSET_P_BUILD_INFO_TABLE = "Assets/Editor Default Resources/Scriptables/BuildInfoTable.asset";
	#endregion // 상수

	#region 클래스 변수
	private static CBuildInfoTable m_oBuildInfoTable = null;
	#endregion // 클래스 변수

	#region 클래스 프로퍼티
	public static CBuildInfoTable BuildInfoTable
	{
		get
		{
			// 빌드 정보 테이블이 없을 경우
			if (CPlatformOptsSetter.m_oBuildInfoTable == null)
			{
				CPlatformOptsSetter.m_oBuildInfoTable = AssetDatabase.LoadAssetAtPath<CBuildInfoTable>(CPlatformOptsSetter.B_ASSET_P_BUILD_INFO_TABLE);
			}

			return CPlatformOptsSetter.m_oBuildInfoTable;
		}
	}
	#endregion // 클래스 프로퍼티

	#region 클래스 함수
	/** 생성자 */
	static CPlatformOptsSetter()
	{
		// 플레이 모드가 아닐 경우
		if (!EditorApplication.isPlaying)
		{
			CPlatformOptsSetter.EditorInitialize();
		}
	}

	/** 초기화 */
	public static void EditorInitialize()
	{
		// Do Something		
	}
	#endregion // 클래스 함수
}
#endif // #if UNITY_EDITOR
