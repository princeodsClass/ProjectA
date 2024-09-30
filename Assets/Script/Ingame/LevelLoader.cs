using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public partial class LevelLoader : MonoBehaviour
{
	Transform m_Tracked;
	GameObject pre, obj = null;

	NavMeshData navMeshData;
	NavMeshDataInstance navMeshDataInstance;

	Vector3 m_Size;
	List<NavMeshBuildSource> m_Sources = new List<NavMeshBuildSource>();

	void Awake()
	{
		NavMesh.RemoveAllNavMeshData();
		Transform t = GameObject.Find(ComType.OBJECT_ROOT_NAME).transform;

		int s = GameManager.Singleton.user.m_nEpisode;
		int c = GameManager.Singleton.user.m_nChapter;

		#region 추가
#if DEBUG || DEVELOPMENT_BUILD
		// 플레이 맵 정보가 없을 경우
		if (GameDataManager.Singleton.PlayMapInfo == null)
		{
			CMapInfoTable.Singleton.LoadMapInfos();

			GameDataManager.Singleton.SetupPlayMapInfo(EMapInfoType.CAMPAIGN,
				EPlayMode.TEST, CMapInfoTable.Singleton.GetChapterMapInfos(EMapInfoType.CAMPAIGN, 0, 0));
		}
#endif // #if DEBUG || DEVELOPMENT_BUILD

		// 광원을 설정한다 {
		var oLight = GameObject.Find("Directional Light").GetComponent<Light>();
		oLight.color = GameDataManager.Singleton.PlayMapInfo.m_stLightInfo.m_stColor;
		oLight.intensity = GameDataManager.Singleton.PlayMapInfo.m_stLightInfo.m_fIntensity;

		oLight.transform.localPosition = GameDataManager.Singleton.PlayMapInfo.m_stLightInfo.m_stTransInfo.m_stPos;
		oLight.transform.localScale = GameDataManager.Singleton.PlayMapInfo.m_stLightInfo.m_stTransInfo.m_stScale;
		oLight.transform.localEulerAngles = GameDataManager.Singleton.PlayMapInfo.m_stLightInfo.m_stTransInfo.m_stRotate;
		// 광원을 설정한다 }

		this.MapObjsRootList.Clear();

		switch (GameDataManager.Singleton.PlayMapInfoType)
		{
			case EMapInfoType.ABYSS: this.SetupMapObjs(); break;
			default: this.SetupMapObjs(GameDataManager.Singleton.PlayMapInfo, true); break;
		}

#if NEVER_USE_THIS
		// 기존 구문
        string n = $"{s.ToString("X4")}{c.ToString("X4")}";

        pre = GameResourceManager.Singleton.CreateObject(EResourceType.ObjectLevel, n, t);
#endif // #if NEVER_USE_THIS
		#endregion // 추가

		if (m_Tracked == null) m_Tracked = transform;
	}

	void OnEnable()
	{
		navMeshData = new NavMeshData();
		m_Size = new Vector3(100000.0f, 500.0f, 500.0f);

		navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData);
		NavMeshSourceTag.Collect(ref m_Sources);

		NavMeshBuildSettings defaultBuildSettings = NavMesh.GetSettingsByID(0);

		Bounds bounds = CalculateBounds();

		NavMeshBuilder.UpdateNavMeshData(navMeshData, defaultBuildSettings, m_Sources, bounds);
		//NavMeshBuilder.BuildNavMeshData(defaultBuildSettings, m_Sources, bounds, Vector3.zero, Quaternion.identity);

		for(int i = 0; i < this.InteractableMapObjList.Count; ++i)
		{
			this.InteractableMapObjList[i].gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		Destroy(pre);
	}

	Bounds CalculateBounds()
	{
		var center = m_Tracked ? m_Tracked.position : transform.position;
		return new Bounds(Quantize(center, 0.1f * m_Size), m_Size); // m_Size), m_Size);
	}

	static Vector3 Quantize(Vector3 v, Vector3 quant)
	{
		float x = quant.x * Mathf.Floor(v.x / quant.x);
		float y = quant.y * Mathf.Floor(v.y / quant.y);
		float z = quant.z * Mathf.Floor(v.z / quant.z);
		return new Vector3(x, y, z);
	}

	void OnDrawGizmosSelected()
	{
		if (navMeshData)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(navMeshData.sourceBounds.center, navMeshData.sourceBounds.size);
		}

		Gizmos.color = Color.yellow;
		var bounds = CalculateBounds();
		Gizmos.DrawWireCube(bounds.center, bounds.size);

		Gizmos.color = Color.green;
		var center = m_Tracked ? m_Tracked.position : transform.position;
		Gizmos.DrawWireCube(center, m_Size);
	}
}

#region 추가
/** 레벨 배치 */
public partial class LevelLoader : MonoBehaviour
{
	#region 프로퍼티
	public List<Bounds> BoundsList { get; } = new List<Bounds>();
	public List<GameObject> MapObjsRootList { get; } = new List<GameObject>();
	private List<GameObject> InteractableMapObjList { get; } = new List<GameObject>();

