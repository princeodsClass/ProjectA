using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.EventSystems;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
using RTG;
using DG.Tweening;

namespace MapEditor
{
	/** 맵 에디터 페이지 */
	public partial class PageMapEditor : UIDialog
	{
		/** 모드 */
		private enum EMode
		{
			NONE = -1,
			FREE,
			GRID,
			[HideInInspector] MAX_VAL
		}

		#region 변수
		[SerializeField] private Image m_oIndicatorImg = null;
		[SerializeField] private GameObject m_oIndicatorUIs = null;

		private bool m_bIsDirtySaveInfo = false;
		private bool m_bIsDirtyResetMapInfo = false;
		private bool m_bIsDirtyUpdateUIsState = true;
		private bool m_bIsDirtyResetPrefabObject = true;

		private bool m_bIsEnableHelpRangeInfo = false;
		private bool m_bIsEnableSighRangeInfo = false;

		private float m_fDefLightIntensity = 0.0f;

		private Color m_stDefLightColor;
		private STTransInfo m_stDefLightTransInfo;
		private System.DateTime m_stPrevTime = System.DateTime.Now;

		private EMode m_eSelMode = EMode.FREE;

		private Light m_oLight = null;
		private Tween m_oIndicatorAni = null;
		private MeshRenderer m_oLightRenderer = null;

		private GameObject m_oObjsRoot = null;
		private GameObject m_oPlayerPos = null;
		private LineRenderer m_oWayPointLine = null;
		private List<STPrefabObjInfo> m_oPrefabObjInfoList = new List<STPrefabObjInfo>();
		#endregion // 변수

		#region 프로퍼티
		public CMapInfo SelMapInfo { get; private set; } = null;
		public List<NPCTable> NPCTableList { get; private set; } = null;

		public CObjInfo SelObjInfo => this.SelTransGizmosTargetObjIdx.ExIsValidIdx() ?
			m_oPrefabObjInfoList[this.SelTransGizmosTargetObjIdx].m_oObjInfo : null;

		public CWayPointInfo SelWayPointInfo => this.SelObjInfo as CWayPointInfo;

		public GameObject SelTransGizmosTargetObj => this.SelTransGizmosTargetObjIdx.ExIsValidIdx() ?
			m_oPrefabObjInfoList[this.SelTransGizmosTargetObjIdx].m_oGameObj : null;
		#endregion // 프로퍼티

		#region 함수
		/** 초기화 */
		public void Awake()
		{
			this.Initialize();
		}

		/** 초기화 */
		public override void Initialize()
		{
			base.Initialize();
			m_oABSetBtnUIs.SetActive(false);

			m_oObjsRoot = GameObject.Find(ComType.OBJECT_ROOT_NAME);
			m_oPlayerPos = GameObject.Find(ComType.G_BG_N_PLAYER_POS);
			m_oWayPointLine = GameObject.Find(ComType.OBJECT_ROOT_NAME).transform.Find("WayPointLine").GetComponentInChildren<LineRenderer>();

			var oPrefabInfoList = ComUtil.ReadJSONObj<List<STPrefabInfo>>(ComType.G_RUNTIME_DATA_P_MAP_EDITOR_PREFAB_GROUPS,
				false);

			for (int i = 0; i < ComType.G_MAX_NUM_EPISODE_THEMES; ++i)
			{
				var stPrefabEditorInfo = new STPrefabEditorInfo()
				{
					m_oPrefabInfoList = new List<STPrefabInfo>(),
					m_oPrefabListContainer = new List<List<GameObject>>()
				};

				oPrefabInfoList.ExCopyTo(stPrefabEditorInfo.m_oPrefabInfoList, (a_stPrefabInfo) => a_stPrefabInfo);
				m_oPrefabEditorInfoList.ExAddVal(stPrefabEditorInfo);
			}

			// 광원을 설정한다 {
			m_oLight = GameObject.Find("Directional Light").GetComponent<Light>();
			m_oLightRenderer = m_oLight.GetComponent<MeshRenderer>();
			m_fDefLightIntensity = m_oLight.intensity;

			m_stDefLightColor = m_oLight.color;

			m_stDefLightTransInfo = new STTransInfo(m_oLight.transform.localPosition,
				m_oLight.transform.localScale, m_oLight.transform.localEulerAngles);
			// 광원을 설정한다 }

			foreach (var stKeyVal in CMapInfoTable.Singleton.MapInfoWrapperDict)
			{
				// 맵 정보가 존재 할 경우
				if (stKeyVal.Value.m_oMapInfoDictContainer.Count >= 1)
				{
					continue;
				}

				var oMapInfo = ComUtil.MakeMapInfo(stKeyVal.Key, 0, 0, 0);
				this.SetupDefValues(oMapInfo);

				CMapInfoTable.Singleton.AddMapInfo(oMapInfo, stKeyVal.Key);
				CMapInfoTable.Singleton.SaveMapInfos(stKeyVal.Key);
			}
		}

		/** 기본 값을 설정한다 */
		private void SetupDefValues()
		{
			foreach (var stKeyVal in CMapInfoTable.Singleton.MapInfoWrapperDict)
			{
				this.SetupDefValues(stKeyVal.Key, stKeyVal.Value);
			}
		}

		/** 기본 값을 설정한다 */
		private void SetupDefValues(EMapInfoType a_eMapInfoType, CMapInfoWrapper a_oMapInfoWrapper)
		{
			foreach (var stKeyVal in a_oMapInfoWrapper.m_oMapInfoDictContainer)
			{
				foreach (var stChapterKeyVal in stKeyVal.Value)
				{
					foreach (var stStageKeyVal in stChapterKeyVal.Value)
					{
						this.SetupDefValues(stStageKeyVal.Value);
					}
				}
			}

			CMapInfoTable.Singleton.SaveMapInfos(a_eMapInfoType);
		}

		/** 기본 값을 설정한다 */
		private void SetupDefValues(CMapInfo a_oMapInfo)
		{
			// 기본 값이 설정 되었을 경우
			if (a_oMapInfo.m_bIsSetupDefValues)
			{
				return;
			}

			this.SetupDefLightValues(a_oMapInfo);

			a_oMapInfo.m_stPlayerPos = Vector3.zero;
			a_oMapInfo.m_stPlayerRotate = Vector3.zero;

			a_oMapInfo.m_stLightInfo.m_stTransInfo.m_stPos = m_stDefLightTransInfo.m_stPos;
			a_oMapInfo.m_stLightInfo.m_stTransInfo.m_stScale = m_stDefLightTransInfo.m_stScale;
			a_oMapInfo.m_stLightInfo.m_stTransInfo.m_stRotate = m_stDefLightTransInfo.m_stRotate;
		}

