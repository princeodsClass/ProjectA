using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTG;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	/** 맵 에디터 카메라 제어자 */
	public partial class CCameraController : MonoBehaviour
	{
		#region 프로퍼티
		[SerializeField] private Vector3 m_stOriginPos;
		[SerializeField] private Vector3 m_stOriginScale;
		[SerializeField] private Vector3 m_stOriginRotate;

		[SerializeField] private float m_fMoveSpeed = 10.0f;
		[SerializeField] private float m_fWheelSpeed = 10.0f;

		[SerializeField] private GameObject m_oGizmos = null;
		#endregion // 프로퍼티

		#region 함수
		/** 초기화 */
		public void Awake()
		{
			m_stOriginPos = this.transform.localPosition;
			m_stOriginScale = this.transform.localScale;
			m_stOriginRotate = this.transform.localEulerAngles;

			this.GetComponent<Camera>().farClipPlane = ushort.MaxValue * 2.0f;
			RTFocusCamera.Get.MoveSettings.AccelerationRate = m_fMoveSpeed * 2.0f;

			RTFocusCamera.Get.PanSettings.StandardPanSensitivity = m_fMoveSpeed / 7.5f;
			RTFocusCamera.Get.ZoomSettings.OrthoStandardZoomSensitivity = m_fWheelSpeed * 3.0f;
			RTFocusCamera.Get.ZoomSettings.PerspStandardZoomSensitivity = m_fWheelSpeed * 3.0f;
		}

		/** 상태를 갱신한다 */
		public void Update()
		{
			m_oGizmos.transform.rotation = Quaternion.Inverse(this.transform.rotation);
			RTFocusCamera.Get.MoveSettings.MoveSpeed = Input.GetKey(KeyCode.LeftShift) ? m_fMoveSpeed * 3.0f : m_fMoveSpeed;
		}

		/** 트랜스 폼 상태를 리셋한다 */
		public void ResetTransState()
		{
			this.transform.localPosition = m_stOriginPos;
			this.transform.localScale = m_stOriginScale;
			this.transform.localEulerAngles = m_stOriginRotate;
		}
		#endregion // 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
