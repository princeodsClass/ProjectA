using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	/** 맵 에디터 페이지 - 프리팹 */
	public partial class PageMapEditor : UIDialog
	{
		/** 프리팹 선택 모드 */
		private enum EPrefabSelMode
		{
			NONE = -1,
			CREATE,
			CHANGE,
			[HideInInspector] MAX_VAL
		}

		/** 프리팹 객체 정보 */
		private struct STPrefabObjInfo
		{
			public int m_nTheme;
			public int m_nGroup;
			public int m_nPrefab;

			public CObjInfo m_oObjInfo;
			public GameObject m_oGameObj;
		}

		/** 프리팹 에디터 정보 */
		private struct STPrefabEditorInfo
		{
			public List<STPrefabInfo> m_oPrefabInfoList;
			public List<List<GameObject>> m_oPrefabListContainer;
		}

		#region 변수
		private EPrefabSelMode m_ePrefabSelMode = EPrefabSelMode.CREATE;
		private List<STPrefabEditorInfo> m_oPrefabEditorInfoList = new List<STPrefabEditorInfo>();
		#endregion // 변수

		#region 함수
		/** 프리팹 버튼을 눌렀을 경우 */
		private void OnTouchREUIsPrefabBtn(CREUIsScrollerCellView a_oSender, int a_nTheme, int a_nGroup, int a_nPrefab)
		{
			switch (m_ePrefabSelMode)
			{
				case EPrefabSelMode.CREATE: this.HandlePrefabSelModeCreate(a_oSender, a_nTheme, a_nGroup, a_nPrefab); break;
				case EPrefabSelMode.CHANGE: this.HandlePrefabSelModeChange(a_oSender, a_nTheme, a_nGroup, a_nPrefab); break;
			}
		}

		/** 프리팹 생성 모드를 처리한다 */
		private void HandlePrefabSelModeCreate(CREUIsScrollerCellView a_oSender, int a_nTheme, int a_nGroup, int a_nPrefab)
		{
			var stPrefabObjInfo = this.CreatePrefabObj(a_nTheme, a_nGroup, a_nPrefab);
			m_oPrefabObjInfoList.Add(stPrefabObjInfo);

			this.AddMapObjInfo(stPrefabObjInfo, Input.GetKeyDown(KeyCode.LeftShift));
		}

		/** 프리팹 변경 모드를 처리한다 */
		private void HandlePrefabSelModeChange(CREUIsScrollerCellView a_oSender, int a_nTheme, int a_nGroup, int a_nPrefab)
		{
			for (int i = 0; i < m_oSelTransGizmosTargetObjIdxList.Count; ++i)
			{
				int nIdx = m_oSelTransGizmosTargetObjIdxList[i];
				var oPrefabEditorInfo = m_oPrefabEditorInfoList[a_nTheme];

				var stPrefabObjInfo = m_oPrefabObjInfoList[nIdx];
				stPrefabObjInfo.m_oObjInfo.m_stPrefabInfo.m_oName = oPrefabEditorInfo.m_oPrefabListContainer[a_nGroup][a_nPrefab].name;
				stPrefabObjInfo.m_oObjInfo.m_stPrefabInfo.m_nTheme = a_nTheme;
				stPrefabObjInfo.m_oObjInfo.m_stPrefabInfo.m_eResType = oPrefabEditorInfo.m_oPrefabInfoList[a_nGroup].m_eResType;

				m_oPrefabObjInfoList[nIdx] = stPrefabObjInfo;
				m_bIsDirtyResetPrefabObject = true;
			}
		}
		#endregion // 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
