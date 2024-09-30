using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if APPS_FLYER_MODULE_ENABLE && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
using AppsFlyerSDK;
using UnityEngine.Purchasing;

/** 앱스 플라이어 관리자 */
public class CAppsFlyerManager : SingletonMono<CAppsFlyerManager>, IAppsFlyerConversionData
{
	/** 매개 변수 */
	public struct STParams
	{
		public string m_oAppID;
		public string m_oDevKey;
		public System.Action<CAppsFlyerManager, bool> m_oInitCallback;
	}

	#region 프로퍼티
	public STParams Params { get; private set; }
	public bool IsInit { get; private set; } = false;
	#endregion // 프로퍼티

	#region IAppsFlyerConversionData
	/** 데이터가 변환 되었을 경우 */
	public virtual void onConversionDataSuccess(string a_oConversion)
	{
		// Do Something
	}

	/** 데이터 변환에 실패했을 경우 */
	public virtual void onConversionDataFail(string a_oError)
	{
		// Do Something
	}

	/** 앱 실행 속성을 수신했을 경우 */
	public virtual void onAppOpenAttribution(string a_oAttribution)
	{
		// Do Something
	}

	/** 앱 실행 속성 수신에 실패했을 경우 */
	public virtual void onAppOpenAttributionFailure(string a_oError)
	{
		// Do Something
	}
	#endregion // IAppsFlyerConversionData

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		// 초기화 되었을 경우
		if (this.IsInit)
		{
			a_stParams.m_oInitCallback?.Invoke(this, this.IsInit);
		}
		else
		{
			this.Params = a_stParams;
			AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);

#if DEBUG || DEVELOPMENT_BUILD
			AppsFlyer.setIsDebug(true);
#else
			AppsFlyer.setIsDebug(false);
#endif // #if DEBUG || DEVELOPMENT_BUILD

#if UNITY_IOS
			AppsFlyer.initSDK(a_stParams.m_oDevKey, a_stParams.m_oAppID, this);
#elif UNITY_ANDROID
			AppsFlyer.initSDK(a_stParams.m_oDevKey, string.Empty, this);
#endif // #if UNITY_IOS

			this.ExLateCallFunc((a_oSender) => this.OnInit());
		}
	}

	// 초기화 되었을 경우
	private void OnInit()
	{
#if DEBUG || DEVELOPMENT_BUILD
		AppsFlyer.stopSDK(true);
#else
		AppsFlyer.startSDK();
#endif // #if DEBUG || DEVELOPMENT_BUILD

		this.IsInit = true;
		this.Params.m_oInitCallback?.Invoke(this, this.IsInit);
	}

	/** 결제 로그를 전송한다 */
	public void SendPurchaseLog(Product a_oProduct, int a_nNumProducts)
	{
		// 초기화가 안되었을 경우
		if (!this.IsInit)
		{
			return;
		}

#if !(DEBUG || DEVELOPMENT_BUILD)
		AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, new Dictionary<string, string>()
		{
			[AFInAppEvents.CONTENT_ID] = a_oProduct.definition.id,
			[AFInAppEvents.CURRENCY] = a_oProduct.metadata.isoCurrencyCode,
			[AFInAppEvents.QUANTITY] = $"{a_nNumProducts}",
			[AFInAppEvents.REVENUE] = $"{a_oProduct.metadata.localizedPrice}"
		});
#endif // #if !(DEBUG || DEVELOPMENT_BUILD)
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(string a_oAppID, string a_oDevKey, System.Action<CAppsFlyerManager, bool> a_oInitCallback)
	{
		return new STParams()
		{
			m_oAppID = a_oAppID,
			m_oDevKey = a_oDevKey,
			m_oInitCallback = a_oInitCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}
#endif // #if APPS_FLYER_MODULE_ENABLE && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
