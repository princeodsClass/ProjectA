using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using DG.Tweening;

/** 유닛 제어자 */
public abstract partial class UnitController : MonoBehaviour, IUpdatable
{
	#region 변수
	[Header("=====> Unit Controller - Etc <=====")]
	[SerializeField] private Renderer m_oRenderer = null;
	[SerializeField] private SkinnedMeshRenderer m_oSkinnedMeshRenderer = null;
	[SerializeField] private SkinnedMeshRenderer m_oSkinnedMeshRendererOutline = null;

	private bool m_bIsDirtyUpdateUIsState = true;

	private int m_nWalkableAreaMask = 0;
	private string m_oOriginTag = string.Empty;

#if UNITY_EDITOR
	[SerializeField] protected bool m_bIsTestUntouchable = false;
#endif // #if UNITY_EDITOR

	private Tween m_oHitAni = null;
	private FXModelInfo m_oFXModelInfo = null;

	[Header("=====> Unit Controller - UIs <=====")]
	[SerializeField] private CGaugeUIsHandler m_oGaugeUIsHandler = null;
	private CGaugeUIsHandler m_oExtraGaugeUIsHandler = null;

	[Header("=====> Unit Controller - Game Objects <=====")]
	[SerializeField] private GameObject m_oSpine = null;
	[SerializeField] private GameObject m_oCanvas = null;
	[SerializeField] private GameObject m_oRotateTarget = null;
	[SerializeField] private GameObject m_oForwardTarget = null;
	[SerializeField] private GameObject m_oShootingPoint = null;
	[SerializeField] private GameObject m_oFootstepDummy = null;
	[SerializeField] private GameObject m_oFXAnimEventRoot = null;
	#endregion // 변수

	#region 프로퍼티
	public bool IsFire { get; private set; } = false;
	public bool IsAiming { get; private set; } = false;
	public bool IsDestroy { get; private set; } = false;
	public bool IsSurvive { get; private set; } = false;
	public bool IsDoubleShot { get; private set; } = false;
	public bool IsDoubleShotFire { get; private set; } = false;
	public bool IsEnableStateMachine { get; private set; } = false;

	public int MoveLayerMask { get; private set; } = 0;
	public int AvoidLayerMask { get; private set; } = 0;
	public int AttackLayerMask { get; private set; } = 0;
	public int StructureLayerMask { get; private set; } = 0;

	public int FireTimes { get; private set; } = 0;
	public float UntouchableTime { get; private set; } = 0.0f;

	public Vector3 Velocity { get; private set; } = Vector3.zero;
	public Vector3 Acceleration { get; private set; } = Vector3.zero;
	public STMagazineInfo MagazineInfo { get; private set; }

	public Animator Animator { get; private set; } = null;
	public Rigidbody Rigidbody { get; private set; } = null;
	public Transform WeaponTrans { get; private set; } = null;
	public NavMeshAgent NavMeshAgent { get; private set; } = null;
	public RagdollController RagdollController { get; private set; } = null;

	public CStateMachine<UnitController> StateMachine { get; } = new CStateMachine<UnitController>();
	public CStateMachine<UnitController> ContactStateMachine { get; } = new CStateMachine<UnitController>();

	public UnitController LockOnTarget { get; private set; } = null;
	public UnitController NearestTarget { get; private set; } = null;
	public UnitController TrackingTarget { get; private set; } = null;

	public List<STEffectStackInfo> EffectStackInfoList { get; } = new List<STEffectStackInfo>();
	public List<STEffectStackInfo> RemoveEffectStackInfoList { get; } = new List<STEffectStackInfo>();
	public List<STEffectStackInfo> ActiveEffectStackInfoList { get; } = new List<STEffectStackInfo>();
	public List<STEffectStackInfo> PassiveEffectStackInfoList { get; } = new List<STEffectStackInfo>();

	public List<UnitController> AttackableTargetList = new List<UnitController>();
	public Dictionary<EEquipEffectType, GameObject> EffectFXDict { get; } = new Dictionary<EEquipEffectType, GameObject>();

	public bool IsAir => this.Animator.GetBool(ComType.G_PARAMS_IS_AIR);
	public bool IsUntouchable => this.UntouchableTime.ExIsGreat(0.0f);

	public GameObject Canvas => m_oCanvas;
	public GameObject RotateTarget => m_oRotateTarget;
	public GameObject ForwardTarget => m_oForwardTarget;

