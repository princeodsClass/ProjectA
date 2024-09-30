using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 중첩 카메라 컨트롤러 */
public class OverlayCameraController : MonoBehaviour
{
	#region 변수
	[SerializeField] private Camera m_oMainCamera = null;
	private Camera m_oOverlayCamera = null;
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_oOverlayCamera = this.GetComponent<Camera>();
	}

	/** 상태를 갱신한다 */
	public void LateUpdate()
	{
		// 시야 각이 동일 할 경우
		if (m_oOverlayCamera.fieldOfView.ExIsEquals(m_oMainCamera.fieldOfView))
		{
			return;
		}

		m_oOverlayCamera.fieldOfView = m_oMainCamera.fieldOfView;
	}
	#endregion // 함수
}