	public PageBattle PageBattle => MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle);
	public BattleController BattleController => this.PageBattle?.BattleController;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Start()
	{
		var oNavMeshControllerObj = new GameObject(typeof(CNavMeshController).ToString());
		oNavMeshControllerObj.transform.SetParent(this.transform, false);

		var oNavMeshController = oNavMeshControllerObj.AddComponent<CNavMeshController>();
		oNavMeshController.Init(CNavMeshController.MakeParams(NavMesh.CalculateTriangulation()));

		this.BattleController.SetNavMeshController(oNavMeshController);
	}

	/** 내비게이션 메쉬 상태를 갱신한다 */
	public void UpdateNavMeshState()
	{
		NavMesh.RemoveAllNavMeshData();

		navMeshData = new NavMeshData();
		m_Size = new Vector3(100000.0f, 500.0f, 500.0f);

		navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData);
		NavMeshSourceTag.Collect(ref m_Sources);

		NavMeshBuildSettings defaultBuildSettings = NavMesh.GetSettingsByID(0);

		Bounds bounds = CalculateBounds();
		NavMeshBuilder.UpdateNavMeshData(navMeshData, defaultBuildSettings, m_Sources, bounds);
	}

	/** 맵 객체를 설정한다 */
	private void SetupMapObjs()
	{
		for (int i = 0; i < GameDataManager.Singleton.PlayMapInfoDict.Count; ++i)
		{
			this.SetupMapObjs(GameDataManager.Singleton.PlayMapInfoDict[i]);
		}
	}

	/** 맵 객체를 설정한다 */
	private void SetupMapObjs(CMapInfo a_oMapInfo, bool a_bIsIgnoreRootPos = false)
	{
		string oMapObjsRootName = string.Format(ComType.MAP_OBJECT_ROOT_NAME_FMT,
			a_oMapInfo.m_stIDInfo.m_nStageID + 1);

		var oObjsRoot = GameObject.Find(ComType.OBJECT_ROOT_NAME);
		var oMapObjsRoot = new GameObject(oMapObjsRootName);

		oMapObjsRoot.transform.SetParent(oObjsRoot.transform, false);
		oMapObjsRoot.transform.position = a_bIsIgnoreRootPos ? Vector3.zero : new Vector3(500.0f * a_oMapInfo.m_stIDInfo.m_nStageID, 0.0f, 0.0f);

		for (int i = 0; i < a_oMapInfo.m_oMapObjInfoList.Count; ++i)
		{
			int nTheme = a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_nTheme - 1;
			var eResType = a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_eResType;

			// 객체 설정이 불가능 할 경우
			if (eResType < EResourceType.BG_Walkable || eResType >= EResourceType.Character_NPC)
			{
				continue;
			}

			string oPrefabPath = GameResourceManager.Singleton.GetPrefabPath(eResType,
				a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName, nTheme);

			// 프리팹이 없을 경우
			if (!ComUtil.TryLoadRes<GameObject>(oPrefabPath, out GameObject oPrefabObj))
			{
				nTheme += 1;

				oPrefabPath = GameResourceManager.Singleton.GetPrefabPath(eResType, 
					a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName, nTheme);
			}

			// 프리팹이 없을 경우
			if (!ComUtil.TryLoadRes<GameObject>(oPrefabPath, out oPrefabObj))
			{
				GameManager.Log($"{a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName} 프리팹이 존재하지 않습니다!");
				continue;
			}

			var oGameObj = GameResourceManager.Singleton.CreateObject(eResType,
				a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName, oMapObjsRoot.transform, a_nTheme: nTheme);

			oGameObj.isStatic = !a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Contains("FIELD_OBJECT_FIELDOBJECT_");

			oGameObj.transform.localPosition = a_oMapInfo.m_oMapObjInfoList[i].m_stTransInfo.m_stPos;
			oGameObj.transform.localScale = a_oMapInfo.m_oMapObjInfoList[i].m_stTransInfo.m_stScale;
			oGameObj.transform.localEulerAngles = a_oMapInfo.m_oMapObjInfoList[i].m_stTransInfo.m_stRotate;

			// 상호 작용 맵 객체 일 경우
			if(a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Contains("FIELD_OBJECT_FIELDOBJECT_"))
			{
				this.InteractableMapObjList.Add(oGameObj);
			}

			// 충돌체가 존재 할 경우
			if (oGameObj.TryGetComponent(out Collider oCollider))
			{
				bool bIsEnable = eResType == EResourceType.BG_Temp;
				bIsEnable = bIsEnable || a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Contains("FIELD_OBJECT_FIELDOBJECT_");

				oCollider.enabled = bIsEnable || (eResType >= EResourceType.BG_Walkable && eResType <= EResourceType.BG_NotWalkable);
			}
			
			// 메시 렌더러가 존재 할 경우
			if (oGameObj.TryGetComponent(out MeshRenderer oRenderer))
			{
				bool bIsValid01 = a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_eResType != EResourceType.BG_Temp;
				bool bIsValid02 = !a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Equals(ComType.G_BG_N_TEMP_CUBE);
				bool bIsValid03 = !a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Equals("SpawnPos");
				bool bIsValid04 = !a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Equals("BG_BoxSpawnPoint");
				bool bIsValid05 = !a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Equals("BG_PlayerPos");
				bool bIsValid06 = !a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Equals("BG_RouletteSpawnPoint");
				bool bIsValid07 = !a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Equals("BG_StartPoint");
				bool bIsValid08 = !a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Equals("BG_WayPoint");
				bool bIsValid09 = !a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Equals("BG_Light");

				oRenderer.enabled = bIsValid01 && 
					bIsValid02 && bIsValid03 && bIsValid04 && bIsValid05 && bIsValid06 && bIsValid07 && bIsValid08 && bIsValid09;
			}

			// 스폰 위치 처리자가 존재 할 경우
			if(oGameObj.TryGetComponent(out CSpawnPosHandler oSpawnPosHandler))
			{
				oSpawnPosHandler.SetObjInfo(a_oMapInfo.m_oMapObjInfoList[i]);
			}
		}

		pre = oMapObjsRoot;
		this.MapObjsRootList.Add(oMapObjsRoot);
	}
	#endregion // 함수
}
#endregion // 추가
