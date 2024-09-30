using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public partial class BattleController : MonoBehaviour
{
#if UNITY_EDITOR
	[SerializeField] private GameObject m_oEditorOriginSighRange = null;
	public GameObject EditorOriginSighRange => m_oEditorOriginSighRange;
#endif // #if UNITY_EDITOR

	GameManager gameManager;
	UserAccount account;

	GameObject _Player;
	Transform _pathObjRoot;

	int _curChapter, _curEpisode;
	bool _isStart;

	private void Awake()
	{
		if (gameManager == null) gameManager = GameManager.Singleton;
		if (account == null) account = gameManager.user;

		#region 추가
		this.StateMachine.SetOwner(this);
		this.PrevKillSoundPlayTime = System.DateTime.Today;
		_pathObjRoot = GameObject.Find(ComType.OBJECT_ROOT_NAME).transform;

		var oLevelLoader = GameObject.Find(ComType.LEVEL_LOADER_NAME);
		this.NPCEffectTableList.Clear();

		m_oNavMeshPath = new NavMeshPath();
		m_nWalkableAreaMask = 1 << NavMesh.GetAreaFromName(ComType.G_NAV_MESH_AREA_WALKABLE);

		this.LevelLoader = oLevelLoader.GetComponentInChildren<LevelLoader>();
		this.LevelLoader.MapObjsRootList.ExCopyTo(this.MapObjsRootList, (a_oMapObjsRoot) => a_oMapObjsRoot);

		// 캠페인 무한 모드 일 경우
		if (GameDataManager.Singleton.IsCampaignInfiniteWaveMode())
		{
			int nChapterID = GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nChapterID;
			int nEpisodeID = GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID;

			var nChapterTableKey = ComUtil.GetChapterKey(nEpisodeID + 1, nChapterID + 1);
			var oCampaignChapterTable = ChapterTable.GetData(nChapterTableKey);

			this.MaxInfinitePoint = oCampaignChapterTable.TargetEndPoint;
		}

		switch (GameDataManager.Singleton.PlayMapInfoType)
		{
			case EMapInfoType.ABYSS:
				uint nAbyssTableKey = ComUtil.GetAbyssKey(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID,
					GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nChapterID + 1);

				var oAbyssTable = AbyssTable.GetData(nAbyssTableKey);
				this.MaxAbyssPoint = oAbyssTable.TargetPoint;

				this.MaxPlayTime = oAbyssTable.LimitTime * ComType.G_UNIT_MS_TO_S;
				this.RemainPlayTime = this.MaxPlayTime;

				var oEffectGroupTableList = EffectGroupTable.GetGroup(oAbyssTable.BonusBuffGroup);

				for (int i = 0; i < oEffectGroupTableList.Count; ++i)
				{
					var oEffectTable = EffectTable.GetData(oEffectGroupTableList[i].EffectKey);
					this.NPCEffectTableList.Add(oEffectTable);
				}

				break;
			case EMapInfoType.BONUS:
				this.MaxPlayTime = GameManager.Singleton.user.GetMaxBattlePlayTimeBonus(GameManager.Singleton.user.m_nLevel) * ComType.G_UNIT_MS_TO_S;
				this.RemainPlayTime = this.MaxPlayTime;

				break;
		}

		this.InitializeCamera();
		this.InitializeNonPlayerTables();
		this.InitializeNonPlayers();

		_floatingJoystick = GameObject.Find("Joystick").GetComponent<FloatingJoystick>();
		MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle)?.SetBattleController(this);
		#endregion // 추가

#if NEVER_USE_THIS
		InitializeCampaignInfo();
		InitializePlayer();
		InitializeWeapon();
		InitializeCamera();

		#region 추가
		this.State = EState.PLAY;
		this.InitializeNonPlayers();

		ComUtil.SetTimeScale(1.0f);
		StopCoroutine("CoCustomUpdate");
		StartCoroutine(this.CoCustomUpdate());
		#endregion // 추가
#endif // #if NEVER_USE_THIS
	}

	public void Start()
	{
		InitializeCampaignInfo();
		InitializePlayer();
		InitializeWeapon();

		#region 추가
		this.SetupNavMeshCameraBounds();
		this.StateMachine.SetState(this.CreateReadyState());

		m_oNumKillsStr = UIStringTable.GetValue("ui_component_mission_zombie_count");
		m_nMaxNumReleaseUnitStacks = GlobalTable.GetData<int>(ComType.G_COUNT_KEEP_CORPSE);

		this.IsBonusStage = GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.BONUS;
		Physics.gravity = new Vector3(0.0f, -GlobalTable.GetData<float>(ComType.G_VALUE_GRAVITY_FOR_GRENADE), 0.0f);

		ComUtil.SetTimeScale(1.0f);
		StopCoroutine("CoCustomUpdate");
		StartCoroutine(this.CoCustomUpdate());

		this.UpdateInfiniteWaveUIsState();
		int nBonusNPSAppearEpisode = GlobalTable.GetData<int>(ComType.G_VALUE_SAMI_BOSS_START_EPISODE) - 1;

		bool bIsEnableAppearBonusNPC = GameDataManager.Singleton.BonusNPCAppearStageID >= 0;
		bIsEnableAppearBonusNPC = bIsEnableAppearBonusNPC && GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID >= nBonusNPSAppearEpisode;
		bIsEnableAppearBonusNPC = bIsEnableAppearBonusNPC && GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nStageID == GameDataManager.Singleton.BonusNPCAppearStageID;

		for (int i = 0; i < this.InteractableMapObjHandlerList.Count; ++i)
		{
			this.InteractableMapObjHandlerList[i].SetInteractableTarget(this.PlayerController.gameObject);
		}

		// 캠페인 모드 일 경우
		if (GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.CAMPAIGN || 
			GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.TUTORIAL)
		{
			uint nEpisodeKey = ComUtil.GetEpisodeKey(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID + 1);
			EpisodeTable oEpisodeTable = EpisodeTable.GetData(nEpisodeKey);

			bIsEnableAppearBonusNPC = bIsEnableAppearBonusNPC &&
				GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nChapterID == (oEpisodeTable.MaxChapter / 2 - 1);
		}

#if UNITY_EDITOR
		bIsEnableAppearBonusNPC = bIsEnableAppearBonusNPC && GameDataManager.Singleton.PlayMode != EPlayMode.HUNT;
		bIsEnableAppearBonusNPC = bIsEnableAppearBonusNPC && GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.CAMPAIGN;
		bIsEnableAppearBonusNPC = bIsEnableAppearBonusNPC && GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.TUTORIAL;
#else
		bIsEnableAppearBonusNPC = bIsEnableAppearBonusNPC && GameDataManager.Singleton.PlayMode == EPlayMode.CAMPAIGN;
#endif // #if UNITY_EDITOR

		bIsEnableAppearBonusNPC = bIsEnableAppearBonusNPC || GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.BONUS;

		// 보너스 NPC 등장이 가능 할 경우
		if (bIsEnableAppearBonusNPC)
		{
			bool bIsSuccess = false;

			uint nBonusNPCKeyA = GlobalTable.GetData<uint>(ComType.G_ROBBERY_NPC_KEY);
			uint nBonusNPCKeyB = GlobalTable.GetData<uint>(ComType.G_SAMI_BOSS_NPC_KEY);

			uint nBonusNPCKey = (GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.BONUS) ? nBonusNPCKeyA : nBonusNPCKeyB;
			this.PlayerController.NavMeshAgent.enabled = true;

			var oNonPlayerController = this.CreateNonPlayer(NPCTable.GetData(nBonusNPCKey),
				this.GetSummonPos(this.PlayerController.NavMeshAgent, ref bIsSuccess, -5, 10), this.MapObjsRootList[0], null, true, this.NPCEffectTableList);

			oNonPlayerController.LookTarget(this.PlayerController);
			oNonPlayerController.SetTrackingTarget(this.PlayerController);

			this.PlayerController.NavMeshAgent.enabled = false;
			GameDataManager.Singleton.SetBonusNPCAppearStageID(-1);

			bool bIsEnableNotiBonusNPCAppear = PlayerPrefs.GetInt(ComType.G_KEY_NOTI_BONUS_NPC_APPEAR) == 0;

			while (!bIsSuccess)
			{
				var stPos = this.GetSummonPos(this.PlayerController.NavMeshAgent, ref bIsSuccess, -5, 10);
				oNonPlayerController.transform.position = stPos;
			}

			// 캠페인 보너스 NPC 첫 등장 일 경우
			if (GameDataManager.Singleton.PlayMapInfoType != EMapInfoType.BONUS)
			{
#if DISABLE_THIS
				this.PageBattle.ShowHelpTutorialUIs(EHelpTutorialKinds.BONUS_NPC_APPEAR);
#endif // #if DISABLE_THIS

				PlayerPrefs.SetInt(ComType.G_KEY_NOTI_BONUS_NPC_APPEAR, 1);
				PlayerPrefs.Save();
			}
		}

		// 보스 스테이지 일 경우
		if (this.IsBossStage)
		{
			var oNonPlayerController = this.FindNonPlayerControllerBoss();
			oNonPlayerController.SetExtraGaugeUIsHandler(this.PageBattle.GaugeBossUIsHandler);

			this.PageBattle.GaugeBossUIsHandler.Init(CGaugeBossUIsHandler.MakeParams(oNonPlayerController.Table));
		}

		// 보너스 스테이지 일 경우
		if (this.IsBonusStage)
		{
			var oNonPlayerControllersBonusList = this.FindNonPlayerControllersBonus();

			for (int i = 0; i < oNonPlayerControllersBonusList.Count; ++i)
			{
				var oGaugeBonusUIsHandler = GameResourceManager.Singleton.CreateObject<CGaugeBonusUIsHandler>(this.PageBattle.OriginGaugeBonusUIsHandler,
					this.PageBattle.BonusUIs.transform, null);

				(oGaugeBonusUIsHandler.transform as RectTransform).sizeDelta = (this.PageBattle.OriginGaugeBonusUIsHandler.transform as RectTransform).sizeDelta;
				(oGaugeBonusUIsHandler.transform as RectTransform).anchoredPosition = (this.PageBattle.OriginGaugeBonusUIsHandler.transform as RectTransform).anchoredPosition;

				oNonPlayerControllersBonusList[i].SetExtraGaugeUIsHandler(oGaugeBonusUIsHandler);
				oGaugeBonusUIsHandler.Init(CGaugeBonusUIsHandler.MakeParams(oNonPlayerControllersBonusList[i].Table));

				this.PageBattle.GaugeBonusUIsHandlerList.Add(oGaugeBonusUIsHandler);
			}
		}

		int nPlayEpisodeID = GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID;

#if DISABLE_THIS
		// 초반 에피소드 일 경우
		if (nPlayEpisodeID <= 0)
		{
			int nIsCompleteNPCLockOnTutorial = PlayerPrefs.GetInt(ComType.HelpTutorialKindsKeyDict[EHelpTutorialKinds.NPC_LOCK_ON]);
			int nIsCompleteEquipWeaponChangeTutorial = PlayerPrefs.GetInt(ComType.HelpTutorialKindsKeyDict[EHelpTutorialKinds.EQUIP_WEAPON_CHANGE]);

			// NPC 조준 튜토리얼 진행이 가능 할 경우
			if (nIsCompleteNPCLockOnTutorial <= 0)
			{
				this.PageBattle.ShowHelpTutorialUIs(EHelpTutorialKinds.NPC_LOCK_ON);
			}
			// 장착 무기 변경 튜토리얼 진행이 가능 할 경우
			else if (nIsCompleteEquipWeaponChangeTutorial <= 0 && this.PlayerController.NumEquipWeapons >= 2)
			{
				this.PageBattle.ShowHelpTutorialUIs(EHelpTutorialKinds.EQUIP_WEAPON_CHANGE);
			}
		}
#endif // #if DISABLE_THIS

		// 터치 전달자를 설정한다
		this.ExLateCallFunc((a_oSender) =>
		{
			var oTouchDispatcher = this.PageBattle.TutorialOverlayUIs.GetComponentInChildren<CTouchDispatcher>();
			oTouchDispatcher.SetBeginCallback(this.PageBattle.HandleOnTouchBegin);
			oTouchDispatcher.SetMoveCallback(this.PageBattle.HandleOnTouchMove);
			oTouchDispatcher.SetEndCallback(this.PageBattle.HandleOnTouchEnd);

#if DISABLE_THIS
			this.PageBattle.ShowHelpTutorialUIs(this.PageBattle.ShowedHelpTutorialKinds);
#endif // #if DISABLE_THIS
		});
		#endregion // 추가

		this.POAHandlerList.Clear();
		this.WarpGateHandlerList.Clear();
		this.SpawnPosHandlerList.Clear();

		ComUtil.GetComponentsInChildren<CPOAHandler>(_pathObjRoot.gameObject, this.POAHandlerList);
		ComUtil.GetComponentsInChildren<CWarpGateHandler>(_pathObjRoot.gameObject, this.WarpGateHandlerList);
		ComUtil.GetComponentsInChildren<CSpawnPosHandler>(_pathObjRoot.gameObject, this.SpawnPosHandlerList);

		this.POAHandlerList.ExCopyTo(this.GateObjList, (a_oPOAHandler) => a_oPOAHandler.gameObject);
		this.WarpGateHandlerList.ExCopyTo(this.GateObjList, (a_oWarpGateHandler) => a_oWarpGateHandler.gameObject, false);

		for (int i = 0; i < this.POAHandlerList.Count; ++i)
		{
			this.POAHandlerList[i].GetComponent<MeshRenderer>().enabled = false;
		}

		this.PageBattle.SetupBoxMarkerUIs();
		this.PageBattle.SetupGateMarkerUIs();
	}

	void InitializePlayer()
	{
		string cName = string.Empty;

		foreach (ItemCharacter character in gameManager.invenCharacter)
			if (character.id == account.m_nCharacterID)
				cName = CharacterTable.GetData(character.nKey).Prefab;

		_Player = GameResourceManager.Singleton.CreateObject(EResourceType.Character, cName, this.PlayMapObjsRoot.transform);

		#region 추가
		_Player.transform.localPosition = GameDataManager.Singleton.PlayMapInfo.m_stPlayerPos;
		_Player.transform.localEulerAngles = GameDataManager.Singleton.PlayMapInfo.m_stPlayerRotate;

		int nWalkableAreaMask = 1 << NavMesh.GetAreaFromName(ComType.G_NAV_MESH_AREA_WALKABLE);

		// 내비게이션 영역이 존재 할 경우
		if (NavMesh.SamplePosition(_Player.transform.position + (Vector3.up * 1.5f), out NavMeshHit stNavMeshHit, float.MaxValue / 2.0f, nWalkableAreaMask))
		{
			_Player.transform.position = stNavMeshHit.position;
		}

		m_oAudioListenerController.SetFollowTarget(_Player);
		this.PlayerController = _Player.GetComponent<PlayerController>();

		this.AddUnitController(this.PlayerController);
		MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle)?.SetPlayerController(_Player.GetComponentInChildren<PlayerController>());

		// 충돌 전달자가 없을 경우
		if (!this.PlayerController.TryGetComponent(out CTriggerDispatcher oDispatcher))
		{
			oDispatcher = this.PlayerController.gameObject.AddComponent<CTriggerDispatcher>();
		}

		this.SetupNavMeshCameraBounds();
		oDispatcher.SetEnterCallback(this.HandleOnTriggerEnter);

		// 워프 효과 재생이 불가능 할 경우
		if (GameDataManager.Singleton.PlayStageID <= 0 || GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.ABYSS)
		{
			return;
		}

		// 워프 효과가 존재 할 경우
		if (this.FXModelInfo.WarpFXInfo.m_oWarpEndFX != null)
		{
			var oWarpEndFX = GameResourceManager.Singleton.CreateObject<ParticleSystem>(this.FXModelInfo.WarpFXInfo.m_oWarpEndFX,
				this.PlayerController.transform, null, 5.0f);

			oWarpEndFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			oWarpEndFX.Play(true);
		}
		#endregion // 추가
	}

	void InitializeWeapon()
	{
		MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle)?.SetupWeaponUIs();
	}

	void InitializeCampaignInfo()
	{
		_curChapter = account.m_nChapter;
		_curEpisode = account.m_nEpisode;
	}

	void InitializeCamera()
	{
		this.CamDummy = GameObject.Find("CamDummy");
		this.CamDummy.GetComponent<CamDummy>().m_eMapInfoType = GameDataManager.Singleton.PlayMapInfoType;

		this.MainCamera = Camera.main;
		this.Cam = this.CamDummy.GetComponent<CamDummy>();

		var oCameraMove = this.MainCamera.GetComponent<CameraMove>();

#if DEBUG || DEVELOPMENT_BUILD
		bool bIsOverride = PlayerPrefs.GetInt("IS_OVERRIDE_CAMERA_VALS", 0) != 0;
		this.MainCamera.fieldOfView = bIsOverride ? PlayerPrefs.GetFloat("CAMERA_FOV") : GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fFOV;

		var oCamDummy = this.CamDummy.GetComponent<CamDummy>();
		oCamDummy._fDistance = bIsOverride ? PlayerPrefs.GetFloat("CAMERA_DISTANCE") : GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fDistance;
		oCamDummy.ttt = bIsOverride ? PlayerPrefs.GetFloat("CAMERA_SMOOTH_TIME") : GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fSmoothTime;

		this.CameraMove._fCamHeight = bIsOverride ? PlayerPrefs.GetFloat("CAMERA_HEIGHT") : GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fHeight;
		this.CameraMove._fCamForward = bIsOverride ? PlayerPrefs.GetFloat("CAMERA_FORWARD") : GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fForward;
#else
		this.MainCamera.fieldOfView = GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fFOV;

		var oCamDummy = this.CamDummy.GetComponent<CamDummy>();
		oCamDummy._fDistance = GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fDistance;
		oCamDummy.ttt = GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fSmoothTime;

		this.CameraMove._fCamHeight = GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fHeight;
		this.CameraMove._fCamForward = GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fForward;
#endif // #if DEBUG || DEVELOPMENT_BUILD

		m_fOriginCameraHeight = oCameraMove._fCamHeight;
		m_fOriginCameraForward = oCameraMove._fCamForward;
		m_fOriginCamDummyDistance = oCamDummy._fDistance;

#if NEVER_USE_THIS
		// 기존 구문
		GameObject.Find("CamDummy").GetComponent<CamDummy>().SetTarget(_Player);
#endif // #if NEVER_USE_THIS
	}
}

