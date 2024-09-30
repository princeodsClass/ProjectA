using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/** NPC 제어자 */
public partial class NonPlayerController : UnitController
{
	#region 변수
	[Header("=====> Non Player Controller - Etc <=====")]
	[SerializeField] private bool m_bIsWeaponFixType = false;
	[SerializeField] private int m_nOpenAniType = 0;
	[SerializeField] private AudioClip m_oOpenAudioClip = null;

	private bool m_bIsEnableShowWarning = true;

	private int m_nFireFXGroup = 0;
	private int m_nNumFireProjectilesAtOnce = 0;

	private float m_fAttackRange = 0.0f;
	private float m_fMoveSpeedRun = 0.0f;
	private float m_fMoveSpeedWalk = 0.0f;

	private ENPCGrade m_eNPCGrade = ENPCGrade.NONE;
	private EDamageType m_eDamageType = EDamageType.NONE;
	private EAttackType m_eNextAttackType = EAttackType.NONE;
	private EWeaponAnimationType m_eWeaponAniType = EWeaponAnimationType.END;

	private Tween m_oTalkAni = null;
	private Tween m_oWarningAni = null;
	private Tween m_oLookAroundAni = null;

	private SkillModelInfo m_oSkillModelInfo = null;
	private SoundModelInfo m_oSoundModelInfo = null;
	private WeaponModelInfo m_oWeaponModelInfo = null;
	private CSightLineHandler m_oSightLineHandler = null;

	private List<string> m_oApplyHitSkillList = new List<string>();
	private List<string> m_oDropHitGroundItemsList = new List<string>();

	public Dictionary<EEquipEffectType, float> m_oAbilityValDict = new Dictionary<EEquipEffectType, float>();
	public Dictionary<EEquipEffectType, float> m_oOriginAbilityValDict = new Dictionary<EEquipEffectType, float>();
	public Dictionary<EEquipEffectType, float> m_oStandardAbilityValDict = new Dictionary<EEquipEffectType, float>();

	[Header("=====> Non Player Controller - UIs <=====")]
	[SerializeField] private TMP_Text m_oLVText = null;
	private TMP_Text m_oTalkText = null;
	private Image m_oWarningImg = null;

	[Header("=====> Non Player Controller - Game Objects <=====")]
	[SerializeField] private GameObject m_oGradeUIs = null;
	private GameObject m_oEtcUIs = null;
	private GameObject m_oTalkUIs = null;
	private GameObject m_oEquipWeaponObj = null;

	private List<GameObject> m_oGradeUIsList = new List<GameObject>();

#if UNITY_EDITOR
	private GameObject m_oEditorSightRange = null;
	public GameObject EditorSightRange => m_oEditorSightRange;
#endif // #if UNITY_EDITOR
	#endregion // 변수

	#region 프로퍼티
	public bool IsWaveNPC { get; private set; } = false;
	public bool IsNeedUIs { get; private set; } = false;
	public bool IsNeedAppear { get; private set; } = false;
	public bool IsAvoidAction { get; private set; } = false;

	public Vector3 StartPos { get; private set; } = Vector3.zero;

	public NPCTable Table { get; private set; } = null;
	public CObjInfo ObjInfo { get; private set; } = null;
	public NPCStatTable StatTable { get; private set; } = null;
	public SkillGroupTable NextSelectionSkillGroupTable { get; private set; } = null;
	public List<CWayPointInfo> WayPointInfoList { get; } = new List<CWayPointInfo>();

	public Transform[] SkinnedBones { get; private set; } = null;
	public Transform[] OriginSkinnedBones { get; private set; } = null;

	public List<string> BoneNameList { get; } = new List<string>();

	public bool IsBossNPC => m_eNPCGrade == ENPCGrade.BOSS;
	public bool IsBonusNPC => m_eNPCGrade == ENPCGrade.BONUS;
	public bool IsWeaponFixType => m_bIsWeaponFixType;

	public virtual bool IsEnableMove => true;
	public virtual bool IsEnableTracking => this.ObjInfo == null || this.ObjInfo.m_ePatrolType != EPatrolType.FIX;

	public override bool IsEnableKnockBack => (ENPCType)this.Table.Type != ENPCType.TURRET;
	public override bool IsIgnoreTargetSurvive => GameDataManager.Singleton.IsWaveMode();

	public override int TargetGroup => this.Table.Camp;
	public override int FireFXGroup => m_nFireFXGroup;
	public override int NumFireProjectilesAtOnce => m_nNumFireProjectilesAtOnce;

	public override float MoveSpeed => this.GetMoveSpeed();
	public override float AttackRangeStandard => m_fAttackRange;

	public override EDamageType DamageType => m_eDamageType;
	public override EWeaponAnimationType WeaponAniType => m_eWeaponAniType;

	public override GameObject CurWeaponObj => m_oEquipWeaponObj;
	public override SkillModelInfo CurSkillModelInfo => m_oSkillModelInfo;
	public override SoundModelInfo CurSoundModelInfo => m_oSoundModelInfo;
	public override WeaponModelInfo CurWeaponModelInfo => m_oWeaponModelInfo;
	public override CSightLineHandler CurSightLineHandler => m_oSightLineHandler;

	public override Dictionary<EEquipEffectType, float> CurAbilityValDict => m_oAbilityValDict;
	public override Dictionary<EEquipEffectType, float> CurOriginAbilityValDict => m_oOriginAbilityValDict;
	public override Dictionary<EEquipEffectType, float> CurStandardAbilityValDict => m_oStandardAbilityValDict;
	#endregion // 프로퍼티

	#region 함수
	/** 발자국 이벤트를 수신했을 경우 */
	public override void Footstep(Object a_oParams)
	{
		// 달리기 상태 일 경우
		if (this.TrackingTarget != null && this.Table.Theme != 3)
		{
			base.Footstep(a_oParams);
			this.BattleController.PlayRunSound(this.gameObject);
		}
		else
		{
			this.BattleController.PlayWalkSound(this.gameObject);
		}
	}

