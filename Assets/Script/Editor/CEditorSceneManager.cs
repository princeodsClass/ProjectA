using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/** 에디터 씬 관리자 */
[InitializeOnLoad]
public static partial class CEditorSceneManager
{
	#region 클래스 함수
	/** 생성자 */
	static CEditorSceneManager()
	{
		EditorApplication.update -= CEditorSceneManager.Update;
		EditorApplication.update += CEditorSceneManager.Update;
	}

	/** 상태를 갱신한다 */
	private static void Update()
	{
		// 플레이 모드 일 경우
		if(EditorApplication.isPlaying)
		{
			return;
		}

		Physics.gravity = new Vector3(0.0f, -29.43f, 0.0f);
	}
	#endregion // 클래스 함수
}
#endif // #if UNITY_EDITOR
