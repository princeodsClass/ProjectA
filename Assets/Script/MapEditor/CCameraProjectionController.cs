using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	/** 카메라 투영 제어자 */
	public class CCameraProjectionController : MonoBehaviour
	{
		#region 변수
		private Camera m_oCamera = null;

		[SerializeField] private bool m_bIsPerspective = true;
		[SerializeField] private GameObject m_oObjsRoot = null;
		#endregion // 변수

		#region 프로퍼티
		private float PlaneHeight => 1920.0f * 1.5f;
		private float PlaneDistance => ((this.PlaneHeight / 2.0f) / Mathf.Tan(30.0f * Mathf.Deg2Rad)) * ComType.G_UNIT_SCALE;

		public float ResolutionScale => this.DeviceScreenSize.x.ExIsLess(this.ResolutionScreenSize.x) ? this.DeviceScreenSize.x / this.ResolutionScreenSize.x : 1.0f;
		public Vector3 ResolutionScreenSize => new Vector3(this.DeviceScreenSize.y * (1080.0f / 1920.0f), this.DeviceScreenSize.y, this.DeviceScreenSize.z);

		public Vector3 DeviceScreenSize
		{
			get
			{
#if UNITY_EDITOR
				return new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, 0.0f);
#else
				return new Vector3(Screen.width, Screen.height, 0.0f);
#endif // #if UNITY_EDITOR
			}
		}
		#endregion // 프로퍼티

		#region 함수
		/** 초기화 */
		public void Awake()
		{
			m_oCamera = this.GetComponentInChildren<Camera>();
			m_oObjsRoot.transform.localScale = new Vector3(this.ResolutionScale, this.ResolutionScale, this.ResolutionScale);
		}

		/** 초기화 */
		public void Start()
		{
			m_oCamera.orthographic = !m_bIsPerspective;
			m_oCamera.transform.localScale = Vector3.one;

			// 직교 투영 일 경우
			if (m_oCamera.orthographic)
			{
				this.Setup2DCamera();
			}
			else
			{
				this.Setup3DCamera();
			}
		}

		/** 상태를 리셋한다 */
		public void Reset()
		{
			m_oCamera.transform.localPosition = new Vector3(0.0f, 0.0f, -this.PlaneDistance);
		}

		/** 2 차원 카메라를 설정한다 */
		private void Setup2DCamera()
		{
			float fPlaneHeight = this.PlaneHeight * ComType.G_UNIT_SCALE;
			m_oCamera.orthographicSize = fPlaneHeight / 2.0f;
		}

		/** 3 차원 카메라를 설정한다 */
		private void Setup3DCamera()
		{
			float fPlaneHeight = this.PlaneHeight * ComType.G_UNIT_SCALE;
			m_oCamera.fieldOfView = (Mathf.Atan((fPlaneHeight / 2.0f) / this.PlaneDistance) * 2.0f) * Mathf.Rad2Deg;
		}
		#endregion // 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
