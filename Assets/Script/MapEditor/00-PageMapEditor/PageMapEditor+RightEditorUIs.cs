using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RTG;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	/** 맵 에디터 페이지 - 오른쪽 에디터 UI */
	public partial class PageMapEditor : UIDialog
	{
		/** 벡터 입력 UI */
		[System.Serializable]
		private struct STVec3InputUIs
		{
			public InputField m_oXInput;
			public InputField m_oYInput;
			public InputField m_oZInput;
		}

		/** 슬라이더 입력 UI */
		[System.Serializable]
		private struct STSliderInputUIs
		{
			public Slider m_oSlider;
			public InputField m_oInput;
		}

		/** 스크롤 뷰 UI */
		[System.Serializable]
		private struct STScrollViewUIs
		{
			public List<Button> m_oTapBtnList;
			public List<ScrollRect> m_oScrollRectList;
		}

		#region 변수
		private int m_nNumPrefabsOnRow = 3;
		private int m_nREUIsSelEditorInputIdx = -1;
		private int m_nREUIsSelInspectorInputIdx = -1;

		private int m_nREUIsSelThemeTabBtnIdx = 0;
		private int m_nREUIsSelPrefabTabBtnIdx = 0;

		private List<InputField> m_oREUIsInputList = new List<InputField>();
		private List<InputField> m_oREUIsEditorInputList = new List<InputField>();
		private List<InputField> m_oREUIsInspectorInputList = new List<InputField>();
		private List<UIBehaviour> m_oREUIsBehaviourList = new List<UIBehaviour>();

		[Header("=====> Right Editor UIs <=====")]
		[SerializeField] private STVec3InputUIs m_stREUIsPosInputUIs;
		[SerializeField] private STVec3InputUIs m_stREUIsScaleInputUIs;
		[SerializeField] private STVec3InputUIs m_stREUIsRotateInputUIs;

		[SerializeField] private Text m_oREUIsTitleText = null;
		[SerializeField] private InputField m_oREUIsStageInput = null;
		[SerializeField] private List<Button> m_oREUIsThemeTapBtnList = new List<Button>();
		[SerializeField] private List<STScrollViewUIs> m_oREUIsScrollViewUIsList = new List<STScrollViewUIs>();

		[Header("=====> Right Editor UIs - AI <=====")]
		[SerializeField] private Toggle m_oREUIsPatrolIdleToggle = null;
		[SerializeField] private Toggle m_oREUIsPatrolWayPointToggle = null;
		[SerializeField] private Toggle m_oREUIsPatrolLookAroundToggle = null;
		[SerializeField] private Toggle m_oREUIsPatrolFixToggle = null;

		[SerializeField] private Toggle m_oREUIsWayPointPassToggle = null;
		[SerializeField] private Toggle m_oREUIsWayPointLookAroundToggle = null;

		[SerializeField] private Toggle m_oREUIsReturnRotate01Toggle = null;
		[SerializeField] private Toggle m_oREUIsReturnRotate02Toggle = null;

		[SerializeField] private Button m_oREUIsAddWayPointBtn = null;
		[SerializeField] private Button m_oREUIsAddStartPointBtn = null;

		[SerializeField] private InputField m_oREUIsPreDelayInput = null;
		[SerializeField] private InputField m_oREUIsPostDelayInput = null;

		[SerializeField] private InputField m_oREUIsDuration01Input = null;
		[SerializeField] private InputField m_oREUIsDuration02Input = null;

		[SerializeField] private InputField m_oREUIsRotateAngle01Input = null;
		[SerializeField] private InputField m_oREUIsRotateAngle02Input = null;

		[Header("=====> Right Editor UIs - Random <=====")]
		[SerializeField] private Toggle m_oREUIsRandomExcludeToggle = null;

		[Header("=====> Right Editor UIs - Game Objects <=====")]
		[SerializeField] private GameObject m_oREUIsEditorUIs = null;
		[SerializeField] private GameObject m_oREUIsInspectorlUIs = null;
		[SerializeField] private GameObject m_oREUIsHierarchyScrollViewContents = null;
		[SerializeField] private GameObject m_oREUIsMapObjTemplateScrollViewContents = null;

		[SerializeField] private GameObject m_oREUIsOriginScrollerCellView = null;
		[SerializeField] private GameObject m_oREUIsOriginHierarchyScrollerCellView = null;
		[SerializeField] private GameObject m_oREUIsOriginMapObjTemplateScrollerCellView = null;
		#endregion // 변수

		#region 함수
		/** 트랜스 폼 리셋 버튼을 눌렀을 경우 */
		public void OnTouchREUIsResetTransBtn()
		{
			for (int i = 0; i < m_oSelTransGizmosTargetObjIdxList.Count; ++i)
			{
				int nIdx = m_oSelTransGizmosTargetObjIdxList[i];

				// 광원 객체 일 경우
				if (m_oPrefabObjInfoList[nIdx].m_oGameObj == m_oLight.gameObject)
				{
					m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localPosition = m_stDefLightTransInfo.m_stPos;
					m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localScale = m_stDefLightTransInfo.m_stScale;
					m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localEulerAngles = m_stDefLightTransInfo.m_stRotate;
				}
				else
				{
					int nTheme = m_oPrefabObjInfoList[nIdx].m_nTheme;
					int nGroup = m_oPrefabObjInfoList[nIdx].m_nGroup;
					int nPrefab = m_oPrefabObjInfoList[nIdx].m_nPrefab;

					var oPrefabEditorInfo = m_oPrefabEditorInfoList[nTheme];

					m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localPosition = Vector3.zero;
					m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localScale = oPrefabEditorInfo.m_oPrefabListContainer[nGroup][nPrefab].transform.localScale;
					m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localEulerAngles = oPrefabEditorInfo.m_oPrefabListContainer[nGroup][nPrefab].transform.localEulerAngles;
				}
			}

			this.UpdateGizmosTargetObjTrans();
		}

		/** 맵 객체 템플릿 추가 버튼을 눌렀을 경우 */
		public void OnTouchREUIsAddMapObjTemplateBtn()
		{
			// 선택 된 객체가 없을 경우
			if(!m_oSelTransGizmosTargetObjIdxList.ExIsValid())
			{
				return;
			}

			m_eInputPopup = EInputPopup.MAP_OBJ_TEMPLATE;

			this.ShowInputPopup((a_oInput) =>
			{
				var oMapObjTemplateInfo = new CMapObjTemplateInfo();
				oMapObjTemplateInfo.m_oName = a_oInput;

				for(int i = 0; i < m_oSelTransGizmosTargetObjIdxList.Count; ++i)
				{
					int nIdx = m_oSelTransGizmosTargetObjIdxList[i];
					oMapObjTemplateInfo.m_oMapObjInfoList.Add(m_oPrefabObjInfoList[nIdx].m_oObjInfo.Clone() as CObjInfo);
				}

				m_bIsDirtyUpdateUIsState = true;
				this.AddTransGizmosTargetObj(null, true);

				CMapObjTemplateInfoTable.Singleton.AddMapObjTemplateInfo(oMapObjTemplateInfo);
				CMapObjTemplateInfoTable.Singleton.SaveMapObjTemplateInfos();
			});
		}

		/** 이동 지점 추가 버튼을 눌렀을 경우 */
		public void OnTouchREUIsAddWayPointBtn()
		{
			var stGroup = this.FindPrefabGroup("BG_WayPoint");

			// 이동 지점 추가가 불가능 할 경우
			if (!this.IsEnableAddWayPoint() || !stGroup.ExIsValidIdx())
			{
				return;
			}

			int nIdx = m_oSelTransGizmosTargetObjIdxList[0];
			this.AddTransGizmosTargetObj(null, true);

			var stPrefabObjInfo = this.CreateWayPointPrefabObj(stGroup.Item1, stGroup.Item2, stGroup.Item3);
			stPrefabObjInfo.m_oGameObj.transform.localPosition = m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localPosition;

			m_oPrefabObjInfoList.Add(stPrefabObjInfo);
			this.AddMapObjInfo(stPrefabObjInfo, Input.GetKeyDown(KeyCode.LeftShift), m_oPrefabObjInfoList[nIdx].m_oObjInfo);
		}

		/** 시작 지점 추가 버튼을 눌렀을 경우 */
		public void OnTouchREUIsAddStartPointBtn()
		{
			var stGroup = this.FindPrefabGroup("BG_StartPoint");

			// 시작 지점 추가가 불가능 할 경우
			if(!this.IsEnableAddStartPoint() || !stGroup.ExIsValidIdx())
			{
				return;
			}

			int nIdx = m_oSelTransGizmosTargetObjIdxList[0];
			this.AddTransGizmosTargetObj(null, true);

			var stPrefabObjInfo = this.CreateWayPointPrefabObj(stGroup.Item1, stGroup.Item2, stGroup.Item3);
			stPrefabObjInfo.m_oGameObj.transform.localPosition = m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localPosition;

			m_oPrefabObjInfoList.Add(stPrefabObjInfo);
			this.AddMapObjInfo(stPrefabObjInfo, Input.GetKeyDown(KeyCode.LeftShift), m_oPrefabObjInfoList[nIdx].m_oObjInfo);
		}

		/** 제어 값 리셋 버튼을 눌렀을 경우 */
		public void OnTouchREUIsResetControlValuesBtn()
		{
			for (int i = 0; i < m_oSelTransGizmosTargetObjIdxList.Count; ++i)
			{
				int nIdx = m_oSelTransGizmosTargetObjIdxList[i];
				this.SetupDefControlValues(m_oPrefabObjInfoList[nIdx].m_oObjInfo);
			}

			m_bIsDirtyUpdateUIsState = true;
		}

		/** 순찰 토글을 눌렀을 경우 */
		public void OnTouchREUIsPatrolToggle(bool a_bIsOn)
		{
			var ePatrolType = m_oREUIsPatrolIdleToggle.isOn ? EPatrolType.IDLE : EPatrolType.FIX;
			ePatrolType = m_oREUIsPatrolWayPointToggle.isOn ? EPatrolType.WAY_POINT : ePatrolType;

			this.SetPatrolType(m_oREUIsPatrolLookAroundToggle.isOn ? EPatrolType.LOOK_AROUND : ePatrolType);
		}

		/** 이동 지점 토글을 눌렀을 경우 */
		public void OnTouchREUIsWayPointToggle(bool a_bIsOn)
		{
			this.SetWayPointType(m_oREUIsWayPointPassToggle.isOn ? EWayPointType.PASS : EWayPointType.LOOK_AROUND);
		}

		/** 회전 복귀 여부 토글을 눌렀을 경우 */
		public void OnTouchREUIsReturnRotateToggle(bool a_bIsOn)
		{
			this.SetIsReturnRotate(m_oREUIsReturnRotate01Toggle.isOn, m_oREUIsReturnRotate02Toggle.isOn);
		}

		/** 랜덤 제외 여부 토글을 눌렀을 경우 */
		public void OnTouchREUIsRandomExcludeToggle(bool a_bIsOn)
		{
			// 객체 정보가 존재 할 경우
			if (this.SelObjInfo != null)
			{
				this.SelObjInfo.m_bIsRandExclude = a_bIsOn;
			}

			m_bIsDirtyUpdateUIsState = true;
		}

		/** 객체 복사 버튼을 눌렀을 경우 */
		public void OnTouchREUIsCopyObjBtn()
		{
			var oSelTransGizmosTargetObjIdxList = CCollectionPoolManager.Singleton.SpawnList<int>();

			try
			{
				m_oSelTransGizmosTargetObjIdxList.ExCopyTo(oSelTransGizmosTargetObjIdxList, (a_nIdx) => a_nIdx);
				m_oSelTransGizmosTargetObjIdxList.Clear();

				for (int i = 0; i < oSelTransGizmosTargetObjIdxList.Count; ++i)
				{
					CObjInfo oOwner = null;
					STPrefabObjInfo stPrefabObjInfo;

					int nIdx = oSelTransGizmosTargetObjIdxList[i];

					// 고유 객체 일 경우
					if (this.IsUniqueGameObj(m_oPrefabObjInfoList[nIdx].m_oGameObj))
					{
						continue;
					}

					// 이동 지점 프리팹 객체 일 경우
					if (m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stPrefabInfo.m_oName.Equals("BG_WayPoint"))
					{
						oOwner = (m_oPrefabObjInfoList[nIdx].m_oObjInfo as CWayPointInfo).m_oOwner;
						stPrefabObjInfo = this.CreateWayPointPrefabObj(m_oPrefabObjInfoList[nIdx].m_nTheme, m_oPrefabObjInfoList[nIdx].m_nGroup, m_oPrefabObjInfoList[nIdx].m_nPrefab);
					}
					// 시작 지점 프리팹 객체 일 경우
					else if(m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stPrefabInfo.m_oName.Equals("BG_StartPoint"))
					{
						oOwner = (m_oPrefabObjInfoList[nIdx].m_oObjInfo as CWayPointInfo).m_oOwner;
						stPrefabObjInfo = this.CreateWayPointPrefabObj(m_oPrefabObjInfoList[nIdx].m_nTheme, m_oPrefabObjInfoList[nIdx].m_nGroup, m_oPrefabObjInfoList[nIdx].m_nPrefab);
					}
					else
					{
						stPrefabObjInfo = this.CreatePrefabObj(m_oPrefabObjInfoList[nIdx].m_nTheme, m_oPrefabObjInfoList[nIdx].m_nGroup, m_oPrefabObjInfoList[nIdx].m_nPrefab);
					}

					m_oPrefabObjInfoList[nIdx].m_oObjInfo.SetupCloneInst(stPrefabObjInfo.m_oObjInfo);
					stPrefabObjInfo.m_oObjInfo.m_oWayPointInfoList.Clear();

					stPrefabObjInfo.m_oGameObj.transform.localPosition = m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localPosition;
					stPrefabObjInfo.m_oGameObj.transform.localScale = m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localScale;
					stPrefabObjInfo.m_oGameObj.transform.localEulerAngles = m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localEulerAngles;

					m_oPrefabObjInfoList.Add(stPrefabObjInfo);
					this.AddMapObjInfo(stPrefabObjInfo, false, oOwner);
				}

				m_bIsDirtyUpdateUIsState = true;
			}
			finally
			{
				CCollectionPoolManager.Singleton.DespawnList(oSelTransGizmosTargetObjIdxList);
			}
		}

		/** 객체 변경 버튼을 눌렀을 경우 */
		public void OnTouchREUIsChangeObjBtn()
		{
			int nResult = m_oSelTransGizmosTargetObjIdxList.FindIndex((a_nIdx) =>
				m_oPrefabObjInfoList[a_nIdx].m_oObjInfo is CWayPointInfo);

			// 이동 지점 객체가 존재 할 경우
			if (m_oSelTransGizmosTargetObjIdxList.ExIsValidIdx(nResult))
			{
				return;
			}

			m_ePrefabSelMode = EPrefabSelMode.CHANGE;
			m_bIsDirtyUpdateUIsState = true;
		}

		/** 객체 제거 버튼을 눌렀을 경우 */
		public void OnTouchREUIsRemoveObjBtn()
		{
			var oRemoveObjList = CCollectionPoolManager.Singleton.SpawnList<GameObject>();

			try
			{
				for (int i = 0; i < m_oSelTransGizmosTargetObjIdxList.Count; ++i)
				{
					int nIdx = m_oSelTransGizmosTargetObjIdxList[i];

					// 고유 객체 일 경우
					if (this.IsUniqueGameObj(m_oPrefabObjInfoList[nIdx].m_oGameObj))
					{
						continue;
					}

					oRemoveObjList.ExAddVal(m_oPrefabObjInfoList[nIdx].m_oGameObj);
				}

				for (int i = 0; i < oRemoveObjList.Count; ++i)
				{
					this.RemovePrefabObj(oRemoveObjList[i]);
				}

				this.AddTransGizmosTargetObj(null, true);
			}
			finally
			{
				CCollectionPoolManager.Singleton.DespawnList(oRemoveObjList);
			}
		}

		/** 스테이지 로드 버튼을 눌렀을 경우 */
		public void OnTouchREUIsLoadStageBtn()
		{
			bool bIsValid = int.TryParse(m_oREUIsStageInput.text, NumberStyles.Any, null, out int nStageID);

			// 맵 정보가 존재 할 경우
			if (bIsValid && CMapInfoTable.Singleton.TryGetMapInfo(m_eSelMapInfoType, nStageID - 1, this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID, out CMapInfo oMapInfo))
			{
				this.SetSelMapInfo(oMapInfo);
				m_bIsDirtyUpdateUIsState = true;
			}
		}

		/** 모든 스테이지 제거 버튼을 눌렀을 경우 */
		public void OnTouchREUIsRemoveAllStagesBtn()
		{
			this.ShowInputPopup((a_oInput) =>
			{
				var oTokens = a_oInput.Split("-");

				int nMinID = 0;
				int nMaxID = 0;

				bool bIsValid01 = oTokens.Length >= 2 && int.TryParse(oTokens[0], NumberStyles.Any, null, out nMinID);
				bool bIsValid02 = oTokens.Length >= 2 && int.TryParse(oTokens[1], NumberStyles.Any, null, out nMaxID);

				// 식별자가 유효 할 경우
				if (bIsValid01 && bIsValid02)
				{
					ComUtil.Swap(ref nMinID, ref nMaxID, ESwapType.LESS);
					var stIDInfo = new STIDInfo(nMinID - 1, this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID);

					for (int i = 0; i <= nMaxID - nMinID; ++i)
					{
						// 레벨 정보가 존재 할 경우
						if (CMapInfoTable.Singleton.TryGetMapInfo(m_eSelMapInfoType, stIDInfo.m_nStageID, stIDInfo.m_nChapterID, stIDInfo.m_nEpisodeID, out CMapInfo oMapInfo))
						{
							this.RemoveMapInfos(m_oLEUIsStageScrollViewContents, stIDInfo);
						}
					}
				}
			});
		}

		/** 스테이지 입력을 종료했을 경우 */
		public void OnEndInputREUIsStage(string a_oStr)
		{
			bool bIsValid = int.TryParse(a_oStr, NumberStyles.Any, null, out int nStageID);
			int nNumStages = CMapInfoTable.Singleton.GetNumStages(m_eSelMapInfoType, this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID);

			nStageID = bIsValid ? Mathf.Clamp(nStageID, 0, nNumStages - 1) : this.SelMapInfo.m_stIDInfo.m_nStageID;
			this.SetSelMapInfo(CMapInfoTable.Singleton.GetMapInfo(m_eSelMapInfoType, nStageID, this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID));

			m_oREUIsStageInput.SetTextWithoutNotify($"{nStageID + 1}");
			m_bIsDirtyUpdateUIsState = true;
		}

		/** 기즈모 타겟 트랜스 폼을 입력했을 경우 */
		private void OnInputREUIsGizmosTargetTrans(STVec3InputUIs a_stInputUIs, ETransModifyInputType a_eInputType, string a_oStr)
		{
			ETransModifyType eTransModifyType = ETransModifyType.EMPTY;
			eTransModifyType |= a_stInputUIs.Equals(m_stREUIsPosInputUIs) ? ETransModifyType.POS : ETransModifyType.EMPTY;
			eTransModifyType |= a_stInputUIs.Equals(m_stREUIsScaleInputUIs) ? ETransModifyType.SCALE : ETransModifyType.EMPTY;
			eTransModifyType |= a_stInputUIs.Equals(m_stREUIsRotateInputUIs) ? ETransModifyType.ROTATE : ETransModifyType.EMPTY;

			this.SetSelTransGizmosTargetObjsTrans(
				this.GetVec3(m_stREUIsPosInputUIs, 0.0f), this.GetVec3(m_stREUIsScaleInputUIs, 1.0f), this.GetVec3(m_stREUIsRotateInputUIs, 0.0f), eTransModifyType, a_eInputType);

			m_bIsDirtyUpdateUIsState = true;
		}

		/** 기즈모 타겟 트랜스 폼을 입력을 종료했을 경우 */
		private void OnEndInputREUIsGizmosTargetTrans(STVec3InputUIs a_stInputUIs, ETransModifyInputType a_eInputType, string a_oStr)
		{
			this.UpdateREUIsVec3InputUIs(m_stREUIsPosInputUIs, this.GetVec3(m_stREUIsPosInputUIs, 0.0f));
			this.UpdateREUIsVec3InputUIs(m_stREUIsScaleInputUIs, this.GetVec3(m_stREUIsScaleInputUIs, 1.0f));
			this.UpdateREUIsVec3InputUIs(m_stREUIsRotateInputUIs, this.GetVec3(m_stREUIsRotateInputUIs, 0.0f));

			this.OnInputREUIsGizmosTargetTrans(a_stInputUIs, a_eInputType, a_oStr);
		}

		/** AI 제어 값 입력을 종료했을 경우 */
		public void OnEndInputREUIsAIControlVal(string a_oStr)
		{
			// 객체 정보가 존재 할 경우
			if (this.SelObjInfo != null)
			{
				bool bIsValid01 = float.TryParse(m_oREUIsPreDelayInput.text, NumberStyles.Any, null, out float fPreDelay);
				bool bIsValid02 = float.TryParse(m_oREUIsPostDelayInput.text, NumberStyles.Any, null, out float fPostDelay);

				bool bIsValid03 = float.TryParse(m_oREUIsDuration01Input.text, NumberStyles.Any, null, out float fDuration01);
				bool bIsValid04 = float.TryParse(m_oREUIsDuration02Input.text, NumberStyles.Any, null, out float fDuration02);

				bool bIsValid05 = float.TryParse(m_oREUIsRotateAngle01Input.text, NumberStyles.Any, null, out float fRotateAngle01);
				bool bIsValid06 = float.TryParse(m_oREUIsRotateAngle02Input.text, NumberStyles.Any, null, out float fRotateAngle02);

				this.SelObjInfo.m_fPreDelay = bIsValid01 ? Mathf.Max(0.0f, fPreDelay) : 0.0f;
				this.SelObjInfo.m_fPostDelay = bIsValid02 ? Mathf.Max(0.0f, fPostDelay) : 0.0f;

				this.SelObjInfo.m_fDuration01 = bIsValid03 ? Mathf.Max(0.0f, fDuration01) : 0.0f;
				this.SelObjInfo.m_fDuration02 = bIsValid04 ? Mathf.Max(0.0f, fDuration02) : 0.0f;

				this.SelObjInfo.m_fRotateAngle01 = bIsValid05 ? fRotateAngle01 : 0.0f;
				this.SelObjInfo.m_fRotateAngle02 = bIsValid06 ? fRotateAngle02 : 0.0f;
			}

			m_bIsDirtyUpdateUIsState = true;
		}

		/** UI 를 설정한다 */
		private void SetupRightEditorUIs()
		{
			this.SetupREUIsEditorUIs();
			this.SetupREUIsInspectorUIs();
			this.SetupREUIsHierarchyUIs();

			this.SetupREUIsLightUIs();
			this.SetupREUIsCameraUIs();

			// 버튼을 설정한다 {
			for (int i = 0; i < m_oREUIsThemeTapBtnList.Count; ++i)
			{
				int nIdx = i;
				m_oREUIsThemeTapBtnList[i].onClick.AddListener(() => this.OnTouchREUIsThemeTabBtn(nIdx));
			}

			for (int i = 0; i < m_oREUIsScrollViewUIsList.Count; ++i)
			{
				for (int j = 0; j < m_oREUIsScrollViewUIsList[i].m_oTapBtnList.Count; ++j)
				{
					int nIdx = j;
					m_oREUIsScrollViewUIsList[i].m_oTapBtnList[j].onClick.AddListener(() => this.OnTouchREUIsPrefabTabBtn(nIdx));
				}
			}
			// 버튼을 설정한다 }

			// 입력 필드를 설정한다 {
			this.GetComponentsInChildren<InputField>(true, m_oREUIsInputList);
			EventSystem.current.SetSelectedGameObject(m_oREUIsStageInput.gameObject);

			m_oREUIsEditorInputList.Add(m_oREUIsStageInput);

			m_oREUIsInspectorInputList.Add(m_stREUIsPosInputUIs.m_oXInput);
			m_oREUIsInspectorInputList.Add(m_stREUIsPosInputUIs.m_oYInput);
			m_oREUIsInspectorInputList.Add(m_stREUIsPosInputUIs.m_oZInput);

			m_oREUIsInspectorInputList.Add(m_stREUIsRotateInputUIs.m_oXInput);
			m_oREUIsInspectorInputList.Add(m_stREUIsRotateInputUIs.m_oYInput);
			m_oREUIsInspectorInputList.Add(m_stREUIsRotateInputUIs.m_oZInput);

			m_oREUIsInspectorInputList.Add(m_stREUIsScaleInputUIs.m_oXInput);
			m_oREUIsInspectorInputList.Add(m_stREUIsScaleInputUIs.m_oYInput);
			m_oREUIsInspectorInputList.Add(m_stREUIsScaleInputUIs.m_oZInput);

			m_oREUIsInspectorInputList.Add(m_oREUIsRotateAngle01Input);
			m_oREUIsInspectorInputList.Add(m_oREUIsDuration01Input);

			m_oREUIsInspectorInputList.Add(m_oREUIsRotateAngle02Input);
			m_oREUIsInspectorInputList.Add(m_oREUIsDuration02Input);

			m_oREUIsInspectorInputList.Add(m_oREUIsPreDelayInput);
			m_oREUIsInspectorInputList.Add(m_oREUIsPostDelayInput);
			// 입력 필드를 설정한다 }
		}

		/** 에디터 UI 를 설정한다 */
		private void SetupREUIsEditorUIs()
		{
			// 버튼을 설정한다
			m_oREUIsAddWayPointBtn.onClick.AddListener(this.OnTouchREUIsAddWayPointBtn);
			m_oREUIsAddStartPointBtn.onClick.AddListener(this.OnTouchREUIsAddStartPointBtn);

			// 슬라이더 입력 UI 를 설정한다 {
			m_stRColorSliderInputUIs.m_oInput.onEndEdit.AddListener(this.OnEndInputREUIsLightColor);
			m_stRColorSliderInputUIs.m_oSlider.onValueChanged.AddListener(this.OnChangeREUIsLightColor);

			m_stGColorSliderInputUIs.m_oInput.onEndEdit.AddListener(this.OnEndInputREUIsLightColor);
			m_stGColorSliderInputUIs.m_oSlider.onValueChanged.AddListener(this.OnChangeREUIsLightColor);

			m_stBColorSliderInputUIs.m_oInput.onEndEdit.AddListener(this.OnEndInputREUIsLightColor);
			m_stBColorSliderInputUIs.m_oSlider.onValueChanged.AddListener(this.OnChangeREUIsLightColor);

			m_stIntensitySliderInputUIs.m_oInput.onEndEdit.AddListener(this.OnEndInputREUIsLightIntensity);
			m_stIntensitySliderInputUIs.m_oSlider.onValueChanged.AddListener(this.OnChangeREUIsLightIntensity);

			m_stCameraFOVSliderInputUIs.m_oInput.onEndEdit.AddListener(this.OnEndInputREUIsCameraVal);
			m_stCameraFOVSliderInputUIs.m_oSlider.onValueChanged.AddListener(this.OnChangeREUIsCameraVal);

			m_stCameraHeightSliderInputUIs.m_oInput.onEndEdit.AddListener(this.OnEndInputREUIsCameraVal);
			m_stCameraHeightSliderInputUIs.m_oSlider.onValueChanged.AddListener(this.OnChangeREUIsCameraVal);

			m_stCameraForwardSliderInputUIs.m_oInput.onEndEdit.AddListener(this.OnEndInputREUIsCameraVal);
			m_stCameraForwardSliderInputUIs.m_oSlider.onValueChanged.AddListener(this.OnChangeREUIsCameraVal);

			m_stCameraDistanceSliderInputUIs.m_oInput.onEndEdit.AddListener(this.OnEndInputREUIsCameraVal);
			m_stCameraDistanceSliderInputUIs.m_oSlider.onValueChanged.AddListener(this.OnChangeREUIsCameraVal);

			m_stCameraSmoothTimeSliderInputUIs.m_oInput.onEndEdit.AddListener(this.OnEndInputREUIsCameraVal);
			m_stCameraSmoothTimeSliderInputUIs.m_oSlider.onValueChanged.AddListener(this.OnChangeREUIsCameraVal);
			// 슬라이더 입력 UI 를 설정한다 }
		}

		/** 인스펙터 UI 를 설정한다 */
		private void SetupREUIsInspectorUIs()
		{
			// 토글을 설정한다 {
			m_oREUIsPatrolIdleToggle.onValueChanged.AddListener(this.OnTouchREUIsPatrolToggle);
			m_oREUIsPatrolWayPointToggle.onValueChanged.AddListener(this.OnTouchREUIsPatrolToggle);
			m_oREUIsPatrolLookAroundToggle.onValueChanged.AddListener(this.OnTouchREUIsPatrolToggle);
			m_oREUIsPatrolFixToggle.onValueChanged.AddListener(this.OnTouchREUIsPatrolToggle);

			m_oREUIsWayPointPassToggle.onValueChanged.AddListener(this.OnTouchREUIsWayPointToggle);
			m_oREUIsWayPointLookAroundToggle.onValueChanged.AddListener(this.OnTouchREUIsWayPointToggle);

			m_oREUIsReturnRotate01Toggle.onValueChanged.AddListener(this.OnTouchREUIsReturnRotateToggle);
			m_oREUIsReturnRotate02Toggle.onValueChanged.AddListener(this.OnTouchREUIsReturnRotateToggle);

			m_oREUIsRandomExcludeToggle.onValueChanged.AddListener(this.OnTouchREUIsRandomExcludeToggle);
			// 토글을 설정한다 }

			// 입력 필드를 설정한다 {
			var oVec3InputUIsList = new List<STVec3InputUIs>() {
				m_stREUIsPosInputUIs, m_stREUIsScaleInputUIs, m_stREUIsRotateInputUIs
			};

			for (int i = 0; i < oVec3InputUIsList.Count; ++i)
			{
				var stVec3InputUIs = oVec3InputUIsList[i];

				oVec3InputUIsList[i].m_oXInput.onEndEdit.AddListener((a_oStr) => this.OnEndInputREUIsGizmosTargetTrans(stVec3InputUIs, ETransModifyInputType.X, a_oStr));
				oVec3InputUIsList[i].m_oXInput.onValueChanged.AddListener((a_oStr) => this.OnInputREUIsGizmosTargetTrans(stVec3InputUIs, ETransModifyInputType.X, a_oStr));

				oVec3InputUIsList[i].m_oYInput.onEndEdit.AddListener((a_oStr) => this.OnEndInputREUIsGizmosTargetTrans(stVec3InputUIs, ETransModifyInputType.Y, a_oStr));
				oVec3InputUIsList[i].m_oYInput.onValueChanged.AddListener((a_oStr) => this.OnInputREUIsGizmosTargetTrans(stVec3InputUIs, ETransModifyInputType.Y, a_oStr));

				oVec3InputUIsList[i].m_oZInput.onEndEdit.AddListener((a_oStr) => this.OnEndInputREUIsGizmosTargetTrans(stVec3InputUIs, ETransModifyInputType.Z, a_oStr));
				oVec3InputUIsList[i].m_oZInput.onValueChanged.AddListener((a_oStr) => this.OnInputREUIsGizmosTargetTrans(stVec3InputUIs, ETransModifyInputType.Z, a_oStr));
			}

			m_oREUIsPreDelayInput.onEndEdit.AddListener(this.OnEndInputREUIsAIControlVal);
			m_oREUIsPostDelayInput.onEndEdit.AddListener(this.OnEndInputREUIsAIControlVal);

			m_oREUIsDuration01Input.onEndEdit.AddListener(this.OnEndInputREUIsAIControlVal);
			m_oREUIsDuration02Input.onEndEdit.AddListener(this.OnEndInputREUIsAIControlVal);

			m_oREUIsRotateAngle01Input.onEndEdit.AddListener(this.OnEndInputREUIsAIControlVal);
			m_oREUIsRotateAngle02Input.onEndEdit.AddListener(this.OnEndInputREUIsAIControlVal);
			// 입력 필드를 설정한다 }

			// 스크롤러 셀 뷰를 설정한다 {
			for (int i = 0; i < m_oPrefabEditorInfoList.Count; ++i)
			{
				var oNPCPrefabList = new List<GameObject>();
				var stPrefabEditorInfo = m_oPrefabEditorInfoList[i];

				for (int j = 0; j < stPrefabEditorInfo.m_oPrefabInfoList.Count - 1; ++j)
				{
					string oPrefabPath = string.Format("Prefabs/{0}/{1:00}-Theme", stPrefabEditorInfo.m_oPrefabInfoList[j].m_oName, i);

					switch (stPrefabEditorInfo.m_oPrefabInfoList[j].m_eResType)
					{
						case EResourceType.BG_Etc:
						case EResourceType.BG_Temp:
							oPrefabPath = string.Format("Prefabs/{0}", stPrefabEditorInfo.m_oPrefabInfoList[j].m_oName);
							break;
					}

					stPrefabEditorInfo.m_oPrefabListContainer.ExAddVal(Resources.LoadAll<GameObject>(oPrefabPath).ToList());
				}

				for (int j = 0; j < this.NPCTableList.Count; ++j)
				{
					string oDirName = stPrefabEditorInfo.m_oPrefabInfoList.Last().m_oName;
					string oPrefabPath = string.Format("Prefabs/{0}/{1:00}-Theme/{2}", oDirName, i, this.NPCTableList[j].Prefab);

					// 프리팹이 존재 할 경우
					if (ComUtil.TryLoadRes<GameObject>(oPrefabPath, out GameObject oPrefab))
					{
						oNPCPrefabList.Add(oPrefab);
					}
				}

				stPrefabEditorInfo.m_oPrefabListContainer.ExAddVal(oNPCPrefabList);
			}

			for (int i = 0; i < m_oPrefabEditorInfoList.Count; ++i)
			{
				var stScrollViewUIs = m_oREUIsScrollViewUIsList[i];

				for (int j = 0; j < stScrollViewUIs.m_oScrollRectList.Count; ++j)
				{
					var oScrollRect = stScrollViewUIs.m_oScrollRectList[j];

					this.SetupREUIsPrefabScrollView(oScrollRect.transform.Find("Viewport/Content").gameObject,
						m_oPrefabEditorInfoList[i].m_oPrefabListContainer[j], i, j);
				}
			}
			// 스크롤러 셀 뷰를 설정한다 }
		}

		/** 계층 UI 를 설정한다 */
		private void SetupREUIsHierarchyUIs()
		{
			// Do Something
		}

		/** 프리팹 스크롤 뷰를 설정한다 */
		private void SetupREUIsPrefabScrollView(GameObject a_oScrollViewContents, 
			List<GameObject> a_oPrefabList, int a_nTheme, int a_nIdx)
		{
			int nMaxNumRows = a_oPrefabList.Count / m_nNumPrefabsOnRow;
			nMaxNumRows += (a_oPrefabList.Count % m_nNumPrefabsOnRow != 0) ? 1 : 0;

			for (int i = 0; i < nMaxNumRows; ++i)
			{
				var stParams = new CREUIsScrollerCellView.STParams()
				{
					m_nTheme = a_nTheme,
					m_nGroup = a_nIdx,
					m_nStart = i * m_nNumPrefabsOnRow,
					m_oPrefabList = a_oPrefabList,
					m_oCallback = this.OnTouchREUIsPrefabBtn
				};

				var oScrollerCellView = GameResourceManager.Singleton.CreateObject<CREUIsScrollerCellView>(m_oREUIsOriginScrollerCellView, a_oScrollViewContents.transform, null);
				oScrollerCellView.Init(stParams);
			}
		}

		/** 기본 제어 값을 설정한다 */
		private void SetupDefControlValues(CObjInfo a_oObjInfo)
		{
			a_oObjInfo.m_bIsReturnRotate01 = true;
			a_oObjInfo.m_bIsReturnRotate02 = true;

			a_oObjInfo.m_fPreDelay = 1.5f;
			a_oObjInfo.m_fPostDelay = 1.5f;

			a_oObjInfo.m_fDuration01 = 0.5f;
			a_oObjInfo.m_fDuration02 = 0.5f;

			a_oObjInfo.m_fRotateAngle01 = -30.0f;
			a_oObjInfo.m_fRotateAngle02 = 30.0f;
		}

		/** 에디터 UI 상태를 갱신한다 */
		private void UpdateREUIsEditorUIsState()
		{
			// 광원을 설정한다 {
			m_oLight.color = this.SelMapInfo.m_stLightInfo.m_stColor;
			m_oLight.intensity = this.SelMapInfo.m_stLightInfo.m_fIntensity;

			m_oLightRenderer.material.color = this.SelMapInfo.m_stLightInfo.m_stColor;
			// 광원을 설정한다 }

			// 슬라이더 입력 UI 를 갱신한다 {
			this.SetREUIsLightColor(this.SelMapInfo.m_stLightInfo.m_stColor.m_fR * byte.MaxValue,
				this.SelMapInfo.m_stLightInfo.m_stColor.m_fG * byte.MaxValue, this.SelMapInfo.m_stLightInfo.m_stColor.m_fB * byte.MaxValue, false);

			this.SetREUIsCameraVal(this.SelMapInfo.m_stCameraInfo.m_fFOV,
				this.SelMapInfo.m_stCameraInfo.m_fHeight, this.SelMapInfo.m_stCameraInfo.m_fForward, this.SelMapInfo.m_stCameraInfo.m_fDistance, this.SelMapInfo.m_stCameraInfo.m_fSmoothTime, false);

			this.SetREUIsLightIntensity(this.SelMapInfo.m_stLightInfo.m_fIntensity, false);
			// 슬라이더 입력 UI 를 갱신한다 }
		}

		/** 인스펙터 UI 상태를 갱신한다 */
		private void UpdateREUIsInspectorUIsState()
		{
			// 버튼을 갱신한다
			m_oREUIsAddWayPointBtn.interactable = this.IsEnableAddWayPoint();
			m_oREUIsAddStartPointBtn.interactable = this.IsEnableAddStartPoint();

			// 토글을 갱신한다 {
			m_oREUIsPatrolIdleToggle.interactable = this.SelObjInfo != null && this.SelObjInfo.m_stPrefabInfo.m_eResType == EResourceType.Character;
			m_oREUIsPatrolWayPointToggle.interactable = this.SelObjInfo != null && this.SelObjInfo.m_stPrefabInfo.m_eResType == EResourceType.Character;
			m_oREUIsPatrolLookAroundToggle.interactable = this.SelObjInfo != null && this.SelObjInfo.m_stPrefabInfo.m_eResType == EResourceType.Character;
			m_oREUIsPatrolFixToggle.interactable = this.SelObjInfo != null && this.SelObjInfo.m_stPrefabInfo.m_eResType == EResourceType.Character;

			m_oREUIsWayPointPassToggle.interactable = this.SelWayPointInfo != null;
			m_oREUIsWayPointLookAroundToggle.interactable = this.SelWayPointInfo != null;

			// 객체 정보가 존재 할 경우
			if (this.SelObjInfo != null)
			{
				m_oREUIsPatrolIdleToggle.SetIsOnWithoutNotify(this.SelObjInfo.m_ePatrolType == EPatrolType.IDLE);
				m_oREUIsPatrolWayPointToggle.SetIsOnWithoutNotify(this.SelObjInfo.m_ePatrolType == EPatrolType.WAY_POINT);
				m_oREUIsPatrolLookAroundToggle.SetIsOnWithoutNotify(this.SelObjInfo.m_ePatrolType == EPatrolType.LOOK_AROUND);
				m_oREUIsPatrolFixToggle.SetIsOnWithoutNotify(this.SelObjInfo.m_ePatrolType == EPatrolType.FIX);

				m_oREUIsReturnRotate01Toggle.SetIsOnWithoutNotify(this.SelObjInfo.m_bIsReturnRotate01);
				m_oREUIsReturnRotate02Toggle.SetIsOnWithoutNotify(this.SelObjInfo.m_bIsReturnRotate02);

				m_oREUIsRandomExcludeToggle.SetIsOnWithoutNotify(this.SelObjInfo.m_bIsRandExclude);
			}

			// 이동 지점 정보가 존재 할 경우
			if (this.SelWayPointInfo != null)
			{
				m_oREUIsWayPointPassToggle.SetIsOnWithoutNotify(this.SelWayPointInfo.m_eWayPointType == EWayPointType.PASS);
				m_oREUIsWayPointLookAroundToggle.SetIsOnWithoutNotify(this.SelWayPointInfo.m_eWayPointType == EWayPointType.LOOK_AROUND);
			}
			// 토글을 갱신한다 }

			// 스크롤 뷰를 갱신한다 {
			for (int i = 0; i < m_oREUIsThemeTapBtnList.Count; ++i)
			{
				m_oREUIsThemeTapBtnList[i].image.color = (i == m_nREUIsSelThemeTabBtnIdx) ? Color.white : Color.gray;
			}

			for (int i = 0; i < m_oREUIsScrollViewUIsList.Count; ++i)
			{
				for (int j = 0; j < m_oREUIsScrollViewUIsList[i].m_oTapBtnList.Count; ++j)
				{
					bool bIsValid = i == m_nREUIsSelThemeTabBtnIdx;

					m_oREUIsScrollViewUIsList[i].m_oTapBtnList[j].image.color = (j == m_nREUIsSelPrefabTabBtnIdx) ? Color.white : Color.gray;
					m_oREUIsScrollViewUIsList[i].m_oTapBtnList[j].gameObject.SetActive(bIsValid);

					m_oREUIsScrollViewUIsList[i].m_oScrollRectList[j].gameObject.SetActive(bIsValid && j == m_nREUIsSelPrefabTabBtnIdx);
				}
			}
			// 스크롤 뷰를 갱신한다 }

			// 객체 정보가 존재 할 경
			if (this.SelObjInfo != null)
			{
				m_oREUIsPreDelayInput.SetTextWithoutNotify($"{System.Math.Round(this.SelObjInfo.m_fPreDelay, 3)}");
				m_oREUIsPostDelayInput.SetTextWithoutNotify($"{System.Math.Round(this.SelObjInfo.m_fPostDelay, 3)}");

				m_oREUIsDuration01Input.SetTextWithoutNotify($"{System.Math.Round(this.SelObjInfo.m_fDuration01, 3)}");
				m_oREUIsDuration02Input.SetTextWithoutNotify($"{System.Math.Round(this.SelObjInfo.m_fDuration02, 3)}");

				m_oREUIsRotateAngle01Input.SetTextWithoutNotify($"{System.Math.Round(this.SelObjInfo.m_fRotateAngle01, 3)}");
				m_oREUIsRotateAngle02Input.SetTextWithoutNotify($"{System.Math.Round(this.SelObjInfo.m_fRotateAngle02, 3)}");
			}
		}

		/** 계층 UI 상태를 갱신한다 */
		private void UpdateREUIsHierarchyUIsState()
		{
			this.UpdateREUIsScrollViewState(m_oREUIsHierarchyScrollViewContents, m_oPrefabObjInfoList.Count);
		}

		/** UI 상태를 갱신한다 */
		private void UpdateREUIsState()
		{
			this.UpdateREUIsEditorUIsState();
			this.UpdateREUIsInspectorUIsState();
			this.UpdateREUIsHierarchyUIsState();

			this.UpdateREUIsMapObjTemplateScrollViewState(m_oREUIsMapObjTemplateScrollViewContents, 
				CMapObjTemplateInfoTable.Singleton.MapObjTemplateInfo.Count);

			// 객체를 갱신한다 {
			bool bIsEnableEditorUIs = !m_oSelTransGizmosTargetObjIdxList.ExIsValid() || m_ePrefabSelMode == EPrefabSelMode.CHANGE;

			m_oREUIsEditorUIs.SetActive(bIsEnableEditorUIs);
			m_oREUIsInspectorlUIs.SetActive(!bIsEnableEditorUIs);
			// 객체를 갱신한다 }

			// 텍스트를 갱신한다
			int nNumStages = CMapInfoTable.Singleton.GetNumStages(m_eSelMapInfoType, this.SelMapInfo.m_stIDInfo.m_nChapterID, this.SelMapInfo.m_stIDInfo.m_nEpisodeID);
			m_oREUIsTitleText.text = $"스테이지 {this.SelMapInfo.m_stIDInfo.m_nStageID + 1:00}/{nNumStages:00}";

			// 입력 필드를 갱신한다
			m_oREUIsStageInput.SetTextWithoutNotify($"{this.SelMapInfo.m_stIDInfo.m_nStageID + 1}");

			// 레이아웃을 재배치한다 {
			m_oREUIsBehaviourList.Clear();
			m_oREUIsInspectorlUIs.GetComponentsInChildren<UIBehaviour>(m_oREUIsBehaviourList);

			for (int i = 0; i < m_oREUIsBehaviourList.Count; ++i)
			{
				// 레이아웃 제어자 일 경우
				if (m_oREUIsBehaviourList[i].transform is RectTransform && m_oREUIsBehaviourList[i] is ILayoutController)
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(m_oREUIsBehaviourList[i].transform as RectTransform);
				}
			}
			// 레이아웃을 재배치한다 }
		}

		/** 벡터 입력 UI 를 갱신한다 */
		private void UpdateREUIsVec3InputUIs(STVec3InputUIs a_stInputUIs, Vector3 a_stVec3)
		{
			a_stInputUIs.m_oXInput.SetTextWithoutNotify($"{System.Math.Round(a_stVec3.x, 3)}");
			a_stInputUIs.m_oYInput.SetTextWithoutNotify($"{System.Math.Round(a_stVec3.y, 3)}");
			a_stInputUIs.m_oZInput.SetTextWithoutNotify($"{System.Math.Round(a_stVec3.z, 3)}");

			m_bIsDirtyUpdateUIsState = true;
		}

		/** 스크롤 뷰 상태를 갱신한다 */
		private void UpdateREUIsScrollViewState(GameObject a_oScrollViewContents, int a_nNumChildren)
		{
			GameObject oScrollerCellView = null;

			for (int i = 0; i < Mathf.Max(a_nNumChildren, a_oScrollViewContents.transform.childCount); ++i)
			{
				// 스크롤러 셀 뷰가 존재 할 경우
				if (i < a_oScrollViewContents.transform.childCount)
				{
					oScrollerCellView = a_oScrollViewContents.transform.GetChild(i).gameObject;
				}
				else
				{
					oScrollerCellView = GameResourceManager.Singleton.CreateObject(m_oREUIsOriginHierarchyScrollerCellView,
						a_oScrollViewContents.transform, null);
				}

				oScrollerCellView.SetActive(i < a_nNumChildren);
				this.UpdateREUIsScrollerCellViewState(oScrollerCellView, i, a_nNumChildren);
			}

			// 선택 된 객체 정보가 없을 경우
			if (this.SelObjInfo == null)
			{
				return;
			}

			this.ExLateCallFunc((a_oSender) =>
			{
				var oViewport = m_oREUIsHierarchyScrollViewContents.transform.parent.gameObject;
				var oScrollRect = m_oREUIsHierarchyScrollViewContents.GetComponentInParent<ScrollRect>();

				float fContentsHeight = (oScrollerCellView.transform as RectTransform).rect.height + 20.0f;
				float fScrollViewNormPosY = oScrollRect.ExGetNormPosV(oViewport, m_oREUIsHierarchyScrollViewContents, Vector3.up * ((this.SelTransGizmosTargetObjIdx - 1) * fContentsHeight));

				oScrollRect.verticalNormalizedPosition = fScrollViewNormPosY;
			});
		}

		/** 스크롤러 셀 뷰 상태를 갱신한다 */
		private void UpdateREUIsScrollerCellViewState(GameObject a_oScrollerCellView, int a_nIdx, int a_nNumChildren)
		{
			a_oScrollerCellView.SetActive(a_nIdx < m_oPrefabObjInfoList.Count);

			// 초기화가 불가능 할 경우
			if (a_nIdx >= m_oPrefabObjInfoList.Count || !a_oScrollerCellView.TryGetComponent(out CREUIsHierarchyScrollerCellView oScrollerCellView))
			{
				return;
			}

			var stParams = CREUIsHierarchyScrollerCellView.MakeParams(m_oPrefabObjInfoList[a_nIdx].m_oGameObj,
				m_oSelTransGizmosTargetObjIdxList.Contains(a_nIdx), this.OnTouchREUIsHierarchyScrollerCellView);

			oScrollerCellView.Init(stParams);
		}

		/** 맵 객체 템플릿 스크롤러 셀 뷰 상태를 갱신한다 */
		private void UpdateREUIsMapObjTemplateScrollViewState(GameObject a_oScrollViewContents, int a_nNumChildren)
		{
			GameObject oScrollerCellView = null;

			for (int i = 0; i < Mathf.Max(a_nNumChildren, a_oScrollViewContents.transform.childCount); ++i)
			{
				// 스크롤러 셀 뷰가 존재 할 경우
				if (i < a_oScrollViewContents.transform.childCount)
				{
					oScrollerCellView = a_oScrollViewContents.transform.GetChild(i).gameObject;
				}
				else
				{
					oScrollerCellView = GameResourceManager.Singleton.CreateObject(m_oREUIsOriginMapObjTemplateScrollerCellView,
						a_oScrollViewContents.transform, null);

					(oScrollerCellView.transform as RectTransform).sizeDelta = (m_oREUIsOriginMapObjTemplateScrollerCellView.transform as RectTransform).sizeDelta;
				}

				oScrollerCellView.SetActive(i < a_nNumChildren);
				this.UpdateREUIsMapObjTemplateScrollViewState(oScrollerCellView, i, a_nNumChildren);
			}
		}

		/** 맵 객체 템플릿 스크롤러 셀 뷰 상태를 갱신한다 */
		private void UpdateREUIsMapObjTemplateScrollViewState(GameObject a_oScrollerCellView, int a_nIdx, int a_nNumChildren)
		{
			a_oScrollerCellView.SetActive(a_nIdx < a_nNumChildren);

			// 초기화가 불가능 할 경우
			if (a_nIdx >= a_nNumChildren || !a_oScrollerCellView.TryGetComponent(out CREUIsMapObjTemplateScrollerCellView oScrollerCellView))
			{
				return;
			}

			var oMapObjTemplateInfo = CMapObjTemplateInfoTable.Singleton.MapObjTemplateInfo[a_nIdx];

			oScrollerCellView.NameText.text = CMapObjTemplateInfoTable.Singleton.MapObjTemplateInfo[a_nIdx].m_oName.ExIsValid() ? 
				CMapObjTemplateInfoTable.Singleton.MapObjTemplateInfo[a_nIdx].m_oName : $"Preset - {a_nIdx + 1} ({oMapObjTemplateInfo.m_oMapObjInfoList.Count})";

			oScrollerCellView.SelBtn.onClick.RemoveAllListeners();
			oScrollerCellView.RemoveBtn.onClick.RemoveAllListeners();

			oScrollerCellView.SelBtn.onClick.AddListener(() => this.OnTouchREUIsMapObjTemplateScrollerCellView(oScrollerCellView, a_nIdx));
			oScrollerCellView.RemoveBtn.onClick.AddListener(() => this.OnTouchREUIsMapObjTemplateScrollerCellViewRemoveBtn(oScrollerCellView, a_nIdx));
		}

		/** 계층 스크롤러 셀 뷰를 눌렀을 경우 */
		private void OnTouchREUIsHierarchyScrollerCellView(CREUIsHierarchyScrollerCellView a_oSender, GameObject a_oPrefabObj)
		{
			this.AddTransGizmosTargetObj(a_oPrefabObj, !Input.GetKey(ComType.CtrlKeyCode));
		}

		/** 맵 객체 템플릿 스크롤러 셀 뷰를 눌렀을 경우 */
		private void OnTouchREUIsMapObjTemplateScrollerCellView(CREUIsMapObjTemplateScrollerCellView a_oScrollerCellView, int a_nIdx)
		{
			this.AddTransGizmosTargetObj(null, true);
			var oMapObjTemplateInfo = CMapObjTemplateInfoTable.Singleton.MapObjTemplateInfo[a_nIdx];

			for(int i = 0; i < oMapObjTemplateInfo.m_oMapObjInfoList.Count; ++i)
			{
				var stGroup = this.FindPrefabGroup(oMapObjTemplateInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName);
				var stPrefabObjInfo = this.CreatePrefabObj(stGroup.Item1, stGroup.Item2, stGroup.Item3, oMapObjTemplateInfo.m_oMapObjInfoList[i].Clone() as CObjInfo);

				m_oPrefabObjInfoList.Add(stPrefabObjInfo);
				this.SetupTrans(stPrefabObjInfo.m_oGameObj.transform, oMapObjTemplateInfo.m_oMapObjInfoList[i].m_stTransInfo);

				this.AddMapObjInfo(stPrefabObjInfo, false);
			}
		}

		/** 맵 객체 템플릿 스크롤러 셀 뷰 제거 버튼을 눌렀을 경우 */
		private void OnTouchREUIsMapObjTemplateScrollerCellViewRemoveBtn(CREUIsMapObjTemplateScrollerCellView a_oScrollerCellView, int a_nIdx)
		{
			m_bIsDirtyUpdateUIsState = true;
			CMapObjTemplateInfoTable.Singleton.MapObjTemplateInfo.ExRemoveValAt(a_nIdx);

			CMapObjTemplateInfoTable.Singleton.SaveMapObjTemplateInfos();
		}

		/** 테마 탭 버튼을 눌렀을 경우 */
		private void OnTouchREUIsThemeTabBtn(int a_nIdx)
		{
			m_bIsDirtyUpdateUIsState = true;
			m_nREUIsSelThemeTabBtnIdx = a_nIdx;
		}

		/** 프리팹 탭 버튼을 눌렀을 경우 */
		private void OnTouchREUIsPrefabTabBtn(int a_nIdx)
		{
			m_bIsDirtyUpdateUIsState = true;
			m_nREUIsSelPrefabTabBtnIdx = a_nIdx;
		}

		/** 맵 객체 정보를 추가한다 */
		private void AddMapObjInfo(STPrefabObjInfo a_stPrefabObjInfo, bool a_bIsFocus, CObjInfo a_oOwner = null)
		{
			this.AddTransGizmosTargetObj(a_stPrefabObjInfo.m_oGameObj);
			this.UpdateGizmosTargetObjTrans();

			// 소유자가 존재 할 경우
			if (a_oOwner != null && a_stPrefabObjInfo.m_oObjInfo is CWayPointInfo)
			{
				a_oOwner.m_oWayPointInfoList.Add(a_stPrefabObjInfo.m_oObjInfo as CWayPointInfo);
				(a_stPrefabObjInfo.m_oObjInfo as CWayPointInfo).m_oOwner = a_oOwner;
			}
			else
			{
				this.SelMapInfo.m_oMapObjInfoList.Add(a_stPrefabObjInfo.m_oObjInfo);
			}

			// 포커스 설정이 필요 할 경우
			if (a_bIsFocus && Input.GetKey(KeyCode.LeftShift))
			{
				RTFocusCamera.Get.Focus(new List<GameObject>() { a_stPrefabObjInfo.m_oGameObj });
			}
		}
		#endregion // 함수

		#region 접근 함수
		/** 벡터 값을 반환한다 */
		private Vector3 GetVec3(STVec3InputUIs a_stInputUIs, float a_fDefVal)
		{
			bool bIsValid01 = float.TryParse(a_stInputUIs.m_oXInput.text, NumberStyles.Any, null, out float fX) && (fX < -float.Epsilon || fX > float.Epsilon);
			bool bIsValid02 = float.TryParse(a_stInputUIs.m_oYInput.text, NumberStyles.Any, null, out float fY) && (fY < -float.Epsilon || fY > float.Epsilon);
			bool bIsValid03 = float.TryParse(a_stInputUIs.m_oZInput.text, NumberStyles.Any, null, out float fZ) && (fZ < -float.Epsilon || fZ > float.Epsilon);

			return new Vector3(bIsValid01 ? fX : a_fDefVal, bIsValid02 ? fY : a_fDefVal, bIsValid03 ? fZ : a_fDefVal);
		}
		#endregion // 접근 함수

		#region 팩토리 함수
		/** 프리팹 객체를 생성한다 */
		private STPrefabObjInfo CreatePrefabObj(int a_nTheme, int a_nGroup, int a_nPrefab, CObjInfo a_oObjInfo = null)
		{
			var stPrefabEditorInfo = m_oPrefabEditorInfoList[a_nTheme];

			var oGameObj = GameResourceManager.Singleton.CreateObject(stPrefabEditorInfo.m_oPrefabListContainer[a_nGroup][a_nPrefab], m_oObjsRoot.transform,
				stPrefabEditorInfo.m_oPrefabListContainer[a_nGroup][a_nPrefab].transform);

			oGameObj.isStatic = false;
			oGameObj.transform.localScale = stPrefabEditorInfo.m_oPrefabListContainer[a_nGroup][a_nPrefab].transform.localScale;

			var oMeshRenderer = oGameObj.GetComponentInChildren<MeshRenderer>();
			var oSkinnedMeshRenderer = oGameObj.GetComponentInChildren<SkinnedMeshRenderer>();

			// 메시 렌더러가 없을 경우
			if (oMeshRenderer == null && oSkinnedMeshRenderer == null)
			{
				var oMeshFilter = oGameObj.GetComponentInChildren<MeshFilter>();

				// 메시 필터가 없을 경우
				if (oMeshFilter == null)
				{
					oMeshFilter = oGameObj.AddComponent<MeshFilter>();
					oMeshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
				}

				oMeshRenderer = oGameObj.AddComponent<MeshRenderer>();
			}

			uint nKey = 0;
			int nResult = this.NPCTableList.FindIndex((a_oTable) => a_oTable.Prefab.Equals(oGameObj.name));

			// NPC 일 경우
			if (nResult >= 0 && nResult < this.NPCTableList.Count)
			{
				var oHelpRange = oGameObj.transform.Find("HelpRange")?.gameObject ?? GameResourceManager.Singleton.CreateObject(m_oMEUIsOriginHelpRange, oGameObj.transform, null);
				oHelpRange.name = "HelpRange";
				oHelpRange.transform.localScale = Vector3.one * this.NPCTableList[nResult].HelpRange * ComType.G_UNIT_MM_TO_M * 2.0f;
				oHelpRange.transform.localPosition = Vector3.up * 0.25f;

				var oSighRange = oGameObj.transform.Find("SighRange")?.gameObject ?? GameResourceManager.Singleton.CreateObject(m_oMEUIsOriginSighRange, oGameObj.transform, null);
				oSighRange.name = "SighRange";
				oSighRange.transform.localPosition = Vector3.up * 0.25f;

				var oLineFX = oSighRange.GetComponentInChildren<LineRenderer>();
				var oPosList = CCollectionPoolManager.Singleton.SpawnList<Vector3>();

				nKey = this.NPCTableList[nResult].PrimaryKey;

				try
				{
					float fAngle = this.NPCTableList[nResult].SightAngle;
					float fOffset = fAngle / 10.0f;

					for (float i = 0.0f; i <= fAngle; i += fOffset)
					{
						var stDirection = (Vector3)(Quaternion.AngleAxis(i - (fAngle / 2.0f), Vector3.up) * Vector3.forward);
						oPosList.Add(stDirection * (this.NPCTableList[nResult].SightRange * ComType.G_UNIT_MM_TO_M) + (Vector3.up * 0.25f));
					}

					oPosList.Add(Vector3.zero + (Vector3.up * 0.25f));
					oPosList.Insert(0, Vector3.zero + (Vector3.up * 0.25f));

					oLineFX.positionCount = oPosList.Count;
					oLineFX.SetPositions(oPosList.ToArray());
				}
				finally
				{
					CCollectionPoolManager.Singleton.DespawnList(oPosList);
				}
			}

			return this.MakePrefabObjInfo(nKey,
				a_nTheme, a_nGroup, a_nPrefab, stPrefabEditorInfo.m_oPrefabInfoList[a_nGroup].m_eResType, oGameObj, a_oObjInfo);
		}

		/** 이동 지점 프리팹 객체를 생성한다 */
		private STPrefabObjInfo CreateWayPointPrefabObj(int a_nTheme, int a_nGroup, int a_nPrefab, CWayPointInfo a_oWayPointInfo = null)
		{
			var stPrefabEditorInfo = m_oPrefabEditorInfoList[a_nTheme];

			var oGameObj = GameResourceManager.Singleton.CreateObject(stPrefabEditorInfo.m_oPrefabListContainer[a_nGroup][a_nPrefab],
				m_oObjsRoot.transform, stPrefabEditorInfo.m_oPrefabListContainer[a_nGroup][a_nPrefab].transform);

			oGameObj.isStatic = false;
			oGameObj.transform.localScale = stPrefabEditorInfo.m_oPrefabListContainer[a_nGroup][a_nPrefab].transform.localScale;

			return this.MakeWayPointPrefabObjInfo(a_nTheme,
				a_nGroup, a_nPrefab, stPrefabEditorInfo.m_oPrefabInfoList[a_nGroup].m_eResType, oGameObj, a_oWayPointInfo);
		}
		#endregion // 팩토리 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