	public Renderer Renderer => m_oRenderer;
	public PageBattle PageBattle => MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle);
	public BattleController BattleController => this.PageBattle?.BattleController;

	public virtual bool IsEnableFire => false;
	public virtual bool IsEnableKnockBack => false;
	public virtual bool IsIgnoreTargetSurvive => false;

	public virtual int TargetGroup => 0;
	public virtual int FireFXGroup => 0;
	public virtual int NumFireProjectilesAtOnce => 0;

	public virtual float Gravity => Mathf.Abs(Physics.gravity.y);
	public virtual float MoveSpeed => 0.0f;
	public virtual float DoubleShotDelay => GlobalTable.GetData<float>(ComType.G_TIME_DOUBLI_SHOT);
	public virtual float AttackRangeStandard => 0.0f;

	public virtual EAttackType AttackType => ComType.G_THROW_WEAPON_ANI_TYPE_LIST.Contains(this.WeaponAniType) ? EAttackType.THROW : EAttackType.SHOOT;
	public virtual EDamageType DamageType => EDamageType.NONE;
	public virtual EWeaponAnimationType WeaponAniType => EWeaponAnimationType.END;

	public virtual ItemWeapon CurWeapon => null;
	public virtual FXModelInfo FXModelInfo => m_oFXModelInfo;
	public virtual WeaponTable CurWeaponTable => null;

	public virtual SkillModelInfo CurSkillModelInfo => null;
	public virtual SoundModelInfo CurSoundModelInfo => null;
	public virtual WeaponModelInfo CurWeaponModelInfo => null;
	public virtual CSightLineHandler CurSightLineHandler => null;

	public CGaugeUIsHandler GaugeUIsHandler => m_oGaugeUIsHandler;
	public CGaugeUIsHandler ExtraGaugeUIsHandler => m_oExtraGaugeUIsHandler;

	public SkinnedMeshRenderer SkinnedMeshRenderer => m_oSkinnedMeshRenderer;
	public SkinnedMeshRenderer SkinnedMeshRendererOutline => m_oSkinnedMeshRendererOutline;

	public virtual GameObject Spine => m_oSpine;
	public virtual GameObject CurWeaponObj => null;
	public virtual GameObject ShootingPoint => m_oShootingPoint;
	#endregion // 프로퍼티

	#region IUpdatable
	/** 상태를 갱신한다 */
	public virtual void OnUpdate(float a_fDeltaTime)
	{
		// 상태 머신이 유효 할 경우
		if (this.IsEnableStateMachine)
		{
			this.StateMachine.OnUpdate(a_fDeltaTime);
			this.ContactStateMachine.OnUpdate(a_fDeltaTime);
		}

		this.UntouchableTime = Mathf.Max(0.0f, this.UntouchableTime - a_fDeltaTime);
	}

	/** 상태를 갱신한다 */
	public virtual void OnLateUpdate(float a_fDeltaTime)
	{
		// UI 갱신이 필요 할 경우
		if (m_bIsDirtyUpdateUIsState)
		{
			m_bIsDirtyUpdateUIsState = false;
			this.UpdateUIsState();
		}

		// 상태 머신이 유효 할 경우
		if (this.IsEnableStateMachine)
		{
			this.StateMachine.OnLateUpdate(a_fDeltaTime);
			this.ContactStateMachine.OnLateUpdate(a_fDeltaTime);
		}

		this.UpdateEffectStackInfoState();
		var oTarget = this.LockOnTarget ?? this.TrackingTarget;

		this.CurSightLineHandler?.SetTarget(oTarget?.ShootingPoint);
		this.CurSightLineHandler?.UpdateSightLine();
	}

	/** 상태를 갱신한다 */
	public virtual void OnFixedUpdate(float a_fDeltaTime)
	{
		// 상태 머신이 유효하지 않을 경우
		if (!this.IsEnableStateMachine)
		{
			return;
		}

		this.StateMachine.OnFixedUpdate(a_fDeltaTime);
		this.ContactStateMachine.OnFixedUpdate(a_fDeltaTime);
	}

	/** 상태를 갱신한다 */
	public virtual void OnCustomUpdate(float a_fDeltaTime)
	{
		// 상태 머신이 유효하지 않을 경우
		if (!this.IsEnableStateMachine)
		{
			return;
		}

		this.StateMachine.OnCustomUpdate(a_fDeltaTime);
		this.ContactStateMachine.OnCustomUpdate(a_fDeltaTime);
	}
	#endregion // IUpdatable

	#region 함수
	/** 초기화 */
	public virtual void Awake()
	{
		this.Animator = this.GetComponent<Animator>();
		this.Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

		this.Rigidbody = this.GetComponent<Rigidbody>();
		this.NavMeshAgent = this.GetComponent<NavMeshAgent>();
		this.RagdollController = this.GetComponentInChildren<RagdollController>();

		// 레이어 마스크를 설정한다
		this.MoveLayerMask = LayerMask.GetMask(ComType.G_LAYER_STRUCTURE);
		this.AvoidLayerMask = LayerMask.GetMask(ComType.G_LAYER_STRUCTURE);
		this.AttackLayerMask = LayerMask.GetMask(ComType.G_LAYER_STRUCTURE);
		this.StructureLayerMask = LayerMask.GetMask(ComType.G_LAYER_STRUCTURE);

		// 회전 대상이 없을 경우
		if (m_oRotateTarget == null)
		{
			m_oRotateTarget = this.gameObject;
		}

		// 전방 대상이 없을 경우
		if (m_oForwardTarget == null)
		{
			m_oForwardTarget = this.gameObject;
		}

		// 내비게이션 메시 에이전트가 존재 할 경우
		if (this.NavMeshAgent != null)
		{
			this.NavMeshAgent.enabled = false;
		}

		// 맵 에디터 씬이 아닐 경우
		if (MenuManager.Singleton.CurScene != ESceneType.MapEditor)
		{
			var oBoneRootTrans = (this.RagdollController?.BoneRoot != null) ?
				this.RagdollController.BoneRoot.transform : this.transform;

			m_oFXModelInfo = this.GetComponent<FXModelInfo>();
			this.WeaponTrans = ComUtil.FindChildByName(ComType.NAME_WEAPON_DUMMY, oBoneRootTrans);

			this.SetIsSurvive(true);
			this.SetIsEnableStateMachine(MenuManager.Singleton.CurScene == ESceneType.Battle);
		}
	}

	/** 초기화 */
	public virtual void Start()
	{
		m_nWalkableAreaMask = 1 << NavMesh.GetAreaFromName(ComType.G_NAV_MESH_AREA_WALKABLE);

		// 맵 에디터 씬이 아닐 경우
		if (this.IsEnableStateMachine && MenuManager.Singleton.CurScene != ESceneType.MapEditor)
		{
			m_oOriginTag = this.tag;
			this.StateMachine.SetOwner(this);

			this.ContactStateMachine.SetOwner(this);
			this.ContactStateMachine.SetState(this.CreateContactGroundState());
		}
	}

	/** 제거 되었을 경우 */
	public virtual void OnDestroy()
	{
		this.IsDestroy = true;
		ComUtil.AssignVal(ref m_oHitAni, null);
	}

	/** 조준선을 리셋한다 */
	public virtual void ResetSightLineHandler()
	{
		this.CurSightLineHandler?.SetColor(Color.white);
		this.CurSightLineHandler?.gameObject.SetActive(false);
	}

	/** 초기화 */
	public virtual void Init()
	{
		this.EffectStackInfoList.Clear();
		this.RemoveEffectStackInfoList.Clear();
		this.ActiveEffectStackInfoList.Clear();
		this.PassiveEffectStackInfoList.Clear();
	}

	/** 공격 가능 대상을 갱신한다 */
	public virtual void UpdateAttackableTargets()
	{
		float fMinDistance = float.MaxValue / 2.0f;
		float fAttackRange = this.AttackRange * ComType.G_UNIT_MM_TO_M;

		UnitController oNearestTarget = null;
		this.AttackableTargetList.Clear();

		foreach (var stKeyVal in this.BattleController.UnitControllerDictContainer)
		{
			// 동일한 그룹 일 경우
			if (stKeyVal.Key == this.TargetGroup || stKeyVal.Key == (int)ETargetGroup.COMMON)
			{
				continue;
			}

			for (int i = 0; i < stKeyVal.Value.Count; ++i)
			{
				float fDistance = Vector3.Distance(this.transform.position, stKeyVal.Value[i].transform.position);

				// 가까운 대상 일 경우
				if (fDistance.ExIsLess(fMinDistance))
				{
					fMinDistance = fDistance;
					oNearestTarget = stKeyVal.Value[i];
				}

				// 공격이 가능 할 경우
				if (this.IsAttackableTarget(stKeyVal.Value[i], fAttackRange))
				{
					var oNonPlayerController = stKeyVal.Value[i];
					this.AttackableTargetList.Add(oNonPlayerController);
				}
			}
		}

		this.SetNearestTarget(oNearestTarget);
		this.AttackableTargetList.Sort(this.CompareDistance);
	}

	/** 부활한다 */
	public virtual void Revive(float a_fUntouchableTime)
	{
		this.SetHP(this.MaxHP);
		this.UpdateUIsState(true);

		this.RagdollController?.StopRagdollAni();
		this.StartUntouchableFX(a_fUntouchableTime);

		// 부활 효과가 존재 할 경우
		if (m_oFXModelInfo != null && m_oFXModelInfo.ReviveFX != null)
		{
			GameResourceManager.Singleton.CreateObject(m_oFXModelInfo.ReviveFX,
				m_oFXModelInfo.ReviveFXDummy, null, ComType.G_LIFE_T_COMMON_FX);
		}
	}

	/** 무적 효과를 시작한다 */
	public virtual void StartUntouchableFX(float a_fUntouchableTime)
	{
		this.UntouchableTime = a_fUntouchableTime;

		// 무적 효과가 없을 경우
		if (this.BattleController.FXModelInfo.UntouchableFXInfo.m_oUntouchableFX == null)
		{
			return;
		}

		var oFXHandler = GameResourceManager.Singleton.CreateObject<CTimeFXHandler>(this.BattleController.FXModelInfo.UntouchableFXInfo.m_oUntouchableFX,
			this.transform, null, a_fUntouchableTime + ComType.G_LIFE_T_CONTINUOUS_FX);

		oFXHandler.StartFX(a_fUntouchableTime);
	}

	/** 효과 이벤트를 수신했을 경우 */
	public virtual void FX(Object a_oParams)
	{
		var oPrefab = a_oParams as GameObject;

		// 프리팹이 아닐 경우
		if (oPrefab == null)
		{
			return;
		}

		this.PlayFX(oPrefab, (m_oFXAnimEventRoot != null) ? m_oFXAnimEventRoot : this.gameObject);
	}

	/** 점프 효과 이벤트를 수신했을 경우 */
	public virtual void JumpFX(Object a_oParams)
	{
		var oPrefab = a_oParams as GameObject;

		// 프리팹이 아닐 경우
		if (oPrefab == null)
		{
			return;
		}

		this.PlayFX(oPrefab, null, a_oDummy: this.gameObject);
	}

	/** 화염 효과 이벤트를 수신했을 경우 */
	public virtual void MuzzleFX(Object a_oParams)
	{
		// 무기 모델 정보가 없을 경우
		if (this.CurWeaponModelInfo == null)
		{
			return;
		}

		var oMuzzleFlashObj = this.CurWeaponModelInfo.GetMuzzleFlash();
		var oMuzzleFlashDummy = this.CurWeaponModelInfo.GetMuzzleFlashDummyTransform();

		this.FireMuzzleFlash(oMuzzleFlashObj, oMuzzleFlashDummy);
		this.BattleController.PlaySound(this.CurSoundModelInfo?.FireSoundList, (this.TrackingTarget != null) ? this.gameObject : null);
	}

	/** 재장전 이벤트를 수신했을 경우 */
	public virtual void Reload(Object a_oParams)
	{
		// 전투 씬이 아닐 경우
		if (MenuManager.Singleton.CurScene != ESceneType.Battle || this.CurSoundModelInfo == null)
		{
			return;
		}

		this.BattleController.PlaySound(this.CurSoundModelInfo.ReloadSoundList, this.gameObject);
	}

	/** 착지 이벤트를 수신했을 경우 */
	public virtual void Land(Object a_oParams)
	{
		// 프리팹 일 경우
		if (a_oParams is GameObject)
		{
			this.PlayStepFX(a_oParams as GameObject);
		}

		this.BattleController.PlayLandSound(this.gameObject);
	}

	/** 발자국 이벤트를 수신했을 경우 */
	public virtual void Footstep(Object a_oParams)
	{
		var oPrefab = a_oParams as GameObject;

		// 프리팹이 아닐 경우
		if (oPrefab == null)
		{
			return;
		}

		this.PlayStepFX(a_oParams as GameObject);
	}

	/** 이동 상태를 갱신한다 */
	public virtual void UpdateMoveState()
	{
		// Do Something
	}

	/** 물리 상태를 갱신한다 */
	public virtual void UpdatePhysicsState()
	{
		// Do Something
	}

	/** 조준 대상을 갱신한다 */
	public virtual void UpdateLockOnTarget(bool a_bIsForce = false)
	{
		this.UpdateAttackableTargets();
		float fAttackRange = this.AttackRange * ComType.G_UNIT_MM_TO_M;

		bool bIsNeedsUpdate = a_bIsForce || this.LockOnTarget == null || !this.LockOnTarget.IsSurvive;
		bool bIsIgnoreStructure = this.AttackType == EAttackType.THROW;

		// 조준 대상 갱신이 필요 할 경우
		if (bIsNeedsUpdate || !this.IsAimableTarget(this.LockOnTarget, fAttackRange, bIsIgnoreStructure))
		{
			this.SetLockOnTarget(null);
			var oLockOnTarget = this.FindLockOnTarget();

			// 조준 대상이 존재 할 경우
			if(oLockOnTarget != null && this.TrackingTarget == null)
			{
				this.SetLockOnTarget(oLockOnTarget);
			}
			else
			{
				for (int i = 0; i < this.AttackableTargetList.Count; ++i)
				{
					// 조준이 가능 할 경우
					if (this.IsAimableTarget(this.AttackableTargetList[i], fAttackRange, bIsIgnoreStructure))
					{
						this.SetLockOnTarget(this.AttackableTargetList[i]);
						break;
					}
				}
			}
		}
	}

	/** 효과 스택 정보를 설정한다 */
	public virtual void SetupEffectStackInfos()
	{
		this.EffectStackInfoList.Clear();

		this.SetupEffectStackInfos(this.ActiveEffectStackInfoList, this.EffectStackInfoList);
		this.SetupEffectStackInfos(this.PassiveEffectStackInfoList, this.EffectStackInfoList);
	}

	/** 효과 스택 정보를 설정한다 */
	public virtual void SetupEffectStackInfos(List<STEffectStackInfo> a_oEffectStackInfoList,
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

	/** 전투 플레이 상태가 되었을 경우 */
	public virtual void OnBattlePlay()
	{
		this.SetupAbilityValues(true);
		this.SetHP(this.MaxHP);

		this.UpdateUIsState();
		this.StateMachine.SetState(this.CreateInitState());
	}

	/** 효과를 재생한다 */
	public GameObject PlayFX(GameObject a_oPrefab, GameObject a_oParent, GameObject a_oDummy = null, float a_fLifeTime = ComType.G_LIFE_T_FX)
	{
		// 프리팹이 없을 경우
		if (a_oPrefab == null)
		{
			return null;
		}

		var oFX = GameResourceManager.Singleton.CreateObject(a_oPrefab,
			a_oParent?.transform, a_oDummy?.transform, a_fLifeTime);

		var oParticle = oFX.GetComponentInChildren<ParticleSystem>();
		oParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		oParticle?.Play(true);

		return oFX;
	}

	/** 스탭 효과를 재생한다 */
	public GameObject PlayStepFX(GameObject a_oPrefab)
	{
		return this.PlayFX(a_oPrefab,
			this.BattleController.PathObjRoot.gameObject, m_oFootstepDummy, ComType.G_LIFE_T_STEP_FX);
	}

	/** 상태 이상 효과를 재생한다 */
	public virtual void PlayStatusFX(EEquipEffectType a_eType)
	{
		switch (a_eType)
		{
			case EEquipEffectType.FREEZE:
				var oFXObj = this.PlayFX(this.BattleController.FXModelInfo.IceFXInfo.m_oIceBreakFX,
					this.BattleController.PathObjRoot.gameObject);

				oFXObj.transform.position = this.transform.position;
				this.EffectFXDict.GetValueOrDefault(EEquipEffectType.FREEZE)?.gameObject.SetActive(false);

				break;
		}
	}

	/** 모든 행동을 중지한다 */
	public virtual void StopAllActions()
	{
		this.LockOnTarget = null;
		this.NearestTarget = null;
	}

	/** 조준 대상을 공격한다 */
	public virtual void AttackLockOnTarget()
	{
		this.SetIsFire(true);
		this.CurSightLineHandler?.gameObject.SetActive(false);

		this.Animator.SetTrigger(ComType.G_PARAMS_SHOT);
	}

	/** 지정 위치를 바라본다 */
	public virtual void LookAt(Vector3 a_stPos, bool a_bIsLerp = false, float a_fLerpTime = 0.0f)
	{
		var stDirection = a_stPos - this.transform.position;
		stDirection.y = 0.0f;

		// 같은 위치 일 경우
		if (stDirection.ExIsEquals(Vector3.zero))
		{
			return;
		}

		this.RotateTarget.transform.forward = a_bIsLerp ?
			Vector3.Slerp(this.RotateTarget.transform.forward, stDirection.normalized, a_fLerpTime) : stDirection.normalized;
	}

	/** 대상을 바라본다 */
	public virtual void LookTarget(UnitController a_oTarget, bool a_bIsLerp = false, float a_fLerpTime = 0.0f)
	{
		// 대상이 없을 경우
		if (a_oTarget == null)
		{
			return;
		}

		this.LookAt(a_oTarget.transform.position, a_bIsLerp, a_fLerpTime);
	}

	/** 스파인를 회전한다 */
	public virtual void RotateSpine(UnitController a_oTarget, float a_fWeight)
	{
		// 대상이 없을 경우
		if (a_oTarget == null || this.Spine == null)
		{
			return;
		}

		var stDelta = this.transform.position - a_oTarget.transform.position;
		var stRotate = Quaternion.AngleAxis(Mathf.Asin(stDelta.normalized.y) * Mathf.Rad2Deg * a_fWeight, this.transform.right);

		this.Spine.transform.rotation = stRotate * this.Spine.transform.rotation;
	}

	/** 무기를 잠근다 */
	public virtual void LockWeapon(int a_nSlotIdx)
	{
		// Do Something
	}

	/** 무기를 재장전한다 */
	public virtual void ReloadWeapon(int a_nSlotIdx, bool a_bIsAutoEquipNextWeapon = true)
	{
		// Do Something
	}

	/** 피격 연출을 시작한다 */
	public virtual void StartHitDirecting(STHitInfo a_stHitInfo, bool a_bIsShowDamage = true)
	{
		// 데미지 출력 모드 일 경우
		if (a_bIsShowDamage)
		{
			this.BattleController.ShowDamageText(a_stHitInfo.m_nDamage,
				a_stHitInfo.m_eHitType, this.transform.position);
		}

		m_oHitAni?.ExKill();
		m_oRenderer.material.SetFloat(ComType.G_DAMAGE_AMOUNT, 0.0f);

		var oSequence = DOTween.Sequence();
		oSequence.Append(DOTween.To(() => m_oRenderer.material.GetFloat(ComType.G_DAMAGE_AMOUNT), (a_fVal) => m_oRenderer.material.SetFloat(ComType.G_DAMAGE_AMOUNT, a_fVal), 1.0f, 0.05f));
		oSequence.Append(DOTween.To(() => m_oRenderer.material.GetFloat(ComType.G_DAMAGE_AMOUNT), (a_fVal) => m_oRenderer.material.SetFloat(ComType.G_DAMAGE_AMOUNT, a_fVal), 0.0f, 0.05f));

		ComUtil.AssignVal(ref m_oHitAni, oSequence);
	}

	/** 투사체 충돌 처리 시작을 처리한다 */
	public virtual void HandleOnTriggerEnterProjectile(ProjectileController a_oSender, Collider a_oCollider)
	{
#if DISABLE_THIS
		// 유닛이 아닐 경우
		if (a_oCollider.CompareTag(m_oOriginTag) || !this.IsUnit(a_oCollider.gameObject))
		{
			return;
		}
#else
		// 유닛이 아닐 경우
		if (!this.IsUnit(a_oCollider.gameObject) || a_oCollider.gameObject.layer == this.gameObject.layer)
		{
			return;
		}
#endif // #if DISABLE_THIS

		bool bIsReleaseObj = true;
		float fPenetratePercent = Random.Range(0.0f, 1.0f);

		// 발사 공격 타입 일 경우
		if (a_oSender.Params.m_eAttackType == EAttackType.SHOOT)
		{
			a_oSender.PlayHitFX(a_oCollider);
		}

		float fPenetrateChance = ComUtil.GetAbilityVal(EEquipEffectType.PenetrateChance, a_oSender.Params.m_oAbilityValDict);

		// 관통 되었을 경우
		if (fPenetratePercent.ExIsLess(fPenetrateChance))
		{
			bIsReleaseObj = false;
		}

		// 유닛 제어자가 존재 할 경우
		if (a_oCollider.gameObject.TryGetComponent(out UnitController oController))
		{
			oController.OnHit(this, a_oSender);
			float fRicochetPercent = Random.Range(0.0f, 1.0f);

			// 넉백이 가능 할 경우
			if (!a_oCollider.gameObject.CompareTag(ComType.G_TAG_PLAYER) && oController.IsEnableKnockBack)
			{
				var stDirection = this.transform.forward;
				float fKnockBackRange = ComUtil.GetAbilityVal(EEquipEffectType.KnockBackRange, a_oSender.Params.m_oAbilityValDict) * ComType.G_UNIT_MM_TO_M;

				// 투척 타입 일 경우
				if (a_oSender.Params.m_eAttackType == EAttackType.THROW)
				{
					stDirection = this.transform.position - a_oSender.transform.position;
					stDirection.y = 0.0f;
				}

				bool bIsHit = Physics.Raycast(this.GetMoveRayOriginPos(), stDirection.normalized, out RaycastHit stRaycastHit);

				// 넉백 방향에 장애물이 존재 할 경우
				if (bIsHit && this.IsStructure(stRaycastHit.collider.gameObject) && stRaycastHit.distance.ExIsLess(1.0f))
				{
					fKnockBackRange = 0.0f;
				}

				oController.Rigidbody.AddForce(stDirection.normalized * fKnockBackRange, ForceMode.VelocityChange);
			}

			float fRicochetChance = ComUtil.GetAbilityVal(EEquipEffectType.RicochetChance, a_oSender.Params.m_oAbilityValDict);
			fRicochetChance += ComUtil.GetAbilityVal(EEquipEffectType.FORCE_RICOCHET_CHANCE, a_oSender.Params.m_oAbilityValDict);

			// 도탄 되었을 경우
			if (fRicochetPercent.ExIsLess(fRicochetChance))
			{
				bIsReleaseObj = !this.TryHandleRicochet(a_oSender, a_oCollider);
			}
		}

		// 제거가 필요 할 경우
		if (bIsReleaseObj && a_oSender.Params.m_eAttackType == EAttackType.SHOOT)
		{
			GameResourceManager.Singleton.ReleaseObject(a_oSender.gameObject);
		}
	}

	/** 추가 타격 효과를 처리한다 */
	public virtual void HandleExtraHitEffect(ProjectileController a_oController, Collider a_oCollider, Vector3 a_stPos)
	{
		// Do Something
	}

	/** 데미지 필드 적용을 처리한다 */
	public virtual void HandleOnApplyDamageField(DamageFieldController a_oSender, Collider a_oCollider)
	{
#if DISABLE_THIS
		// 유닛이 아닐 경우
		if (a_oCollider.CompareTag(m_oOriginTag) || !this.IsUnit(a_oCollider.gameObject))
		{
			return;
		}
#else
		// 유닛이 아닐 경우
		if (!this.IsUnit(a_oCollider.gameObject) || a_oCollider.gameObject.layer == this.gameObject.layer)
		{
			return;
		}
#endif // #if DISABLE_THIS

		// 유닛 제어자가 없을 경우
		if (!a_oCollider.gameObject.TryGetComponent(out UnitController oController))
		{
			return;
		}

		oController.OnHit(this, a_oSender);
	}

	/** UI 상태를 갱신한다 */
	protected virtual void UpdateUIsState(bool a_bIsIncr = false)
	{
		// 최대 체력이 없을 경우
		if (this.MaxHP <= 0)
		{
			return;
		}

		float fPercent = this.HP / (float)this.MaxHP;
		m_oCanvas.SetActive(this.HP >= 1);

		// 증가 모드 일 경우
		if (a_bIsIncr)
		{
			m_oGaugeUIsHandler.IncrPercent(fPercent);
			m_oExtraGaugeUIsHandler?.IncrPercent(fPercent);
		}
		else
		{
			m_oGaugeUIsHandler.DecrPercent(fPercent);
			m_oExtraGaugeUIsHandler?.DecrPercent(fPercent);
		}
	}

	/** 도탄을 처리한다 */
	protected virtual bool TryHandleRicochet(ProjectileController a_oSender, Collider a_oCollider)
	{
		return false;
	}

	/** 거리를 비교한다 */
	protected int CompareDistance(UnitController a_oLhs, UnitController a_oRhs)
	{
		float fDistanceA = Vector3.Distance(this.transform.position, a_oLhs.transform.position);
		float fDistanceB = Vector3.Distance(this.transform.position, a_oRhs.transform.position);

		// 거리가 동일 할 경우
		if (fDistanceA.AlmostEquals(fDistanceB, 0.0f))
		{
			return ComType.G_COMPARE_EQUALS;
		}

		return fDistanceA.ExIsLess(fDistanceB) ? ComType.G_COMPARE_LESS : ComType.G_COMPARE_GREAT;
	}

	/** 피해량을 계산한다 */
	protected virtual int CalcDamage(float a_fATK, float a_fDefense)
	{
		return Mathf.Max(1, Mathf.FloorToInt(a_fATK - a_fDefense));
	}

	/** 장착 무기를 리셋한다 */
	public virtual void ResetEquipWeapon(GameObject a_oWeapon)
	{
		// 리지드 바디가 없을 경우
		if (!a_oWeapon.TryGetComponent(out Rigidbody oRigidbody))
		{
			return;
		}

		oRigidbody.useGravity = false;
		oRigidbody.isKinematic = true;
		oRigidbody.constraints = RigidbodyConstraints.FreezeAll;
	}

	/** 모든 무기를 떨어뜨린다 */
	protected virtual void DropAllWeapons()
	{
		// 무기 트랜스 폼이 없을 경우
		if (this.WeaponTrans == null || this is PlayerController)
		{
			return;
		}

		for (int i = 0; i < this.WeaponTrans.childCount; ++i)
		{
			var oWeapon = this.WeaponTrans.GetChild(i);
			oWeapon.transform.SetParent(this.BattleController.PathObjRoot, true);
			oWeapon.transform.localScale = Vector3.one;

			this.BattleController.DropWeaponList.Add(oWeapon.gameObject);

			// 물리 컴포넌트가 없을 경우
			if (!oWeapon.TryGetComponent(out Rigidbody oRigidbody))
			{
				continue;
			}

			var stVelocity = new Vector3(Random.Range(-1.0f, 1.0f), 1.0f, Random.Range(-1.0f, 1.0f)) * Random.Range(1.5f, 2.5f);
			oRigidbody.useGravity = true;
			oRigidbody.isKinematic = false;
			oRigidbody.constraints = RigidbodyConstraints.None;

			oRigidbody.AddForceAtPosition(stVelocity, oWeapon.transform.position + stVelocity.normalized, ForceMode.VelocityChange);
		}
	}

	/** 렉돌 애니메이션을 처리한다 */
	protected virtual void HandleRagdollAni(Vector3 a_stDirection, STHitInfo a_stHitInfo)
	{
		float fForce = Random.Range(a_stHitInfo.m_fRagdollMinForce, a_stHitInfo.m_fRagdollMaxForce);
		this.RagdollController?.StartRagdollAni(a_stDirection * fForce);
	}

	/** 사망 상태를 처리한다 */
	protected virtual void HandleDieState(UnitController a_oAttacker, Vector3 a_stDirection, STHitInfo a_stHitInfo)
	{
		this.DropAllWeapons();
		this.HandleRagdollAni(a_stDirection, a_stHitInfo);
		this.HandleDieStateEffect(a_oAttacker);

		// 오오라 효과가 존재 할 경우
		if (m_oFXModelInfo != null && m_oFXModelInfo.AuraFX != null)
		{
			m_oFXModelInfo.AuraFX.SetActive(false);
		}

		// 사망 효과가 존재 할 경우
		if (m_oFXModelInfo != null && m_oFXModelInfo.DieFX != null)
		{
			GameResourceManager.Singleton.CreateObject(m_oFXModelInfo.DieFX,
				m_oFXModelInfo.DieFXDummy, null, (this is TurretController) ? 0.0f : ComType.G_LIFE_T_COMMON_FX);
		}

		foreach (var stKeyVal in this.EffectFXDict)
		{
			stKeyVal.Value.SetActive(false);
		}

		this.CurSightLineHandler?.gameObject.SetActive(false);
		this.gameObject.ExSetLayer(LayerMask.NameToLayer(ComType.G_LAYER_GROUND_OBJECT));
	}

	/** 사망 상태 효과를 처리한다 */
	private void HandleDieStateEffect(UnitController a_oAttacker)
	{
		// 공격자가 없을 경우
		if (a_oAttacker == null)
		{
			return;
		}

		int nDurationVal = (int)EOperationType.ADD * (int)EEquipEffectType.DURATION_VAL;

		float fCureRatio = ComUtil.GetAbilityVal(EEquipEffectType.CureAfterKill, a_oAttacker.CurAbilityValDict);
		float fAttackPowerRatio = ComUtil.GetAbilityVal(EEquipEffectType.AttackPowerAfterKill, a_oAttacker.CurAbilityValDict);
		float fMoveSpeedRatio = ComUtil.GetAbilityVal(EEquipEffectType.MoveSpeedRatioAfterKill, a_oAttacker.CurAbilityValDict);

		// 체력 회복 효과 발동이 가능 할 경우
		if (!fCureRatio.ExIsEquals(0.0f))
		{
			int nExtraHP = (int)(a_oAttacker.MaxHP * fCureRatio);
			a_oAttacker.SetHP(a_oAttacker.HP + nExtraHP);

			a_oAttacker.PlayFX(this.BattleController.FXModelInfo.BuffFXInfo.m_oHPBuffFX, a_oAttacker.gameObject);
			a_oAttacker.UpdateUIsState(true);
		}

		// 공격력 증가 효과 발동이 가능 할 경우
		if (!fAttackPowerRatio.ExIsEquals(0.0f))
		{
			float fDuration = ComUtil.GetAbilityVal(EEquipEffectType.AttackPowerAfterKill + nDurationVal,
				a_oAttacker.CurAbilityValDict);

			ComUtil.AddEffect(EEquipEffectType.ATTACK_POWER_UP,
				EEquipEffectType.AttackPowerRatio, fAttackPowerRatio, fDuration, 0.0f, a_oAttacker.ActiveEffectStackInfoList, (int)GameManager.Singleton.user.m_nAttackPowerAfterKillMaxStack);

			a_oAttacker.PlayEffectFX(EEquipEffectType.ATTACK_POWER_UP);
		}

		// 이동 속도 증가 효과 발동이 가능 할 경우
		if (!fMoveSpeedRatio.ExIsEquals(0.0f))
		{
			float fDuration = ComUtil.GetAbilityVal(EEquipEffectType.MoveSpeedRatioAfterKill + nDurationVal,
				a_oAttacker.CurAbilityValDict);

			ComUtil.AddEffect(EEquipEffectType.MOVE_SPEED_UP,
				EEquipEffectType.MoveSpeed, fMoveSpeedRatio, fDuration, 0.0f, a_oAttacker.ActiveEffectStackInfoList, (int)GameManager.Singleton.user.m_nMoveSpeedRatioAfterKillMaxStack);

			a_oAttacker.PlayEffectFX(EEquipEffectType.MOVE_SPEED_UP);
		}

		a_oAttacker.SetupAbilityValues(true);
	}

	/** 조준 대상을 탐색한다 */
	protected virtual UnitController FindLockOnTarget()
	{
		return null;
	}
	#endregion // 함수

	#region 접근 함수
	/** 공격 가능 여부를 검사한다 */
	public virtual bool IsEnableAttack()
	{
		return (this.IsDoubleShot || !this.IsFire) && this.MagazineInfo.m_nNumBullets >= 1;
	}

	/** 유닛 여부를 검사한다 */
	public virtual bool IsUnit(GameObject a_oGameObj)
	{
		return a_oGameObj.CompareTag(ComType.G_TAG_NPC) || a_oGameObj.CompareTag(ComType.G_TAG_PLAYER);
	}

	/** 구조물 여부를 검사한다 */
	public virtual bool IsStructure(GameObject a_oGameObj)
	{
		return a_oGameObj.CompareTag(ComType.G_TAG_STRUCTURE) || a_oGameObj.CompareTag(ComType.G_TAG_STRUCTURE_WOOD);
	}

	/** 조준 가능 여부를 검사한다 */
	public virtual bool IsAimableTarget(UnitController a_oController, float a_fAttackRange, bool a_bIsIgnoreStructure = false)
	{
		// 공격이 불가능 할 경우
		if (!this.IsAttackableTarget(a_oController, a_fAttackRange))
		{
			return false;
		}

		// 장애물 무시 모드 일 경우
		if (a_bIsIgnoreStructure)
		{
			return true;
		}

		var stDelta = a_oController.GetAttackRayOriginPos() - this.GetAttackRayOriginPos();
		return !Physics.SphereCast(new Ray(this.GetAttackRayOriginPos(), stDelta.normalized), 0.15f, stDelta.magnitude, this.AttackLayerMask);
	}

	/** 공격 가능 여부를 검사한다 */
	public virtual bool IsAttackableTarget(UnitController a_oController, float a_fAttackRange)
	{
		// 공격 대상이 없을 경우
		if (a_oController == null || (!this.IsIgnoreTargetSurvive && !this.BattleController.IsPlaying))
		{
			return false;
		}

		var stPos = a_oController.transform.position;
		return (this.IsIgnoreTargetSurvive || a_oController.IsSurvive) && this.transform.position.AlmostEquals(stPos, a_fAttackRange);
	}

	/** 타격 범위를 반환한다 */
	public float GetHitRangeShoot(int a_nAccuracy = 0)
	{
		float fAttackRange = (a_nAccuracy <= 0) ? this.Accuracy : a_nAccuracy;
		float fStandardRange = GlobalTable.GetData<int>(ComType.G_VALUE_STANDARD_ACCURACY);

		return (fStandardRange / fAttackRange) * ComType.G_UNIT_MM_TO_M;
	}

	/** 타격 범위를 반환한다 */
	public float GetHitRangeThrow(float a_fAttackRange = 0.0f)
	{
		float fAttackRange = a_fAttackRange.ExIsLessEquals(0.0f) ? this.AttackRange : a_fAttackRange;
		float fStandardRange = GlobalTable.GetData<int>(ComType.G_VALUE_STANDARD_ACCURACY);

		return (fStandardRange / fAttackRange) * ComType.G_UNIT_MM_TO_M;
	}

	/** 이동 광선 원점을 반환한다 */
	public Vector3 GetMoveRayOriginPos()
	{
		return this.transform.position + (Vector3.up * 0.35f);
	}

	/** 바닥 광선 원점을 반환한다 */
	public Vector3 GetGroundRayOriginPos()
	{
		return this.transform.position + (Vector3.up * 0.35f);
	}

	/** 공격 광선 원점을 반환한다 */
	public virtual Vector3 GetAttackRayOriginPos()
	{
		return m_oShootingPoint.transform.position;
	}

	/** 탄피 발사 방향을 반환한다 */
	public Vector3 GetShellDisposeDirection()
	{
		float fMinRange = ComType.G_MIN_RANGE_SHELL_DISPOSE_DIRECTION;
		float fMaxRange = ComType.G_MAX_RANGE_SHELL_DISPOSE_DIRECTION;

		var stDirection = Vector3.Lerp(this.transform.right, this.transform.up, Random.Range(fMinRange, fMaxRange));
		return Vector3.Lerp(stDirection.normalized, this.transform.forward, Random.Range(-fMinRange, fMinRange)).normalized;
	}

	/** 발사 여부를 변경한다 */
	public void SetIsFire(bool a_bIsFire)
	{
		this.IsFire = a_bIsFire;
	}

	/** 조준 여부를 변경한다 */
	public void SetIsAiming(bool a_bIsAiming)
	{
		this.IsAiming = a_bIsAiming;
		this.SetFireTimes(a_bIsAiming ? 0 : this.FireTimes);
	}

	/** 생존 여부를 변경한다 */
	public void SetIsSurvive(bool a_bIsSurvive)
	{
		this.IsSurvive = a_bIsSurvive;
	}

	/** 더블 샷 여부를 변경한다 */
	public void SetIsDoubleShot(bool a_bIsDoubleShot)
	{
		this.IsDoubleShot = a_bIsDoubleShot;
	}

	/** 더블 샷 발사 여부를 변경한다 */
	public void SetIsDoubleShotFire(bool a_bIsDoubleShotFire)
	{
		this.IsDoubleShotFire = a_bIsDoubleShotFire;
	}

	/** 상태 머신 유효 여부를 변경한다 */
	public void SetIsEnableStateMachine(bool a_bIsEnable)
	{
		this.IsEnableStateMachine = a_bIsEnable;
	}

	/** UI 상태 갱신 여부를 변경한다 */
	public void SetIsDirtyUpdateUIsState(bool a_bIsDirth)
	{
		m_bIsDirtyUpdateUIsState = a_bIsDirth;
	}

	/** 발사 횟수를 변경한다 */
	public void SetFireTimes(int a_nTimes)
	{
		this.FireTimes = a_nTimes;
	}

	/** 무적 시간을 변경한다 */
	public void SetUntouchableTime(float a_fUntouchableTime)
	{
		this.UntouchableTime = a_fUntouchableTime;
	}

	/** 이동 속도를 변경한다 */
	public void SetVelocity(Vector3 a_stVelocity)
	{
		this.Velocity = a_stVelocity;
	}

	/** 가속도를 변경한다 */
	public void SetAcceleration(Vector3 a_stAcceleration)
	{
		this.Acceleration = a_stAcceleration;
	}

	/** 탄창 정보를 변경한다 */
	public void SetMagazineInfo(STMagazineInfo a_stMagazineInfo)
	{
		this.MagazineInfo = a_stMagazineInfo;
	}

	/** 추가 게이지 UI 처리자를 변경한다 */
	public void SetExtraGaugeUIsHandler(CGaugeUIsHandler a_oGaugeUIsHandler)
	{
		m_oExtraGaugeUIsHandler = a_oGaugeUIsHandler;
	}

	/** 조준 대상을 변경한다 */
	public virtual void SetLockOnTarget(UnitController a_oController)
	{
		// 동일한 그룹 일 경우
		if (a_oController != null && a_oController.TargetGroup == this.TargetGroup)
		{
			return;
		}

		this.LockOnTarget = a_oController;
	}

	/** 가까운 대상을 변경한다 */
	public virtual void SetNearestTarget(UnitController a_oController)
	{
		this.NearestTarget = a_oController;
	}

	/** 추적 대상을 변경한다 */
	public virtual void SetTrackingTarget(UnitController a_oController)
	{
		// 동일한 그룹 일 경우
		if (a_oController != null && a_oController.TargetGroup == this.TargetGroup)
		{
			return;
		}

		this.TrackingTarget = a_oController;
	}
	#endregion // 접근 함수
}