		/** 초기화 */
		public void Start()
		{
			m_eSelMapInfoType = (GameDataManager.Singleton.PlayMapInfo != null) ? GameDataManager.Singleton.PlayMapInfoType : m_eSelMapInfoType;
			m_eSelMapInfoType = (m_eSelMapInfoType != EMapInfoType.HUNT) ? m_eSelMapInfoType : EMapInfoType.CAMPAIGN;

			this.NPCTableList = NPCTable.GetList();

			var stIDInfo = (GameDataManager.Singleton.PlayMapInfo != null) ? GameDataManager.Singleton.PlayMapInfo.m_stIDInfo : new STIDInfo(0, 0, 0);
			this.SetSelMapInfo(CMapInfoTable.Singleton.GetMapInfo(m_eSelMapInfoType, stIDInfo.m_nStageID, stIDInfo.m_nChapterID, stIDInfo.m_nEpisodeID));

			this.SetupDefValues();
			this.SetupMidEditorUIs();
			this.SetupLeftEditorUIs();
			this.SetupRightEditorUIs();

			Canvas.ForceUpdateCanvases();
			NavMesh.RemoveAllNavMeshData();

			// 애니메이션을 설정한다
			ComUtil.AssignVal(ref m_oIndicatorAni,
				m_oIndicatorImg.transform.DORotate(Vector3.forward * -360.0f, 2.0f, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental));

			// 기즈모를 설정한다 {
			m_oMoveTransGizmos = m_oMoveTransGizmos ?? RTGizmosEngine.Get.CreateObjectMoveGizmo();
			m_oScaleTransGizmos = m_oScaleTransGizmos ?? RTGizmosEngine.Get.CreateObjectScaleGizmo();
			m_oRotateTransGizmos = m_oRotateTransGizmos ?? RTGizmosEngine.Get.CreateObjectRotationGizmo();

			this.SetGizmosSpace(m_eSelGizmosSpace);
			this.SetGizmosPivot(m_eSelGizmosPivot);

			this.AddTransGizmosTargetObj(null, true);
			// 기즈모를 설정한다 }

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
			Screen.SetResolution((int)ComType.CorrectDesktopScreenSize.x, (int)ComType.CorrectDesktopScreenSize.y, FullScreenMode.Windowed);
#else
			Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow);
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)

			// 튜토리얼이 아닐 경우
			if (m_eSelMapInfoType != EMapInfoType.TUTORIAL)
			{
				// FIXME: 임시 주석 처리
				// StartCoroutine(this.CoLoadMapVersions());
			}

