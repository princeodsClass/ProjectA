using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	/** 맵 에디터 페이지 - 왼쪽 에디터 UI */
	public partial class PageMapEditor : UIDialog
	{
		#region 변수
		private EMapInfoType m_eSelMapInfoType = EMapInfoType.CAMPAIGN;

		[Header("=====> Left Editor UIs <=====")]
		[SerializeField] private Button m_oLEUIsTutorialBtn = null;
		[SerializeField] private Button m_oLEUIsCampaignBtn = null;
		[SerializeField] private Button m_oLEUIsAbyssBtn = null;
		[SerializeField] private Button m_oLEUIsWaveBtn = null;
		[SerializeField] private Button m_oLEUIsAdventureBtn = null;
		[SerializeField] private Button m_oLEUIsBonusBtn = null;
		[SerializeField] private Button m_oLEUIsInfiniteBtn = null;

		[SerializeField] private InputField m_oLEUIsVerInput = null;

		[SerializeField] private ScrollRect m_oLEUIsEpisodeScrollRect = null;
		[SerializeField] private ScrollRect m_oLEUIsChapterScrollRect = null;

		[Header("=====> Left Editor UIs - Game Objects <=====")]
		[SerializeField] private GameObject m_oABSetBtnUIs = null;
		[SerializeField] private GameObject m_oLEUIsOriginScrollerCellView = null;

		[SerializeField] private GameObject m_oLEUIsEpisodeScrollViewContents = null;
		[SerializeField] private GameObject m_oLEUIsChapterScrollViewContents = null;
		[SerializeField] private GameObject m_oLEUIsStageScrollViewContents = null;
		#endregion // 변수

		#region 함수
		/** 캠페인 버튼을 눌렀을 경우 */
		public void OnTouchCampaignBtn()
		{
			m_eSelMapInfoType = EMapInfoType.CAMPAIGN;
			this.OnTouchMEUIsResetBtn();
		}

		/** 탐험 버튼을 눌렀을 경우 */
		public void OnTouchAdventureBtn()
		{
			m_eSelMapInfoType = EMapInfoType.ADVENTURE;
			this.OnTouchMEUIsResetBtn();
		}

		/** 심연 버튼을 눌렀을 경우 */
		public void OnTouchAbyssBtn()
		{
			m_eSelMapInfoType = EMapInfoType.ABYSS;
			this.OnTouchMEUIsResetBtn();
		}

		/** 웨이브 버튼을 눌렀을 경우 */
		public void OnTouchWaveBtn()
		{
			m_eSelMapInfoType = EMapInfoType.DEFENCE;
			this.OnTouchMEUIsResetBtn();
		}

		/** 보너스 버튼을 눌렀을 경우 */
		public void OnTouchBonusBtn()
		{
			m_eSelMapInfoType = EMapInfoType.BONUS;
			this.OnTouchMEUIsResetBtn();
		}

		/** 무한 버튼을 눌렀을 경우 */
		public void OnTouchInfiniteBtn()
		{
			m_eSelMapInfoType = EMapInfoType.INFINITE;
			this.OnTouchMEUIsResetBtn();
		}

		/** 튜토리얼 버튼을 눌렀을 경우 */
		public void OnTouchTutorialBtn()
		{
			m_eSelMapInfoType = EMapInfoType.TUTORIAL;
			this.OnTouchMEUIsResetBtn();
		}

		/** A 세트 버튼을 눌렀을 경우 */
		public void OnTouchASetBtn()
		{
			// Do Something
		}

		/** B 세트 버튼을 눌렀을 경우 */
		public void OnTouchBSetBtn()
		{
			// Do Something
		}

		/** 에피소드 추가 버튼을 눌렀을 경우 */
		public void OnTouchAddEpisodeBtn()
		{
			this.AddMapInfo(0, 0, CMapInfoTable.Singleton.GetNumEpisodes(m_eSelMapInfoType));
		}

		/** 챕터 추가 버튼을 눌렀을 경우 */
		public void OnTouchAddChapterBtn()
		{
			int nNumChapters = CMapInfoTable.Singleton.GetNumChapters(m_eSelMapInfoType, this.SelMapInfo.m_stIDInfo.m_nEpisodeID);
			this.AddMapInfo(0, nNumChapters, this.SelMapInfo.m_stIDInfo.m_nEpisodeID);
		}

		/** 스테이지 추가 버튼을 눌렀을 경우 */
		public void OnTouchAddStageBtn()
		{
			int nNumStages = CMapInfoTable.Singleton.GetNumStages(m_eSelMapInfoType, this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID);
			this.AddMapInfo(nNumStages, this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID);
		}

		/** UI 를 설정한다 */
		private void SetupLeftEditorUIs()
		{
			// 입력 필드를 설정한다
			m_oLEUIsVerInput.onEndEdit.AddListener(this.OnEndInputLEUIsVer);
		}

		/** UI 상태를 갱신한다 */
		private void UpdateLEUIsState()
		{
			int nVer = this.GetMapVer(m_eSelMapInfoType, this.SelMapInfo.m_stIDInfo.m_nChapterID,
				this.SelMapInfo.m_stIDInfo.m_nEpisodeID);

			this.SetLEUIsVer(nVer, false);

			// 버튼을 갱신한다
			m_oLEUIsTutorialBtn.image.color = (m_eSelMapInfoType == EMapInfoType.TUTORIAL) ? Color.yellow : Color.white;
			m_oLEUIsCampaignBtn.image.color = (m_eSelMapInfoType == EMapInfoType.CAMPAIGN) ? Color.yellow : Color.white;
			m_oLEUIsAbyssBtn.image.color = (m_eSelMapInfoType == EMapInfoType.ABYSS) ? Color.yellow : Color.white;
			m_oLEUIsWaveBtn.image.color = (m_eSelMapInfoType == EMapInfoType.DEFENCE) ? Color.yellow : Color.white;
			m_oLEUIsAdventureBtn.image.color = (m_eSelMapInfoType == EMapInfoType.ADVENTURE) ? Color.yellow : Color.white;
			m_oLEUIsBonusBtn.image.color = (m_eSelMapInfoType == EMapInfoType.BONUS) ? Color.yellow : Color.white;
			m_oLEUIsInfiniteBtn.image.color = (m_eSelMapInfoType == EMapInfoType.INFINITE) ? Color.yellow : Color.white;

			// 스크롤 뷰를 갱신한다
			this.UpdateLEUIsScrollViewState(m_oLEUIsEpisodeScrollViewContents, CMapInfoTable.Singleton.GetNumEpisodes(m_eSelMapInfoType));
			this.UpdateLEUIsScrollViewState(m_oLEUIsChapterScrollViewContents, CMapInfoTable.Singleton.GetNumChapters(m_eSelMapInfoType, this.SelMapInfo.m_stIDInfo.m_nEpisodeID));
		}

		/** 스크롤 뷰 상태를 갱신한다 */
		private void UpdateLEUIsScrollViewState(GameObject a_oScrollViewContents, int a_nNumChildren)
		{
			for (int i = 0; i < Mathf.Max(a_nNumChildren, a_oScrollViewContents.transform.childCount); ++i)
			{
				GameObject oScrollerCellView = null;

				// 스크롤러 셀 뷰가 존재 할 경우
				if (i < a_oScrollViewContents.transform.childCount)
				{
					oScrollerCellView = a_oScrollViewContents.transform.GetChild(i).gameObject;
				}
				else
				{
					oScrollerCellView = GameResourceManager.Singleton.CreateObject(m_oLEUIsOriginScrollerCellView, a_oScrollViewContents.transform, null);
				}

				oScrollerCellView.SetActive(i < a_nNumChildren);
				this.UpdateLEUIsScrollerCellViewState(oScrollerCellView, i, a_nNumChildren);
			}
		}

		/** 스크롤러 셀 뷰 상태를 갱신한다 */
		private void UpdateLEUIsScrollerCellViewState(GameObject a_oScrollerCellView, int a_nIdx, int a_nNumChildren)
		{
			bool bIsEpisode = a_oScrollerCellView.transform.parent.gameObject == m_oLEUIsEpisodeScrollViewContents;
			STIDInfo stIDInfo = bIsEpisode ? new STIDInfo(0, 0, a_nIdx) : new STIDInfo(0, a_nIdx, this.SelMapInfo.m_stIDInfo.m_nEpisodeID);

			int nCompareID = bIsEpisode ? this.SelMapInfo.m_stIDInfo.m_nEpisodeID : this.SelMapInfo.m_stIDInfo.m_nChapterID;

			var oScrollerCellView = a_oScrollerCellView.GetComponent<CLEUIsScrollerCellView>();
			(a_oScrollerCellView.transform as RectTransform).sizeDelta = new Vector2(0.0f, 100.0f);

			// 텍스트를 갱신한다
			oScrollerCellView.NameText.text = string.Format("{0} {1:00}", bIsEpisode ? "에피소드" : "챕터", a_nIdx + 1);

			// 이미지를 갱신한다
			oScrollerCellView.SelBtn.image.color = (a_nIdx == nCompareID) ? Color.white : Color.gray;

			// 버튼을 갱신한다 {
			oScrollerCellView.MoveBtn.interactable = a_nNumChildren > 1;
			oScrollerCellView.RemoveBtn.interactable = a_nNumChildren > 1;

			oScrollerCellView.SelBtn.onClick.RemoveAllListeners();
			oScrollerCellView.CopyBtn.onClick.RemoveAllListeners();
			oScrollerCellView.MoveBtn.onClick.RemoveAllListeners();
			oScrollerCellView.RemoveBtn.onClick.RemoveAllListeners();

			oScrollerCellView.SelBtn.onClick.AddListener(() => this.OnTouchLEUIsScrollerCellView(stIDInfo, bIsEpisode));
			oScrollerCellView.CopyBtn.onClick.AddListener(() => this.CopyMapInfos(a_oScrollerCellView.transform.parent.gameObject, stIDInfo));
			oScrollerCellView.MoveBtn.onClick.AddListener(() => this.MoveMapInfos(a_oScrollerCellView.transform.parent.gameObject, stIDInfo));
			oScrollerCellView.RemoveBtn.onClick.AddListener(() => this.RemoveMapInfos(a_oScrollerCellView.transform.parent.gameObject, stIDInfo));
			// 버튼을 갱신한다 }
		}

		/** 스크롤러 셀 뷰를 눌렀을 경우 */
		private void OnTouchLEUIsScrollerCellView(STIDInfo a_stIDInfo, bool a_bIsEpisode)
		{
			m_bIsDirtyUpdateUIsState = true;
			this.SetSelMapInfo(CMapInfoTable.Singleton.GetMapInfo(m_eSelMapInfoType, a_stIDInfo.m_nStageID, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID));
		}

		/** 버전을 입력했을 경우 */
		private void OnEndInputLEUIsVer(string a_oStr)
		{
			bool bIsValid = int.TryParse(a_oStr, NumberStyles.Any, null, out int nVer);
			this.SetLEUIsVer(bIsValid ? Mathf.Max(1, nVer) : Mathf.Max(1, this.SelMapInfo.m_nVer));
		}
		#endregion // 함수

		#region 접근 함수
		/** 버전을 변경한다 */
		private void SetLEUIsVer(int a_nVer, bool a_bIsUpdateUIsState = true)
		{
			m_bIsDirtyUpdateUIsState = a_bIsUpdateUIsState;
			m_oLEUIsVerInput.SetTextWithoutNotify($"{a_nVer}");

			this.SetMapVer(m_eSelMapInfoType,
				this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID, a_nVer);
		}
		#endregion // 접근 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
