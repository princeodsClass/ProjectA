using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

/** 렉돌 제어자 */
public class RagdollController : MonoBehaviour
{
	/** 렉돌 조인트 정보 */
	public struct STRagdollJointInfo
	{
		public Joint m_oJoint;
		public Rigidbody m_oRigidbody;
	}

	#region 변수
	[SerializeField] private bool m_bIsExplosion = false;

	private Vector3 m_stPelvisPos = Vector3.zero;
	private Vector3 m_stPelvisRotate = Vector3.zero;

	private Tween m_oAni = null;
	private Animator m_oAnimator = null;
	private Collider m_oCollider = null;
	private Rigidbody m_oRigidbody = null;
	private NavMeshAgent m_oNavMeshAgent = null;

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oPelvis = null;
	[SerializeField] private GameObject m_oBoneRoot = null;
	[SerializeField] private GameObject m_oRagdollRoot = null;
	#endregion // 변수

	#region 프로퍼티
	public List<Collider> RagdollColliderList { get; } = new List<Collider>();
	public List<Rigidbody> RagdollRigidbodyList { get; } = new List<Rigidbody>();
	public List<STRagdollJointInfo> RagdollJointInfoList { get; } = new List<STRagdollJointInfo>();

	public GameObject BoneRoot => m_oBoneRoot;
	public GameObject RagdollRoot => m_oRagdollRoot;

