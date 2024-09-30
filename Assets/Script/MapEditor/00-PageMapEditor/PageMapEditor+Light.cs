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
		[Header("=====> Right Editor UIs - Light <=====")]
		[SerializeField] private STSliderInputUIs m_stRColorSliderInputUIs;
		[SerializeField] private STSliderInputUIs m_stGColorSliderInputUIs;
		[SerializeField] private STSliderInputUIs m_stBColorSliderInputUIs;
		[SerializeField] private STSliderInputUIs m_stIntensitySliderInputUIs;
		#endregion // 변수

		#region 함수
		/** 광원 UI 를 설정한다 */
		private void SetupREUIsLightUIs()
		{
			m_oREUIsEditorInputList.Add(m_stRColorSliderInputUIs.m_oInput);
			m_oREUIsEditorInputList.Add(m_stGColorSliderInputUIs.m_oInput);
			m_oREUIsEditorInputList.Add(m_stBColorSliderInputUIs.m_oInput);
			m_oREUIsEditorInputList.Add(m_stIntensitySliderInputUIs.m_oInput);
		}

		/** 기본 광원 값을 설정한다 */
		private void SetupDefLightValues(CMapInfo a_oMapInfo)
		{
			a_oMapInfo.m_bIsSetupDefValues = true;

			a_oMapInfo.m_stLightInfo.m_stColor = m_stDefLightColor;
			a_oMapInfo.m_stLightInfo.m_fIntensity = m_fDefLightIntensity;
		}

		/** 광원 리셋 버튼을 눌렀을 경우 */
		public void OnTouchREUIsResetLightBtn()
		{
			m_bIsDirtyUpdateUIsState = true;
			this.SetupDefLightValues(this.SelMapInfo);
		}

		/** 광원 색상을 변경했을 경우 */
		private void OnChangeREUIsLightColor(float a_fVal)
		{
			this.SetREUIsLightColor(m_stRColorSliderInputUIs.m_oSlider.value, m_stGColorSliderInputUIs.m_oSlider.value, m_stBColorSliderInputUIs.m_oSlider.value);
		}

		/** 광원 세가를 변경했을 경우 */
		private void OnChangeREUIsLightIntensity(float a_fVal)
		{
			this.SetREUIsLightIntensity(m_stIntensitySliderInputUIs.m_oSlider.value);
		}

		/** 광원 색상을 입력했을 경우 */
		private void OnEndInputREUIsLightColor(string a_oStr)
		{
			bool bIsValid01 = float.TryParse(m_stRColorSliderInputUIs.m_oInput.text, NumberStyles.Any, null, out float fR);
			bool bIsValid02 = float.TryParse(m_stGColorSliderInputUIs.m_oInput.text, NumberStyles.Any, null, out float fG);
			bool bIsValid03 = float.TryParse(m_stBColorSliderInputUIs.m_oInput.text, NumberStyles.Any, null, out float fB);

			this.SetREUIsLightColor(bIsValid01 ? fR : 0.0f, bIsValid02 ? fG : 0.0f, bIsValid03 ? fB : 0.0f);
		}

		/** 광원 세기를 입력했을 경우 */
		private void OnEndInputREUIsLightIntensity(string a_oStr)
		{
			bool bIsValid = float.TryParse(m_stIntensitySliderInputUIs.m_oInput.text, NumberStyles.Any, null, out float fIntensity);
			this.SetREUIsLightIntensity(bIsValid ? fIntensity : 0.0f);
		}

		/** 광원 색상을 변경한다 */
		private void SetREUIsLightColor(float a_fR, float a_fG, float a_fB, bool a_bIsUpdateUIsState = true)
		{
			float fR = Mathf.Clamp(a_fR, 0.0f, (float)byte.MaxValue);
			float fG = Mathf.Clamp(a_fG, 0.0f, (float)byte.MaxValue);
			float fB = Mathf.Clamp(a_fB, 0.0f, (float)byte.MaxValue);

			this.SelMapInfo.m_stLightInfo.m_stColor.m_fR = fR / (float)byte.MaxValue;
			this.SelMapInfo.m_stLightInfo.m_stColor.m_fG = fG / (float)byte.MaxValue;
			this.SelMapInfo.m_stLightInfo.m_stColor.m_fB = fB / (float)byte.MaxValue;
			this.SelMapInfo.m_stLightInfo.m_stColor.m_fA = (float)byte.MaxValue;

			m_stRColorSliderInputUIs.m_oSlider.SetValueWithoutNotify(this.SelMapInfo.m_stLightInfo.m_stColor.m_fR * byte.MaxValue);
			m_stGColorSliderInputUIs.m_oSlider.SetValueWithoutNotify(this.SelMapInfo.m_stLightInfo.m_stColor.m_fG * byte.MaxValue);
			m_stBColorSliderInputUIs.m_oSlider.SetValueWithoutNotify(this.SelMapInfo.m_stLightInfo.m_stColor.m_fB * byte.MaxValue);

			m_stRColorSliderInputUIs.m_oInput.SetTextWithoutNotify($"{(byte)(this.SelMapInfo.m_stLightInfo.m_stColor.m_fR * byte.MaxValue):0}");
			m_stGColorSliderInputUIs.m_oInput.SetTextWithoutNotify($"{(byte)(this.SelMapInfo.m_stLightInfo.m_stColor.m_fG * byte.MaxValue):0}");
			m_stBColorSliderInputUIs.m_oInput.SetTextWithoutNotify($"{(byte)(this.SelMapInfo.m_stLightInfo.m_stColor.m_fB * byte.MaxValue):0}");

			m_bIsDirtyUpdateUIsState = a_bIsUpdateUIsState;
		}

		/** 광원 세기를 변경한다 */
		private void SetREUIsLightIntensity(float a_fIntensity, bool a_bIsUpdateUIsState = true)
		{
			float fIntensity = Mathf.Clamp(a_fIntensity, m_stIntensitySliderInputUIs.m_oSlider.minValue, m_stIntensitySliderInputUIs.m_oSlider.maxValue);
			this.SelMapInfo.m_stLightInfo.m_fIntensity = fIntensity;

			m_stIntensitySliderInputUIs.m_oSlider.SetValueWithoutNotify(fIntensity);
			m_stIntensitySliderInputUIs.m_oInput.SetTextWithoutNotify($"{fIntensity:0.000}");

			m_bIsDirtyUpdateUIsState = a_bIsUpdateUIsState;
		}
		#endregion // 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