#region 추가
/** 전투 제어자 */
public partial class BattleController : MonoBehaviour
{
	#region 변수
	[Header("=====> Battle Controller - Etc <=====")]
	[SerializeField] private Light m_oLight = null;
	[SerializeField] private CameraMove m_oCameraMove = null;
	[SerializeField] private FXModelInfo m_oFXModelInfoCommon = null;
	[SerializeField] private SoundModelInfo m_oSoundModelInfoCommon = null;
	[SerializeField] private CTouchPointHandler m_oTouchPointHandler = null;
	[SerializeField] private AudioListenerController m_oAudioListenerController = null;

	public Tween m_oPlayerWarpAni = null;
	public Tween m_oCameraHeightAni = null;
	public Tween m_oCameraForwardAni = null;

	private int m_nWalkableAreaMask = 0;
	private int m_nNumKillNonPlayers = 0;
	private int m_nMaxNumReleaseUnitStacks = 0;

	private NavMeshPath m_oNavMeshPath = null;
	private FloatingJoystick _floatingJoystick = null;
	#endregion // 변수

	#region 프로퍼티
	public bool IsBossStage { get; private set; } = false;
	public bool IsBonusStage { get; private set; } = false;
	public bool IsApplyEffects { get; private set; } = false;
	public bool IsApplyContinueSkill { get; private set; } = false;
	public bool IsUpdateWeapons { get; private set; } = false;
	public bool IsMakeAppearBossNPC { get; private set; } = false;
	public bool IsPlaySecneDirecting { get; private set; } = false;

	public bool IsKillNonPlayers { get; private set; } = false;
	public bool IsMoveToNextStage { get; private set; } = false;
	public bool IsOpenNextStagePassage { get; private set; } = false;
	public bool IsEnableUpdate { get; private set; } = false;

	public int AbyssPoint { get; private set; } = 0;
	public int MaxAbyssPoint { get; private set; } = 0;
	public int CurSkillSoundIdx { get; private set; } = 0;

	public float GoldenPoint { get; private set; } = 0.0f;
	public float RunningTime { get; private set; } = 0.0f;
	public float MaxPlayTime { get; private set; } = 0.0f;
	public float InstEXPRatio { get; private set; } = 0.0f;
	public float RemainPlayTime { get; private set; } = 0.0f;

	public Bounds CameraBounds { get; private set; }
	public Bounds PlayerNavMeshBounds { get; private set; }
	public System.DateTime PrevKillSoundPlayTime { get; private set; }

	public STBattlePlayInfo BattlePlayInfo { get; private set; }

	public Camera MainCamera { get; private set; } = null;
	public LevelLoader LevelLoader { get; private set; } = null;
	
	public List<Vector3> BoxSpawnPointList { get; } = new List<Vector3>();
	public List<Vector3> RouletteSpawnPointList { get; } = new List<Vector3>();

	public List<STEffectStackInfo> NonPlayerEffectStackInfoList { get; } = new List<STEffectStackInfo>();
	public List<STEffectStackInfo> NonPlayerPassiveEffectStackInfoList { get; } = new List<STEffectStackInfo>();

	public List<CInteractableMapObjHandler> InteractableMapObjHandlerList { get; } = new List<CInteractableMapObjHandler>();
	public List<CInteractableMapObjHandler> BoxInteractableMapObjHandlerList { get; } = new List<CInteractableMapObjHandler>();

	public CamDummy Cam { get; private set; } = null;
	public GameObject CamDummy { get; private set; } = null;
	public StageTable StageTable { get; private set; } = null;
	public PlayerController PlayerController { get; private set; } = null;
	public CNavMeshController NavMeshController { get; private set; } = null;
	public CStateMachine<BattleController> StateMachine { get; } = new CStateMachine<BattleController>();

	public List<NPCTable> HuntNPCTableList { get; } = new List<NPCTable>();
	public List<NPCTable> BossNPCTableList { get; } = new List<NPCTable>();
	public List<NPCTable> SummonNPCTableList { get; } = new List<NPCTable>();
	
	public List<EffectTable> NPCEffectTableList { get; } = new List<EffectTable>();

	public List<CObjInfo> BossNPCObjInfoList { get; } = new List<CObjInfo>();
	public List<CPOAHandler> POAHandlerList { get; } = new List<CPOAHandler>();
	public List<CWarpGateHandler> WarpGateHandlerList { get; } = new List<CWarpGateHandler>();