	public PageBattle PageBattle => MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle);
	public BattleController BattleController => this.PageBattle?.BattleController;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_oBoneRoot = m_oBoneRoot ?? m_oRagdollRoot;
		var oJointList = CCollectionPoolManager.Singleton.SpawnList<Joint>();

		try
		{
			m_oAnimator = this.GetComponentInChildren<Animator>();
			m_oCollider = this.GetComponentInChildren<Collider>();
			m_oRigidbody = this.GetComponentInChildren<Rigidbody>();
			m_oNavMeshAgent = this.GetComponentInChildren<NavMeshAgent>();

			m_oRagdollRoot?.GetComponentsInChildren<Joint>(true, oJointList);
			m_oRagdollRoot?.GetComponentsInChildren<Collider>(true, this.RagdollColliderList);
			m_oRagdollRoot?.GetComponentsInChildren<Rigidbody>(true, this.RagdollRigidbodyList);

			for (int i = 0; i < oJointList.Count; ++i)
			{
				var stJointInfo = new STRagdollJointInfo()
				{
					m_oJoint = oJointList[i],
					m_oRigidbody = oJointList[i].connectedBody
				};

				this.RagdollJointInfoList.Add(stJointInfo);
			}

			// 골반이 존재 할 경우
			if (m_oPelvis != null)
			{
				m_stPelvisPos = m_oPelvis.transform.localPosition;
				m_stPelvisRotate = m_oPelvis.transform.localEulerAngles;
			}

			this.UpdateRagdollState(false);
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oJointList);
		}
	}

	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oAni, null);
	}

	/** 렉돌 애니메이션을 시작한다 */
	public void StartRagdollAni(Vector3 a_stVelocity)
	{
		this.UpdateRagdollState(true);

		var oSequence = DOTween.Sequence().SetDelay(0.0f);
		oSequence.AppendCallback(() => this.DoStartRagdollAni(a_stVelocity));
		oSequence.AppendInterval(ComType.G_DELAY_RAGDOLL_ANI);
		oSequence.AppendCallback(() => this.OnCompleteRagdollAni(oSequence));

		ComUtil.AssignVal(ref m_oAni, oSequence);
	}

	/** 렉돌 애니메이션을 중지한다 */
	public void StopRagdollAni()
	{
		this.SetIsEnable(m_oAnimator, true);
		this.SetIsEnable(m_oCollider, true);
		this.SetIsEnable(m_oNavMeshAgent, true);

		this.UpdateRagdollState(false);

		// 골반이 존재 할 경우
		if (m_oPelvis != null)
		{
			m_oPelvis.transform.localPosition = m_stPelvisPos;
			m_oPelvis.transform.localEulerAngles = m_stPelvisRotate;
		}

		// 리지드 바디가 존재 할 경우
		if (m_oRigidbody != null)
		{
			m_oRigidbody.velocity = Vector3.zero;
		}

		for (int i = 0; i < this.RagdollRigidbodyList.Count; ++i)
		{
			this.RagdollRigidbodyList[i].velocity = Vector3.zero;
		}
	}

	/** 렉돌 상태를 갱신한다 */
	private void UpdateRagdollState(bool a_bIsEnable)
	{
		for (int i = 0; i < this.RagdollColliderList.Count; ++i)
		{
			this.SetIsEnable(this.RagdollColliderList[i], a_bIsEnable);
		}

		for (int i = 0; i < this.RagdollRigidbodyList.Count; ++i)
		{
			this.RagdollRigidbodyList[i].constraints = a_bIsEnable ? 
				RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;

			this.RagdollRigidbodyList[i].useGravity = a_bIsEnable;
			this.RagdollRigidbodyList[i].isKinematic = !a_bIsEnable;
			this.RagdollRigidbodyList[i].collisionDetectionMode = CollisionDetectionMode.Discrete;
		}

		for (int i = 0; i < this.RagdollJointInfoList.Count; ++i)
		{
			this.RagdollJointInfoList[i].m_oJoint.connectedBody = a_bIsEnable ? 
				this.RagdollJointInfoList[i].m_oRigidbody : null;

			this.RagdollJointInfoList[i].m_oJoint.enableCollision = false;
			this.RagdollJointInfoList[i].m_oJoint.enablePreprocessing = false;
			this.RagdollJointInfoList[i].m_oJoint.connectedArticulationBody = null;
		}
	}

	/** 렉돌 애니메이션이 완료 되었을 경우 */
	public void OnCompleteRagdollAni(Sequence a_oSender)
	{
		ComUtil.AssignVal(ref m_oAni, null);

		for (int i = 0; i < this.RagdollRigidbodyList.Count; ++i)
		{
			this.RagdollRigidbodyList[i].constraints = RigidbodyConstraints.FreezeAll;
		}
	}

	/** 렉돌 애니메이션을 시작한다 */
	private void DoStartRagdollAni(Vector3 a_stVelocity)
	{
		this.SetIsEnable(m_oAnimator, false);
		this.SetIsEnable(m_oCollider, false);
		this.SetIsEnable(m_oNavMeshAgent, false);

		// 리지드 바디가 존재 할 경우
		if (m_oRigidbody != null)
		{
			m_oRigidbody.constraints = RigidbodyConstraints.FreezeAll;
		}

		var stOffset = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));

		for (int i = 0; i < this.RagdollRigidbodyList.Count; ++i)
		{
			// 폭발 모드 일 경우
			if (m_bIsExplosion)
			{
				this.BattleController.DropWeaponList.Add(this.RagdollRigidbodyList[i].gameObject);
				
				this.RagdollRigidbodyList[i].constraints = RigidbodyConstraints.None;
				this.RagdollRigidbodyList[i].transform.SetParent(null);

				this.RagdollRigidbodyList[i].AddExplosionForce(a_stVelocity.magnitude, 
					this.transform.position + stOffset + (this.transform.forward * 0.1f), 10.0f, 1.0f, ForceMode.VelocityChange);
			}
			else
			{
				this.RagdollRigidbodyList[i].AddForceAtPosition(a_stVelocity,
					m_oRagdollRoot.transform.position + stOffset, ForceMode.VelocityChange);
			}
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 활성 여부를 변경한다 */
	public void SetIsEnable(Behaviour a_oComponent, bool a_bIsEnable)
	{
		// 컴포넌트가 없을 경우
		if (a_oComponent == null)
		{
			return;
		}

		a_oComponent.enabled = a_bIsEnable;
	}

	/** 활성 여부를 변경한다 */
	public void SetIsEnable(Collider a_oCollider, bool a_bIsEnable)
	{
		// 충돌체가 없을 경우
		if (a_oCollider == null || a_oCollider is CharacterController)
		{
			return;
		}

		a_oCollider.enabled = a_bIsEnable;
		a_oCollider.isTrigger = !a_bIsEnable;
	}
	#endregion // 접근 함수
}
