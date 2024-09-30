using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	/** 맵 에디터 페이지 - 기타 */
	public partial class PageMapEditor : UIDialog
	{
		#region 변수
		private bool m_bIsOpenEtcUIs = false;

		[Header("=====> Etc <=====")]
		[SerializeField] private List<GameObject> m_oOpenEtcUIsList = new List<GameObject>();
		[SerializeField] private List<GameObject> m_oCloseEtcUIsList = new List<GameObject>();
		#endregion // 변수

		#region 함수
		/** 옵션 버튼을 눌렀을 경우 */
		public void OnTouchEtcOptsBtn()
		{
			m_bIsOpenEtcUIs = !m_bIsOpenEtcUIs;
			m_bIsDirtyUpdateUIsState = true;
		}

		/** UI 상태를 갱신한다 */
		private void UpdateEtcUIsState()
		{
			for (int i = 0; i < m_oOpenEtcUIsList.Count; ++i)
			{
				m_oOpenEtcUIsList[i].SetActive(m_bIsOpenEtcUIs);
			}

			for (int i = 0; i < m_oCloseEtcUIsList.Count; ++i)
			{
				m_oCloseEtcUIsList[i].SetActive(!m_bIsOpenEtcUIs);
			}
		}
		#endregion // 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
