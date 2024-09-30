using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 상호 작용 맵 오브젝트 처리자 */
public partial class CInteractableMapObjHandler : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public FieldObjectTable m_oTable;
	}

	#region 변수
	[Header("=====> Interactable Map Obj Handler - Etc <=====")]
	[SerializeField] private AudioClip m_oOpenAudioClip = null;
	[SerializeField] private AudioClip m_oCastingAudioClip = null;

	[SerializeField] private Animator m_oAnimator = null;
	[SerializeField] private AudioSource m_oAudioSource = null;
	private bool m_bIsInteractable = false;

	private float m_fInteractableRange = 0.0f;
	private float m_fInteractableSkipTime = 0.0f;
	private float m_fMaxInteractableSkipTime = 0.0f;

	[Header("=====> Interactable Map Obj Handler - UIs <=====")]
	[SerializeField] private Image m_oImg = null;
	[SerializeField] private TMP_Text m_oText = null;

	[SerializeField] private Canvas m_oCanvas = null;

	[Header("=====> Interactable Map Obj Handler - Game Objects <=====")]
	[SerializeField] private List<GameObject> m_oContentsObjList = new List<GameObject>();

	private GameObject m_oInteractableTarget = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public bool IsInteractable => m_bIsInteractable;

	public AudioClip OpenAudioClip => m_oOpenAudioClip;
	public AudioClip CastingAudioClip => m_oCastingAudioClip;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		m_bIsInteractable = true;

		m_fInteractableRange = a_stParams.m_oTable.InteractionRange * ComType.G_UNIT_MM_TO_M;
		m_fMaxInteractableSkipTime = a_stParams.m_oTable.CastingTime * ComType.G_UNIT_MS_TO_S;

		for (int i = 0; i < m_oContentsObjList.Count; ++i)
		{
			m_oContentsObjList[i].SetActive(false);
		}

		// 오디오 소스가 존재 할 경우
		if (m_oAudioSource != null)
		{
			m_oAudioSource.clip = m_oCastingAudioClip;
			m_oAudioSource.Play();
		}

		StartCoroutine(this.CoAppear());
	}

	/** 상호 작용 맵 오브젝트를 닫는다 */
	public void Close()
	{
		StartCoroutine(this.CoClose());
	}

	/** 상태를 갱신한다 */
	public void LateUpdate()
	{
		m_oCanvas.gameObject.SetActive(false);
		m_oCanvas.transform.forward = Camera.main.transform.forward;

		// 상호 작용이 불가능 할 경우
		if (!m_bIsInteractable || m_oInteractableTarget == null)
		{
			return;
		}

		float fDistance = Vector3.Distance(m_oInteractableTarget.transform.position, this.transform.position);
		float fRemainTime = m_fMaxInteractableSkipTime - m_fInteractableSkipTime;

		m_oText.text = $"{fRemainTime:0.0}";
		m_oImg.fillAmount = fRemainTime / m_fMaxInteractableSkipTime;

		// 상호 작용 범위를 벗어났을 경우
		if (fDistance.ExIsGreat(m_fInteractableRange))
		{
			m_fInteractableSkipTime = 0.0f;
			return;
		}

		m_fInteractableSkipTime += Time.deltaTime;
		m_oCanvas.gameObject.SetActive(true);

		// 상호 작용이 대기 시간이 지났을 경우
		if (m_fInteractableSkipTime.ExIsGreatEquals(m_fMaxInteractableSkipTime))
		{
			m_bIsInteractable = false;
			m_oAnimator.SetTrigger(ComType.G_PARAMS_OPEN);

			// 오디오 소스가 존재 할 경우
			if (m_oAudioSource != null)
			{
				m_oAudioSource.Stop();
			}

			StartCoroutine(this.CoHandleInteractable());
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 상호 작용 대상을 변경한다 */
	public void SetInteractableTarget(GameObject a_oTarget)
	{
		m_oInteractableTarget = a_oTarget;
	}
	#endregion // 접근 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(FieldObjectTable a_oTable)
	{
		return new STParams()
		{
			m_oTable = a_oTable
		};
	}
	#endregion // 클래스 팩토리 함수
}

/** 상호 작용 맵 오브젝트 처리자 - 코루틴 */
public partial class CInteractableMapObjHandler : MonoBehaviour
{
	#region 함수
	/** 상호 작용 맵 오브젝트를 닫는다 */
	private IEnumerator CoClose()
	{
		yield return new WaitForSeconds(0.5f);
		m_oAnimator.SetTrigger(ComType.G_PARAMS_CLOSE);

		var oColliders = this.GetComponentsInChildren<Collider>();

		for (int i = 0; i < oColliders.Length; ++i)
		{
			oColliders[i].enabled = false;
		}
	}

	/** 상호 작용 맵 오브젝트를 출력한다 */
	private IEnumerator CoAppear()
	{
		yield return new WaitForSeconds(0.5f);
		m_oAnimator.SetTrigger(ComType.G_PARAMS_APPEAR);

		yield return new WaitForEndOfFrame();

		for (int i = 0; i < m_oContentsObjList.Count; ++i)
		{
			m_oContentsObjList[i].SetActive(true);
		}
	}

	/** 상호 작용을 처리한다 */
	private IEnumerator CoHandleInteractable()
	{
		yield return new WaitForSecondsRealtime(0.25f);
		m_oInteractableTarget.GetComponent<PlayerController>()?.OnReceiveInteractableEvent(this);
	}
	#endregion // 함수
}