	public List<CGroundItemHandler> GroundItemHandlerList { get; } = new List<CGroundItemHandler>();
	public List<NonPlayerController> NonPlayerControllerList { get; } = new List<NonPlayerController>();
	public List<NonPlayerController> TargetNonPlayerControllerList { get; } = new List<NonPlayerController>();

	public Dictionary<int, List<UnitController>> UnitControllerDictContainer { get; } = new Dictionary<int, List<UnitController>>();
	public Dictionary<int, List<UnitController>> OriginUnitControllerDictContainer { get; } = new Dictionary<int, List<UnitController>>();

	public List<GameObject> GateObjList { get; } = new List<GameObject>();
	public List<GameObject> DropWeaponList { get; } = new List<GameObject>();
	public List<GameObject> MapObjsRootList { get; } = new List<GameObject>();

	public Queue<STHitUIsInfo> HitUIsInfoQueue { get; private set; } = new Queue<STHitUIsInfo>();
	public Queue<STReleaseUnitInfo> ReleaseUnitInfoQueue { get; private set; } = new Queue<STReleaseUnitInfo>();

	public bool IsFinish => this.StateMachine.State != null && this.StateMachine.State.GetType().Equals(typeof(CStateBattleControllerFinish));
	public bool IsPlaying => this.StateMachine.State != null && this.StateMachine.State.GetType().Equals(typeof(CStateBattleControllerPlay));
	public bool IsRunning => this.RunningTime.ExIsGreat(0.15f);

	public float SkipPlayTime => Mathf.Max(0.0f, this.MaxPlayTime - this.RemainPlayTime);
	public CTouchPointHandler TouchPointHandler => m_oTouchPointHandler;