	/** 초기화 */
	public override void Awake()
	{
		base.Awake();

		// 등급 UI 가 존재 할 경우
		if (m_oGradeUIs != null)
		{
			for (int i = 0; i < m_oGradeUIs.transform.childCount; ++i)
			{
				m_oGradeUIsList.Add(m_oGradeUIs.transform.GetChild(i).gameObject);
			}
		}

		// 스킨드 메시 렌더러가 존재 할 경우
		if (this.SkinnedMeshRenderer != null)
		{
			for (int i = 0; i < this.SkinnedMeshRenderer.bones.Length; ++i)
			{
				this.BoneNameList.Add(this.SkinnedMeshRenderer.bones[i].name);
			}

			this.SkinnedBones = new Transform[this.BoneNameList.Count];
			this.OriginSkinnedBones = this.SkinnedMeshRenderer.bones;
		}

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
		// 맵 에디터 씬 일 경우
		if (MenuManager.Singleton.CurScene == ESceneType.MapEditor)
		{
			var oColliders = this.GetComponentsInChildren<Collider>();

			for (int i = 0; i < oColliders.Length; ++i)
			{
				oColliders[i].isTrigger = true;
			}
		}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
	}

	/** 초기화 */
	public override void Start()
	{
		base.Start();

		// 맵 에디터 씬 일 경우
		if (MenuManager.Singleton.CurScene == ESceneType.MapEditor)
		{
			return;
		}

		int nAreaMask = 1 << NavMesh.GetAreaFromName(ComType.G_NAV_MESH_AREA_WALKABLE);
		NavMeshHit stNavMeshHit;

		// 내비 메쉬 위치가 존재 할 경우
		if (NavMesh.SamplePosition(this.transform.position + (Vector3.up * 1.5f), out stNavMeshHit, float.MaxValue / 2.0f, nAreaMask))
		{
			this.transform.position = stNavMeshHit.position;
		}

		// 이동 지점 정보를 설정한다
		for (int i = 0; i < this.WayPointInfoList.Count; ++i)
		{
			// 내비 메쉬 위치가 존재 할 경우
			if (NavMesh.SamplePosition(this.WayPointInfoList[i].m_stTransInfo.m_stPos + (Vector3.up * 1.5f), out stNavMeshHit, float.MaxValue / 2.0f, nAreaMask))
			{
				this.WayPointInfoList[i].m_stTransInfo.m_stPos = stNavMeshHit.position;
			}
		}

		this.NavMeshAgent.enabled = true;
		this.NavMeshAgent.isStopped = true;
		this.NavMeshAgent.autoBraking = false;
		this.NavMeshAgent.acceleration = short.MaxValue;
		this.NavMeshAgent.angularSpeed = short.MaxValue;

#if UNITY_EDITOR
		var oSighRange = this.transform.Find("SighRange")?.gameObject ??
			GameResourceManager.Singleton.CreateObject(this.BattleController.EditorOriginSighRange, this.transform, null);

		oSighRange.name = "SighRange";
		oSighRange.transform.localPosition = Vector3.up * 0.25f;

		var oLineFX = oSighRange.GetComponentInChildren<LineRenderer>();
		var oPosList = CCollectionPoolManager.Singleton.SpawnList<Vector3>();

		try
		{
			float fAngle = this.Table.SightAngle;
			float fOffset = fAngle / 10.0f;

			for (float i = 0.0f; i <= fAngle; i += fOffset)
			{
				var stDirection = (Vector3)(Quaternion.AngleAxis(i - (fAngle / 2.0f), Vector3.up) * Vector3.forward);
				oPosList.Add(stDirection * (this.Table.SightRange * ComType.G_UNIT_MM_TO_M) + (Vector3.up * 0.25f));
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

		m_oEditorSightRange = oSighRange.gameObject;
		m_oEditorSightRange.SetActive(false);
#endif // #if UNITY_EDITOR
	}

	/** 제거 되었을 경우 */
	public override void OnDestroy()
	{
		base.OnDestroy();
		this.StopLookAround();

		ComUtil.AssignVal(ref m_oTalkAni, null);
		ComUtil.AssignVal(ref m_oWarningAni, null);
	}

	/** 초기화 */
	public virtual void Init(NPCTable a_oTable, NPCStatTable a_oStatTable, CObjInfo a_oObjInfo)
	{
		base.Init();

		this.IsNeedUIs = true;
		this.IsWaveNPC = false;
		this.IsNeedAppear = false;

		this.Table = a_oTable;
		this.ObjInfo = a_oObjInfo;
		this.StatTable = a_oStatTable;

		m_nFireFXGroup = this.Table.HitEffectGroup;
		m_nNumFireProjectilesAtOnce = this.Table.ProjectileCount;

		m_fAttackRange = (float)this.Table.AttackRange;
		m_fMoveSpeedRun = this.Table.SpeedRun;
		m_fMoveSpeedWalk = this.Table.SpeedWalk;

		m_eNPCGrade = (ENPCGrade)this.Table.Grade;
		m_eDamageType = (EDamageType)this.Table.DamageType;
		m_eWeaponAniType = (EWeaponAnimationType)this.Table.AnimationType;

		m_eNextAttackType = this.AttackType;
		m_bIsEnableShowWarning = true;

		this.Renderer.material.SetColor(ComType.G_DAMAGE_TINT, Color.white);
		this.Animator.SetInteger(ComType.G_PARAMS_SKILL_TYPE, 0);

		this.Animator.SetFloat(ComType.G_PARAMS_SKILL_ANI_TYPE, 0.0f);
		this.Animator.SetFloat(ComType.G_PARAMS_WEAPON_ANI_TYPE, (int)this.WeaponAniType);

		// 기준 능력치를 설정한다 {
		this.CurStandardAbilityValDict.Clear();

		this.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.HP, this.StatTable.HP);
		this.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.ATK, this.StatTable.PowerAttack);
		this.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.DEF, this.StatTable.PowerDefence);

