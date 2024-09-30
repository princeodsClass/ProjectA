using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 리지드 바디 중력 비율 처리자 */
public class RigidbodyGravityScaler : MonoBehaviour
{
	#region 변수
	private Rigidbody m_oRigidbody = null;
	#endregion // 변수

	#region 프로퍼티
	public float GravityScale { get; private set; } = 1.0f;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_oRigidbody = this.GetComponentInChildren<Rigidbody>();
		m_oRigidbody.useGravity = false;
	}

	/** 상태를 갱신한다 */
	public void FixedUpdate()
	{
		var stGravity = Physics.gravity * this.GravityScale;
		m_oRigidbody.AddForce(stGravity, ForceMode.Acceleration);
	}
	#endregion // 함수

	#region 접근 함수
	/** 중력 비율을 변경한다 */
	public void SetGravityScale(float a_fScale)
	{
		this.GravityScale = a_fScale;
	}
	#endregion // 접근 함수
}
