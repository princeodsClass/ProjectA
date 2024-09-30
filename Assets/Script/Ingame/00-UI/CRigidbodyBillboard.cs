using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 리지드 바디 빌보드 */
public class CRigidbodyBillboard : MonoBehaviour
{
	#region 변수
	private Rigidbody m_oRigidbody = null;
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_oRigidbody = this.GetComponent<Rigidbody>();
	}

	/** 상태를 갱신한다 */
	public void LateUpdate()
	{
		// 방향이 없을 경우
		if (m_oRigidbody.velocity.ExIsEquals(Vector3.zero))
		{
			return;
		}

		m_oRigidbody.angularVelocity = Vector3.zero;
		this.transform.forward = m_oRigidbody.velocity.normalized;
	}
	#endregion // 함수
}
