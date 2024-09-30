using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

/** 투사체 제어자 */
public partial class ProjectileController : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public float m_fATK;
		public float m_fDamageFieldATK;

		public EAttackType m_eAttackType;
		public EDamageType m_eDamageType;
		public EWeaponType m_eWeaponType;

		public Vector3 m_stFirePos;
		public ItemWeapon m_oWeapon;
		public UnitController m_oOwner;
		public Dictionary<EEquipEffectType, float> m_oAbilityValDict;
	}

	#region 변수
	private bool m_bIsExplosion = false;

	private int m_nTargetLayerMask = 0;
	private int m_nWalkableAreaMask = 0;
	private int m_nStructureLayerMask = 0;

	private float m_fFXRange = 0.0f;
	private float m_fFXDuration = 0.0f;
	private float m_fExtraRange = 0.0f;
	private float m_fFXExtraRangeRatio = 0.0f;
	private Vector3 m_stTargetPos = Vector3.zero;

	private Collider m_oCollider = null;
	private Rigidbody m_oRigidbody = null;
	private EffectTable m_oDelayFXTable = null;
	private TrailRenderer m_oTrailRenderer = null;
	private ParticleSystem m_oParticleSystem = null;

	private Tween m_oExplosionBoundsAni = null;
	private CTriggerDispatcher m_oTriggerDispatcher = null;
	private CRigidbodyBillboard m_oRigidbodyBillboard = null;
	private CCollisionDispatcher m_oCollisionDispatcher = null;
	private System.Action<ProjectileController, Collider> m_oCollisionCallback = null;

	private GameObject m_oExplosionWarning = null;
	private GameObject m_oExplosionBoundsSphere = null;
	private List<EffectTable> m_oEffectTableList = new List<EffectTable>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	
	public int NumHitTargets { get; private set; } = 0;
	public SoundModelInfo SoundModelInfo { get; private set; } = null;
	public ProjectileModelInfo ProjectileModelInfo { get; private set; } = null;

	public PageBattle PageBattle => MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle);
	public BattleController BattleController => this.PageBattle?.BattleController;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_oCollider = this.GetComponentInChildren<Collider>();
		m_oRigidbody = this.GetComponentInChildren<Rigidbody>();
		m_oTrailRenderer = this.GetComponentInChildren<TrailRenderer>();
		m_oParticleSystem = this.GetComponentInChildren<ParticleSystem>();
		m_oRigidbodyBillboard = this.GetComponentInChildren<CRigidbodyBillboard>();
		m_oExplosionBoundsSphere = ComUtil.FindChildByName("FX_Grenade_Explosion_01_Sphere", this.transform)?.gameObject;

		m_nStructureLayerMask = LayerMask.GetMask(ComType.G_LAYER_STRUCTURE);
		m_nWalkableAreaMask = 1 << NavMesh.GetAreaFromName(ComType.G_NAV_MESH_AREA_WALKABLE);

		this.SoundModelInfo = this.GetComponentInChildren<SoundModelInfo>();
		this.ProjectileModelInfo = this.GetComponentInChildren<ProjectileModelInfo>();

		// 충돌 전달자를 설정한다 {
		m_oTriggerDispatcher = this.GetComponentInChildren<CTriggerDispatcher>();
		m_oTriggerDispatcher?.SetEnterCallback(this.HandleOnTriggerEnter);

		m_oCollisionDispatcher = this.GetComponentInChildren<CCollisionDispatcher>();
		m_oCollisionDispatcher?.SetEnterCallback(this.HandleOnCollisionEnter);
		// 충돌 전달자를 설정한다 }
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		this.NumHitTargets = 0;

		m_bIsExplosion = false;
		m_oDelayFXTable = null;
		m_oCollisionCallback = null;

		this.ResetFX();
		this.ResetRigidbody();

		m_nTargetLayerMask = LayerMask.GetMask(ComType.G_LAYER_PLAYER, 
			ComType.G_LAYER_NON_PLAYER_01, ComType.G_LAYER_NON_PLAYER_02, ComType.G_LAYER_NON_PLAYER_03, ComType.G_LAYER_NON_PLAYER_04);

		// 유닛이 존재 할 경우
		if(a_stParams.m_oOwner != null)
		{
			string oLayer = ComType.G_UNIT_LAYER_LIST.ExIsValidIdx(a_stParams.m_oOwner.TargetGroup) ? 
				ComType.G_UNIT_LAYER_LIST[a_stParams.m_oOwner.TargetGroup] : ComType.G_LAYER_NON_PLAYER_01;

			m_nTargetLayerMask &= ~LayerMask.GetMask(oLayer);
		}

		m_oTrailRenderer?.Clear();
		m_oParticleSystem?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		m_oExplosionBoundsSphere?.SetActive(false);

		// 리지드 바디 빌보드가 존재 할 경우
		if (m_oRigidbodyBillboard != null)
		{
			m_oRigidbodyBillboard.enabled = true;
		}
	}

	/** 이펙트 상태를 리셋한다 */
	public virtual void ResetFX()
	{
		// 폭발 경고가 없을 경우
		if (m_oExplosionWarning == null)
		{
			return;
		}
		
		// 트랜스 폼 프리징이 존재 할 경우
		if(m_oExplosionWarning.TryGetComponent(out CTransFreezing oTransFreezing))
		{
			oTransFreezing.enabled = false;
		}

		GameResourceManager.Singleton.ReleaseObject(m_oExplosionWarning);
		m_oExplosionWarning = null;
	}

	/** 리지드 바디 상태를 리셋한다 */
	public virtual void ResetRigidbody()
	{
		m_oRigidbody.velocity = Vector3.zero;
		m_oRigidbody.angularVelocity = Vector3.zero;
		m_oRigidbody.interpolation = RigidbodyInterpolation.None;
		m_oRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
	}

	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oExplosionBoundsAni, null);
	}

	/** 도탄 시킨다 */
	public void Ricochet(UnitController a_oSender)
	{
		float fSpeed = m_oRigidbody.velocity.magnitude;
		this.ResetRigidbody();

		var stDelta = a_oSender.GetAttackRayOriginPos() - this.transform.position;
		this.transform.forward = stDelta.normalized;

		m_oParticleSystem?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		m_oParticleSystem?.Play(true);

		m_oTrailRenderer?.Clear();
		m_oRigidbody.AddForce(stDelta.normalized * fSpeed, ForceMode.VelocityChange);
	}

	/** 타격 효과를 재생한다 */
	public void PlayHitFX(Collider a_oCollider)
	{
		switch (this.Params.m_eAttackType)
		{
			case EAttackType.SHOOT: this.PlayHitFXShoot(a_oCollider); break;
			case EAttackType.THROW: this.PlayHitFXThrow(a_oCollider); break;
			case EAttackType.DROP: this.PlayHitFXDrop(a_oCollider); break;
		}
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(UnitController a_oOwner,
		Dictionary<EEquipEffectType, float> a_oAbilityValDict, EAttackType a_eAttackType, EDamageType a_eDamageType, EWeaponType a_eWeaponType, float a_fATK, Vector3 a_stFirePos, float a_fDamageFieldATK = 0.0f, ItemWeapon a_oWeapon = null)
	{
		return new STParams()
		{
			m_fATK = a_fATK,
			m_fDamageFieldATK = a_fDamageFieldATK,
			
			m_eAttackType = a_eAttackType,
			m_eDamageType = a_eDamageType,
			m_eWeaponType = a_eWeaponType,

			m_oOwner = a_oOwner,
			m_oWeapon = a_oWeapon,
			m_stFirePos = a_stFirePos,
			m_oAbilityValDict = a_oAbilityValDict
		};
	}
	#endregion // 클래스 팩토리 함수
}
