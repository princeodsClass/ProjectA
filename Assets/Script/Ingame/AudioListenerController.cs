using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 오디오 리스너 처리자 */
public class AudioListenerController : MonoBehaviour
{
	#region 변수
	[SerializeField] private Vector3 m_stOffset = new Vector3(0.0f, 1.5f, 0.0f);

	private Camera m_oCamera = null;
	private GameObject m_oFollowTarget = null;
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_oCamera = Camera.main;
	}

	/** 상태를 처리한다 */
	public void LateUpdate()
	{
		var stForward = m_oCamera.transform.forward;
		stForward.y = 0.0f;

		// 추적 대상이 존재 할 경우
		if (m_oFollowTarget != null)
		{
			this.transform.position = m_oFollowTarget.transform.position + m_stOffset;
			this.transform.forward = stForward.normalized;
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 추적 대상이 변경한다 */
	public void SetFollowTarget(GameObject a_oTarget)
	{
		m_oFollowTarget = a_oTarget;
	}
	#endregion // 접근 함수
}
