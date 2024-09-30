using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/** 아이템 처리자 */
public class CGroundItemHandler : MonoBehaviour
{
	#region 변수
	private float m_fSpeedRate = 0.0f;
	private float m_fAcquireRate = 0.0f;
	private float m_fUpdateSkipTime = 0.0f;

	private Tween m_oRotateAni = null;
	private Rigidbody m_oRigidbody = null;
	private GameObject m_oAcquireTarget = null;
	private List<Collider> m_oColliderList = new List<Collider>();

	[SerializeField] private Image m_oBGImg = null;
	[SerializeField] private TMP_Text m_oNameText = null;
	[SerializeField] private List<GameObject> m_oGradeMarkImgList = new List<GameObject>();

	[SerializeField] private GameObject m_oDummy = null;
	[SerializeField] private GameObject m_oCanvas = null;
	[SerializeField] private GameObject m_oExtraGroundItem = null;
	#endregion // 변수

	#region 프로퍼티
	public uint Key { get; private set; } = 0;
	public int NumItems { get; private set; } = 0;
	public bool IsAcquire { get; private set; } = false;

	public virtual int Grade => 0;
	public virtual string ItemName => string.Empty;

	public PageBattle PageBattle => MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle);

	public GameObject Dummy => m_oDummy;
	public GameObject ExtraGroundItem => m_oExtraGroundItem;
	public List<GameObject> GradeMakrImgList => m_oGradeMarkImgList;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_oRigidbody = this.GetComponent<Rigidbody>();
		this.GetComponentsInChildren<Collider>(m_oColliderList);
	}

	/** 초기화 */
	public virtual void Init(uint a_nKey, int a_nNumItems)
	{
		this.Key = a_nKey;
		this.NumItems = a_nNumItems;
		this.IsAcquire = false;

		m_fSpeedRate = 0.0f;
		m_fAcquireRate = 0.0f;
		m_fUpdateSkipTime = 0.0f;
		m_oAcquireTarget = null;

		m_oRigidbody.velocity = Vector3.zero;
		m_oRigidbody.angularVelocity = Vector3.zero;
		m_oRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

		// 추가 아이템이 존재 할 경우
		if(m_oExtraGroundItem != null)
		{
			m_oExtraGroundItem.SetActive(false);
		}
		
		for (int i = 0; i < m_oGradeMarkImgList.Count; ++i)
		{
			m_oGradeMarkImgList[i].gameObject.SetActive(false);
		}

		this.ExLateCallFunc((a_oSender) => this.OnInit());
	}

	/** 비활성화 되었을 경우 */
	public virtual void OnDisable()
	{
		ComUtil.AssignVal(ref m_oRotateAni, null);
	}

	/** 상태를 갱신한다 */
	public void LateUpdate()
	{
		m_fUpdateSkipTime += Time.deltaTime;

		// 획득 상태 일 경우
		if (this.IsAcquire && m_oAcquireTarget != null && m_fUpdateSkipTime.ExIsGreatEquals(1.5f))
		{
			m_fSpeedRate = Mathf.Min(5.0f, m_fSpeedRate + (Time.deltaTime * m_fAcquireRate));
			m_fAcquireRate += Time.deltaTime * 0.575f;
			m_oRigidbody.velocity = Vector3.zero;

			var stDelta = m_oAcquireTarget.transform.position - this.transform.position;
			var stNextPos = this.transform.position + (stDelta.normalized * ComType.G_SPEED_ACQUIRE_ITEM * m_fSpeedRate * Time.deltaTime);
			var stNextPosDelta = m_oAcquireTarget.transform.position - stNextPos;

			for (int i = 0; i < m_oColliderList.Count; ++i)
			{
				m_oColliderList[i].enabled = false;
			}

			// 획득 대상에 도달했을 경우
			if (Vector3.Dot(stDelta.normalized, stNextPosDelta.normalized).ExIsLess(0.0f))
			{
				stNextPos = m_oAcquireTarget.transform.position;
				this.IsAcquire = true;

#if DISABLE_THIS
				// 획득 연출이 필요 할 경우
				if(m_oExtraGroundItem != null)
				{
					this.StartAcquireDirecting();
				}
#endif // #if DISABLE_THIS

				m_oAcquireTarget.GetComponent<PlayerController>()?.OnAcquireItem(this);
			}

			this.transform.position = stNextPos;
		}
	}

	/** UI 상태를 갱신한다 */
	protected virtual void UpdateUIsState()
	{
		// 이름 텍스트가 존재 할 경우
		if (m_oNameText != null)
		{
			m_oNameText.text = this.ItemName;
		}

		// 배경 이미지가 존재 할 경우
		if (m_oBGImg != null)
		{
			m_oBGImg.color = this.PageBattle.GradeColorWrapper.GetColor(this.Grade);
		}

		for (int i = 0; i < m_oGradeMarkImgList.Count; ++i)
		{
			m_oGradeMarkImgList[i].gameObject.SetActive(i < this.Grade);
		}
	}

	/** 초기화 되었을 경우 */
	protected virtual void OnInit()
	{
		this.UpdateUIsState();
		ComUtil.AssignVal(ref m_oRotateAni, m_oDummy.transform.DORotate(Vector3.up * 360.0f, ComType.G_DURATION_GROUND_ITEM_ANI, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental));

		for (int i = 0; i < m_oColliderList.Count; ++i)
		{
			m_oColliderList[i].enabled = true;
		}

		// 추가 아이템이 존재 할 경우
		if(m_oExtraGroundItem != null)
		{
			m_oExtraGroundItem.SetActive(true);
		}

		StopCoroutine("StopPhysics");
		StartCoroutine(this.StopPhysics());
	}

	/** 획득 연출을 시작한다 */
	protected virtual void StartAcquireDirecting()
	{
		m_oExtraGroundItem.transform.SetParent(null);
	}

	/** 물리를 중지한다 */
	private IEnumerator StopPhysics()
	{
		yield return YieldInstructionCache.WaitForSeconds(ComType.G_DELAY_GROUND_ITEM_PHYSICS_STOP);
		m_oRigidbody.constraints = RigidbodyConstraints.FreezeAll;
	}
	#endregion // 함수

	#region 접근 함수
	/** 획득 대상을 변경한다 */
	public void SetAcquireTarget(GameObject a_oTarget)
	{
		this.IsAcquire = true;
		m_oAcquireTarget = a_oTarget;
	}

	/** 추가 아이템을 변경한다 */
	public void SetExtraGroundItem(GameObject a_oGroundItem)
	{
		m_oExtraGroundItem = a_oGroundItem;
	}
	#endregion // 접근 함수
}