		this.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.ACCURACY, this.StatTable.HitRate);
		this.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.AIMING_DELAY, this.StatTable.AimTime);
		this.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.WARHEAD_SPEED, this.Table.WarheadSpeed);

		this.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.FORCE_MIN, (float)this.Table.ForceMin);
		this.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.FORCE_MAX, (float)this.Table.ForceMax);
		this.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.AttackRange, this.Table.AttackRange);
		// 기준 능력치를 설정한다 }

		this.SetupAbilityValues(true);
		this.SetHP(this.MaxHP);

		this.UpdateUIsState();

		this.SetIsAvoidAction(a_oTable.ActionType == (int)EActionType.ONLY_AVOID);
		this.SetupWayPointInfos();
		this.PreSetupNextAttackAction();

		// UI 를 설정한다 {
		m_oEtcUIs = this.Canvas.transform.Find("EtcUIs").gameObject;
		m_oTalkUIs = this.Canvas.transform.Find("EtcUIs/TalkUIs").gameObject;

		m_oWarningImg = this.Canvas.transform.Find("EtcUIs/WarningImg")?.GetComponent<Image>();
		m_oWarningImg.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f);

		// 대화 UI 가 존재 할 경우
		if (m_oTalkUIs != null && MenuManager.Singleton.CurScene == ESceneType.Battle)
		{
			m_oTalkUIs.transform.SetParent(this.PageBattle.MarkerUIsRoot.transform, false);
			m_oTalkUIs.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f);

			m_oTalkText = m_oTalkUIs.GetComponentInChildren<TMP_Text>();
			this.PageBattle.TalkUIsList.ExAddVal(m_oTalkUIs);
		}
		// UI 를 설정한다 }

		// 무기가 존재 할 경우
		if (this.Table.WeaponPrefab.ExIsValid())
		{
			m_oEquipWeaponObj = GameResourceManager.Singleton.CreateObject(EResourceType.Weapon,
				this.Table.WeaponPrefab, this.WeaponTrans);

			this.Animator.updateMode = ((ENPCGrade)a_oTable.Grade == ENPCGrade.BOSS && GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.ABYSS) ?
				AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;

			this.Rigidbody.useGravity = false;
			this.Rigidbody.isKinematic = false;
			this.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

			this.ResetEquipWeapon(m_oEquipWeaponObj);

			m_oSkillModelInfo = m_oEquipWeaponObj.GetComponentInChildren<SkillModelInfo>();
			m_oSoundModelInfo = m_oEquipWeaponObj.GetComponentInChildren<SoundModelInfo>();
			m_oWeaponModelInfo = m_oEquipWeaponObj.GetComponentInChildren<WeaponModelInfo>();
		}

		// 렉돌 제어자가 존재 할 경우
		if (this.RagdollController != null && this.SkinnedMeshRenderer != null && this.SkinnedMeshRendererOutline != null)
		{
			this.RagdollController.BoneRoot.SetActive(true);
			this.RagdollController.RagdollRoot.SetActive(false);

			for (int i = 0; i < this.BoneNameList.Count; ++i)
			{
				this.SkinnedBones[i] = ComUtil.FindChildByName(this.BoneNameList[i],
					this.RagdollController.BoneRoot.transform);
			}

			this.SkinnedMeshRenderer.rootBone = this.RagdollController.BoneRoot.transform.GetChild(0);
			this.SkinnedMeshRendererOutline.rootBone = this.RagdollController.BoneRoot.transform.GetChild(0);

			this.SkinnedMeshRenderer.bones = this.SkinnedBones;
			this.SkinnedMeshRendererOutline.bones = this.SkinnedBones;

			this.Animator.Rebind();
		}

		// 조준선이 존재 할 경우
		if (m_oWeaponModelInfo != null && m_oWeaponModelInfo.GetSightLine() != null)
		{
			var oSightLineHandler = GameResourceManager.Singleton.CreateObject<CSightLineHandler>(m_oWeaponModelInfo.GetSightLine(), m_oWeaponModelInfo.GetSightLineDummyTransform(), null);
			oSightLineHandler.gameObject.SetActive(false);
			oSightLineHandler.SetTargetLayerMask(LayerMask.GetMask(ComType.G_LAYER_PLAYER, ComType.G_LAYER_NON_PLAYER_01, ComType.G_LAYER_NON_PLAYER_02, ComType.G_LAYER_NON_PLAYER_03, ComType.G_LAYER_NON_PLAYER_04, ComType.G_LAYER_STRUCTURE));
			oSightLineHandler.SetOriginDirectionTarget(m_oWeaponModelInfo.GetSightLineDummyTransform().gameObject);

			m_oSightLineHandler = oSightLineHandler;
		}

		string oLayer = ComType.G_UNIT_LAYER_LIST.ExIsValidIdx(this.Table.Camp) ?
			ComType.G_UNIT_LAYER_LIST[this.Table.Camp] : ComType.G_LAYER_NON_PLAYER_01;

		this.gameObject.ExSetLayer(LayerMask.NameToLayer(oLayer), false);
	}

	/** 상태를 갱신한다 */
	public override void OnLateUpdate(float a_fDeltaTime)
	{
		base.OnLateUpdate(a_fDeltaTime);

#if DISABLE_THIS
		var oTarget = this.LockOnTarget ?? this.TrackingTarget;

		SightLineHandler?.SetTarget(oTarget?.ShootingPoint);
		SightLineHandler?.UpdateSightLine();
#endif // #if DISABLE_THIS
	}
	
	/** 등장 애니메이션을 설정한다 */
	public void SetupOpenAni()
	{
		this.ExLateCallFuncRealtime((a_oSender) =>
		{
			this.Animator.SetFloat(ComType.G_PARAMS_OPEN_ANI_TYPE, m_nOpenAniType);
			this.Animator.SetTrigger("Open");

			this.Animator.SetLayerWeight(1, (m_eNPCGrade == ENPCGrade.BOSS) ? 1.0f : 0.0f);
			this.BattleController.PlaySound(m_oOpenAudioClip, null, "Master/SFX/Voice");
		}, 1.0f);
	}

	/** 다음 공격 액션을 설정한다 */
	public void PreSetupNextAttackAction()
	{
#if DISABLE_THIS
		// 테스트 코드
		this.NextSelectionSkillGroupTable = null;

		if ((ENPCGrade)this.Table.Grade >= ENPCGrade.BOSS)
		{
			this.SetupNextSelectionSkillGroupTable();
			m_eNextAttackType = (EAttackType)SkillTable.GetData(this.NextSelectionSkillGroupTable.Skill).AttackType;
		}
		else
		{
			m_eNextAttackType = this.AttackType;
		}

		return;
#endif // #if DISABLE_THIS

		var oStatTable = this.StatTable;
		int nStateFactor = oStatTable.SelectionFactorAttack + oStatTable.SelectionFactorSkillGroup;
		float fRandomStateFactor = Random.Range(0.0f, (float)nStateFactor);

		this.NextSelectionSkillGroupTable = null;

		// 일반 공격 일 경우
		if (nStateFactor <= 0 || fRandomStateFactor.ExIsLess((float)oStatTable.SelectionFactorAttack))
		{
			m_eNextAttackType = this.AttackType;
		}
		else
		{
			this.SetupNextSelectionSkillGroupTable();

			// 스킬 적용이 불가능 할 경우
			if (this.NextSelectionSkillGroupTable == null)
			{
				m_eNextAttackType = this.AttackType;
			}
			else
			{
				m_eNextAttackType = (EAttackType)SkillTable.GetData(this.NextSelectionSkillGroupTable.Skill).AttackType;
			}
		}
	}

	/** 조준 대상을 갱신한다 */
	public override void UpdateLockOnTarget(bool a_bIsForce = false)
	{
		base.UpdateLockOnTarget(a_bIsForce);
		bool bIsNeedsUpdate = this.LockOnTarget == null || !this.LockOnTarget.IsSurvive;

		// 조준 대상 갱신이 필요 할 경우
		if (bIsNeedsUpdate)
		{
			var oLockOnTarget = this.IsAimableTarget(this.TrackingTarget) ? this.TrackingTarget : null;
			this.SetLockOnTarget(oLockOnTarget);
		}

		var stDelta = this.BattleController.PlayerController.transform.position - this.transform.position;
		stDelta.y = 0.0f;

		float fDetecingRange = this.Table.DetectionRange * ComType.G_UNIT_MM_TO_M;

		// 인지 범위 안에 존재 할 경우
		if (stDelta.magnitude.ExIsLess(fDetecingRange) && this.TrackingTarget == null)
		{
			this.TryHandleTalk(this.Table.HintGroup);
		}

		bool bIsNeedUpdateTrackingTarget = this.TrackingTarget == null || !this.TrackingTarget.IsSurvive;

		// 추적 대상 갱신이 필요 할 경우
		if (bIsNeedUpdateTrackingTarget)
		{
			var oTrackingTarget = this.IsTraceableTarget(this.NearestTarget) ? this.NearestTarget : this.FindTrackingTarget();
			this.SetTrackingTarget(oTrackingTarget);
		}

		// 경고 출력이 가능 할 경우
		if (m_bIsEnableShowWarning && this.TrackingTarget != null)
		{
			m_bIsEnableShowWarning = false;
			this.ShowWarning();
		}
	}

	/** 조준 대상을 탐색한다 */
	protected override UnitController FindLockOnTarget()
	{
		for (int i = 0; i < this.AttackableTargetList.Count; ++i)
		{
			// 추적 가능 할 경우
			if (this.IsTraceableTarget(this.AttackableTargetList[i], true))
			{
				return this.AttackableTargetList[i];
			}
		}

		return null;
	}

	/** 추적 대상을 탐색한다 */
	private UnitController FindTrackingTarget()
	{
		foreach (var stKeyVal in this.BattleController.UnitControllerDictContainer)
		{
			// 동일한 그룹 일 경우
			if (stKeyVal.Key == this.TargetGroup || stKeyVal.Key == (int)ETargetGroup.COMMON)
			{
				continue;
			}

			for (int i = 0; i < stKeyVal.Value.Count; ++i)
			{
				// 추적 가능 대상 일 경우
				if (this.IsTraceableTarget(stKeyVal.Value[i]))
				{
					return stKeyVal.Value[i];
				}
			}
		}

		return null;
	}

	/** 조준 대상을 공격한다 */
	public override void AttackLockOnTarget()
	{
		base.AttackLockOnTarget();
		this.Animator.SetTrigger(ComType.G_PARAMS_LOCK_ON);
	}

	/** 발사 이벤트를 수신했을 경우 */
	public override void Fire(Object a_oParams)
	{
		base.Fire(a_oParams);
		var stMagazineInfo = this.MagazineInfo;

		stMagazineInfo.m_nNumBullets = Mathf.Max(0, stMagazineInfo.m_nNumBullets - 1);
		this.SetMagazineInfo(stMagazineInfo);
	}

	/** 주변 경계를 시작한다 */
	public void StartLookAround(System.Action<NonPlayerController> a_oCallback)
	{
		this.StartLookAround(this.ObjInfo, a_oCallback);
	}

	/** 주변 경계를 시작한다 */
	public void StartLookAround(CObjInfo a_oObjInfo, System.Action<NonPlayerController> a_oCallback)
	{
		m_oLookAroundAni?.ExKill(this.GetInstanceID());

		// 사망 상태 일 경우
		if (!this.IsSurvive || this.TrackingTarget != null)
		{
			return;
		}

		var oSequence = DOTween.Sequence().SetId(this.GetInstanceID());
		this.SetupRotateAni(oSequence, a_oObjInfo.m_fRotateAngle01, Mathf.Max(a_oObjInfo.m_fPreDelay, a_oObjInfo.m_fPostDelay), a_oObjInfo.m_fDuration01, a_oObjInfo.m_bIsReturnRotate01);

		oSequence.AppendInterval(Mathf.Max(a_oObjInfo.m_fPreDelay, a_oObjInfo.m_fPostDelay));
		this.SetupRotateAni(oSequence, a_oObjInfo.m_fRotateAngle02, Mathf.Max(a_oObjInfo.m_fPreDelay, a_oObjInfo.m_fPostDelay), a_oObjInfo.m_fDuration02, a_oObjInfo.m_bIsReturnRotate02);

		oSequence.AppendCallback(() => a_oCallback(this));
		ComUtil.AssignVal(ref m_oLookAroundAni, oSequence, this.GetInstanceID());
	}

	/** 주변 경계를 종료한다 */
	public void StopLookAround()
	{
		this.StopLookAroundRotate();
		ComUtil.AssignVal(ref m_oLookAroundAni, null, this.GetInstanceID());
	}

	/** 전투 플레이 상태가 되었을 경우 */
	public override void OnBattlePlay()
	{
		// 완전 도망 상태 일 경우
		if (this.Table.ActionType == (int)EActionType.ONLY_AVOID)
		{
			this.SetTrackingTarget(this.BattleController.PlayerController);
		}

		base.OnBattlePlay();
		this.SetupAbilityValues(true);
	}

	/** UI 상태를 갱신한다 */
	protected override void UpdateUIsState(bool a_bIsIncr = false)
	{
		base.UpdateUIsState(a_bIsIncr);

#if UNITY_EDITOR
		this.Canvas.SetActive(this.HP >= 1);
#else
		this.Canvas.SetActive(this.HP >= 1 && this.IsNeedUIs);
#endif // #if UNITY_EDITOR

		// 텍스트를 갱신한다
		m_oLVText.text = $"{this.StatTable.Level}";

		// 등급 UI 를 갱신한다
		for (int i = 0; i < m_oGradeUIsList.Count; ++i)
		{
			m_oGradeUIsList[i].SetActive(i == (int)m_eNPCGrade - 1);
		}
	}

	/** 데미지를 계산한다 */
	protected override STHitInfo CalcDamage(UnitController a_oAttacker)
	{
		var stHitInfo = base.CalcDamage(a_oAttacker);
		return this.CalcDamage(a_oAttacker.CurAbilityValDict, stHitInfo);
	}

	/** 데미지를 계산한다 */
	protected override STHitInfo CalcDamage(ProjectileController a_oAttacker)
	{
		var stHitInfo = base.CalcDamage(a_oAttacker);
		return this.CalcDamage(a_oAttacker.Params.m_oAbilityValDict, stHitInfo);
	}

	/** 데미지를 계산한다 */
	private STHitInfo CalcDamage(Dictionary<EEquipEffectType, float> a_oAbilityValDict, STHitInfo a_stHitInfo)
	{
		float fWeakRatio = ComUtil.GetAbilityVal(EEquipEffectType.SmallAttackRatio, a_oAbilityValDict);
		float fWeakPercent = ComUtil.GetAbilityVal(EEquipEffectType.SmallAttackChance, a_oAbilityValDict);

		float fCriticalRatio = ComUtil.GetAbilityVal(EEquipEffectType.CriticalRatio, a_oAbilityValDict);
		float fCriticalPercent = ComUtil.GetAbilityVal(EEquipEffectType.CriticalChance, a_oAbilityValDict);

		float fBossATKRatio = (m_eNPCGrade == ENPCGrade.BOSS) ?
			ComUtil.GetAbilityVal(EEquipEffectType.AttackPowerToB, a_oAbilityValDict) : 0.0f;

		float fEliteATKRatio = (m_eNPCGrade == ENPCGrade.ELITE) ?
			ComUtil.GetAbilityVal(EEquipEffectType.AttackPowerToE, a_oAbilityValDict) : 0.0f;

		float fPercent = Random.Range(0.0f, 1.0f);
		float fATKRatio = 0.0f;
		float fExtraATKRatio = fBossATKRatio + fEliteATKRatio;

		// 일반 공격이 아닐 경우
		if (fPercent.ExIsGreat(1.0f - (fWeakPercent + fCriticalPercent)))
		{
			float fHitPercent = Random.Range(0.0f, fWeakPercent + fCriticalPercent);
			fATKRatio = fHitPercent.ExIsLess(fCriticalPercent) ? fCriticalRatio : fWeakRatio;

			a_stHitInfo.m_eHitType = fHitPercent.ExIsLess(fCriticalPercent) ? EHitType.CRITICAL : EHitType.WEAK;
		}

		int nDamage = Mathf.FloorToInt(a_stHitInfo.m_nDamage * (1.0f + fATKRatio + fExtraATKRatio));
		a_stHitInfo.m_nDamage = nDamage;

		return a_stHitInfo;
	}

	/** 피격을 처리한다 */
	protected override void HandleOnHit(UnitController a_oAttacker, Vector3 a_stDirection, STHitInfo a_stHitInfo, bool a_bIsShowDamage)
	{
#if UNITY_EDITOR
		a_stHitInfo.m_nDamage = m_bIsTestUntouchable ? 0 : a_stHitInfo.m_nDamage;
#endif // #if UNITY_EDITOR

		base.HandleOnHit(a_oAttacker, a_stDirection, a_stHitInfo, a_bIsShowDamage);

		// 주변 도움 요청이 가능 할 경우
		if (GameDataManager.Singleton.PlayMapInfoType != EMapInfoType.DEFENCE &&
			GameDataManager.Singleton.PlayMapInfoType != EMapInfoType.INFINITE)
		{
			this.HelpAround(a_oAttacker, this.Table.HelpRange * ComType.G_UNIT_MM_TO_M);
		}

		// 다른 그룹 일 경우
		if (a_oAttacker != null && a_oAttacker.TargetGroup != this.TargetGroup)
		{
			this.SetTrackingTarget(a_oAttacker);
		}

		int nPlayEpisodeID = GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID;

#if DISABLE_THIS
		// 초반 에피소드 일 경우
		if (nPlayEpisodeID <= 0)
		{
			int nIsCompleteNPCHelpAroundTutorial = PlayerPrefs.GetInt(ComType.HelpTutorialKindsKeyDict[EHelpTutorialKinds.NPC_HELP_AROUND]);

			// NPC 도움 요청 튜토리얼이 진행 가능 할 경우
			if (nIsCompleteNPCHelpAroundTutorial <= 0)
			{
				ComUtil.SetTimeScale(0.0f);
				this.PageBattle.ShowHelpTutorialUIs(EHelpTutorialKinds.NPC_HELP_AROUND);
			}
		}
#endif // #if DISABLE_THIS

		// 보스 NPC 일 경우
		if (this.IsBossNPC)
		{
			this.TryApplyHitSkill();

			this.GaugeUIsHandler.SetPercent(this.HP / (float)this.MaxHP);
			this.ExtraGaugeUIsHandler?.SetPercent(this.HP / (float)this.MaxHP);
		}

		// 보너스 NPC 일 경우
		if (this.IsBonusNPC && a_oAttacker is PlayerController)
		{
			this.TryDropHitGroundItems();

			this.GaugeUIsHandler.SetPercent(this.HP / (float)this.MaxHP);
			this.ExtraGaugeUIsHandler?.SetPercent(this.HP / (float)this.MaxHP);

			for (int i = 0; i < this.PageBattle.GaugeBonusUIsHandlerList.Count; ++i)
			{
				var oGaugeBonusUIsHandler = this.PageBattle.GaugeBonusUIsHandlerList[i];
				oGaugeBonusUIsHandler.gameObject.SetActive(this.ExtraGaugeUIsHandler == oGaugeBonusUIsHandler);
			}
		}

		// 도망 타입 일 경우
		if (this.Table.ActionType == (int)EActionType.ONLY_AVOID)
		{
			int nRandom = Random.Range(0, 2);

			// 도망 상태 변경이 가능 할 경우
			if (nRandom <= 0)
			{
				this.StateMachine.SetState(this.CreateAvoidState());
			}
		}

		// 체력이 존재 할 경우
		if (this.HP > 0)
		{
			return;
		}

		this.SetIsSurvive(false);
		this.StateMachine.SetState(null);

		this.StopLookAround();

		// 플레이어에 의해 사망했을 경우
		if (a_oAttacker != null && a_oAttacker is PlayerController)
		{
			this.BattleController.SetIsKillNonPlayers(true);
		}

		m_oTalkUIs.SetActive(false);
		m_oTalkUIs.transform.SetParent(m_oEtcUIs.transform, false);

		this.PageBattle.TalkUIsList.Remove(m_oTalkUIs);
		this.BattleController.RemoveUnitController(this);

		var oGroundItemObjList = CCollectionPoolManager.Singleton.SpawnList<GameObject>();

#if UNITY_EDITOR
		// 시야 범위가 존재 할 경우
		if (m_oEditorSightRange != null)
		{
			m_oEditorSightRange.SetActive(false);
		}
#endif // #if UNITY_EDITOR

		try
		{
			QuestActionControl(a_stHitInfo);
			this.BattleController.IncrGoldenPoint(this.Table.GoldenPoint);

			// 아이템 생성이 가능 할 경우
			if (a_oAttacker is PlayerController && GameDataManager.Singleton.PlayMapInfoType != EMapInfoType.DEFENCE)
			{
				var oRewardDict = RewardTable.RandomResultInGroup(this.StatTable.RewardGroup,
					GameDataManager.Singleton.BattleType == EBattleType.INFINITE);

				this.BattleController.CreateGroundItemObjs(oRewardDict, oGroundItemObjList);
				this.BattleController.AddGroundItemObjs(oGroundItemObjList, this.transform.position);
			}

			// 보스 NPC 일 경우
			if (m_eNPCGrade == ENPCGrade.BOSS)
			{
				var oNonPlayerControllerList = new List<NonPlayerController>();

				this.BattleController.NonPlayerControllerList.ExCopyTo(oNonPlayerControllerList,
					(a_oNonPlayerController) => a_oNonPlayerController);

				for (int i = 0; i < oNonPlayerControllerList.Count; ++i)
				{
					var stHitInfo = new STHitInfo()
					{
						m_nDamage = int.MaxValue,
						m_eHitType = EHitType.NORM
					};

					oNonPlayerControllerList[i].HandleOnHit(this.BattleController.PlayerController,
						Vector3.zero, stHitInfo, false);
				}

				this.BattleController.StartBossDestroyDirecting();
			}

			bool bIsContinueStage = GameDataManager.Singleton.IsWaveMode();
			bIsContinueStage = bIsContinueStage || GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.ABYSS;

			// NPC 가 존재 할 경우
			if (this.BattleController.TargetNonPlayerControllerList.Count >= 1 || bIsContinueStage)
			{
				return;
			}

			this.BattleController.OpenNextStagePassage();
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oGroundItemObjList);
		}
	}

	void QuestActionControl(STHitInfo a_stHitInfo)
	{
		if (this.DamageType == 0)
			GameManager.Singleton.SetQuestCounter(EQuestActionType.KillMelee, 1);
		else
			GameManager.Singleton.SetQuestCounter(EQuestActionType.KillRange, 1);

		switch (a_stHitInfo.m_eAtackWeaponType)
		{
			case EWeaponType.Pistol:
				GameManager.Singleton.SetQuestCounter(EQuestActionType.KillWithPistol, 1);
				break;
			case EWeaponType.AR:
				GameManager.Singleton.SetQuestCounter(EQuestActionType.KillWithAR, 1);
				break;
			case EWeaponType.SR:
				GameManager.Singleton.SetQuestCounter(EQuestActionType.KillWithSR, 1);
				break;
			case EWeaponType.Grenade:
				GameManager.Singleton.SetQuestCounter(EQuestActionType.KillWithGE, 1);
				break;
			case EWeaponType.SG:
				GameManager.Singleton.SetQuestCounter(EQuestActionType.KillWithSG, 1);
				break;
		}
	}

	/** 렉돌 애니메이션을 처리한다 */
	protected override void HandleRagdollAni(Vector3 a_stDirection, STHitInfo a_stHitInfo)
	{
		// 렉돌 제어자가 존재 할 경우
		if (this.RagdollController != null && this.SkinnedMeshRenderer != null && this.SkinnedMeshRendererOutline != null)
		{
			this.RagdollController.BoneRoot.SetActive(false);
			this.RagdollController.RagdollRoot.SetActive(true);

			this.SkinnedMeshRenderer.rootBone = this.RagdollController.RagdollRoot.transform.GetChild(0);
			this.SkinnedMeshRendererOutline.rootBone = this.RagdollController.RagdollRoot.transform.GetChild(0);

			this.SkinnedMeshRenderer.bones = this.OriginSkinnedBones;
			this.SkinnedMeshRendererOutline.bones = this.OriginSkinnedBones;

			this.Animator.Rebind();
		}

		base.HandleRagdollAni(a_stDirection, a_stHitInfo);
	}

	/** 이동 지점 정보를 설정한다 */
	private void SetupWayPointInfos()
	{
		// 이동 지점 정보가 없을 경우
		if (this.ObjInfo == null || this.ObjInfo.m_oWayPointInfoList.Count <= 0)
		{
			return;
		}

		int nAreaMask = 1 << NavMesh.GetAreaFromName(ComType.G_NAV_MESH_AREA_WALKABLE);

#if DISABLE_THIS
		var oWayPointInfo = ComUtil.MakeWayPointInfo(new STTransInfo(this.transform.localPosition, this.transform.localScale, this.transform.localEulerAngles), STPrefabInfo.INVALID);
		oWayPointInfo.m_fPreDelay = a_oObjInfo.m_fPreDelay;
		oWayPointInfo.m_fPostDelay = a_oObjInfo.m_fPostDelay;
		oWayPointInfo.m_eWayPointType = EWayPointType.PASS;

		this.WayPointInfoList.Add(oWayPointInfo);
#endif // #if DISABLE_THIS

		for (int i = 0; i < this.ObjInfo.m_oWayPointInfoList.Count; ++i)
		{
			var oWayPointInfo = this.ObjInfo.m_oWayPointInfoList[i].Clone() as CWayPointInfo;
			oWayPointInfo.m_stTransInfo.m_stPos += this.transform.parent.transform.position;

			bool bIsValid = NavMesh.SamplePosition((Vector3)oWayPointInfo.m_stTransInfo.m_stPos + (Vector3.up * 1.5f),
				out NavMeshHit stNavMeshHit, float.MaxValue / 2.0f, nAreaMask);

			oWayPointInfo.m_stTransInfo.m_stPos = bIsValid ? stNavMeshHit.position : oWayPointInfo.m_stTransInfo.m_stPos;
			this.WayPointInfoList.Add(oWayPointInfo);
		}
	}

	/** 회전 애니메이션을 설정한다 */
	private void SetupRotateAni(Sequence a_oSequence, float a_fAngle, float a_fDelay, float a_fDuration, bool a_bIsReturn)
	{
		// 각도가 유효 할 경우
		if (!Mathf.Approximately(a_fAngle, 0.0f))
		{
			a_oSequence.AppendCallback(this.StartLookAroundRotate);
			a_oSequence.Append(this.transform.DORotate(Vector3.up * a_fAngle, a_fDuration, RotateMode.WorldAxisAdd).SetEase(Ease.Linear));

			a_oSequence.AppendCallback(this.StopLookAroundRotate);

			// 복귀 모드 일 경우
			if (a_bIsReturn)
			{
				a_oSequence.AppendInterval(a_fDelay);
				a_oSequence.AppendCallback(this.StartLookAroundRotate);

				a_oSequence.Append(this.transform.DORotate(Vector3.up * -a_fAngle, a_fDuration, RotateMode.WorldAxisAdd).SetEase(Ease.Linear));
				a_oSequence.AppendCallback(this.StopLookAroundRotate);
			}
		}
	}

	/** 주변에 도움을 요청한다 */
	private void HelpAround(UnitController a_oAttacker, float a_fHelpRange)
	{
		for (int i = 0; i < this.BattleController.NonPlayerControllerList.Count; ++i)
		{
			// 도움 요청이 불가능 할 경우
			if (!this.transform.position.AlmostEquals(this.BattleController.NonPlayerControllerList[i].transform.position, a_fHelpRange))
			{
				continue;
			}

			// 다른 그룹 일 경우
			if (a_oAttacker != null && a_oAttacker.TargetGroup != this.TargetGroup && this.BattleController.NonPlayerControllerList[i].TrackingTarget == null)
			{
				this.BattleController.NonPlayerControllerList[i].ShowWarning();
				this.BattleController.NonPlayerControllerList[i].SetTrackingTarget(a_oAttacker);
			}

#if DISABLE_THIS
			var oTrackingTarget = this.FindTrackingTarget();

			// 알림 처리가 필요 할 경우
			if (oTrackingTarget != null && this.BattleController.NonPlayerControllerList[i].TrackingTarget == null)
			{
				this.BattleController.NonPlayerControllerList[i].ShowWarning();
				this.BattleController.NonPlayerControllerList[i].SetTrackingTarget(oTrackingTarget);
			}
#endif // #if DISABLE_THIS
		}
	}

	/** 대화를 출력한다 */
	public void ShowTalk(string a_oTalk, bool a_bIsRealtime = false)
	{
		// 대화 UI 가 없을 경우
		if (m_oTalkUIs == null || m_oTalkText == null)
		{
			return;
		}

		var oSequence = DOTween.Sequence().SetUpdate(a_bIsRealtime);
		oSequence.Append(m_oTalkUIs.transform.DOScaleY(1.0f, 0.15f).SetUpdate(a_bIsRealtime));
		oSequence.AppendInterval(3.0f);
		oSequence.Append(m_oTalkUIs.transform.DOScaleY(0.0f, 0.15f).SetUpdate(a_bIsRealtime));
		oSequence.AppendCallback(() => m_oTalkUIs.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f));

		m_oTalkText.text = a_oTalk;

		ComUtil.AssignVal(ref m_oTalkAni, oSequence);
		ComUtil.RebuildLayouts(m_oTalkUIs);
	}

	/** 경고를 출력한다 */
	private void ShowWarning(bool a_bIsRealtime = false)
	{
		var oSequence = DOTween.Sequence().SetUpdate(a_bIsRealtime);
		oSequence.Append(m_oWarningImg.transform.DOScaleY(1.0f, 0.25f).SetUpdate(a_bIsRealtime));
		oSequence.AppendInterval(2.0f);
		oSequence.Append(m_oWarningImg.transform.DOScaleY(0.0f, 0.25f).SetUpdate(a_bIsRealtime));
		oSequence.AppendCallback(() => m_oWarningImg.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f));

		ComUtil.AssignVal(ref m_oWarningAni, oSequence);
	}

	/** 주변 경계 회전을 시작한다 */
	private void StartLookAroundRotate()
	{
		this.Animator?.SetBool(ComType.G_PARAMS_IS_MOVE, false);
		this.Animator?.SetBool(ComType.G_PARAMS_IS_ROTATE, this.IsEnableMove);
	}

	/** 주변 경계 회전을 중지한다 */
	private void StopLookAroundRotate()
	{
		this.Animator?.SetBool(ComType.G_PARAMS_IS_ROTATE, false);
	}

	/** 아이템을 떨어뜨린다 */
	private void TryDropHitGroundItems()
	{
		// 체력이 없을 경우
		if (!this.IsSurvive || !this.IsBonusNPC)
		{
			return;
		}

		var oGroundItemObjList = CCollectionPoolManager.Singleton.SpawnList<GameObject>();
		var oRewardInfoDict = CCollectionPoolManager.Singleton.SpawnDict<string, (int, float)>();

		try
		{
			if (StatTable.MidiumRewardGroup10 > 0) oRewardInfoDict.Add(ComType.G_MIDIUM_REWARD_GROUP_10, (this.StatTable.MidiumRewardGroup10, 0.1f));
			if (StatTable.MidiumRewardGroup20 > 0) oRewardInfoDict.Add(ComType.G_MIDIUM_REWARD_GROUP_20, (this.StatTable.MidiumRewardGroup20, 0.2f));
			if (StatTable.MidiumRewardGroup30 > 0) oRewardInfoDict.Add(ComType.G_MIDIUM_REWARD_GROUP_30, (this.StatTable.MidiumRewardGroup30, 0.3f));
			if (StatTable.MidiumRewardGroup40 > 0) oRewardInfoDict.Add(ComType.G_MIDIUM_REWARD_GROUP_40, (this.StatTable.MidiumRewardGroup40, 0.4f));
			if (StatTable.MidiumRewardGroup50 > 0) oRewardInfoDict.Add(ComType.G_MIDIUM_REWARD_GROUP_50, (this.StatTable.MidiumRewardGroup50, 0.5f));
			if (StatTable.MidiumRewardGroup60 > 0) oRewardInfoDict.Add(ComType.G_MIDIUM_REWARD_GROUP_60, (this.StatTable.MidiumRewardGroup60, 0.6f));
			if (StatTable.MidiumRewardGroup70 > 0) oRewardInfoDict.Add(ComType.G_MIDIUM_REWARD_GROUP_70, (this.StatTable.MidiumRewardGroup70, 0.7f));
			if (StatTable.MidiumRewardGroup80 > 0) oRewardInfoDict.Add(ComType.G_MIDIUM_REWARD_GROUP_80, (this.StatTable.MidiumRewardGroup80, 0.8f));
			if (StatTable.MidiumRewardGroup90 > 0) oRewardInfoDict.Add(ComType.G_MIDIUM_REWARD_GROUP_90, (this.StatTable.MidiumRewardGroup90, 0.9f));

			float fPercent = this.HP / (float)this.MaxHP;

			foreach (var stKeyVal in oRewardInfoDict)
			{
				// 아이템 드롭이 가능 할 경우
				if (!m_oDropHitGroundItemsList.Contains(stKeyVal.Key) && fPercent.ExIsLessEquals(stKeyVal.Value.Item2))
				{
					var oRewardDict = RewardTable.RandomResultInGroup(stKeyVal.Value.Item1);
					this.BattleController.CreateGroundItemObjs(oRewardDict, oGroundItemObjList);

					m_oDropHitGroundItemsList.Add(stKeyVal.Key);
					this.BattleController.AddGroundItemObjs(oGroundItemObjList, this.transform.position);
				}
			}
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oGroundItemObjList);
			CCollectionPoolManager.Singleton.DespawnDict(oRewardInfoDict);
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 등장 위치를 변경한다 */
	public void SetStartPos(Vector3 a_stPos)
	{
		this.StartPos = a_stPos;
		this.IsNeedAppear = true;
	}

	/** 조준 가능 여부를 검사한다 */
	public bool IsAimableTarget(UnitController a_oController)
	{
		// 가능 여부 검사가 불가능 할 경우
		if (a_oController == null)
		{
			return false;
		}

		float fAttackRange = this.Table.AttackRange * ComType.G_UNIT_MM_TO_M;
		return this.IsAimableTarget(a_oController, fAttackRange, m_eNextAttackType == EAttackType.THROW);
	}

	/** 공격 가능 여부를 검사한다 */
	public override bool IsEnableAttack()
	{
		return base.IsEnableAttack() && this.Table.ActionType <= (int)EActionType.AVOID;
	}

	/** 추적 가능 여부를 검사한다 */
	public bool IsTraceableTarget(UnitController a_oController, bool a_bIsIgnoreHearning = false)
	{
		// 대상이 없을 경우
		if (a_oController == null)
		{
			return false;
		}

		var stPos = a_oController.transform.position;
		var stDelta = a_oController.GetAttackRayOriginPos() - this.GetAttackRayOriginPos();

		float fHearingRange = this.Table.HearingRange * ComType.G_UNIT_MM_TO_M;

		bool bIsValid = this.transform.position.AlmostEquals(stPos, fHearingRange);
		bIsValid = bIsValid && a_oController.Animator.GetBool(ComType.G_PARAMS_IS_MOVE);

		// 청각 무시 모드 일 경우
		if (a_bIsIgnoreHearning)
		{
			bIsValid = false;
		}

		// 추적이 불가능 할 경우
		if (!bIsValid && Physics.SphereCast(new Ray(this.GetAttackRayOriginPos(), stDelta.normalized), 0.05f, stDelta.magnitude, this.AttackLayerMask))
		{
			return false;
		}

		stDelta.y = 0.0f;
		float fSightRange = this.Table.SightRange * ComType.G_UNIT_MM_TO_M;

		return this.BattleController.IsPlaying &&
			(bIsValid || (this.transform.position.AlmostEquals(stPos, fSightRange) && Vector3.Angle(this.transform.forward, stDelta.normalized).ExIsLessEquals(this.Table.SightAngle / 2.0f)));
	}

	/** 이동 속도를 반환한다 */
	public float GetMoveSpeed()
	{
		float fMoveSpeed = (this.TrackingTarget != null) ? m_fMoveSpeedRun : m_fMoveSpeedWalk;
		return Mathf.Max(0.0f, fMoveSpeed * (1.0f + ComUtil.GetAbilityVal(EEquipEffectType.MoveSpeed, this.CurAbilityValDict)));
	}

	/** 웨이브 NPC 여부를 변경한다 */
	public void SetIsWaveNPC(bool a_bIsWaveNPC)
	{
		this.IsWaveNPC = a_bIsWaveNPC;
	}

	/** UI 필요 여부를 변경한다 */
	public void SetIsNeedUIs(bool a_bIsNeed)
	{
		this.IsNeedUIs = a_bIsNeed;
	}

	/** 등장 필요 여부를 변경한다 */
	public void SetIsNeedAppear(bool a_bIsNeed)
	{
		this.IsNeedAppear = a_bIsNeed;
	}

	/** 도망 액션 여부를 변경한다 */
	public void SetIsAvoidAction(bool a_bIsAvoid)
	{
		this.IsAvoidAction = a_bIsAvoid;
	}

	/** 추적 대상을 변경한다 */
	public override void SetTrackingTarget(UnitController a_oController)
	{
		// 대화 처리가 필요 할 경우
		if (this.TrackingTarget == null && a_oController != null)
		{
			this.TryHandleTalk(this.Table.HintGroup);
		}

		base.SetTrackingTarget(a_oController);
	}
	#endregion // 접근 함수
}
