using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	/** 최상단 맵 에디터 UI */
	public partial class UIRootMapEditor : UIRootBase
	{
		#region 클래스 변수
		private static Vector3 m_stPrevMainCameraPos = new Vector3(0.0f, 14.5f, -31.5f);
		private static Vector3 m_stPrevMainCameraRotate = new Vector3(25.0f, 0.0f, 0.0f);
		#endregion // 클래스 변수

		#region 프로퍼티
		public override ESceneType SceneType => ESceneType.MapEditor;
		#endregion // 프로퍼티

		#region 함수
		/** 초기화 */
		protected override void Initialize()
		{
			base.Initialize();

			CMapInfoTable.Singleton.LoadMapInfos();
			CMapObjTemplateInfoTable.Singleton.LoadMapObjTemplateInfos();

			Camera.main.transform.localPosition = UIRootMapEditor.m_stPrevMainCameraPos;
			Camera.main.transform.localEulerAngles = UIRootMapEditor.m_stPrevMainCameraRotate;

			m_MenuMgr.OpenPage<PageMapEditor>(EUIPage.PageMapEditor);
		}
		#endregion // 함수

		#region 클래스 함수
		/** 이전 메인 카메라 정보를 설정한다 */
		public static void SetPrevMainCameraInfo(Vector3 a_stPos, Vector3 a_stRotate)
		{
			UIRootMapEditor.m_stPrevMainCameraPos = a_stPos;
			UIRootMapEditor.m_stPrevMainCameraRotate = a_stRotate;
		}
		#endregion // 클래스 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
