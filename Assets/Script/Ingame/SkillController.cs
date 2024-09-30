using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

/** 스킬 제어자 */
public partial class SkillController : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public bool m_bIsContinuing;
		public float m_fATK;
		public EWeaponType m_eWeaponType;
		public Vector3 m_stTargetPos;

		public SkillTable m_oTable;
		public UnitController m_oOwner;
		public UnitController m_oTarget;

		public System.Action<SkillController, Collider> m_oCallback;
		public System.Action<SkillController> m_oCompleteCallback;
	}

	#region 변수
	[SerializeField] private float m_fJumpAttackJumpOffset = 25.0f;
	[SerializeField] private float m_fJumpAttackSkillJumpDelay = 0.35f;
	[SerializeField] private float m_fJumpAttackSkillJumpDuration = 0.25f;

	private int m_nTargetLayerMask = 0;
	private int m_nWalkableAreaMask = 0;

	private Tween m_oSummonAni = null;
	private Tween m_oJumpAttackAni = null;
	private FXModelInfo m_oFXModelInfo = null;
	private SoundModelInfo m_oSoundModelInfo = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }

	public uint SkillType => this.Params.m_oTable.PrimaryKey;
	public int SkillAniType => this.Params.m_oTable.SkillAniType;
	public PageBattle PageBattle => MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle);
	public BattleController BattleController => this.PageBattle?.BattleController;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_oFXModelInfo = this.GetComponent<FXModelInfo>();
		m_oSoundModelInfo = this.GetComponent<SoundModelInfo>();

		m_nWalkableAreaMask = 1 << NavMesh.GetAreaFromName(ComType.G_NAV_MESH_AREA_WALKABLE);
	}

	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oSummonAni, null);
		ComUtil.AssignVal(ref m_oJumpAttackAni, null);
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		
		m_nTargetLayerMask = LayerMask.GetMask(ComType.G_LAYER_PLAYER, 
			ComType.G_LAYER_NON_PLAYER_01, ComType.G_LAYER_NON_PLAYER_02, ComType.G_LAYER_NON_PLAYER_03, ComType.G_LAYER_NON_PLAYER_04);

		// 유닛이 존재 할 경우
		if(a_stParams.m_oOwner != null)
		{
			string oLayer = ComType.G_UNIT_LAYER_LIST.ExIsValidIdx(a_stParams.m_oOwner.TargetGroup) ? 
				ComType.G_UNIT_LAYER_LIST[a_stParams.m_oOwner.TargetGroup] : ComType.G_LAYER_NON_PLAYER_01;

			m_nTargetLayerMask &= ~LayerMask.GetMask(oLayer);
		}

		switch ((ESkillType)a_stParams.m_oTable.SkillType)
		{
			case ESkillType.SUMMON: this.HandleSummonSkill(); break;
			case ESkillType.LOCK_WEAPON: this.HandleLockWeaponSkill(); break;
			case ESkillType.BOMBING_REQUEST: this.HandleBombingRequestSkill(); break;
			case ESkillType.JUMP_ATTACK: this.HandleJumpAttackSkill(); break;
			case ESkillType.FLAME_FIELD: this.HandleFlameFieldSkill(); break;
			case ESkillType.RICOCHET: this.HandleRicochetSkill(); break;
			case ESkillType.UNTOUCHABLE: this.HandleUntouchableSkill(); break;
			case ESkillType.ICE: this.HandleIceSkill(); break;
		}
	}
	#endregion // 함수
	
	#region 클래스 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(float a_fATK, 
		EWeaponType a_eWeaponType, Vector3 a_stTargetPos, SkillTable a_oTable, UnitController a_oOwner, UnitController a_oTarget, System.Action<SkillController, Collider> a_oCallback, System.Action<SkillController> a_oCompleteCallback, bool a_bIsContinuing = false)
	{
		return new STParams()
		{
			m_bIsContinuing = a_bIsContinuing,
			m_fATK = a_fATK,
			m_eWeaponType = a_eWeaponType,
			m_stTargetPos = a_stTargetPos,
			m_oTable = a_oTable,
			m_oOwner = a_oOwner,
			m_oTarget = a_oTarget,
			m_oCallback = a_oCallback,
			m_oCompleteCallback = a_oCompleteCallback
		};
	}
	#endregion // 클래스 함수
}
