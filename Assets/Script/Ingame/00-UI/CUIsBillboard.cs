using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** UI 빌보드 */
public class CUIsBillboard : MonoBehaviour
{
	#region 변수
	private Camera m_oCamera = null;
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public void Start()
	{
		m_oCamera = Camera.main;
	}

	/** 상태를 처리한다 */
	public void LateUpdate()
	{
		// 카메라가 없을 경우
		if (m_oCamera == null)
		{
			return;
		}

		this.transform.forward = m_oCamera.transform.forward;
	}
	#endregion // 함수
}
