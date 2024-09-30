using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
using System.Globalization;

namespace MapEditor
{
	/** 맵 에디터 페이지 - 카메라 */
	public partial class PageMapEditor : UIDialog
	{
		#region 변수
		[Header("=====> Right Editor UIs - Camera <=====")]
		[SerializeField] private STSliderInputUIs m_stCameraFOVSliderInputUIs;
		[SerializeField] private STSliderInputUIs m_stCameraHeightSliderInputUIs;
		[SerializeField] private STSliderInputUIs m_stCameraForwardSliderInputUIs;
		[SerializeField] private STSliderInputUIs m_stCameraDistanceSliderInputUIs;
		[SerializeField] private STSliderInputUIs m_stCameraSmoothTimeSliderInputUIs;
		#endregion // 변수

		#region 함수
		/** 카메라 UI 를 설정한다 */
		private void SetupREUIsCameraUIs()
		{
			m_oREUIsEditorInputList.Add(m_stCameraFOVSliderInputUIs.m_oInput);
			m_oREUIsEditorInputList.Add(m_stCameraHeightSliderInputUIs.m_oInput);
			m_oREUIsEditorInputList.Add(m_stCameraForwardSliderInputUIs.m_oInput);
			m_oREUIsEditorInputList.Add(m_stCameraDistanceSliderInputUIs.m_oInput);
			m_oREUIsEditorInputList.Add(m_stCameraSmoothTimeSliderInputUIs.m_oInput);
		}

		/** 카메라 리셋 버튼을 눌렀을 경우 */
		public void OnTouchREUIsResetCameraBtn()
		{
			m_bIsDirtyUpdateUIsState = true;
			this.SetupDefCameraValues(this.SelMapInfo);
		}

		/** 기본 카메라 값을 설정한다 */
		private void SetupDefCameraValues(CMapInfo a_oMapInfo)
		{
			a_oMapInfo.m_bIsSetupDefCameraValues = true;
			a_oMapInfo.m_stCameraInfo = new STCameraInfo(35.0f, 25.0f, -30.0f, 4.0f, 0.5f);
		}

		/** 카메라 값을 변경 되었을 경우 */
		private void OnChangeREUIsCameraVal(float a_fVal)
		{
			this.SetREUIsCameraVal(m_stCameraFOVSliderInputUIs.m_oSlider.value,
				m_stCameraHeightSliderInputUIs.m_oSlider.value, m_stCameraForwardSliderInputUIs.m_oSlider.value, m_stCameraDistanceSliderInputUIs.m_oSlider.value, m_stCameraSmoothTimeSliderInputUIs.m_oSlider.value);
		}

		/** 카메라 값을 입력했을 경우 */
		private void OnEndInputREUIsCameraVal(string a_oStr)
		{
			bool bIsValid01 = float.TryParse(m_stCameraFOVSliderInputUIs.m_oInput.text, NumberStyles.Any, null, out float fFOV);
			bool bIsValid02 = float.TryParse(m_stCameraHeightSliderInputUIs.m_oInput.text, NumberStyles.Any, null, out float fHeight);
			bool bIsValid03 = float.TryParse(m_stCameraForwardSliderInputUIs.m_oInput.text, NumberStyles.Any, null, out float fForward);
			bool bIsValid04 = float.TryParse(m_stCameraDistanceSliderInputUIs.m_oInput.text, NumberStyles.Any, null, out float fDistance);
			bool bIsValid05 = float.TryParse(m_stCameraSmoothTimeSliderInputUIs.m_oInput.text, NumberStyles.Any, null, out float fSmoothTime);

			this.SetREUIsCameraVal(bIsValid01 ? fFOV : 35.0f, 
				bIsValid02 ? fHeight : 25.0f, bIsValid03 ? fForward : -30.0f, bIsValid04 ? fDistance : 4.0f, bIsValid05 ? fSmoothTime : 5.0f);
		}

		/** 카메라 값을 변경한다 */
		private void SetREUIsCameraVal(float a_fFOV, float a_fHeight, float a_fForward, float a_fDistance, float a_fSmoothTime, bool a_bIsUpdateUIsState = true)
		{
			float fFOV = Mathf.Clamp(a_fFOV, m_stCameraFOVSliderInputUIs.m_oSlider.minValue, m_stCameraFOVSliderInputUIs.m_oSlider.maxValue);
			float fHeight = Mathf.Clamp(a_fHeight, m_stCameraHeightSliderInputUIs.m_oSlider.minValue, m_stCameraHeightSliderInputUIs.m_oSlider.maxValue);
			float fForward = Mathf.Clamp(a_fForward, m_stCameraForwardSliderInputUIs.m_oSlider.minValue, m_stCameraForwardSliderInputUIs.m_oSlider.maxValue);
			float fDistance = Mathf.Clamp(a_fDistance, m_stCameraDistanceSliderInputUIs.m_oSlider.minValue, m_stCameraDistanceSliderInputUIs.m_oSlider.maxValue);
			float fSmoothTime = Mathf.Clamp(a_fSmoothTime, m_stCameraSmoothTimeSliderInputUIs.m_oSlider.minValue, m_stCameraSmoothTimeSliderInputUIs.m_oSlider.maxValue);

			this.SelMapInfo.m_stCameraInfo.m_fFOV = fFOV;
			this.SelMapInfo.m_stCameraInfo.m_fHeight = fHeight;
			this.SelMapInfo.m_stCameraInfo.m_fForward = fForward;
			this.SelMapInfo.m_stCameraInfo.m_fDistance = fDistance;
			this.SelMapInfo.m_stCameraInfo.m_fSmoothTime = fSmoothTime;

			m_stCameraFOVSliderInputUIs.m_oSlider.SetValueWithoutNotify(this.SelMapInfo.m_stCameraInfo.m_fFOV);
			m_stCameraHeightSliderInputUIs.m_oSlider.SetValueWithoutNotify(this.SelMapInfo.m_stCameraInfo.m_fHeight);
			m_stCameraForwardSliderInputUIs.m_oSlider.SetValueWithoutNotify(this.SelMapInfo.m_stCameraInfo.m_fForward);
			m_stCameraDistanceSliderInputUIs.m_oSlider.SetValueWithoutNotify(this.SelMapInfo.m_stCameraInfo.m_fDistance);
			m_stCameraSmoothTimeSliderInputUIs.m_oSlider.SetValueWithoutNotify(this.SelMapInfo.m_stCameraInfo.m_fSmoothTime);

			m_stCameraFOVSliderInputUIs.m_oInput.SetTextWithoutNotify($"{this.SelMapInfo.m_stCameraInfo.m_fFOV:0.00}");
			m_stCameraHeightSliderInputUIs.m_oInput.SetTextWithoutNotify($"{this.SelMapInfo.m_stCameraInfo.m_fHeight:0.00}");
			m_stCameraForwardSliderInputUIs.m_oInput.SetTextWithoutNotify($"{this.SelMapInfo.m_stCameraInfo.m_fForward:0.00}");
			m_stCameraDistanceSliderInputUIs.m_oInput.SetTextWithoutNotify($"{this.SelMapInfo.m_stCameraInfo.m_fDistance:0.00}");
			m_stCameraSmoothTimeSliderInputUIs.m_oInput.SetTextWithoutNotify($"{this.SelMapInfo.m_stCameraInfo.m_fSmoothTime:0.00}");

			m_bIsDirtyUpdateUIsState = a_bIsUpdateUIsState;
		}
		#endregion // 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