			m_oMEUIsUploadBtn.interactable = SystemInfo.deviceUniqueIdentifier.Equals("2B8AAB24-64BA-58AF-BD02-0A268B4604FB");
		}

		/** 제거 되었을 경우 */
		public void OnDestroy()
		{
			// 앱이 플레이 상태가 아닐 경우
			if (!Application.isPlaying)
			{
				return;
			}

			for (int i = (int)EMapInfoType.CAMPAIGN; i < (int)EMapInfoType.MAX_VAL; ++i)
			{
				// 맵 정보 타입이 유효하지 않을 경우
				if (!System.Enum.IsDefined(typeof(EMapInfoType), i))
				{
					continue;
				}

				this.SaveInfo((EMapInfoType)i);
			}

			m_oIndicatorAni?.ExKill();
			ComUtil.AssignVal(ref m_oIndicatorAni, null);
		}

		/** 상태를 갱신한다 */
		public void Update()
		{
			this.HandleHotKeys();
			RTSceneGrid.Get.YOffset = 0.0f;

			// 이동이 발생했을 경우
			if (RTGizmosEngine.Get.DraggedGizmo != null)
			{
				this.UpdateGizmosTargetObjTrans();
			}

			// 마우스 버튼을 눌렀을 경우
			if (Input.GetMouseButtonDown((int)EMouseBtn.LEFT) && !EventSystem.current.IsPointerOverGameObject())
			{
				bool bIsValid01 = RTGizmosEngine.Get.HoveredGizmo == null;
				bool bIsValid02 = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit stRaycastHit);

				// 객체를 선택했을 경우
				if (bIsValid01 && bIsValid02)
				{
					var stCurTime = System.DateTime.Now;
					var stDeltaTime = stCurTime - m_stPrevTime;

					// 더블 클릭 일 경우
					if (stDeltaTime.TotalSeconds <= 0.25f)
					{
						RTFocusCamera.Get.Focus(new List<GameObject>() {
							stRaycastHit.collider.gameObject
						});
					}

					m_stPrevTime = stCurTime;
					this.AddTransGizmosTargetObj(stRaycastHit.collider.gameObject, !Input.GetKey(ComType.CtrlKeyCode));
				}
			}

			// 기즈모를 갱신한다
			m_oMoveTransGizmos.Gizmo.SetEnabled(m_oSelTransGizmosTargetObjIdxList.ExIsValid() && m_eSelTransGizmosType == ETransGizmosType.MOVE);
			m_oScaleTransGizmos.Gizmo.SetEnabled(m_oSelTransGizmosTargetObjIdxList.ExIsValid() && m_eSelTransGizmosType == ETransGizmosType.SCALE);
			m_oRotateTransGizmos.Gizmo.SetEnabled(m_oSelTransGizmosTargetObjIdxList.ExIsValid() && m_eSelTransGizmosType == ETransGizmosType.ROTATE);

			// 입력 필드를 갱신한다 {
			int nIdx01 = m_oREUIsEditorInputList.FindIndex((a_oInput) => a_oInput.isFocused);
			int nIdx02 = m_oREUIsInspectorInputList.FindIndex((a_oInput) => a_oInput.isFocused);

			m_nREUIsSelEditorInputIdx = (nIdx01 >= 0 && nIdx01 < m_oREUIsEditorInputList.Count) ? nIdx01 : -1;
			m_nREUIsSelInspectorInputIdx = (nIdx02 >= 0 && nIdx02 < m_oREUIsInspectorInputList.Count) ? nIdx02 : -1;

			for (int i = 0; i < m_oREUIsInputList.Count; ++i)
			{
				m_oREUIsInputList[i].image.color = m_oREUIsInputList[i].isFocused ? Color.yellow : Color.white;
			}
			// 입력 필드를 갱신한다 }
		}

		/** 상태를 갱신한다 */
		public void LateUpdate()
		{
			// 정보 저장이 필요 할 경우
			if (m_bIsDirtySaveInfo)
			{
				m_bIsDirtySaveInfo = false;
				this.SaveInfo(m_eSelMapInfoType);
			}

			// UI 상태 갱신이 필요 할 경우
			if (m_bIsDirtyUpdateUIsState)
			{
				m_bIsDirtyUpdateUIsState = false;
				this.UpdateUIsState();
			}

			// 맵 정보 리셋이 필요 할 경우
			if (m_bIsDirtyResetMapInfo)
			{
				var stSelMapIDInfo = this.SelMapInfo.m_stIDInfo;
				CMapInfoTable.Singleton.LoadMapInfos(m_eSelMapInfoType);

				// 맵 정보가 없을 경우
				if (!CMapInfoTable.Singleton.TryGetMapInfo(m_eSelMapInfoType, stSelMapIDInfo.m_nStageID, stSelMapIDInfo.m_nChapterID, stSelMapIDInfo.m_nEpisodeID, out CMapInfo oMapInfo))
				{
					oMapInfo = CMapInfoTable.Singleton.GetMapInfo(m_eSelMapInfoType, 0, 0, 0);
				}

				this.SetSelMapInfo(oMapInfo);

				// 튜토리얼이 아닐 경우
				if (m_eSelMapInfoType != EMapInfoType.TUTORIAL)
				{
					// FIXME: 임시 주석 처리
					// StartCoroutine(this.CoLoadMapVersions());
				}

				m_bIsDirtyResetMapInfo = false;
			}

			// 프리팹 객체 리셋이 필요 할 경우
			if (m_bIsDirtyResetPrefabObject)
			{
				this.ResetPrefabObjs();
				m_bIsDirtyResetPrefabObject = false;
			}
		}

		/** 프리팹 객체를 리셋한다 */
		private void ResetPrefabObjs()
		{
			for (int i = 0; i < m_oPrefabObjInfoList.Count; ++i)
			{
				// 고유 객체 일 경우
				if (this.IsUniqueGameObj(m_oPrefabObjInfoList[i].m_oGameObj))
				{
					continue;
				}

				GameResourceManager.Singleton.ReleaseObject(m_oPrefabObjInfoList[i].m_oGameObj);
			}

			m_oPrefabObjInfoList.Clear();
			this.AddTransGizmosTargetObj(null, true);

			for (int i = 0; i < this.SelMapInfo.m_oMapObjInfoList.Count; ++i)
			{
				this.ResetPrefabObjs(this.SelMapInfo.m_oMapObjInfoList[i]);
			}

			var stLightGroup = this.FindPrefabGroup("BG_Light");
			var stLightPrefabEditorInfo = m_oPrefabEditorInfoList[stLightGroup.Item1];

			var stPlayerPosGroup = this.FindPrefabGroup(ComType.G_BG_N_PLAYER_POS);
			var stPlayerPosPrefabEditorInfo = m_oPrefabEditorInfoList[stPlayerPosGroup.Item1];

			var stLightPrefabObjInfo = this.MakePrefabObjInfo(0,
				stLightGroup.Item1, stLightGroup.Item2, stLightGroup.Item3, stLightPrefabEditorInfo.m_oPrefabInfoList[stLightGroup.Item2].m_eResType, m_oLight.gameObject, null);

			var stPlayerPosPrefabObjInfo = this.MakePrefabObjInfo(0,
				stPlayerPosGroup.Item1, stPlayerPosGroup.Item2, stPlayerPosGroup.Item3, stPlayerPosPrefabEditorInfo.m_oPrefabInfoList[stPlayerPosGroup.Item2].m_eResType, m_oPlayerPos, null);

			m_oPrefabObjInfoList.Add(stLightPrefabObjInfo);
			m_oPrefabObjInfoList.Add(stPlayerPosPrefabObjInfo);

			// 광원을 설정한다 {
			m_oLight.gameObject.transform.localPosition = m_stDefLightTransInfo.m_stPos;
			m_oLight.gameObject.transform.localScale = m_stDefLightTransInfo.m_stScale;
			m_oLight.gameObject.transform.localEulerAngles = m_stDefLightTransInfo.m_stRotate;

			// 기본 값 설정 상태 일 경우
			if (this.SelMapInfo.m_bIsSetupDefValues)
			{
				m_oLight.gameObject.transform.localPosition = this.SelMapInfo.m_stLightInfo.m_stTransInfo.m_stPos;
				m_oLight.gameObject.transform.localScale = this.SelMapInfo.m_stLightInfo.m_stTransInfo.m_stScale;
				m_oLight.gameObject.transform.localEulerAngles = this.SelMapInfo.m_stLightInfo.m_stTransInfo.m_stRotate;
			}
			// 광원을 설정한다 }

			// 플레이어 위치를 설정한다
			m_oPlayerPos.transform.localPosition = this.SelMapInfo.m_stPlayerPos;
			m_oPlayerPos.transform.localEulerAngles = this.SelMapInfo.m_stPlayerRotate;
		}

		/** 프리팹 객체를 리셋한다 */
		private void ResetPrefabObjs(CObjInfo a_oObjInfo)
		{
			var stGroup = this.FindPrefabGroup(a_oObjInfo.m_stPrefabInfo.m_oName);

			// 그룹이 존재 할 경우
			if (stGroup.ExIsValidIdx())
			{
				var stPrefabObjInfo = this.CreatePrefabObj(stGroup.Item1, stGroup.Item2, stGroup.Item3, a_oObjInfo);
				m_oPrefabObjInfoList.Add(stPrefabObjInfo);

				this.SetupTrans(stPrefabObjInfo.m_oGameObj.transform, a_oObjInfo.m_stTransInfo);
			}

			// 이동 지점 정보가 존재 할 경우
			if (a_oObjInfo.m_oWayPointInfoList != null)
			{
				for (int i = 0; i < a_oObjInfo.m_oWayPointInfoList.Count; ++i)
				{
					stGroup = this.FindPrefabGroup(a_oObjInfo.m_oWayPointInfoList[i].m_stPrefabInfo.m_oName);

					// 그룹이 존재 할 경우
					if (stGroup.Item1 >= 0 && stGroup.Item2 >= 0)
					{
						var stWayPointPrefabObjInfo = this.CreateWayPointPrefabObj(stGroup.Item1,
							stGroup.Item2, stGroup.Item3, a_oObjInfo.m_oWayPointInfoList[i]);

						m_oPrefabObjInfoList.Add(stWayPointPrefabObjInfo);
						this.SetupTrans(stWayPointPrefabObjInfo.m_oGameObj.transform, a_oObjInfo.m_oWayPointInfoList[i].m_stTransInfo);
					}
				}
			}
		}

		/** 트랜스 폼을 설정한다 */
		private void SetupTrans(Transform a_oTrans, STTransInfo a_stTransInfo)
		{
			a_oTrans.localPosition = a_stTransInfo.m_stPos;
			a_oTrans.localScale = a_stTransInfo.m_stScale;
			a_oTrans.localEulerAngles = a_stTransInfo.m_stRotate;
		}

		/** UI 상태를 갱신한다 */
		private void UpdateUIsState()
		{
			this.UpdatePUIsState();
			this.UpdateMEUIsState();
			this.UpdateLEUIsState();
			this.UpdateREUIsState();
			this.UpdateEtcUIsState();

			for (int i = 0; i < m_oPrefabObjInfoList.Count; ++i)
			{
				var oMeshRenderer = m_oPrefabObjInfoList[i].m_oGameObj.GetComponentInChildren<MeshRenderer>();

				// 프로퍼티가 존재 할 경우
				if (oMeshRenderer != null && oMeshRenderer.material.HasProperty("_Emission_Color"))
				{
					oMeshRenderer.material.SetColor("_Emission_Color", m_oSelTransGizmosTargetObjIdxList.Contains(i) ? Color.blue : Color.white);
				}
			}

			// 기즈모 상태를 갱신한다
			m_oMoveTransGizmos.RefreshPositionAndRotation();
			m_oScaleTransGizmos.RefreshPositionAndRotation();
			m_oRotateTransGizmos.RefreshPositionAndRotation();

			// 라인 상태를 갱신한다 {
			var oObjInfo = (this.SelWayPointInfo != null) ? this.SelWayPointInfo.m_oOwner : this.SelObjInfo;
			m_oWayPointLine.gameObject.SetActive(oObjInfo != null && oObjInfo.m_oWayPointInfoList.Count >= 1);

			// 객체 정보가 존재 할 경우
			if (oObjInfo != null)
			{
				var oPosList = new List<Vector3>();
				oPosList.Add(new Vector3(oObjInfo.m_stTransInfo.m_stPos.m_fX, oObjInfo.m_stTransInfo.m_stPos.m_fZ, -oObjInfo.m_stTransInfo.m_stPos.m_fY - 0.1f));

				for (int i = 0; i < oObjInfo.m_oWayPointInfoList.Count; ++i)
				{
					oPosList.Add(new Vector3(oObjInfo.m_oWayPointInfoList[i].m_stTransInfo.m_stPos.m_fX, oObjInfo.m_oWayPointInfoList[i].m_stTransInfo.m_stPos.m_fZ, -oObjInfo.m_oWayPointInfoList[i].m_stTransInfo.m_stPos.m_fY - 0.1f));
				}

				m_oWayPointLine.positionCount = oPosList.Count;
				m_oWayPointLine.SetPositions(oPosList.ToArray());
			}
			// 라인 상태를 갱신한다 }

			// 범위 상태를 갱신한다
			for (int i = 0; i < m_oPrefabObjInfoList.Count; ++i)
			{
				var oHelpRange = m_oPrefabObjInfoList[i].m_oGameObj.transform.Find("HelpRange");
				oHelpRange?.gameObject.SetActive(m_bIsEnableHelpRangeInfo || m_oSelTransGizmosTargetObjIdxList.Contains(i));

				var oSighRange = m_oPrefabObjInfoList[i].m_oGameObj.transform.Find("SighRange");
				oSighRange?.gameObject.SetActive(m_bIsEnableSighRangeInfo || m_oSelTransGizmosTargetObjIdxList.Contains(i));
			}
		}

		/** 기즈모 타겟 트랜스 폼을 갱신한다 */
		private void UpdateGizmosTargetObjTrans()
		{
			// 선택 된 트랜스 폼 기즈모 타겟 객체가 없을 경우
			if (SelTransGizmosTargetObj == null)
			{
				return;
			}

			this.UpdateREUIsVec3InputUIs(m_stREUIsPosInputUIs, SelTransGizmosTargetObj.transform.localPosition);
			this.UpdateREUIsVec3InputUIs(m_stREUIsScaleInputUIs, SelTransGizmosTargetObj.transform.localScale);
			this.UpdateREUIsVec3InputUIs(m_stREUIsRotateInputUIs, SelTransGizmosTargetObj.transform.localEulerAngles);

			for (int i = 0; i < m_oSelTransGizmosTargetObjIdxList.Count; ++i)
			{
				int nIdx = m_oSelTransGizmosTargetObjIdxList[i];
				var oGameObj = m_oPrefabObjInfoList[nIdx].m_oGameObj;

				// 광원 객체 일 경우
				if (oGameObj == m_oLight.gameObject)
				{
					this.SelMapInfo.m_stLightInfo.m_stTransInfo = new STTransInfo(oGameObj.transform.localPosition,
						oGameObj.transform.localScale, oGameObj.transform.localEulerAngles);
				}
				// 플레이어 위치 일 경우
				else if (oGameObj == m_oPlayerPos)
				{
					this.SelMapInfo.m_stPlayerPos = oGameObj.transform.localPosition;
					this.SelMapInfo.m_stPlayerRotate = oGameObj.transform.localEulerAngles;
				}
				else
				{
					m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stTransInfo.m_stPos = oGameObj.transform.localPosition;
					m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stTransInfo.m_stScale = oGameObj.transform.localScale;
					m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stTransInfo.m_stRotate = oGameObj.transform.localEulerAngles;
				}
			}
		}

		/** 맵 정보를 추가한다 */
		private void AddMapInfo(int a_nStageID, int a_nChapterID, int a_nEpisodeID)
		{
			var oMapInfo = ComUtil.MakeMapInfo(m_eSelMapInfoType, a_nStageID, a_nChapterID, a_nEpisodeID);
			this.SetupDefValues(oMapInfo);

			this.SetSelMapInfo(oMapInfo);
			CMapInfoTable.Singleton.AddMapInfo(this.SelMapInfo, m_eSelMapInfoType);

			m_bIsDirtyUpdateUIsState = true;
		}

		/** 정보를 저장한다 */
		private void SaveInfo()
		{
			this.UpdateObjInfos();

			for (int i = (int)EMapInfoType.CAMPAIGN; i < (int)EMapInfoType.MAX_VAL; ++i)
			{
				// 맵 정보 타입이 유효하지 않을 경우
				if (!System.Enum.IsDefined(typeof(EMapInfoType), i))
				{
					continue;
				}

				this.SaveInfo((EMapInfoType)i);
			}
		}

		/** 정보를 저장한다 */
		private void SaveInfo(EMapInfoType a_eMapInfoType)
		{
			CMapInfoTable.Singleton.SaveMapInfos(a_eMapInfoType);
		}

		/** 객체 정보를 갱신한다 */
		private void UpdateObjInfos()
		{
			for (int i = 0; i < m_oPrefabObjInfoList.Count; ++i)
			{
				m_oPrefabObjInfoList[i].m_oObjInfo.m_stTransInfo.m_stPos = m_oPrefabObjInfoList[i].m_oGameObj.transform.localPosition;
				m_oPrefabObjInfoList[i].m_oObjInfo.m_stTransInfo.m_stScale = m_oPrefabObjInfoList[i].m_oGameObj.transform.localScale;
				m_oPrefabObjInfoList[i].m_oObjInfo.m_stTransInfo.m_stRotate = m_oPrefabObjInfoList[i].m_oGameObj.transform.localEulerAngles;
			}
		}

		/** 맵 정보를 복사한다 */
		private void CopyMapInfos(GameObject a_oScrollViewContents, STIDInfo a_stIDInfo)
		{
			// 스테이지 일 경우
			if (a_oScrollViewContents == m_oLEUIsStageScrollViewContents)
			{
				this.SetSelMapInfo(CMapInfoTable.Singleton.GetMapInfo(m_eSelMapInfoType, a_stIDInfo.m_nStageID, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID).Clone() as CMapInfo);
				this.SelMapInfo.m_stIDInfo.m_nStageID = CMapInfoTable.Singleton.GetNumStages(m_eSelMapInfoType, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID);

				CMapInfoTable.Singleton.AddMapInfo(this.SelMapInfo, m_eSelMapInfoType);
			}
			else
			{
				// 챕터 일 경우
				if (a_oScrollViewContents == m_oLEUIsChapterScrollViewContents)
				{
					int nNumChapters = CMapInfoTable.Singleton.GetNumChapters(m_eSelMapInfoType, a_stIDInfo.m_nEpisodeID);
					CMapInfoTable.Singleton.TryGetChapterMapInfos(m_eSelMapInfoType, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID, out Dictionary<int, CMapInfo> oChapterMapInfoDict);

					for (int i = 0; i < oChapterMapInfoDict.Count; ++i)
					{
						var oCloneMapInfo = oChapterMapInfoDict[i].Clone() as CMapInfo;
						oCloneMapInfo.m_stIDInfo.m_nChapterID = nNumChapters;

						CMapInfoTable.Singleton.AddMapInfo(oCloneMapInfo, m_eSelMapInfoType);
					}
				}
				// 에피소드 일 경우
				else if (a_oScrollViewContents == m_oLEUIsEpisodeScrollViewContents)
				{
					int nNumEpisodes = CMapInfoTable.Singleton.GetNumEpisodes(m_eSelMapInfoType);
					CMapInfoTable.Singleton.TryGetEpisodeMapInfos(m_eSelMapInfoType, a_stIDInfo.m_nEpisodeID, out Dictionary<int, Dictionary<int, CMapInfo>> oEpisodeMapInfoDictContainer);

					for (int i = 0; i < oEpisodeMapInfoDictContainer.Count; ++i)
					{
						for (int j = 0; j < oEpisodeMapInfoDictContainer[i].Count; ++j)
						{
							var oCloneMapInfo = oEpisodeMapInfoDictContainer[i][j].Clone() as CMapInfo;
							oCloneMapInfo.m_stIDInfo.m_nEpisodeID = nNumEpisodes;

							CMapInfoTable.Singleton.AddMapInfo(oCloneMapInfo, m_eSelMapInfoType);
						}
					}
				}

				int nStageID = 0;
				int nChapterID = (a_oScrollViewContents == m_oLEUIsChapterScrollViewContents) ? CMapInfoTable.Singleton.GetNumChapters(m_eSelMapInfoType, a_stIDInfo.m_nEpisodeID) - 1 : 0;
				int nEpisodeID = (a_oScrollViewContents == m_oLEUIsChapterScrollViewContents) ? CMapInfoTable.Singleton.GetNumEpisodes(m_eSelMapInfoType) - 1 : a_stIDInfo.m_nEpisodeID;

				this.SetSelMapInfo(CMapInfoTable.Singleton.GetMapInfo(m_eSelMapInfoType, nStageID, nChapterID, nEpisodeID));
			}

			this.SetSelMapInfo(this.SelMapInfo ?? CMapInfoTable.Singleton.GetMapInfo(m_eSelMapInfoType, 0, 0, 0));
			m_bIsDirtyUpdateUIsState = true;
		}

		/** 맵 정보를 이동한다 */
		private void MoveMapInfos(GameObject a_oScrollViewContents, STIDInfo a_stIDInfo)
		{
			m_eInputPopup = (a_oScrollViewContents == m_oLEUIsStageScrollViewContents) ? EInputPopup.STAGE_MOVE : EInputPopup.NONE;

			var stIDInfo = this.SelMapInfo.m_stIDInfo;
			var oInputPopup = m_oInputPopup.GetComponentInChildren<CInputPopup>();

			oInputPopup.StageInput.text = string.Empty;
			oInputPopup.ChapterInput.text = string.Empty;
			oInputPopup.EpisodeInput.text = string.Empty;

			this.ShowInputPopup((a_oInput) =>
			{
				// 스테이지 이동 일 경우
				if (m_eInputPopup == EInputPopup.STAGE_MOVE)
				{
					bool bIsValid01 = int.TryParse(oInputPopup.StageInput.text, NumberStyles.Any, null, out int nStageID);
					bool bIsValid02 = int.TryParse(oInputPopup.ChapterInput.text, NumberStyles.Any, null, out int nChapterID);
					bool bIsValid03 = int.TryParse(oInputPopup.EpisodeInput.text, NumberStyles.Any, null, out int nEpisodeID);

					nStageID = bIsValid01 ? Mathf.Max(0, nStageID - 1) : this.SelMapInfo.m_stIDInfo.m_nStageID;
					nChapterID = bIsValid02 ? Mathf.Max(0, nChapterID - 1) : this.SelMapInfo.m_stIDInfo.m_nChapterID;
					nEpisodeID = bIsValid03 ? Mathf.Max(0, nEpisodeID - 1) : this.SelMapInfo.m_stIDInfo.m_nEpisodeID;

					// 맵 정보가 존재 할 경우
					if (CMapInfoTable.Singleton.TryGetMapInfo(m_eSelMapInfoType, 0, nChapterID, nEpisodeID, out CMapInfo oMapInfo))
					{
						CMapInfoTable.Singleton.MoveMapInfo(m_eSelMapInfoType, stIDInfo, new STIDInfo(0, nChapterID, nEpisodeID));
						this.MoveMapInfos(a_oScrollViewContents, this.SelMapInfo.m_stIDInfo, nStageID + 1);
					}
				}
				// 식별자가 유효 할 경우
				else if (int.TryParse(a_oInput, NumberStyles.Any, null, out int nID))
				{
					this.MoveMapInfos(a_oScrollViewContents, a_stIDInfo, nID);
				}
			});
		}

		/** 맵 정보를 이동한다 */
		private void MoveMapInfos(GameObject a_oScrollViewContents, STIDInfo a_stIDInfo, int a_nDestID)
		{
			// 스테이지 일 경우
			if (a_oScrollViewContents == m_oLEUIsStageScrollViewContents)
			{
				int nNumStages = CMapInfoTable.Singleton.GetNumStages(m_eSelMapInfoType, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID);
				CMapInfoTable.Singleton.MoveMapInfo(m_eSelMapInfoType, a_stIDInfo.m_nStageID, Mathf.Clamp(a_nDestID, 1, nNumStages) - 1, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID);
			}
			// 챕터 일 경우
			else if (a_oScrollViewContents == m_oLEUIsChapterScrollViewContents)
			{
				int nNumChapters = CMapInfoTable.Singleton.GetNumChapters(m_eSelMapInfoType, a_stIDInfo.m_nEpisodeID);
				CMapInfoTable.Singleton.MoveChapterMapInfos(m_eSelMapInfoType, a_stIDInfo.m_nChapterID, Mathf.Clamp(a_nDestID, 1, nNumChapters) - 1, a_stIDInfo.m_nEpisodeID);
			}
			// 에피소드 일 경우
			else if (a_oScrollViewContents == m_oLEUIsEpisodeScrollViewContents)
			{
				int nNumEpisodes = CMapInfoTable.Singleton.GetNumEpisodes(m_eSelMapInfoType);
				CMapInfoTable.Singleton.MoveEpisodeMapInfos(m_eSelMapInfoType, a_stIDInfo.m_nEpisodeID, Mathf.Clamp(a_nDestID, 1, nNumEpisodes) - 1);
			}

			m_bIsDirtyUpdateUIsState = true;
		}

		/** 맵 정보를 제거한다 */
		private void RemoveMapInfos(GameObject a_oScrollViewContents, STIDInfo a_stIDInfo)
		{
			// 스테이지 일 경우
			if (a_oScrollViewContents == m_oLEUIsStageScrollViewContents)
			{
				CMapInfoTable.Singleton.RemoveMapInfo(m_eSelMapInfoType, a_stIDInfo.m_nStageID, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID);
			}
			// 챕터 일 경우
			else if (a_oScrollViewContents == m_oLEUIsChapterScrollViewContents)
			{
				CMapInfoTable.Singleton.RemoveChapterMapInfos(m_eSelMapInfoType, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID);
			}
			// 에피소드 일 경우
			else if (a_oScrollViewContents == m_oLEUIsEpisodeScrollViewContents)
			{
				CMapInfoTable.Singleton.RemoveEpisodeMapInfos(m_eSelMapInfoType, a_stIDInfo.m_nEpisodeID);
			}

			var oMapInfoWrapper = CMapInfoTable.Singleton.MapInfoWrapperDict[m_eSelMapInfoType];

			// 맵 정보가 없을 경우
			if (oMapInfoWrapper.m_oMapInfoDictContainer.Count <= 0)
			{
				this.AddMapInfo(0, 0, 0);
			}
			else
			{
				var oIDInfoList = new List<(STIDInfo, STIDInfo, bool)>() {
					(new STIDInfo(a_stIDInfo.m_nStageID - 1, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID), new STIDInfo(a_stIDInfo.m_nStageID, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID), a_oScrollViewContents == m_oLEUIsStageScrollViewContents),
					(new STIDInfo(0, a_stIDInfo.m_nChapterID - 1, a_stIDInfo.m_nEpisodeID), new STIDInfo(0, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID), a_oScrollViewContents == m_oLEUIsChapterScrollViewContents),
					(new STIDInfo(0, 0, a_stIDInfo.m_nEpisodeID - 1), new STIDInfo(0, 0, a_stIDInfo.m_nEpisodeID), a_oScrollViewContents == m_oLEUIsEpisodeScrollViewContents)
				};

				CMapInfo oMapInfo = null;

				for (int i = 0; i < oIDInfoList.Count; ++i)
				{
					// 맵 정보가 없을 경우
					if (oMapInfo == null || oIDInfoList[i].Item3)
					{
						this.TryGetMapInfo(oIDInfoList[i].Item1, oIDInfoList[i].Item2, out oMapInfo);
					}
				}

				this.SetSelMapInfo(oMapInfo);
			}

			m_bIsDirtyUpdateUIsState = true;
		}

		/** 프리팹 그룹을 탐색한다 */
		private (int, int, int) FindPrefabGroup(string a_oName)
		{
			for (int i = 0; i < m_oPrefabEditorInfoList.Count; ++i)
			{
				var oPrefabListContainer = m_oPrefabEditorInfoList[i].m_oPrefabListContainer;

				for (int j = 0; j < oPrefabListContainer.Count; ++j)
				{
					for (int k = 0; k < oPrefabListContainer[j].Count; ++k)
					{
						// 프리팹이 존재 할 경우
						if (oPrefabListContainer[j][k].name.Equals(a_oName))
						{
							return (i, j, k);
						}
					}
				}
			}

			return (-1, -1, -1);
		}

		/** 단축키를 처리한다 */
		private void HandleHotKeys()
		{
			// 단축키 처리가 불가능 할 경우
			if (m_eCurPopup != EPopup.NONE || Input.GetMouseButton((int)EMouseBtn.LEFT) || Input.GetMouseButton((int)EMouseBtn.RIGHT))
			{
				return;
			}

			// 편집 취소 키를 눌렀을 경우
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				this.AddTransGizmosTargetObj(null, true);
			}

			// 이동 툴 키를 눌렀을 경우
			if (Input.GetKeyDown(KeyCode.W))
			{
				this.SetTransGizmosType(ETransGizmosType.MOVE);
			}
			// 비율 키를 눌렀을 경우
			else if (Input.GetKeyDown(KeyCode.R))
			{
				this.SetTransGizmosType(ETransGizmosType.SCALE);
			}
			// 회전 키를 눌렀을 경우
			else if (Input.GetKeyDown(KeyCode.E))
			{
				this.SetTransGizmosType(ETransGizmosType.ROTATE);
			}

			// 되돌리기 키를 눌렀을 경우
			if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Z))
			{
				this.UpdateGizmosTargetObjTrans();
			}
			// 카메라 리셋 키를 눌렀을 경우
			else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.F))
			{
				Camera.main.GetComponent<CCameraController>()?.ResetTransState();
			}

			// 기즈모 타겟이 존재 할 경우
			if (m_oSelTransGizmosTargetObjIdxList.ExIsValid())
			{
				// 제거 키를 눌렀을 경우
				if (this.IsDownDelKey())
				{
					this.OnTouchREUIsRemoveObjBtn();
				}
				// 추가 키를 눌렀을 경우
				else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.A))
				{
					this.OnTouchREUIsAddWayPointBtn();
				}
				// 복사 키를 눌렀을 경우
				else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
				{
					this.OnTouchREUIsCopyObjBtn();
				}
				// 변경 키를 눌렀을 경우
				else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
				{
					this.OnTouchREUIsChangeObjBtn();
				}
				// 템플릿 추가 키를 눌렀을 경우
				else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Q))
				{
					this.OnTouchREUIsAddMapObjTemplateBtn();
				}
				// 기준 키를 눌렀을 경우
				else if (Input.GetKey(KeyCode.A))
				{
					m_oMoveTransGizmos.SetTransformPivot(GizmoObjectTransformPivot.ObjectGroupCenter);
					m_oScaleTransGizmos.SetTransformPivot(GizmoObjectTransformPivot.ObjectGroupCenter);
					m_oRotateTransGizmos.SetTransformPivot(GizmoObjectTransformPivot.ObjectGroupCenter);
				}
				else
				{
					this.SetGizmosPivot(m_eSelGizmosPivot, false);
				}
			}

			// 전체 선택 키를 눌렀을 경우
			if (Input.GetKey(ComType.CtrlKeyCode) && Input.GetKeyDown(KeyCode.A))
			{
				this.AddTransGizmosTargetObj(null, true);

				for (int i = 0; i < m_oPrefabObjInfoList.Count; ++i)
				{
					var oGameObj = m_oPrefabObjInfoList[i].m_oGameObj;

					// 광원 객체 일 경우
					if (oGameObj == m_oLight.gameObject)
					{
						continue;
					}

					this.AddTransGizmosTargetObj(m_oPrefabObjInfoList[i].m_oGameObj, false);
				}
			}

			// 입력 변경 키를 눌렀을 경우
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				m_nREUIsSelEditorInputIdx = (m_nREUIsSelEditorInputIdx + 1) % m_oREUIsEditorInputList.Count;
				m_nREUIsSelInspectorInputIdx = (m_nREUIsSelInspectorInputIdx + 1) % m_oREUIsInspectorInputList.Count;

				// 기즈모 타겟이 존재 할 경우
				if (this.SelTransGizmosTargetObj != null)
				{
					EventSystem.current.SetSelectedGameObject(m_oREUIsInspectorInputList[m_nREUIsSelInspectorInputIdx].gameObject);
				}
				else
				{
					EventSystem.current.SetSelectedGameObject(m_oREUIsEditorInputList[m_nREUIsSelEditorInputIdx].gameObject);
				}
			}
		}

		/** 프리팹 객체를 제거한다 */
		private void RemovePrefabObj(GameObject a_oGameObj)
		{
			int nIdx = m_oPrefabObjInfoList.FindIndex((a_stPrefabObjInfo) => a_stPrefabObjInfo.m_oGameObj == a_oGameObj);
			GameResourceManager.Singleton.ReleaseObject(a_oGameObj);

			// 프리팹 객체 정보가 존재 할 경우
			if (nIdx >= 0)
			{
				var stPrefabObjInfo = m_oPrefabObjInfoList[nIdx];
				m_oPrefabObjInfoList.RemoveAt(nIdx);

				// 이동 지점 정보 일 경우
				if (stPrefabObjInfo.m_oObjInfo is CWayPointInfo)
				{
					var oWayPointInfo = stPrefabObjInfo.m_oObjInfo as CWayPointInfo;
					oWayPointInfo.m_oOwner.m_oWayPointInfoList.Remove(oWayPointInfo);
				}
				else
				{
					this.RemoveAllWayPointPrefabObjs(stPrefabObjInfo.m_oObjInfo);
					this.SelMapInfo.m_oMapObjInfoList.Remove(stPrefabObjInfo.m_oObjInfo);
				}
			}
		}

		/** 모든 이동 지점 프리팹 객체를 제거한다 */
		private void RemoveAllWayPointPrefabObjs(CObjInfo a_oObjInfo)
		{
			while (a_oObjInfo.m_oWayPointInfoList.Count >= 1)
			{
				int nResult = m_oPrefabObjInfoList.FindIndex((a_stPrefabObjInfo) => a_stPrefabObjInfo.m_oObjInfo == a_oObjInfo.m_oWayPointInfoList[0]);

				// 이동 지점 정보가 없을 경우
				if (nResult < 0)
				{
					break;
				}

				this.RemovePrefabObj(m_oPrefabObjInfoList[nResult].m_oGameObj);
			}
		}
		#endregion // 함수

		#region 접근 함수
		/** 고유 객체 여부를 검사한다 */
		private bool IsUniqueGameObj(GameObject a_oGameObj)
		{
			return a_oGameObj == m_oLight.gameObject || a_oGameObj == m_oPlayerPos;
		}

		/** 이동 지점 추가 가능 여부를 검사한다 */
		private bool IsEnableAddWayPoint()
		{
			// 선택 된 객체가 없을 경우
			if (m_oSelTransGizmosTargetObjIdxList.Count != 1)
			{
				return false;
			}

			int nIdx = m_oSelTransGizmosTargetObjIdxList[0];
			return m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stPrefabInfo.m_eResType == EResourceType.Character;
		}

		/** 시작 지점 추가 가능 여부를 검사한다 */
		private bool IsEnableAddStartPoint()
		{
			// 선택 된 객체가 없을 경우
			if (m_oSelTransGizmosTargetObjIdxList.Count != 1)
			{
				return false;
			}

			int nIdx = m_oSelTransGizmosTargetObjIdxList[0];

			return m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stPrefabInfo.m_oName.Equals("SpawnPos") &&
				m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_oWayPointInfoList.Count <= 0;
		}
		#endregion // 접근 함수

		#region 팩토리 함수
		/** 프리팹 객체 정보를 생성한다 */
		private STPrefabObjInfo MakePrefabObjInfo(uint a_nKey, int a_nTheme, int a_nGroup, int a_nPrefab, EResourceType a_eResType, GameObject a_oGameObj, CObjInfo a_oObjInfo = null)
		{
			var stTransInfo = new STTransInfo(a_oGameObj.transform.position, a_oGameObj.transform.localScale, a_oGameObj.transform.localEulerAngles);
			var stPrefabInfo = new STPrefabInfo(a_nKey, a_nTheme, a_oGameObj.name, a_eResType);

			// 객체 정보가 없을 경우
			if (a_oObjInfo == null)
			{
				a_oObjInfo = ComUtil.MakeObjInfo(stTransInfo, stPrefabInfo);
				a_oObjInfo.m_stPrefabInfo.m_nTheme = a_nTheme;

				this.SetupDefControlValues(a_oObjInfo);
			}

			return new STPrefabObjInfo()
			{
				m_nTheme = a_nTheme,
				m_nGroup = a_nGroup,
				m_nPrefab = a_nPrefab,
				m_oGameObj = a_oGameObj,
				m_oObjInfo = a_oObjInfo
			};
		}

		/** 이동 지점 프리팹 객체 정보를 생성한다 */
		private STPrefabObjInfo MakeWayPointPrefabObjInfo(int a_nTheme, int a_nGroup, int a_nPrefab, EResourceType a_eResType, GameObject a_oGameObj, CWayPointInfo a_oWayPointInfo = null)
		{
			var stTransInfo = new STTransInfo(a_oGameObj.transform.position, a_oGameObj.transform.localScale, a_oGameObj.transform.localEulerAngles);
			var stPrefabInfo = new STPrefabInfo(0, 0, a_oGameObj.name, a_eResType);

			// 이동 지점 정보가 없을 경우
			if (a_oWayPointInfo == null)
			{
				a_oWayPointInfo = ComUtil.MakeWayPointInfo(stTransInfo, stPrefabInfo);
				this.SetupDefControlValues(a_oWayPointInfo);
			}

			return this.MakePrefabObjInfo(0, a_nTheme, a_nGroup, a_nPrefab, a_eResType, a_oGameObj, a_oWayPointInfo);
		}
		#endregion // 팩토리 함수
	}

	/** 맵 에디터 페이지 - 접근 */
	public partial class PageMapEditor : UIDialog
	{
		#region 함수
		/** 제거 키 눌림 여부를 검사한다 */
		private bool IsDownDelKey()
		{
#if UNITY_EDITOR_WIN
			return Input.GetKeyDown(ComType.DelKeyCode);
#elif UNITY_EDITOR_OSX
			return Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(ComType.DelKeyCode);
#else
			// 윈도우즈 플랫폼 일 경우
			if (Application.platform == RuntimePlatform.WindowsPlayer)
			{
				return Input.GetKeyDown(ComType.DelKeyCode);
			}
			
			return Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(ComType.DelKeyCode);
#endif // #if UNITY_EDITOR_WIN
		}

		/** 맵 정보를 반환한다 */
		private bool TryGetMapInfo(STIDInfo a_stPrevIDInfo, STIDInfo a_stNextIDInfo, out CMapInfo a_oOutMapInfo)
		{
			CMapInfoTable.Singleton.TryGetMapInfo(m_eSelMapInfoType, a_stPrevIDInfo.m_nStageID, a_stPrevIDInfo.m_nChapterID, a_stPrevIDInfo.m_nEpisodeID, out CMapInfo oPrevMapInfo);
			CMapInfoTable.Singleton.TryGetMapInfo(m_eSelMapInfoType, a_stNextIDInfo.m_nStageID, a_stNextIDInfo.m_nChapterID, a_stNextIDInfo.m_nEpisodeID, out CMapInfo oNextMapInfo);

			a_oOutMapInfo = oPrevMapInfo ?? oNextMapInfo;
			return oPrevMapInfo != null || oNextMapInfo != null;
		}

		/** 모드를 변경한다 */
		private void SetMode(EMode a_eMode)
		{
			m_eSelMode = a_eMode;
			m_bIsDirtyUpdateUIsState = true;
		}

		/** 기즈모 타입을 변경한다 */
		private void SetTransGizmosType(ETransGizmosType a_eType)
		{
			m_eSelTransGizmosType = a_eType;
			m_bIsDirtyUpdateUIsState = true;
		}

		/** 기즈모 공간을 변경한다 */
		private void SetGizmosSpace(GizmoSpace a_eSpace)
		{
			m_eSelGizmosSpace = a_eSpace;
			m_bIsDirtyUpdateUIsState = true;

			m_oMoveTransGizmos.SetTransformSpace(a_eSpace);
			m_oScaleTransGizmos.SetTransformSpace(a_eSpace);
			m_oRotateTransGizmos.SetTransformSpace(a_eSpace);
		}

		/** 기즈모 기준을 변경한다 */
		private void SetGizmosPivot(GizmoObjectTransformPivot a_ePivot, bool a_bIsUpdateUIs = true)
		{
			m_eSelGizmosPivot = a_ePivot;
			m_bIsDirtyUpdateUIsState = m_bIsDirtyUpdateUIsState || a_bIsUpdateUIs;

			m_oMoveTransGizmos.SetTransformPivot(a_ePivot);
			m_oScaleTransGizmos.SetTransformPivot(a_ePivot);
			m_oRotateTransGizmos.SetTransformPivot(a_ePivot);
		}

		/** 순찰 타입을 변경한다 */
		private void SetPatrolType(EPatrolType a_eType)
		{
			for (int i = 0; i < m_oSelTransGizmosTargetObjIdxList.Count; ++i)
			{
				int nIdx = m_oSelTransGizmosTargetObjIdxList[i];
				m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_ePatrolType = a_eType;
			}

			m_bIsDirtyUpdateUIsState = true;
		}

		/** 이동 지점 타입을 변경한다 */
		private void SetWayPointType(EWayPointType a_eType)
		{
			for (int i = 0; i < m_oSelTransGizmosTargetObjIdxList.Count; ++i)
			{
				int nIdx = m_oSelTransGizmosTargetObjIdxList[i];
				var oWayPointInfo = m_oPrefabObjInfoList[nIdx].m_oObjInfo as CWayPointInfo;

				// 이동 지점 정보가 아닐 경우
				if (oWayPointInfo == null)
				{
					continue;
				}

				oWayPointInfo.m_eWayPointType = a_eType;
			}

			m_bIsDirtyUpdateUIsState = true;
		}

		/** 회전 복귀 여부를 변경한다 */
		private void SetIsReturnRotate(bool a_bIsReturnRotate01, bool a_bIsReturnRotate02)
		{
			// 객체 정보가 존재 할 경우
			if (this.SelObjInfo != null)
			{
				this.SelObjInfo.m_bIsReturnRotate01 = a_bIsReturnRotate01;
				this.SelObjInfo.m_bIsReturnRotate02 = a_bIsReturnRotate02;
			}

			m_bIsDirtyUpdateUIsState = true;
		}

		/** 선택 맵 정보를 변경한다 */
		private void SetSelMapInfo(CMapInfo a_oMapInfo)
		{
			this.SelMapInfo = a_oMapInfo;
			m_bIsDirtyResetPrefabObject = true;
		}
		#endregion // 함수
	}
}
#endif //#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
