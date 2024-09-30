using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 약관 동의 팝업 */
public partial class PopupAgree : UIDialog
{
	/** 매개 변수 */
	public struct STParams
	{
		public System.Action<PopupAgree, bool> m_oCallback;
	}

	#region 상수
	public static readonly List<string> B_EU_COUNTRY_CODE_LIST = new List<string>() {
		"BE", "BG", "CZ", "DK", "DE", "EE", "IE", "GR", "ES", "FR", "HR", "IT", "CY", "LV", "LT", "LU", "HU", "MT", "NL", "AT", "PL", "PT", "RO", "SI", "SK", "FI", "SE"
	};
	#endregion // 상수

	#region 변수
	private bool m_bIsAgreePrivacy = false;
	private bool m_bIsAgreeServices = false;

	private string m_oCountryCode = string.Empty;

	[SerializeField] private TextAsset m_oPrivacyTextAsset = null;
	[SerializeField] private TextAsset m_oServicesTextAsset = null;

	[Header("=====> UIs <=====")]
	[SerializeField] private Text m_oPrivacyText = null;
	[SerializeField] private Text m_oServicesText = null;

	[SerializeField] private Image m_oBGImg = null;
	[SerializeField] private Image m_oPrivacyCheckImg = null;
	[SerializeField] private Image m_oServicesCheckImg = null;

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oKRUIs = null;
	[SerializeField] private GameObject m_oEUUIs = null;
	[SerializeField] private GameObject m_oEUUIsDescUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	private void Awake()
	{
		Initialize();
		m_oCountryCode = this.GetCountryCode();
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		this.UpdateUIsState();

		var oTransforms = m_oEUUIsDescUIs.GetComponentsInChildren<RectTransform>();

		for (int i = 0; i < oTransforms.Length; ++i)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(oTransforms[i] as RectTransform);
		}

		// 약관 동의가 필요 없을 경우
		if (!this.IsKR(m_oCountryCode) && !this.IsEU(m_oCountryCode))
		{
			this.HandleAgreeState();
		}
	}

	/** 백 키를 처리한다 */
	public override void Escape()
	{
		// Do Something
	}

	/** UI 상태를 갱신한다 */
	private void UpdateUIsState()
	{
		this.UpdateKRUIsState();
		this.UpdateEUUIsState();

		// 객체를 갱신한다
		m_oKRUIs.gameObject.SetActive(this.IsKR(m_oCountryCode));
		m_oEUUIs.gameObject.SetActive(this.IsEU(m_oCountryCode));

		// 이미지를 갱신한다
		m_oBGImg.gameObject.SetActive(this.IsKR(m_oCountryCode) || this.IsEU(m_oCountryCode));

		// 약관에 동의했을 경우
		if (m_bIsAgreePrivacy && m_bIsAgreeServices)
		{
			this.HandleAgreeState();
		}
	}

	/** 약관 동의 상태를 처리한다 */
	private void HandleAgreeState()
	{
		this.ExLateCallFunc((a_oSender) => this.Params.m_oCallback?.Invoke(this, true), 0.15f);
	}
	#endregion // 함수

	#region 접근 함수
	/** 한국 여부를 검사한다 */
	public bool IsKR(string a_oCountryCode)
	{
		return a_oCountryCode.ToUpper().Equals("KR");
	}

	/** 유럽 연합 여부를 검사한다 */
	public bool IsEU(string a_oCountryCode)
	{
		return PopupAgree.B_EU_COUNTRY_CODE_LIST.Contains(a_oCountryCode.ToUpper());
	}

	/** 국가 코드를 반환한다 */
	private string GetCountryCode()
	{
#if !UNITY_EDITOR && UNITY_IOS
		// TODO: iOS 포팅 시 국가 코드 관련 기능 구현 필요
		return "US";
#elif !UNITY_EDITOR && UNITY_ANDROID
		var oJavaClass = new AndroidJavaClass("java.util.Locale");
		var oLocaleObject = oJavaClass.CallStatic<AndroidJavaObject>("getDefault");

		return oLocaleObject.Call<string>("getCountry");
#else
		return (Application.systemLanguage == SystemLanguage.Korean) ? "KR" : "US";
#endif // #if !UNITY_EDITOR && UNITY_IOS
	}
	#endregion // 접근 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(System.Action<PopupAgree, bool> a_oCallback)
	{
		return new STParams()
		{
			m_oCallback = a_oCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}

/** 약관 동의 팝업 - 한국 */
public partial class PopupAgree : UIDialog
{
	#region 함수
	/** 한국 UI 개인 정보 버튼을 눌렀을 경우 */
	public void OnTouchKRUIsPrivacyBtn()
	{
		m_bIsAgreePrivacy = !m_bIsAgreePrivacy;
		this.UpdateUIsState();
	}

	/** 한국 UI 서비스 버튼을 눌렀을 경우 */
	public void OnTouchKRUIsServicesBtn()
	{
		m_bIsAgreeServices = !m_bIsAgreeServices;
		this.UpdateUIsState();
	}

	/** UI 상태를 갱신한다 */
	private void UpdateKRUIsState()
	{
		m_oPrivacyText.text = m_oPrivacyTextAsset.text;
		m_oServicesText.text = m_oServicesTextAsset.text;

		m_oPrivacyCheckImg.gameObject.SetActive(m_bIsAgreePrivacy);
		m_oServicesCheckImg.gameObject.SetActive(m_bIsAgreeServices);
	}
	#endregion // 함수
}

/** 약관 동의 팝업 - 유럽 연합 */
public partial class PopupAgree : UIDialog
{
	#region 함수
	/** 확인 버튼을 눌렀을 경우 */
	public void OnTouchEUUIsOKBtn()
	{
		this.HandleAgreeState();
	}

	/** 개인 정보 URL 버튼을 눌렀을 경우 */
	public void OnTouchEUUIsPrivacyBtn()
	{
		Application.OpenURL("https://www.ninetap.com/privacy_policy.html");
	}

	/** 서비스 URL 버튼을 눌렀을 경우 */
	public void OnTouchEUUIsServicesBtn()
	{
		Application.OpenURL("https://www.ninetap.com/terms_of_service.html");
	}

	/** 유럽 연합 UI 상태를 갱신한다 */
	private void UpdateEUUIsState()
	{
		// Do Something
	}
	#endregion // 함수
}
