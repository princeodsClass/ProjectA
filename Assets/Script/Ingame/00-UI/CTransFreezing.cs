using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 트랜스 폼 프리징 */
public class CTransFreezing : MonoBehaviour
{
	#region 변수
	private Vector3 m_stPosOffset = Vector3.zero;
	#endregion // 변수

	#region 함수
	/** 상태를 갱신한다 */
	public void LateUpdate()
	{
		this.transform.SetPositionAndRotation(this.transform.parent.position + m_stPosOffset, 
			Quaternion.identity);
	}
	#endregion // 함수

	#region 접근 함수
	/** 위치 간격을 변경한다 */
	public void SetPosOffset(Vector3 a_stOffset)
	{
		m_stPosOffset = a_stOffset;
	}
	#endregion // 접근 함수
}
