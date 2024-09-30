using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	/** 맵 에디터 페이지 - 팝업 */
	public partial class PageMapEditor : UIDialog
	{
		/** 팝업 */
		private enum EPopup
		{
			NONE = -1,
			INPUT,
			[HideInInspector] MAX_VAL
		}

		/** 입력 팝업 */
		private enum EInputPopup
		{
			NONE = -1,
			NORM,
			STAGE_MOVE,
			MAP_OBJ_TEMPLATE,
			[HideInInspector] MAX_VAL
		}

		#region 변수
		private EPopup m_eCurPopup = EPopup.NONE;
		private EInputPopup m_eInputPopup = EInputPopup.NONE;
		private System.Action<string> m_oInputPopupCallback = null;

		[Header("=====> Popup <=====")]
		[SerializeField] private InputField m_oPUIsInput = null;

		[Header("=====> Popup - Game Objects <=====")]
		[SerializeField] private GameObject m_oInputPopup = null;
		[SerializeField] private List<GameObject> m_oPUIsPopupList = new List<GameObject>();
		#endregion // 변수

		#region 함수
		/** 팝업 UI 상태를 갱신한다 */
		private void UpdatePUIsState()
		{
			// 팝업을 갱신한다 {
			var oInputPopup = m_oInputPopup.GetComponentInChildren<CInputPopup>();
			oInputPopup.VContentsUIs.SetActive(m_eInputPopup != EInputPopup.STAGE_MOVE);
			oInputPopup.HContentsUIs.SetActive(m_eInputPopup == EInputPopup.STAGE_MOVE);

			for (int i = 0; i < m_oPUIsPopupList.Count; ++i)
			{
				m_oPUIsPopupList[i].SetActive(i == (int)m_eCurPopup);
			}
			// 팝업을 갱신한다 }
		}

		/** 팝업을 출력한다 */
		private void ShowPopup(EPopup a_ePopup)
		{
			// 팝업 출력이 가능 할 경우
			if (m_eCurPopup == EPopup.NONE)
			{
				m_eCurPopup = a_ePopup;
				m_bIsDirtyUpdateUIsState = true;
			}
		}

		/** 입력 팝업을 출력한다 */
		private void ShowInputPopup(System.Action<string> a_oCallback)
		{
			m_oPUIsInput.text = string.Empty;
			m_oInputPopupCallback = a_oCallback;

			this.ShowPopup(EPopup.INPUT);
		}

		/** 팝업을 닫는다 */
		private void ClosePopup()
		{
			m_eCurPopup = EPopup.NONE;
			m_eInputPopup = EInputPopup.NONE;

			m_bIsDirtyUpdateUIsState = true;
		}
		#endregion // 함수
	}

	/** 맵 에디터 페이지 - 입력 팝업 */
	public partial class PageMapEditor : UIDialog
	{
		#region 함수
		/** 확인 버튼을 눌렀을 경우 */
		public void OnTouchPUIsOKBtn()
		{
			m_oInputPopupCallback?.Invoke(m_oPUIsInput.text);
			this.OnTouchPUIsCancelBtn();
		}

		/** 취소 버튼을 눌렀을 경우 */
		public void OnTouchPUIsCancelBtn()
		{
			this.ClosePopup();
		}
		#endregion // 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
