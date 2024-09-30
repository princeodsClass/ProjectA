using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTG;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	/** 맵 에디터 페이지 - 중앙 에디터 UI */
	public partial class PageMapEditor : UIDialog
	{
		#region 변수
		[Header("=====> Middle Editor UIs <=====")]
		[SerializeField] private Text m_oMEUIsTitleText = null;
		[SerializeField] private Text m_oMEUIsSelPrefabText = null;
		[SerializeField] private Image m_oMEUIsSelPrefabImg = null;

		[SerializeField] private Toggle m_oMEUIsFreeToggle = null;
		[SerializeField] private Toggle m_oMEUIsGridToggle = null;

		[SerializeField] private Toggle m_oMEUIsMoveToggle = null;
		[SerializeField] private Toggle m_oMEUIsScaleToggle = null;
		[SerializeField] private Toggle m_oMEUIsRotateToggle = null;

		[SerializeField] private Toggle m_oMEUIsLocalToggle = null;
		[SerializeField] private Toggle m_oMEUIsGlobalToggle = null;

		[SerializeField] private Toggle m_oMEUIsPivotToggle = null;
		[SerializeField] private Toggle m_oMEUIsCenterToggle = null;

		[SerializeField] private Toggle m_oMEUIsHelpRangeToggle = null;
		[SerializeField] private Toggle m_oMEUIsSighRangeToggle = null;

		[SerializeField] private Button m_oMEUIsUploadBtn = null;
		
		[SerializeField] private Button m_oMEUIsPrevStageBtn = null;
		[SerializeField] private Button m_oMEUIsNextStageBtn = null;

		[SerializeField] private Button m_oMEUIsMoveStageBtn = null;
		[SerializeField] private Button m_oMEUIsRemoveStageBtn = null;

		[Header("=====> Middle Editor UIs - Game Objects <=====")]
		[SerializeField] private GameObject m_oMEUIsSelPrefabInfo = null;
		[SerializeField] private GameObject m_oMEUIsOriginHelpRange = null;
		[SerializeField] private GameObject m_oMEUIsOriginSighRange = null;
		#endregion // 변수

		#region 함수
		/** 이전 스테이지 버튼을 눌렀을 경우 */
		public void OnTouchMEUIsPrevStageBtn()
		{
			// 이전 맵 정보가 존재 할 경우
			if (CMapInfoTable.Singleton.TryGetMapInfo(m_eSelMapInfoType, this.SelMapInfo.m_stIDInfo.m_nStageID - 1, this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID, out CMapInfo oPrevMapInfo))
			{
				this.SetSelMapInfo(oPrevMapInfo);
				m_bIsDirtyUpdateUIsState = true;
			}
		}

		/** 다음 스테이지 버튼을 눌렀을 경우 */
		public void OnTouchMEUIsNextStageBtn()
		{
			// 다음 맵 정보가 존재 할 경우
			if (CMapInfoTable.Singleton.TryGetMapInfo(m_eSelMapInfoType, this.SelMapInfo.m_stIDInfo.m_nStageID + 1, this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID, out CMapInfo oNextMapInfo))
			{
				this.SetSelMapInfo(oNextMapInfo);
				m_bIsDirtyUpdateUIsState = true;
			}
		}

		/** 스테이지 복사 버튼을 눌렀을 경우 */
		public void OnTouchMEUIsCopyStageBtn()
		{
			this.CopyMapInfos(m_oLEUIsStageScrollViewContents, this.SelMapInfo.m_stIDInfo);
		}

		/** 스테이지 이동 버튼을 눌렀을 경우 */
		public void OnTouchMEUIsMoveStageBtn()
		{
			this.MoveMapInfos(m_oLEUIsStageScrollViewContents, this.SelMapInfo.m_stIDInfo);
		}

		/** 스테이지 제거 버튼을 눌렀을 경우 */
		public void OnTouchMEUIsRemoveStageBtn()
		{
			this.RemoveMapInfos(m_oLEUIsStageScrollViewContents, this.SelMapInfo.m_stIDInfo);
		}

		/** 저장 버튼을 눌렀을 경우 */
		public void OnTouchMEUIsSaveBtn()
		{
			m_bIsDirtySaveInfo = true;
		}

		/** 리셋 버튼을 눌렀을 경우 */
		public void OnTouchMEUIsResetBtn()
		{
			m_bIsDirtyResetMapInfo = true;
			m_bIsDirtyResetPrefabObject = true;
		}

		/** 업로드 버튼을 눌렀을 경우 */
		public void OnTouchMEUIsUploadBtn()
		{
			// 튜토리얼 일 경우
			if (m_eSelMapInfoType == EMapInfoType.TUTORIAL)
			{
				return;
			}

			this.IncrMapVersions();
			this.SaveInfo();

			StartCoroutine(this.CoSaveMapVersions());
			m_bIsDirtyUpdateUIsState = true;

#if UNITY_EDITOR
			CMapInfoTable.Singleton.CopyMapInfosDownload();
#endif // #if UNITY_EDITOR
		}

		/** 테스트 버튼을 눌렀을 경우 */
		public void OnTouchMEUIsTestBtn()
		{
			var oChapterMapInfos = CMapInfoTable.Singleton.GetChapterMapInfos(m_eSelMapInfoType, 
				this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID);

#if DISABLE_THIS
			bool bIsInfiniteWave = m_eSelMapInfoType == EMapInfoType.CAMPAIGN && this.SelMapInfo.m_stIDInfo.m_nChapterID == 3;
			bIsInfiniteWave = bIsInfiniteWave || m_eSelMapInfoType == EMapInfoType.DEFENCE;
			bIsInfiniteWave = bIsInfiniteWave || m_eSelMapInfoType == EMapInfoType.INFINITE;
#else
			bool bIsInfiniteWave = m_eSelMapInfoType == EMapInfoType.DEFENCE;
			bIsInfiniteWave = bIsInfiniteWave || m_eSelMapInfoType == EMapInfoType.INFINITE;
#endif // #if DISABLE_THIS

			GameDataManager.Singleton.SetPlayStageID(this.SelMapInfo.m_stIDInfo.m_nStageID);
			GameDataManager.Singleton.SetContinueTimes(0);

			GameDataManager.Singleton.SetupPlayMapInfo(m_eSelMapInfoType, EPlayMode.TEST, oChapterMapInfos, bIsInfiniteWave);
			UIRootMapEditor.SetPrevMainCameraInfo(Camera.main.transform.localPosition, Camera.main.transform.localEulerAngles);

			MenuManager.Singleton.SceneEnd();
			MenuManager.Singleton.SceneNext(ESceneType.Battle);
		}

		/** 나가기 버튼을 눌렀을 경우 */
		public void OnTouchMEUIsLeaveBtn()
		{
			CMapInfoTable.Singleton.SaveMapInfos();
			CMapObjTemplateInfoTable.Singleton.SaveMapObjTemplateInfos();

			MenuManager.Singleton.SceneEnd();
			MenuManager.Singleton.SceneNext(ESceneType.Lobby);
		}

		/** 모드 토글을 눌렀을 경우 */
		public void OnTouchMEUIsModeToggle(bool a_bIsOn)
		{
			this.SetMode(m_oMEUIsFreeToggle.isOn ? EMode.FREE : EMode.GRID);
		}

		/** 타입 토글을 눌렀을 경우 */
		public void OnTouchMEUIsTypeToggle(bool a_bIsOn)
		{
			var eType = m_oMEUIsMoveToggle.isOn ? ETransGizmosType.MOVE : ETransGizmosType.ROTATE;
			this.SetTransGizmosType(m_oMEUIsScaleToggle.isOn ? ETransGizmosType.SCALE : eType);
		}

		/** 공간 토글을 눌렀을 경우 */
		public void OnTouchMEUIsSpaceToggle(bool a_bIsOn)
		{
			this.SetGizmosSpace(m_oMEUIsLocalToggle.isOn ? GizmoSpace.Local : GizmoSpace.Global);
		}

		/** 기준 토글을 눌렀을 경우 */
		public void OnTouchMEUIsPivotToggle(bool a_bIsOn)
		{
			this.SetGizmosPivot(m_oMEUIsPivotToggle.isOn ? GizmoObjectTransformPivot.ObjectMeshPivot : GizmoObjectTransformPivot.ObjectCenterPivot);
		}

		/** 범위 토글을 눌렀을 경우 */
		public void OnTouchMEUIsRangeToggle(bool a_bIsOn)
		{
			m_bIsEnableHelpRangeInfo = m_oMEUIsHelpRangeToggle.isOn;
			m_bIsEnableSighRangeInfo = m_oMEUIsSighRangeToggle.isOn;

			m_bIsDirtyUpdateUIsState = true;
		}

		/** UI 를 설정한다 */
		private void SetupMidEditorUIs()
		{
			// 토글을 설정한다 {
			m_oMEUIsFreeToggle.onValueChanged.AddListener(this.OnTouchMEUIsModeToggle);
			m_oMEUIsGridToggle.onValueChanged.AddListener(this.OnTouchMEUIsModeToggle);

			m_oMEUIsMoveToggle.onValueChanged.AddListener(this.OnTouchMEUIsTypeToggle);
			m_oMEUIsScaleToggle.onValueChanged.AddListener(this.OnTouchMEUIsTypeToggle);
			m_oMEUIsRotateToggle.onValueChanged.AddListener(this.OnTouchMEUIsTypeToggle);

			m_oMEUIsLocalToggle.onValueChanged.AddListener(this.OnTouchMEUIsSpaceToggle);
			m_oMEUIsGlobalToggle.onValueChanged.AddListener(this.OnTouchMEUIsSpaceToggle);

			m_oMEUIsPivotToggle.onValueChanged.AddListener(this.OnTouchMEUIsPivotToggle);
			m_oMEUIsCenterToggle.onValueChanged.AddListener(this.OnTouchMEUIsPivotToggle);

			m_oMEUIsHelpRangeToggle.onValueChanged.AddListener(this.OnTouchMEUIsRangeToggle);
			m_oMEUIsSighRangeToggle.onValueChanged.AddListener(this.OnTouchMEUIsRangeToggle);
			// 토글을 설정한다 }
		}

		/** UI 상태를 갱신한다 */
		private void UpdateMEUIsState()
		{
			int nNumStages = CMapInfoTable.Singleton.GetNumStages(m_eSelMapInfoType, this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID);

			bool bIsValid01 = CMapInfoTable.Singleton.TryGetMapInfo(m_eSelMapInfoType, this.SelMapInfo.m_stIDInfo.m_nStageID - 1, this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID, out CMapInfo oPrevMapInfo);
			bool bIsValid02 = CMapInfoTable.Singleton.TryGetMapInfo(m_eSelMapInfoType, this.SelMapInfo.m_stIDInfo.m_nStageID + 1, this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID, out CMapInfo oNextMapInfo);

			// 텍스트를 갱신한다
			m_oMEUIsTitleText.text = string.Format("스테이지 {0:00}", this.SelMapInfo.m_stIDInfo.m_nStageID + 1);

			// 토글을 갱신한다 {
			m_oMEUIsFreeToggle.SetIsOnWithoutNotify(m_eSelMode == EMode.FREE);
			m_oMEUIsGridToggle.SetIsOnWithoutNotify(m_eSelMode == EMode.GRID);

			m_oMEUIsMoveToggle.SetIsOnWithoutNotify(m_eSelTransGizmosType == ETransGizmosType.MOVE);
			m_oMEUIsScaleToggle.SetIsOnWithoutNotify(m_eSelTransGizmosType == ETransGizmosType.SCALE);
			m_oMEUIsRotateToggle.SetIsOnWithoutNotify(m_eSelTransGizmosType == ETransGizmosType.ROTATE);

			m_oMEUIsLocalToggle.SetIsOnWithoutNotify(m_eSelGizmosSpace == GizmoSpace.Local);
			m_oMEUIsGlobalToggle.SetIsOnWithoutNotify(m_eSelGizmosSpace == GizmoSpace.Global);

			m_oMEUIsPivotToggle.SetIsOnWithoutNotify(m_eSelGizmosPivot == GizmoObjectTransformPivot.ObjectMeshPivot);
			m_oMEUIsCenterToggle.SetIsOnWithoutNotify(m_eSelGizmosPivot == GizmoObjectTransformPivot.ObjectCenterPivot);

			m_oMEUIsHelpRangeToggle.SetIsOnWithoutNotify(m_bIsEnableHelpRangeInfo);
			m_oMEUIsSighRangeToggle.SetIsOnWithoutNotify(m_bIsEnableSighRangeInfo);
			// 토글을 갱신한다 }

			// 버튼을 갱신한다 {
			m_oMEUIsPrevStageBtn.interactable = bIsValid01;
			m_oMEUIsNextStageBtn.interactable = bIsValid02;

			m_oMEUIsMoveStageBtn.interactable = nNumStages > 1;
			m_oMEUIsRemoveStageBtn.interactable = nNumStages > 1;
			// 버튼을 갱신한다 }
		}
		#endregion // 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