	public PageBattle PageBattle => MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle);
	public CameraMove CameraMove => m_oCameraMove;
	public FXModelInfo FXModelInfo => m_oFXModelInfoCommon;
	public SoundModelInfo SoundModelInfo => m_oSoundModelInfoCommon;
	public FloatingJoystick Joystick => _floatingJoystick;

	public Transform PathObjRoot => _pathObjRoot;
	public GameObject Player => _Player;

	public GameObject PlayMapObjsRoot => (GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.ABYSS) ?
		this.MapObjsRootList[GameDataManager.Singleton.PlayStageID] : this.MapObjsRootList[0];
	#endregion // 프로퍼티

	#region 함수
	/** NPC 효과 스택 정보를 설정한다 */
	public virtual void SetupNonPlayerEffectStackInfos()
	{
		this.NonPlayerEffectStackInfoList.Clear();
		this.SetupNonPlayerEffectStackInfos(this.NonPlayerPassiveEffectStackInfoList, this.NonPlayerEffectStackInfoList);
	}

	/** NPC 효과 스택 정보를 설정한다 */
	public virtual void SetupNonPlayerEffectStackInfos(List<STEffectStackInfo> a_oEffectStackInfoList,
		List<STEffectStackInfo> a_oOutEffectStackInfoList)
	{
		for (int i = 0; i < a_oEffectStackInfoList.Count; ++i)
		{
			int nResult = a_oOutEffectStackInfoList.FindIndex((a_stStackInfo) =>
			{
				bool bIsEquals01 = a_oEffectStackInfoList[i].m_fVal.ExIsEquals(a_stStackInfo.m_fVal);
				bool bIsEquals02 = a_oEffectStackInfoList[i].m_fDuration.ExIsEquals(a_stStackInfo.m_fDuration);

				return bIsEquals01 && bIsEquals02 && a_oEffectStackInfoList[i].m_eEffectType == a_stStackInfo.m_eEffectType;
			});

			// 효과 스택 정보가 없을 경우
			if (!a_oOutEffectStackInfoList.ExIsValidIdx(nResult))
			{
				STEffectStackInfo stEffectStackInfo = default;
				stEffectStackInfo.m_fDuration = a_oEffectStackInfoList[i].m_fDuration;
				stEffectStackInfo.m_fRemainTime = a_oEffectStackInfoList[i].m_fDuration;
				stEffectStackInfo.m_bIsIgnoreStandardAbility = a_oEffectStackInfoList[i].m_bIsIgnoreStandardAbility;

				a_oOutEffectStackInfoList.Add(stEffectStackInfo);
				nResult = a_oOutEffectStackInfoList.Count - 1;
			}

			var stStackInfo = a_oOutEffectStackInfoList[nResult];
			stStackInfo.m_fVal = a_oEffectStackInfoList[i].m_fVal;
			stStackInfo.m_fDuration = a_oEffectStackInfoList[i].m_fDuration;
			stStackInfo.m_fInterval = a_oEffectStackInfoList[i].m_fInterval;
			stStackInfo.m_fRemainTime = Mathf.Max(stStackInfo.m_fRemainTime, a_oEffectStackInfoList[i].m_fRemainTime);
			stStackInfo.m_nStackCount = stStackInfo.m_nStackCount + a_oEffectStackInfoList[i].m_nStackCount;
			stStackInfo.m_nMaxStackCount = Mathf.Max(stStackInfo.m_nMaxStackCount, a_oEffectStackInfoList[i].m_nMaxStackCount);
			stStackInfo.m_bIsIgnoreStandardAbility = stStackInfo.m_bIsIgnoreStandardAbility || a_oEffectStackInfoList[i].m_bIsIgnoreStandardAbility;

			stStackInfo.m_eEffectType = a_oEffectStackInfoList[i].m_eEffectType;
			stStackInfo.m_eAbilityEffectType = a_oEffectStackInfoList[i].m_eAbilityEffectType;

			a_oOutEffectStackInfoList[nResult] = stStackInfo;
		}
	}

	/** NPC 를 초기화한다 */
	private void InitializeNonPlayers()
	{
		// 플레이 맵 정보가 없을 경우
		if (GameDataManager.Singleton.PlayMapInfo == null)
		{
			return;
		}

		int nChapterID = GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nChapterID;
		int nEpisodeID = GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID;

		var ePlayMode = GameDataManager.Singleton.PlayMode;

		// 테스트 플레이 모드 일 경우
		if (GameDataManager.Singleton.PlayMode == EPlayMode.TEST)
		{
			ePlayMode = (EPlayMode)(GameDataManager.Singleton.PlayMapInfoType + 1);
		}

		switch (ePlayMode)
		{
			case EPlayMode.HUNT:
				var oHuntTable = HuntTable.GetHuntData(GameDataManager.Singleton.PlayHuntLV);
				int nRewardExp = this.GetHuntClearExp(oHuntTable.PrimaryKey, oHuntTable.RewardExp);

				int nRewardGroup = GameDataManager.Singleton.IsHuntBoost ?
					oHuntTable.BoostRewardGroup : oHuntTable.RewardGroup;

				this.BattlePlayInfo = new STBattlePlayInfo(nRewardExp,
					oHuntTable.RewardPassExp, oHuntTable.MaxCountRevive, oHuntTable.ReviveItemCount, nRewardGroup, oHuntTable.StandardLevel);

				break;
			case EPlayMode.ADVENTURE:
				uint nAdventureTableKey = ComUtil.GetAdventureKey(nEpisodeID + 1, nChapterID + 1);
				var oMissionAdventureTable = MissionAdventureTable.GetData(nAdventureTableKey);

				int nMinStandardNPCLevel = GlobalTable.GetData<int>(ComType.G_VALUE_MIN_NPC_LEVEL);
				int nMaxStandardNPCLevel = GlobalTable.GetData<int>(ComType.G_VALUE_MAX_NPC_LEVEL);

				int nStandardNPCLevel = GameManager.Singleton.user.GetStandardNPCLevel(GameManager.Singleton.user.m_nLevel);
				int nFinalStandardNPCLevel = Mathf.Clamp(nStandardNPCLevel + oMissionAdventureTable.CorrectValueStandardLevel, nMinStandardNPCLevel, nMaxStandardNPCLevel);

				this.BattlePlayInfo = new STBattlePlayInfo(oMissionAdventureTable.RewardExp,
					oMissionAdventureTable.RewardPassExp, oMissionAdventureTable.MaxCountRevive, oMissionAdventureTable.ReviveItemCount, oMissionAdventureTable.RewardGroup, nFinalStandardNPCLevel);

				break;

			case EPlayMode.ABYSS:
				uint nAbyssTableKey = ComUtil.GetAbyssKey(nEpisodeID, nChapterID + 1);
				var oAbyssTable = AbyssTable.GetData(nAbyssTableKey);

				this.BattlePlayInfo = new STBattlePlayInfo(oAbyssTable.RewardExp,
					oAbyssTable.RewardPassExp, oAbyssTable.MaxCountRevive, oAbyssTable.ReviveItemCount, 0, oAbyssTable.StandardNPCLevel);

				break;

			case EPlayMode.DEFENCE:
				uint nDefenceGroupTableKey = ComUtil.GetDefenceGroupKey(nChapterID);
				var oDefenceGroupTable = MissionDefenceGroupTable.GetData(nDefenceGroupTableKey);

				this.BattlePlayInfo = new STBattlePlayInfo(0,
					0, oDefenceGroupTable.MaxCountRevive, oDefenceGroupTable.ReviveItemCount, 0, 0);

				break;

			case EPlayMode.INFINITE:
				var oInfiniteGroupTable = MissionZombieGroupTable.GetDataByEpisode(nEpisodeID + 1);

				this.BattlePlayInfo = new STBattlePlayInfo(0,
					0, oInfiniteGroupTable.MaxCountRevive, oInfiniteGroupTable.ReviveItemCount, 0, 0);

				break;

			case EPlayMode.BONUS:
				int nStandardNPCLevelBonus = GameManager.Singleton.user.GetStandardNPCLevelBonus(GameManager.Singleton.user.m_nLevel);
				this.BattlePlayInfo = new STBattlePlayInfo(0, 0, 0, 0, 0, nStandardNPCLevelBonus);

				break;

			default:
				var nChapterTableKey = ComUtil.GetChapterKey(nEpisodeID + 1, nChapterID + 1);
				var oCampaignChapterTable = ChapterTable.GetData(nChapterTableKey);
				var oCampaignInfiniteGroupTable = MissionZombieGroupTable.GetDataByEpisode(nEpisodeID + 1);

				int nMaxCountRevive = GameDataManager.Singleton.IsWaveMode() ?
					oCampaignInfiniteGroupTable.MaxCountRevive : oCampaignChapterTable.MaxCountRevive;

				int nReviveItemCount = GameDataManager.Singleton.IsWaveMode() ?
					oCampaignInfiniteGroupTable.ReviveItemCount : oCampaignChapterTable.ReviveItemCount;

				this.BattlePlayInfo = new STBattlePlayInfo(0,
					0, nMaxCountRevive, nReviveItemCount, oCampaignChapterTable.RewardGroup, oCampaignChapterTable.StandardLevel);

				// 캠페인 일 경우
				if (ePlayMode == EPlayMode.CAMPAIGN || ePlayMode == EPlayMode.TUTORIAL)
				{
					int nCorrectEpisodeID = (ePlayMode == EPlayMode.CAMPAIGN) ? nEpisodeID + 1 : nEpisodeID;
					var oStageTableList = StageTable.GetStageGroup(nCorrectEpisodeID, 1);

					this.StageTable = oStageTableList[GameDataManager.Singleton.PlayStageID];
					this.InstEXPRatio = this.StageTable.IntanceExpRatio;

					var stBattlePlayInfo = this.BattlePlayInfo;

					// 튜토리얼 진행 상태 일 경우
					if(ePlayMode == EPlayMode.TUTORIAL)
					{
						nChapterTableKey = ComUtil.GetChapterKey(nEpisodeID, nChapterID + 1);
						oCampaignChapterTable = ChapterTable.GetData(nChapterTableKey);

						stBattlePlayInfo.m_nRewardGroup = oCampaignChapterTable.RewardGroup;
						stBattlePlayInfo.m_nMaxCountRevive = oCampaignChapterTable.MaxCountRevive;
						stBattlePlayInfo.m_nStandardNPCLevel = oCampaignChapterTable.StandardLevel;
					}

					stBattlePlayInfo.m_nRewardEXP = this.StageTable.RewardExp;
					stBattlePlayInfo.m_nRewardPassEXP = this.StageTable.RewardPassExp;
					stBattlePlayInfo.m_nStandardNPCLevel += this.StageTable.CorrectedLevel;

					bool bIsShelter = this.StageTable.Type == (int)EStageType.SHELTER;
					this.BattlePlayInfo = stBattlePlayInfo;

					this.PageBattle.BattleInventory.gameObject.SetActive(bIsShelter);

					// 쉼터 일 경우
					if (bIsShelter)
					{
						StartCoroutine(this.UpdateAcquireWeapons());
						this.PageBattle.BattleInventoryUIs.SetActive(true);
					}
				}

				break;
		}

		this.PageBattle.SetContinueInfo(this.BattlePlayInfo.m_nMaxCountRevive, this.BattlePlayInfo.m_nReviveItemCount);

		switch (GameDataManager.Singleton.PlayMapInfoType)
		{
			case EMapInfoType.ABYSS:
				for (int i = 0; i < GameDataManager.Singleton.PlayMapInfoDict.Count; ++i)
				{
					this.SetupNonPlayers(GameDataManager.Singleton.PlayMapInfoDict[i],
						this.MapObjsRootList[GameDataManager.Singleton.PlayStageID]);

					this.SetupEtcMapObjs(GameDataManager.Singleton.PlayMapInfoDict[i],
						this.MapObjsRootList[GameDataManager.Singleton.PlayStageID]);
				}

				for (int i = 0; i < GameDataManager.Singleton.PlayMapInfoDict.Count; ++i)
				{
					this.SetupInteractableMapObjs(GameDataManager.Singleton.PlayMapInfoDict[i],
						this.MapObjsRootList[GameDataManager.Singleton.PlayStageID]);
				}

				this.SetupNonPlayers(GameDataManager.Singleton.PlayMapInfo,
					this.MapObjsRootList[GameDataManager.Singleton.PlayStageID]);

				break;

			default:
				this.SetupNonPlayers(GameDataManager.Singleton.PlayMapInfo, this.MapObjsRootList[0]);
				this.SetupEtcMapObjs(GameDataManager.Singleton.PlayMapInfo, this.MapObjsRootList[0]);
				this.SetupInteractableMapObjs(GameDataManager.Singleton.PlayMapInfo, this.MapObjsRootList[0]);

				break;
		}

		this.LevelLoader.UpdateNavMeshState();
	}

	/** NPC 테이블을 초기화한다 */
	private void InitializeNonPlayerTables()
	{
		var oNPCTableList = NPCTable.GetList();

		for (int i = 0; i < oNPCTableList.Count; ++i)
		{
			// 등장하지 않은 NPC 일 경우
			if (oNPCTableList[i].FirstEpisode > GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID + 1)
			{
				continue;
			}

			// 사냥에 등장 가능한 NPC 일 경우
			if (oNPCTableList[i].isHunt != 0)
			{
				this.HuntNPCTableList.Add(oNPCTableList[i]);
			}

			// 소환 가능한 NPC 일 경우
			if ((ENPCGrade)oNPCTableList[i].Grade <= ENPCGrade.NORM && oNPCTableList[i].isSummon != 0)
			{
				this.SummonNPCTableList.Add(oNPCTableList[i]);
			}
		}

		for (int i = 0; i < GameDataManager.Singleton.PlayMapInfoDict.Count; ++i)
		{
			for (int j = 0; j < GameDataManager.Singleton.PlayMapInfoDict[i].m_oMapObjInfoList.Count; ++j)
			{
				var oObjInfo = GameDataManager.Singleton.PlayMapInfoDict[i].m_oMapObjInfoList[j];
				int nIdx = oNPCTableList.FindIndex((a_oTable) => a_oTable.Prefab.Equals(oObjInfo.m_stPrefabInfo.m_oName));

				// 테이블이 없을 경우
				if (nIdx < 0 || nIdx >= oNPCTableList.Count)
				{
					continue;
				}

				// 보스 NPC 가 아닐 경우
				if ((ENPCGrade)oNPCTableList[nIdx].Grade != ENPCGrade.BOSS)
				{
					continue;
				}

				this.BossNPCTableList.Add(oNPCTableList[nIdx]);
				this.BossNPCObjInfoList.Add(oObjInfo);
			}
		}
	}

	/** NPC 를 설정한다 */
	private void SetupNonPlayers(CMapInfo a_oMapInfo, GameObject a_oMapObjsRoot)
	{
		for (int i = 0; i < this.GroundItemHandlerList.Count; ++i)
		{
			GameResourceManager.Singleton.ReleaseObject(this.GroundItemHandlerList[i].gameObject, false);
		}

		foreach (var stKeyVal in this.OriginUnitControllerDictContainer)
		{
			// 플레이어 그룹 일 경우
			if (stKeyVal.Key == (int)ETargetGroup.PLAYER || stKeyVal.Key == (int)ETargetGroup.COMMON)
			{
				continue;
			}

			for (int i = 0; i < stKeyVal.Value.Count; ++i)
			{
				GameResourceManager.Singleton.ReleaseObject(stKeyVal.Value[i].gameObject, false);
			}
		}

		this.GroundItemHandlerList.Clear();
		this.NonPlayerControllerList.Clear();

		this.UnitControllerDictContainer.Clear();
		this.OriginUnitControllerDictContainer.Clear();

		var oHuntTable = (GameDataManager.Singleton.PlayMode == EPlayMode.HUNT) ?
			HuntTable.GetHuntData(GameDataManager.Singleton.PlayHuntLV) : null;

		var oNPCTableList = NPCTable.GetList();

		for (int i = 0; i < a_oMapInfo.m_oMapObjInfoList.Count; ++i)
		{
			// 캐릭터 타입이 아닐 경우
			if (a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_eResType != EResourceType.Character)
			{
				continue;
			}

			int nIdx = oNPCTableList.FindIndex((a_oTable) => a_oTable.Prefab.Equals(a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName));

			// 테이블이 없을 경우
			if (nIdx < 0 || nIdx >= oNPCTableList.Count)
			{
				continue;
			}

			int nHuntIdx = Random.Range(0, this.HuntNPCTableList.Count);

			bool bIsRandomSetup = GameDataManager.Singleton.PlayMode == EPlayMode.HUNT &&
				!a_oMapInfo.m_oMapObjInfoList[i].m_bIsRandExclude;

			var oNPCTable = bIsRandomSetup ? this.HuntNPCTableList[nHuntIdx] : oNPCTableList[nIdx];
			var stPos = ((Vector3)a_oMapInfo.m_oMapObjInfoList[i].m_stTransInfo.m_stPos).ExToWorld(a_oMapObjsRoot);

			bool bIsBossNPC = oNPCTable.Grade == (int)ENPCGrade.BOSS;
			bool bIsAddIntoList = !bIsBossNPC || GameDataManager.Singleton.PlayMapInfoType != EMapInfoType.ABYSS;

			// 생성이 불가능 할 경우
			if (!bIsAddIntoList)
			{
				continue;
			}

			var oNonPlayerController = this.CreateNonPlayer(oNPCTable,
				stPos, a_oMapObjsRoot, a_oMapInfo.m_oMapObjInfoList[i], bIsAddIntoList, this.NPCEffectTableList);

			oNonPlayerController.transform.localScale = a_oMapInfo.m_oMapObjInfoList[i].m_stTransInfo.m_stScale;
			oNonPlayerController.transform.localEulerAngles = a_oMapInfo.m_oMapObjInfoList[i].m_stTransInfo.m_stRotate;

			// 보스 NPC 일 경우
			if (bIsBossNPC)
			{
				this.IsBossStage = bIsAddIntoList;
			}
		}
	}

	/** 기타 맵 객체를 설정한다 */
	private void SetupEtcMapObjs(CMapInfo a_oMapInfo, GameObject a_oMapObjsRoot)
	{
		this.BoxSpawnPointList.Clear();
		this.RouletteSpawnPointList.Clear();

		var oFieldObjTableList = FieldObjectTable.GetList();

		for (int i = 0; i < a_oMapInfo.m_oMapObjInfoList.Count; ++i)
		{
			var stPos = ((Vector3)a_oMapInfo.m_oMapObjInfoList[i].m_stTransInfo.m_stPos).ExToWorld(a_oMapObjsRoot);
			var oMapObjInfo = a_oMapInfo.m_oMapObjInfoList[i];

			// 상자 등장 위치 일 경우
			if (a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Equals("BG_BoxSpawnPoint"))
			{
				this.BoxSpawnPointList.Add(stPos);
			}
			// 룰렛 등장 위치 일 경우
			else if (a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Equals("BG_RouletteSpawnPoint"))
			{
				this.RouletteSpawnPointList.Add(stPos);
			}
			// 상호 작용 맵 객체 일 경우
			else if (a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Contains("FIELD_OBJECT_FIELDOBJECT_"))
			{
				var oMapObjHandler = GameResourceManager.Singleton.CreateObject<CInteractableMapObjHandler>(EResourceType.BG_Etc,
					a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName, a_oMapObjsRoot.transform);

				oMapObjHandler.transform.position = stPos;
				oMapObjHandler.transform.localScale = a_oMapInfo.m_oMapObjInfoList[i].m_stTransInfo.m_stScale;
				oMapObjHandler.transform.localEulerAngles = a_oMapInfo.m_oMapObjInfoList[i].m_stTransInfo.m_stRotate;

				int nResult = oFieldObjTableList.FindIndex((a_oFieldObjTable) => oMapObjInfo.m_stPrefabInfo.m_oName.Equals(a_oFieldObjTable.Prefab));
				oMapObjHandler.Init(CInteractableMapObjHandler.MakeParams(oFieldObjTableList[nResult]));

				this.InteractableMapObjHandlerList.Add(oMapObjHandler);
			}
		}
	}

	/** 상호 작용 맵 객체를 설정한다 */
	private void SetupInteractableMapObjs(CMapInfo a_oMapInfo, GameObject a_oMapObjsRoot)
	{
		bool bIsExistsBox = false;
		bool bIsExistsRoulette = false;

		var oFieldObjTableList = FieldObjectTable.GetList();
		var oBoxFieldObjTableList = new List<FieldObjectTable>();
		var oRouletteFieldObjTableList = new List<FieldObjectTable>();

		for (int i = 0; i < oFieldObjTableList.Count; ++i)
		{
			// 상자 일 경우
			if (oFieldObjTableList[i].Prefab.Contains("FIELD_OBJECT_FIELDOBJECT_CHECT_"))
			{
				oBoxFieldObjTableList.Add(oFieldObjTableList[i]);
			}
			// 룰렛 일 경우
			else if (oFieldObjTableList[i].Prefab.Contains("FIELD_OBJECT_FIELDOBJECT_ROULETTE_"))
			{
				oRouletteFieldObjTableList.Add(oFieldObjTableList[i]);
			}
		}

		oBoxFieldObjTableList.ExShuffle();
		oRouletteFieldObjTableList.ExShuffle();

		this.BoxSpawnPointList.ExShuffle();
		this.RouletteSpawnPointList.ExShuffle();

		for (int i = 0; i < a_oMapInfo.m_oMapObjInfoList.Count; ++i)
		{
			bIsExistsBox = bIsExistsBox || a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Contains("FIELD_OBJECT_FIELDOBJECT_CHECT_");
			bIsExistsRoulette = bIsExistsRoulette || a_oMapInfo.m_oMapObjInfoList[i].m_stPrefabInfo.m_oName.Contains("FIELD_OBJECT_FIELDOBJECT_ROULETTE_");
		}

		bool bIsEnableBox = this.StageTable != null &&
			Random.Range(0.0f, 1.0f).ExIsLess(this.StageTable.ChectSpawnRatio);

		var oBoxFieldObjTable = this.GetRandomBoxFieldObjTable(this.StageTable);

		// 상자가 없을 경우
		if (!bIsExistsBox && bIsEnableBox && this.BoxSpawnPointList.Count >= 1 && oBoxFieldObjTable != null)
		{
			var oMapObjHandler = GameResourceManager.Singleton.CreateObject<CInteractableMapObjHandler>(EResourceType.BG_Etc,
				oBoxFieldObjTable.Prefab, a_oMapObjsRoot.transform);

			oMapObjHandler.transform.position = this.BoxSpawnPointList[0];
			oMapObjHandler.Init(CInteractableMapObjHandler.MakeParams(oBoxFieldObjTable));

			this.InteractableMapObjHandlerList.Add(oMapObjHandler);
			this.BoxInteractableMapObjHandlerList.Add(oMapObjHandler);
		}

		// 룰렛 생성이 가능 할 경우
		if (!bIsExistsRoulette && this.RouletteSpawnPointList.Count >= 1)
		{
			var oMapObjHandler = GameResourceManager.Singleton.CreateObject<CInteractableMapObjHandler>(EResourceType.BG_Etc,
				oRouletteFieldObjTableList[0].Prefab, a_oMapObjsRoot.transform);

			oMapObjHandler.transform.position = this.RouletteSpawnPointList[0];
			oMapObjHandler.Init(CInteractableMapObjHandler.MakeParams(oRouletteFieldObjTableList[0]));

			this.InteractableMapObjHandlerList.Add(oMapObjHandler);
		}
	}

	/** 상자 필드 객체 테이블을 반환한다 */
	private FieldObjectTable GetRandomBoxFieldObjTable(StageTable a_oTable)
	{
		// 테이블이 없을 경우
		if (a_oTable == null)
		{
			return null;
		}

		float fSumPercent = a_oTable.WeaponChestWeight +
			a_oTable.MaterialChestWeight + a_oTable.CoinChestWeight + a_oTable.CrystalChestWeight;

		float fPercent = Random.Range(0.0f, fSumPercent);
		float fSelectPercent = a_oTable.WeaponChestWeight;

		// 무기 상자 생성이 가능 할 경우
		if (fPercent.ExIsLess(fSelectPercent))
		{
			return FieldObjectTable.GetData((int)EFieldObjKey.WEAPON_BOX);
		}

		fSelectPercent += a_oTable.MaterialChestWeight;

		// 재질 상자 생성이 가능 할 경우
		if (fPercent.ExIsLess(fSelectPercent))
		{
			return FieldObjectTable.GetData((int)EFieldObjKey.MATERIAL_BOX);
		}

		fSelectPercent += a_oTable.CoinChestWeight;

		// 코인 상자 생성이 가능 할 경우
		if (fPercent.ExIsLess(fSelectPercent))
		{
			return FieldObjectTable.GetData((int)EFieldObjKey.COIN_BOX);
		}

		fSelectPercent += a_oTable.CrystalChestWeight;

		// 크리스탈 상자 생성이 가능 할 경우
		if (fPercent.ExIsLess(fSelectPercent))
		{
			return FieldObjectTable.GetData((int)EFieldObjKey.CRYSTAL_BOX);
		}

		return null;
	}

	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oPlayerWarpAni, null);

		ComUtil.AssignVal(ref m_oCameraHeightAni, null);
		ComUtil.AssignVal(ref m_oCameraForwardAni, null);

		ComUtil.AssignVal(ref m_oInfiniteDescAni, null);
		ComUtil.AssignVal(ref m_oInfiniteFocusAni, null);
	}

	/** 상태를 갱신한다 */
	public void Update()
	{
		this.RunningTime += Time.deltaTime;
		this.StateMachine.OnUpdate(Time.deltaTime);

		for (int i = 0; i < this.NonPlayerControllerList.Count; ++i)
		{
			this.NonPlayerControllerList[i].OnUpdate(Time.deltaTime);
		}

		// 갱신이 불가능 할 경우
		if (!this.IsEnableUpdate || !this.PlayerController.StateMachine.IsEnable)
		{
			return;
		}

		this.RemainPlayTime = Mathf.Max(0.0f, this.RemainPlayTime - Time.deltaTime);

		switch (GameDataManager.Singleton.PlayMapInfoType)
		{
			case EMapInfoType.ABYSS:
			case EMapInfoType.BONUS: this.PageBattle.SetIsDirtyUpdateUIsState(true); break;
		}

		#region for 결과 테스트

#if UNITY_EDITOR
		if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.LeftBracket))
		{
			// 캠페인 일 경우
			if (GameDataManager.Singleton.PlayMode == EPlayMode.CAMPAIGN || GameDataManager.Singleton.PlayMode == EPlayMode.TUTORIAL)
			{
				GameDataManager.Singleton.SetPlayStageID(GameDataManager.Singleton.PlayMapInfoDict.Count - 1);
			}

			FinishBattle(true);
		}
		if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.RightBracket))
			FinishBattle(false);
		if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Slash))
			StartClearFailDirecting();
#endif
		#endregion
	}

	/** 상태를 갱신한다 */
	public void LateUpdate()
	{
		for (int i = 0; i < ComType.G_TIMES_CONCURRENT && this.HitUIsInfoQueue.Count >= 1; ++i)
		{
			var stHitUIsInfo = this.HitUIsInfoQueue.Dequeue();

			var oText = GameResourceManager.Singleton.CreateUIObject(EResourceType.UI_Battle,
				"BattleText", this.PageBattle.ContentsUIs.transform).GetComponent<CTextHandler>();

			oText.StartAni(stHitUIsInfo.m_stPos, $"{-stHitUIsInfo.m_nDamage}", stHitUIsInfo.m_eHitType);
		}
		
		// 갱신이 불가능 할 경우
		if (!this.IsEnableUpdate)
		{
			this.PlayerController.OnLateUpdate(Time.deltaTime);

			for (int i = 0; i < this.NonPlayerControllerList.Count; ++i)
			{
				this.NonPlayerControllerList[i].OnLateUpdate(Time.deltaTime);
			}

			return;
		}

		switch (GameDataManager.Singleton.PlayMapInfoType)
		{
			case EMapInfoType.ABYSS:
			case EMapInfoType.BONUS:
				// 심연 일 경우
				if (GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.ABYSS)
				{
					this.TryMakeAppearBoss();
				}

				// 플레이 시간이 지났을 경우
				if (this.RemainPlayTime.ExIsLessEquals(0.0f))
				{
					this.FinishBattle(false);
					this.SetIsEnableUpdate(false);
				}

				break;
		}

		// 상태 갱신이 가능 할 경우
		if (this.IsEnableUpdate)
		{
			this.PlayerController.OnLateUpdate(Time.deltaTime);

			for (int i = 0; i < this.NonPlayerControllerList.Count; ++i)
			{
				this.NonPlayerControllerList[i].OnLateUpdate(Time.deltaTime);
			}
		}

		this.TryHandleHintDirecting();
	}

	/** 상태를 갱신한다 */
	public void FixedUpdate()
	{
		// 갱신이 불가능 할 경우
		if (!this.IsEnableUpdate)
		{
			return;
		}

		for (int i = 0; i < this.NonPlayerControllerList.Count; ++i)
		{
			this.NonPlayerControllerList[i].OnFixedUpdate(Time.fixedDeltaTime);
		}
	}

	/** 이어한다 */
	public void Continue()
	{
		int nUntouchableTime = GlobalTable.GetData<int>(ComType.G_TIME_INVINCIBILITY_AFTER_REVIVE);

		this.StateMachine.SetState(this.CreatePlayState());
		this.PlayerController.Revive(nUntouchableTime * ComType.G_UNIT_MS_TO_S);
	}

	/** 다음 스테이지 통로를 개방한다 */
	public void OpenNextStagePassage(bool a_bIsForce = false)
	{
		// 통로 개방이 불가능 할 경우
		if (this.IsOpenNextStagePassage || (!a_bIsForce && this.TargetNonPlayerControllerList.Count >= 1))
		{
			return;
		}

		for (int i = 0; i < this.POAHandlerList.Count; ++i)
		{
			this.POAHandlerList[i].StartFX();
		}

		this.SetIsEnableUpdate(false);
		this.IsOpenNextStagePassage = true;
	}

	/** 미획득 아이템 정보를 설정한다 */
	public void SetupMissingItemInfos()
	{
		for (int i = 0; i < this.GroundItemHandlerList.Count; ++i)
		{
			uint nKey = this.GroundItemHandlerList[i].Key;
			int nNumItems = this.GroundItemHandlerList[i].NumItems;

			// 획득 가능한 아이템 일 경우
			if (ComUtil.IsEnableAcquireGroundItem(nKey))
			{
				int nNumMissingItems = GameDataManager.Singleton.MissingItemInfoDict.GetValueOrDefault(nKey);
				GameDataManager.Singleton.MissingItemInfoDict.ExReplaceVal(nKey, nNumMissingItems + nNumItems);
			}
		}

		this.GroundItemHandlerList.Clear();
	}

	/** 다음 스테이지로 이동한다 */
	public void MoveToNextStage(Collider a_oCollider)
	{
		// 다음 스테이지 이동 상태 일 경우
		if (this.IsMoveToNextStage || this.IsUpdateWeapons)
		{
			return;
		}

		this.IsMoveToNextStage = true;
		this.SetupMissingItemInfos();

		var oMapInfo = GameDataManager.Singleton.PlayMapInfo;
		int nPlayStageID = GameDataManager.Singleton.PlayStageID;

#if DISABLE_THIS
		// 튜토리얼 상태 일 경우
		if (PlayerPrefs.GetInt(ComType.G_KEY_TUTORIAL_STEP) == (int)ETutorialStep.NPC_LOCK_ON)
		{
			PlayerPrefs.SetInt(ComType.G_KEY_TUTORIAL_STEP, (int)ETutorialStep.NPC_HELP_AROUND);
			PlayerPrefs.Save();
		}
#endif // #if DISABLE_THIS

		switch (GameDataManager.Singleton.PlayMapInfoType)
		{
			case EMapInfoType.ABYSS: this.WarpToNextStage(oMapInfo, nPlayStageID, a_oCollider); break;
			default: this.MoveToNextStage(oMapInfo, nPlayStageID, a_oCollider); break;
		}
	}

	/** 다음 스테이지 이동을 처리한다 */
	private void HandleMoveToNextStage(CMapInfo a_oMapInfo, int a_nPlayStageID, Collider a_oCollider)
	{
		// 획득 경험치를 설정한다
		GameDataManager.Singleton.SetAcquireStageEXP(GameDataManager.Singleton.AcquireStageEXP + this.BattlePlayInfo.m_nRewardEXP);
		GameDataManager.Singleton.SetAcquireStagePassEXP(GameDataManager.Singleton.AcquireStagePassEXP + this.BattlePlayInfo.m_nRewardPassEXP);

		// 다음 맵 정보가 존재 할 경우
		if (GameDataManager.Singleton.PlayMapInfoDict.ContainsKey(a_nPlayStageID + 1))
		{
			StartCoroutine(this.CoHandleMoveToNextStage(a_oMapInfo, a_nPlayStageID, a_oCollider));
		}
		else
		{
			this.SetupAcquireItems();
			this.FinishBattle(true);
		}
	}

	/** 다음 스테이지 이동을 처리한다 */
	private IEnumerator CoHandleMoveToNextStage(CMapInfo a_oMapInfo, int a_nPlayStageID, Collider a_oCollider)
	{
		yield return new WaitForEndOfFrame();
		this.SetupBattlePlayerInfo();

		GameDataManager.Singleton.SetPlayStageID(a_nPlayStageID + 1);

		MenuManager.Singleton.SceneEnd();
		MenuManager.Singleton.SceneNext(ESceneType.Battle);
	}

	/** 다음 스테이지로 이동한다 */
	private void MoveToNextStage(CMapInfo a_oMapInfo, int a_nPlayStageID, Collider a_oCollider)
	{
		this.PlayerController.StateMachine.SetIsEnable(false);
		this.PlayerController.ContactStateMachine.SetState(this.PlayerController.CreateContactWarpState());

		this.PlayerController.SetUntouchableTime(byte.MaxValue);
		this.PlayerController.gameObject.ExSetLayer(LayerMask.NameToLayer("Default"));

		StopCoroutine("CoMoveToNextStage");
		StartCoroutine(CoMoveToNextStage(a_oMapInfo, a_nPlayStageID, a_oCollider));
	}

	/** 다음 스테이지로 이동한다 */
	private IEnumerator CoMoveToNextStage(CMapInfo a_oMapInfo, int a_nPlayStageID, Collider a_oCollider)
	{
		float fWarpDuration = 0.5f;
		float fWarpReadyDuration = 0.5f;
		float fFinalWarpDuration = fWarpReadyDuration + fWarpDuration + 0.15f;

		var oCamDummy = this.CamDummy.GetComponent<CamDummy>();
		var stWarpReadyPos = a_oCollider.transform.position + (Vector3.up * 0.5f);

		m_oPlayerWarpAni?.ExKill();
		this.PlayerController.Canvas.SetActive(false);

		var oSequence = DOTween.Sequence();
		oSequence.Append(this.PlayerController.transform.DOMove(stWarpReadyPos, fWarpReadyDuration));
		oSequence.Append(this.PlayerController.transform.DOMove(stWarpReadyPos + (Vector3.up * -25.0f), 1.5f));
		oSequence.AppendCallback(() => oSequence?.Kill());

		// 워프 효과가 존재 할 경우
		if (this.FXModelInfo.WarpFXInfo.m_oWarpStartFX != null)
		{
			var oWarpStartFX = GameResourceManager.Singleton.CreateObject<ParticleSystem>(this.FXModelInfo.WarpFXInfo.m_oWarpStartFX,
				null, null, 5.0f);

			oWarpStartFX.transform.position = a_oCollider.transform.position;

			oWarpStartFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			oWarpStartFX.Play(true);
		}

		ComUtil.AssignVal(ref m_oPlayerWarpAni, oSequence);
		yield return YieldInstructionCache.WaitForSeconds(fWarpReadyDuration);

		oCamDummy.SetIsEnableUpdate(false);
		yield return YieldInstructionCache.WaitForSeconds(fFinalWarpDuration);

		this.HandleMoveToNextStage(a_oMapInfo, a_nPlayStageID, a_oCollider);
	}

	/** 전투를 종료한다 */
	public void FinishBattle(bool isWin, float a_fDelay = 0.0f)
	{
		(this.StateMachine.State as CStateBattleController).FinishBattle(isWin, a_fDelay);
	}

	/** 전투를 종료한다 */
	public IEnumerator CoFinishBattle(bool a_bIsWin, float a_fDelay)
	{
		PopupWait4Response oWaitPopup = null;
		this.PageBattle.SetIsVisibleUIs(false);

		// 무한 모드가 아닐 경우
		if (!GameDataManager.Singleton.IsInfiniteWaveMode() || GameDataManager.Singleton.IsCampaignInfiniteWaveMode())
		{
			this.StateMachine.SetState(this.CreateFinishState());
			oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		}

		this.SetIsEnableUpdate(false);
		this.PageBattle.UpdateUIsState();

		ComUtil.SetTimeScale(1.0f);
		yield return YieldInstructionCache.WaitForSecondsRealtime(a_fDelay);

		this.StateMachine.SetState(this.CreateFinishState());
		this.SetupMissingItemInfos();

		// 테스트 모드가 아닐 경우
		if (GameDataManager.Singleton.PlayMode != EPlayMode.TEST)
		{
			float fRate = ComUtil.GetGoldenPointRate();
			GameManager.Singleton.user.IncreaseBonusPoint(this.GoldenPoint * fRate);

			yield return YieldInstructionCache.WaitForSecondsRealtime(a_fDelay);
		}

		bool bIsEnableAcquireEXP = a_bIsWin;
		bIsEnableAcquireEXP = bIsEnableAcquireEXP && GameDataManager.Singleton.PlayMode != EPlayMode.TEST;
		bIsEnableAcquireEXP = bIsEnableAcquireEXP && GameDataManager.Singleton.PlayMode != EPlayMode.DEFENCE;
		bIsEnableAcquireEXP = bIsEnableAcquireEXP && GameDataManager.Singleton.PlayMode != EPlayMode.INFINITE;
		bIsEnableAcquireEXP = bIsEnableAcquireEXP && GameDataManager.Singleton.PlayMode != EPlayMode.CAMPAIGN;
		bIsEnableAcquireEXP = bIsEnableAcquireEXP && GameDataManager.Singleton.PlayMode != EPlayMode.TUTORIAL;

		bool bIsCampaign = GameDataManager.Singleton.PlayMode == EPlayMode.CAMPAIGN || 
			GameDataManager.Singleton.PlayMode == EPlayMode.TUTORIAL;

		bool bIsOriginWin = a_bIsWin;
		a_bIsWin = a_bIsWin || GameDataManager.Singleton.IsInfiniteWaveMode() || bIsCampaign;

		ComUtil.SetTimeScale(1.0f);

		int nNumKills = m_nNumKillNonPlayers;
		int nBestNumKills = GameManager.Singleton.user.m_nMaxDefeatZombie;

		// 경험치 획득이 가능 할 경우
		if (bIsEnableAcquireEXP)
		{
			yield return GameManager.Singleton.user.GainExp(this.BattlePlayInfo.m_nRewardEXP,
				this.BattlePlayInfo.m_nRewardPassEXP);
		}

		switch (GameDataManager.Singleton.PlayMode)
		{
			case EPlayMode.CAMPAIGN:
			case EPlayMode.TUTORIAL:
				int nStageID = bIsOriginWin ?
					GameDataManager.Singleton.PlayStageID + 1 : GameDataManager.Singleton.PlayStageID;

				yield return GameManager.Singleton.user.ClearCampaign(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID,
																	  GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nChapterID,
																	  nStageID,
																	  isTutorial: GameDataManager.Singleton.PlayMode == EPlayMode.TUTORIAL);

				break;

			case EPlayMode.HUNT:
				var oHuntTable = HuntTable.GetHuntData(GameDataManager.Singleton.PlayHuntLV);

				yield return GameManager.Singleton.user.ClearHuntLevel(oHuntTable.PrimaryKey,
					a_bIsWin ? ECompleteMapType.Suscces : ECompleteMapType.Fail);

				break;

			case EPlayMode.ADVENTURE:
				yield return GameDataManager.Singleton.EndAdventureChapter(a_bIsWin ?
					ECompleteMapType.Suscces : ECompleteMapType.Fail);

				break;

			case EPlayMode.ABYSS:
				uint nAbyssTableKey = ComUtil.GetAbyssKey(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID,
					GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nChapterID + 1);

				yield return GameManager.Singleton.user.ClearAbyss(nAbyssTableKey,
					a_bIsWin ? ECompleteMapType.Suscces : ECompleteMapType.Fail, Mathf.CeilToInt(this.SkipPlayTime / ComType.G_UNIT_MS_TO_S));

				break;

			case EPlayMode.INFINITE:
				yield return GameManager.Singleton.user.ClearZombie(m_nNumKillNonPlayers, m_nWaveOrder + 1);
				break;
		}

		oWaitPopup?.Close();

		switch (GameDataManager.Singleton.PlayMode)
		{
			case EPlayMode.TEST:
				MenuManager.Singleton.SceneEnd();
				MenuManager.Singleton.SceneNext(ESceneType.MapEditor);

				break;

			case EPlayMode.ADVENTURE:
				MenuManager.Singleton.IsClearMissionAdventure = a_bIsWin && !GameManager.Singleton.IsEnableStartMissionAdventure();
				MenuManager.Singleton.IsMoveToMissionAdventure = true;

				MenuManager.Singleton.SceneEnd();
				MenuManager.Singleton.SceneNext(ESceneType.Lobby);

				break;

			case EPlayMode.ABYSS:
				MenuManager.Singleton.IsMoveToAbyss = true;

				MenuManager.Singleton.SceneEnd();
				MenuManager.Singleton.SceneNext(ESceneType.Lobby);

				break;

			case EPlayMode.DEFENCE:
				// 웨이브 진행에 실패했을 경우
				if (this.WaveOrder <= 0)
				{
					MenuManager.Singleton.SceneEnd();
					MenuManager.Singleton.SceneNext(ESceneType.Lobby);
				}
				else
				{
					PopupMultyReward MultyResult = MenuManager.Singleton.OpenPopup<PopupMultyReward>(EUIPopup.PopupMultyReward);
					MultyResult.InitializeInfo(this.DefenceTableList[this.WaveOrder - 1]);
				}

				MenuManager.Singleton.IsMoveToMissionDefence = true;
				yield return YieldInstructionCache.WaitForSecondsRealtime(1.0f);

				this.PathObjRoot.gameObject.SetActive(false);
				break;

			default:
				var oRewardInfoDict = RewardTable.RandomResultInGroup(this.BattlePlayInfo.m_nRewardGroup);
				var oRewardInfoList = oRewardInfoDict.Keys.ToList();

				bool bIsCompleteCampaign = bIsOriginWin;
				bIsCompleteCampaign = bIsCompleteCampaign && bIsCampaign;

				bool bIsInfiniteWaveMode = GameDataManager.Singleton.IsInfiniteWaveMode() &&
					!GameDataManager.Singleton.IsCampaignInfiniteWaveMode();

				oRewardInfoList.ExShuffle();

				// 캠페인 일 경우
				if (bIsCampaign)
				{
					a_bIsWin = bIsOriginWin || GameDataManager.Singleton.PlayStageID > 0;

					// 완료 상태가 아닐 경우
					if (!bIsCompleteCampaign)
					{
						GameDataManager.Singleton.AcquireWeaponList.Clear();
						GameDataManager.Singleton.AcquireItemInfoDict.Clear();
					}
				}

				PopupBattleReward oBattleResult = MenuManager.Singleton.OpenPopup<PopupBattleReward>(bIsInfiniteWaveMode ?
					EUIPopup.PopupBattleInfiniteReward : EUIPopup.PopupBattleReward);

				// 캠페인 일 경우
				if (bIsCampaign)
				{
					oBattleResult.InitializeRewards(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo,
						a_bIsWin, GameDataManager.Singleton.OriginAcquireWeaponList, GameDataManager.Singleton.OriginAcquireItemInfoDict, GameDataManager.Singleton.MissingItemInfoDict, (a_bIsWin && !bIsInfiniteWaveMode) ? oRewardInfoList.FirstOrDefault() : 0, bIsCompleteCampaign, GameDataManager.Singleton.AcquireWeaponList, GameDataManager.Singleton.AcquireItemInfoDict);
				}
				else
				{
					oBattleResult.InitializeRewards(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo,
						a_bIsWin, GameDataManager.Singleton.AcquireWeaponList, GameDataManager.Singleton.AcquireItemInfoDict, GameDataManager.Singleton.MissingItemInfoDict, (a_bIsWin && !bIsInfiniteWaveMode) ? oRewardInfoList.FirstOrDefault() : 0, bIsCompleteCampaign, GameDataManager.Singleton.AcquireWeaponList, GameDataManager.Singleton.AcquireItemInfoDict);
				}

#if DISABLE_THIS
				oBattleResult.InitializeRewards(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo,
					a_bIsWin, GameDataManager.Singleton.OriginAcquireWeaponList, GameDataManager.Singleton.OriginAcquireItemInfoDict, GameDataManager.Singleton.MissingItemInfoDict, (a_bIsWin && !bIsInfiniteWaveMode) ? oRewardInfoList.FirstOrDefault() : 0, bIsCompleteCampaign, GameDataManager.Singleton.AcquireWeaponList, GameDataManager.Singleton.AcquireItemInfoDict);
#endif // #if DISABLE_THIS

				// 무한 모드 일 경우
				if (bIsInfiniteWaveMode && oBattleResult.TryGetComponent(out PopupBattleInfiniteReward oPopupWave))
				{
					oPopupWave.Init(nNumKills, nBestNumKills);
				}

				break;
		}
	}

	/** 보스 격파 연출을 처리한다 */
	private IEnumerator CoHandleBossDestroyDirecting()
	{
		yield return YieldInstructionCache.WaitForSeconds(0.5f);
		ComUtil.SetTimeScale(1.0f);

		yield return YieldInstructionCache.WaitForSeconds(0.25f);
		this.StopAllNonPlayerRagdolls();

		// 심연이 아닐 경우
		if (GameDataManager.Singleton.PlayMapInfoType != EMapInfoType.ABYSS)
		{
			yield break;
		}

		yield return YieldInstructionCache.WaitForSeconds(1.0f);
		this.FinishBattle(true);
	}

	/** 보스 등장 연출을 시작한다 */
	public void StartBossEnterDirecting()
	{
		var oCamDummy = this.CamDummy.GetComponent<CamDummy>();
		var oNonPlayerController = this.FindNonPlayerControllerBoss();

		var fOriginCameraHeight = this.CameraMove._fCamHeight;
		var fOriginCameraForward = this.CameraMove._fCamForward;
		var fOriginCamDummyDistance = oCamDummy._fDistance;

		this.StartBossGaugeDirecting();
		oNonPlayerController.SetupOpenAni();

		this.StartCameraDirecting(ComType.G_OFFSET_CAMERA_HEIGHT_FOR_FOCUS,
			ComType.G_OFFSET_CAMERA_FORWARD_FOR_FOCUS, ComType.G_OFFSET_CAMERA_DISTANCE_FOR_FOCUS, oNonPlayerController.gameObject, a_bIsIgnoreDirection: true);

		this.ExLateCallFunc((a_oSender) =>
		{
			var oState = this.StateMachine.State as CStateBattleController;
			oState?.SetIsCompleteCameraDirecting(true);

			this.StartCameraDirecting(fOriginCameraHeight,
				fOriginCameraForward, fOriginCamDummyDistance, this.PlayerController.gameObject);
		}, 3.5f);
	}

	/** 보스 게이지 연출을 시작한다 */
	public void StartBossGaugeDirecting()
	{
		var oAnimator = this.PageBattle.GaugeBossUIsHandler.GetComponent<Animator>();
		oAnimator.updateMode = AnimatorUpdateMode.Normal;
	}

	/** 보스 격파 연출을 시작한다 */
	public void StartBossDestroyDirecting()
	{
		ComUtil.SetTimeScale(0.35f);
		this.StartCoroutine(this.CoHandleBossDestroyDirecting());
	}

	/** 클리어 실패 연출을 시작한다 */
	public void StartClearFailDirecting()
	{
		for (int i = 0; i < this.NonPlayerControllerList.Count; ++i)
		{
			this.NonPlayerControllerList[i].StopAllActions();
		}

		ComUtil.SetTimeScale(0.35f);
		this.ExLateCallFunc((a_oSender) => this.OnCompleteClearFailDirecting(), 0.5f);
	}

	/** 카메라 연출을 시작한다 */
	public void StartCameraDirecting(float a_fHeight,
		float a_fForward, float a_fDistance, GameObject a_oTarget, bool a_bIsRealtime = false, bool a_bIsIgnoreDirection = false)
	{
		m_oCameraHeightAni?.ExKill();
		m_oCameraForwardAni?.ExKill();

		ComUtil.AssignVal(ref m_oCameraHeightAni, DOTween.To(() => this.CameraMove._fCamHeight, (a_fVal) => this.CameraMove._fCamHeight = a_fVal, a_fHeight, ComType.G_DURATION_CAMERA_ANI).SetUpdate(a_bIsRealtime));
		ComUtil.AssignVal(ref m_oCameraForwardAni, DOTween.To(() => this.CameraMove._fCamForward, (a_fVal) => this.CameraMove._fCamForward = a_fVal, a_fForward, ComType.G_DURATION_CAMERA_ANI).SetUpdate(a_bIsRealtime));

		var oCamDummy = this.CamDummy.GetComponent<CamDummy>();
		oCamDummy._fDistance = a_fDistance;
		oCamDummy.bIsRealtime = a_bIsRealtime;
		oCamDummy.IsIgnoreDirection = a_bIsIgnoreDirection;
		oCamDummy.SetTarget(a_oTarget);

		this.CameraMove.IsFocus = a_bIsIgnoreDirection && GameDataManager.Singleton.IsInfiniteWaveMode();
	}

	/** 데미지 텍스트를 출력한다 */
	public void ShowDamageText(int a_nDamage, EHitType a_eHitType, Vector3 a_stPos)
	{
		var stHitUIsInfo = new STHitUIsInfo()
		{
			m_nDamage = a_nDamage,
			m_eHitType = a_eHitType,
			m_stPos = a_stPos
		};

		this.HitUIsInfoQueue.Enqueue(stHitUIsInfo);
	}

	/** 유닛을 추가한다 */
	public void AddUnitController(UnitController a_oController)
	{
		var oUnitControllerList = this.UnitControllerDictContainer.GetValueOrDefault(a_oController.TargetGroup) ??
			new List<UnitController>();

		var oOriginUnitControllerList = this.OriginUnitControllerDictContainer.GetValueOrDefault(a_oController.TargetGroup) ??
			new List<UnitController>();

		this.UnitControllerDictContainer.TryAdd(a_oController.TargetGroup, oUnitControllerList);
		this.OriginUnitControllerDictContainer.TryAdd(a_oController.TargetGroup, oOriginUnitControllerList);

		oUnitControllerList.ExAddVal(a_oController);
		oOriginUnitControllerList.ExAddVal(a_oController);

		var oNonPlayerController = a_oController as NonPlayerController;

		// NPC 일 경우
		if (oNonPlayerController != null && a_oController.TargetGroup != (int)ETargetGroup.PLAYER)
		{
			this.NonPlayerControllerList.ExAddVal(oNonPlayerController);
		}

		// 목표 NPC 일 경우
		if (oNonPlayerController != null && oNonPlayerController.Table.isNecessary > 0)
		{
			this.TargetNonPlayerControllerList.ExAddVal(oNonPlayerController);
		}
	}

	/** 유닛을 제거한다 */
	public void RemoveUnitController(UnitController a_oController)
	{
		// 유닛 제거가 불가능 할 경우
		if (!this.UnitControllerDictContainer.TryGetValue(a_oController.TargetGroup, out List<UnitController> oUnitControllerList))
		{
			return;
		}

		oUnitControllerList.ExRemoveVal(a_oController);

		this.NonPlayerControllerList.ExRemoveVal(a_oController as NonPlayerController);
		this.TargetNonPlayerControllerList.ExRemoveVal(a_oController as NonPlayerController);
	}

	/** 보스 NPC 를 반환한다 */
	public NonPlayerController FindNonPlayerControllerBoss()
	{
		for (int i = 0; i < this.NonPlayerControllerList.Count; ++i)
		{
			// 보스 NPC 일 경우
			if (this.NonPlayerControllerList[i].IsBossNPC)
			{
				return this.NonPlayerControllerList[i];
			}
		}

		return null;
	}

	/** 보너스 NPC 를 반환한다 */
	public List<NonPlayerController> FindNonPlayerControllersBonus()
	{
		var oNonPlayerControllerBonusList = new List<NonPlayerController>();

		for (int i = 0; i < this.NonPlayerControllerList.Count; ++i)
		{
			// 보너스 NPC 가 아닐 경우
			if (!this.NonPlayerControllerList[i].IsBonusNPC)
			{
				continue;
			}

			oNonPlayerControllerBonusList.Add(this.NonPlayerControllerList[i]);
		}

		return oNonPlayerControllerBonusList;
	}

	/** 전투 플레이어 정보를 설정한다 */
	private void SetupBattlePlayerInfo()
	{
		GameDataManager.Singleton.SetIsContinuePlay(true);
		GameDataManager.Singleton.SetPlayerHP(this.PlayerController.HP);

		GameDataManager.Singleton.SetGoldenPoint(this.GoldenPoint);
		GameDataManager.Singleton.SetEquipWeaponIdx(this.PlayerController.CurWeaponIdx);

		GameDataManager.Singleton.SetStageInstLV(this.PlayerController.StageInstLV);
		GameDataManager.Singleton.SetAccumulateStageInstEXP(this.PlayerController.AccumulateStageInstEXP);

		GameDataManager.Singleton.SetPlayerActiveSkillPoint(this.PlayerController.IsUseSkill ?
			0.0f : this.PlayerController.CurActiveSkillPoint);

		for (int i = 0; i < this.PlayerController.ReloadInfoList.Count; ++i)
		{
			GameDataManager.Singleton.ReloadInfos[i] = this.PlayerController.ReloadInfoList[i];
		}

		for (int i = 0; i < this.PlayerController.MagazineInfoList.Count; ++i)
		{
			GameDataManager.Singleton.MagazineInfos[i] = this.PlayerController.MagazineInfoList[i];
		}

		this.PageBattle.BuffEffectTableInfoList.ExCopyTo(GameDataManager.Singleton.BuffEffectTableInfoList,
			(a_oTableInfo) => a_oTableInfo);

		this.PlayerController.PassiveEffectStackInfoList.ExCopyTo(GameDataManager.Singleton.PassiveEffectStackInfoList,
			(a_stStackInfo) => a_stStackInfo);

		this.NonPlayerPassiveEffectStackInfoList.ExCopyTo(GameDataManager.Singleton.NonPlayerPassiveEffectStackInfoList,
			(a_stStackInfo) => a_stStackInfo);
	}

	/** 카메라 영역을 설정한다 */
	private void SetupNavMeshCameraBounds()
	{
		this.PlayerNavMeshBounds = this.FindPlayerNavMeshBounds();

		float fCameraOffsetTop = GlobalTable.GetData<int>(ComType.G_RANGE_CAMERA_OFFSET_TOP) * ComType.G_UNIT_MM_TO_M;
		float fCameraOffsetBottom = GlobalTable.GetData<int>(ComType.G_RANGE_CAMERA_OFFSET_BOTTOM) * ComType.G_UNIT_MM_TO_M;
		float fCameraOffsetLeft = GlobalTable.GetData<int>(ComType.G_RANGE_CAMERA_OFFSET_LEFT) * ComType.G_UNIT_MM_TO_M;
		float fCameraOffsetRight = GlobalTable.GetData<int>(ComType.G_RANGE_CAMERA_OFFSET_RIGHT) * ComType.G_UNIT_MM_TO_M;

		var stCameraBounds = this.PlayerNavMeshBounds;
		var stCameraBoundsMin = new Vector3(stCameraBounds.min.x + fCameraOffsetLeft, stCameraBounds.min.y, stCameraBounds.min.z + fCameraOffsetBottom);
		var stCameraBoundsMax = new Vector3(stCameraBounds.max.x - fCameraOffsetRight, stCameraBounds.max.y, stCameraBounds.max.z - fCameraOffsetTop);

		stCameraBounds.min = Vector3.Min(stCameraBoundsMin, stCameraBoundsMax);
		stCameraBounds.max = Vector3.Max(stCameraBoundsMin, stCameraBoundsMax);

		this.CamDummy.GetComponent<CamDummy>().SetTarget(_Player);
		this.CamDummy.GetComponent<CamDummy>().SetBounds(stCameraBounds);
		this.CamDummy.GetComponent<CamDummy>().SetupCameraPos(true);
	}

	/** 획득 아이템을 설정한다 */
	private void SetupAcquireItems()
	{
		for (int i = 0; i < GameDataManager.Singleton.AcquireWeaponList.Count; ++i)
		{
			uint nKey = GameDataManager.Singleton.AcquireWeaponList[i];
			GameDataManager.Singleton.OriginAcquireWeaponList.Add(nKey);
		}

		foreach (var stKeyVal in GameDataManager.Singleton.AcquireItemInfoDict)
		{
			int nNumItems = GameDataManager.Singleton.AcquireItemInfoDict.GetValueOrDefault(stKeyVal.Key);
			int nOriginNumItems = GameDataManager.Singleton.OriginAcquireItemInfoDict.GetValueOrDefault(stKeyVal.Key);

			GameDataManager.Singleton.OriginAcquireItemInfoDict.ExReplaceVal(stKeyVal.Key, nOriginNumItems + nNumItems);
		}
	}

	/** 종료 연출이 완료 되었을 경우 */
	private void OnCompleteFinishDirecting(bool a_bIsWin, float a_fDelay)
	{
		ComUtil.SetTimeScale(1.0f);
		this.FinishBattle(a_bIsWin, a_fDelay);
	}

	/** 클리어 실패 연출이 완료 되었을 경우 */
	private void OnCompleteClearFailDirecting()
	{
		ComUtil.SetTimeScale(1.0f);

		// 이어하기가 가능 할 경우
		if (GameDataManager.Singleton.ContinueTimes < this.PageBattle.MaxContinueTimes)
		{
			this.StateMachine.SetState(this.CreateContinueState());
			this.PageBattle.OpenContinuePopup();
		}
		else
		{
			this.OnCompleteFinishDirecting(false, 1.0f);
		}
	}

	/** 플레이어 내비게이션 메쉬 영역을 탐색한다 */
	private Bounds FindPlayerNavMeshBounds()
	{
		int nLayer = NavMesh.GetAreaFromName(ComType.G_NAV_MESH_AREA_WALKABLE);

		// 인덱스가 존재 할 경우
		if (this.NavMeshController.NavMeshIdxDictContainer.TryGetValue(nLayer, out List<List<int>> oIdxListContainer))
		{
			for (int i = 0; i < oIdxListContainer.Count; ++i)
			{
				for (int j = 0; j < oIdxListContainer[i].Count; j += 3)
				{
					var oVertexList = CCollectionPoolManager.Singleton.SpawnList<Vector3>();

					try
					{
						int nIdx01 = oIdxListContainer[i][j + 0];
						int nIdx02 = oIdxListContainer[i][j + 1];
						int nIdx03 = oIdxListContainer[i][j + 2];

						oVertexList.Add(this.NavMeshController.Params.m_stTriangulation.vertices[nIdx01]);
						oVertexList.Add(this.NavMeshController.Params.m_stTriangulation.vertices[nIdx02]);
						oVertexList.Add(this.NavMeshController.Params.m_stTriangulation.vertices[nIdx03]);

						// 내비게이션 메쉬 영역이 존재 할 경우
						if (this.NavMeshController.IsContains(oVertexList, this.Player.transform.position))
						{
							return this.NavMeshController.BoundsDictContainer[nLayer][i];
						}
					}
					finally
					{
						CCollectionPoolManager.Singleton.DespawnList(oVertexList);
					}
				}
			}
		}

		return ComType.G_EMPTY_BOUNDS;
	}

	/** 상태를 갱신한다 */
	private IEnumerator CoCustomUpdate()
	{
		float fDeltaTime = 1.0f / (GlobalTable.GetData<int>(ComType.G_COUNT_CHECK_AI_CONDITION_PER_SECOND) * 4.0f);
		fDeltaTime = Mathf.Max(1.0f / 60.0f, fDeltaTime);

		do
		{
			yield return YieldInstructionCache.WaitForSeconds(fDeltaTime);
			this.PlayerController.OnCustomUpdate(fDeltaTime);

			for (int i = 0; i < this.NonPlayerControllerList.Count; ++i)
			{
				this.NonPlayerControllerList[i].OnCustomUpdate(fDeltaTime);
			}

			this.StateMachine.OnCustomUpdate(fDeltaTime);
		} while (!this.IsFinish);
	}

	/** 아이템 객체를 추가한다 */
	public void AddGroundItemObjs(List<GameObject> a_oItemObjList, Vector3 a_stOriginPos)
	{
		for (int i = 0; i < a_oItemObjList.Count; ++i)
		{
			var stDirection = new Vector3(Random.Range(-1.0f, 1.0f), 1.0f, Random.Range(-1.0f, 1.0f));
			a_oItemObjList[i].transform.position = a_stOriginPos;

			this.GroundItemHandlerList.ExAddVal(a_oItemObjList[i].GetComponent<CGroundItemHandler>());
			a_oItemObjList[i].GetComponent<Rigidbody>().AddForce((stDirection.normalized + Vector3.up) * Random.Range(ComType.G_MIN_FORCE_GROUND_ITEM, ComType.G_MAX_FORCE_GROUND_ITEM), ForceMode.VelocityChange);
		}
	}

	/** 모든 NPC 렉돌을 정지한다 */
	private void StopAllNonPlayerRagdolls()
	{
		foreach (var stKeyVal in this.OriginUnitControllerDictContainer)
		{
			// 플레이어 그룹 일 경우
			if (stKeyVal.Key == (int)ETargetGroup.PLAYER || stKeyVal.Key == (int)ETargetGroup.COMMON)
			{
				continue;
			}

			for (int i = 0; i < stKeyVal.Value.Count; ++i)
			{
				// 생존 상태 일 경우
				if (stKeyVal.Value[i].IsSurvive)
				{
					continue;
				}

				stKeyVal.Value[i].RagdollController?.OnCompleteRagdollAni(null);
			}
		}
	}

	/** 충돌 시작을 처리한다 */
	private void HandleOnTriggerEnter(CTriggerDispatcher a_oSender, Collider a_oCollider)
	{
		bool bIsPOA = a_oCollider.CompareTag(ComType.G_TAG_POA);
		bool bIsWarpGate = a_oCollider.CompareTag(ComType.G_TAG_WARP_GATE);

		// 다음 스테이지 이동 지점 일 경우
		if (a_oCollider.enabled && (bIsPOA || bIsWarpGate))
		{
			this.MoveToNextStage(a_oCollider);
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 이전 킬 사운드 인덱스를 변경한다 */
	public void SetCurSkillSoundIdx(int a_nIdx)
	{
		this.CurSkillSoundIdx = a_nIdx;
	}

	/** 스테이지 인스턴스 경험치 비율을 변경한다 */
	public void SetInstEXPRatio(float a_fRatio)
	{
		this.InstEXPRatio = a_fRatio;
	}

	/** 이전 킬 사운드 재생 시간을 변경한다 */
	public void SetPrevKillSoundPlayTime(System.DateTime a_stTime)
	{
		this.PrevKillSoundPlayTime = a_stTime;
	}

	/** 릴리즈 유닛을 추가한다 */
	public void AddReleaseUnit(GameObject a_oUnitObj, GameObject a_oWeaponObj)
	{
		// 최대 개수를 넘어갔을 경우
		if (this.ReleaseUnitInfoQueue.Count >= m_nMaxNumReleaseUnitStacks)
		{
			var stReleaseUnitInfo = this.ReleaseUnitInfoQueue.Dequeue();

			GameResourceManager.Singleton.ReleaseObject(stReleaseUnitInfo.m_oUnitObj, false);
			GameResourceManager.Singleton.ReleaseObject(stReleaseUnitInfo.m_oWeaponObj, false);
		}

		this.ReleaseUnitInfoQueue.Enqueue(new STReleaseUnitInfo()
		{
			m_oUnitObj = a_oUnitObj,
			m_oWeaponObj = a_oWeaponObj
		});
	}
	#endregion // 접근 함수
}

/** 전투 제어자 - 코루틴 */
public partial class BattleController : MonoBehaviour
{
	#region 함수
	/** 획득 무기를 갱신한다 */
	private IEnumerator UpdateAcquireWeapons()
	{
		this.IsUpdateWeapons = true;
		yield return new WaitForEndOfFrame();

		this.SetupAcquireItems();
		this.PageBattle.UpdateUIsState();

		for (int i = 0; i < GameDataManager.Singleton.AcquireWeaponList.Count; ++i)
		{
			uint nKey = GameDataManager.Singleton.AcquireWeaponList[i];
			yield return GameManager.Singleton.StartCoroutine(GameManager.Singleton.AddItemCS(nKey, 1));
		}

		foreach (var stKeyVal in GameDataManager.Singleton.AcquireItemInfoDict)
		{
			yield return GameManager.Singleton.StartCoroutine(GameManager.Singleton.AddItemCS(stKeyVal.Key, stKeyVal.Value));
		}

		yield return new WaitForEndOfFrame();
		this.IsUpdateWeapons = false;

		GameDataManager.Singleton.AcquireWeaponList.Clear();
		GameDataManager.Singleton.AcquireItemInfoDict.Clear();

		this.PageBattle.UpdateUIsState();
		this.PageBattle.BattleInventory.Reset();
	}
	#endregion // 함수
}
#endregion // 추가
