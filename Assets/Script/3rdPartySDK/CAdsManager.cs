using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ADS_MODULE_ENABLE && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
/** 광고 관리자 */
public partial class CAdsManager : SingletonMono<CAdsManager>
{
	/** 광고 콜백 */
	private enum EAdsCallback
	{
		NONE = -1,
		CLOSE_REWARD_ADS,
		CLOSE_FULLSCREEN_ADS,
		REWARD_ADS_REWARD,
		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams
	{
		public string m_oAppKey;
		public List<string> m_oAdsIDList;
		public System.Action<CAdsManager, bool> m_oInitCallback;
	}

	/** 광고 보상 정보 */
	public struct STAdsRewardInfo
	{
		public string m_oID;
		public string m_oVal;

		#region 상수
		public static readonly STAdsRewardInfo INVALID = new STAdsRewardInfo()
		{
			m_oID = string.Empty,
			m_oVal = string.Empty
		};
		#endregion // 상수
	}

	#region 변수
	private Dictionary<EAdsCallback, System.Action<CAdsManager>> m_oCallbackDictContainer01 = new Dictionary<EAdsCallback, System.Action<CAdsManager>>()
	{
		[EAdsCallback.CLOSE_REWARD_ADS] = null,
		[EAdsCallback.CLOSE_FULLSCREEN_ADS] = null
	};

	private Dictionary<EAdsCallback, System.Action<CAdsManager, STAdsRewardInfo, bool>> m_oCallbackDictContainer02 = new Dictionary<EAdsCallback, System.Action<CAdsManager, STAdsRewardInfo, bool>>()
	{
		[EAdsCallback.REWARD_ADS_REWARD] = null
	};

	private bool m_bIsLoadIronSrcBannerAds = false;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }

	public bool IsInit { get; private set; } = false;
	public bool IsEnableBannerAds { get; private set; } = true;
	public bool IsEnableRewardAds { get; private set; } = true;
	public bool IsEnableFullscreenAds { get; private set; } = true;
	#endregion // 프로퍼티

	#region 함수
	/** 앱이 정지 되었을 경우 */
	public virtual void OnApplicationPause(bool a_bIsPause)
	{
		// 초기화가 안되었을 경우
		if (!this.IsInit)
		{
			return;
		}

		IronSource.Agent.onApplicationPause(a_bIsPause);
	}

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
			IronSourceEvents.onSdkInitializationCompletedEvent += this.OnInitIronSrc;

			IronSourceBannerEvents.onAdLoadedEvent += this.OnLoadIronSrcBannerAds;
			IronSourceBannerEvents.onAdLoadFailedEvent += this.OnLoadFailIronSrcBannerAds;

			IronSourceRewardedVideoEvents.onAdAvailableEvent += this.OnLoadIronSrcRewardAds;
			IronSourceRewardedVideoEvents.onAdLoadFailedEvent += this.OnLoadFailIronSrcRewardAds;
			IronSourceRewardedVideoEvents.onAdClosedEvent += this.OnCloseIronSrcRewardAds;
			IronSourceRewardedVideoEvents.onAdRewardedEvent += this.OnReceiveIronSrcAdsReward;

			IronSourceInterstitialEvents.onAdReadyEvent += this.OnLoadIronSrcFullscreenAds;
			IronSourceInterstitialEvents.onAdLoadFailedEvent += this.OnLoadFailIronSrcFullscreenAds;
			IronSourceInterstitialEvents.onAdClosedEvent += this.OnCloseIronSrcFullscreenAds;

			IronSource.Agent.init(a_stParams.m_oAppKey, a_stParams.m_oAdsIDList.ToArray());
		}
	}

	/** 배너 광고를 로드한다 */
	public void LoadBannerAds()
	{
		// 배너 광고 로드가 가능 할 경우
		if (this.IsInit && this.IsEnableBannerAds && !this.IsLoadBannerAds())
		{
			IronSource.Agent.loadBanner(new IronSourceBannerSize(320, 50), IronSourceBannerPosition.BOTTOM);
		}
	}

	/** 보상 광고를 로드한다 */
	public void LoadRewardAds()
	{
		// 보상 광고 로드가 가능 할 경우
		if (this.IsInit && this.IsEnableRewardAds && !this.IsLoadRewardAds())
		{
			GameManager.Log($"3rdPartySDKManager.LoadRewardAds");
			IronSource.Agent.loadRewardedVideo();
		}
	}

	/** 전면 광고를 로드한다 */
	public void LoadFullscreenAds()
	{
		// 전면 광고 로드가 가능 할 경우
		if (this.IsInit && this.IsEnableFullscreenAds && !this.IsLoadFullscreenAds())
		{
			IronSource.Agent.loadInterstitial();
		}
	}

	/** 배너 광고를 출력한다 */
	public void ShowBannerAds(System.Action<CAdsManager, bool> a_oCallback)
	{
		// 배너 광고가 로드 되었을 경우
		if (this.IsLoadBannerAds())
		{
			IronSource.Agent.displayBanner();
		}

		a_oCallback?.Invoke(this, this.IsLoadBannerAds());
	}

	/** 보상 광고를 출력한다 */
	public void ShowRewardAds(System.Action<CAdsManager, STAdsRewardInfo, bool> a_oCallback, System.Action<CAdsManager, bool> a_oShowCallback = null, System.Action<CAdsManager> a_oCloseCallback = null)
	{
		// 보상 광고가 로드 되었을 경우
		if (this.IsLoadRewardAds())
		{
			m_oCallbackDictContainer01[EAdsCallback.CLOSE_REWARD_ADS] = a_oCloseCallback;
			m_oCallbackDictContainer02[EAdsCallback.REWARD_ADS_REWARD] = a_oCallback;

			IronSource.Agent.showRewardedVideo();
		}
		else
		{
			a_oCallback?.Invoke(this, STAdsRewardInfo.INVALID, false);
		}

		a_oShowCallback?.Invoke(this, this.IsLoadRewardAds());
	}

	/** 전면 광고를 출력한다 */
	public void ShowFullscreenAds(System.Action<CAdsManager, bool> a_oCallback, System.Action<CAdsManager> a_oCloseCallback = null)
	{
		// 전면 광고가 로드 되었을 경우
		if (this.IsLoadFullscreenAds())
		{
			m_oCallbackDictContainer01[EAdsCallback.CLOSE_FULLSCREEN_ADS] = a_oCloseCallback;
			IronSource.Agent.showInterstitial();
		}

		a_oCallback?.Invoke(this, this.IsLoadFullscreenAds());
	}

	/** 배너 광고를 닫는다 */
	public void CloseBannerAds(System.Action<CAdsManager, bool> a_oCallback)
	{
		// 배너 광고 닫기가 가능 할 경우
		if (this.IsLoadBannerAds())
		{
			IronSource.Agent.hideBanner();
			IronSource.Agent.destroyBanner();
		}

		a_oCallback?.Invoke(this, this.IsLoadBannerAds());
	}

	/** 아이언 소스가 초기화 되었을 경우 */
	private void OnInitIronSrc()
	{
#if DEBUG || DEVELOPMENT_BUILD
		IronSource.Agent.setAdaptersDebug(true);
		IronSource.Agent.validateIntegration();
#endif // #if DEBUG || DEVELOPMENT_BUILD

		this.IsInit = true;
		this.Params.m_oInitCallback?.Invoke(this, true);

		StartCoroutine(this.CoUpdateStateAds());
	}

	/** 아이언 소스 배너 광고가 로드 되었을 경우 */
	private void OnLoadIronSrcBannerAds(IronSourceAdInfo a_oAdsInfo)
	{
		m_bIsLoadIronSrcBannerAds = true;
		this.HandleLoadIronSrcBannerAdsResult();
	}

	/** 아이언 소스 배너 광고 로드에 실패했을 경우 */
	private void OnLoadFailIronSrcBannerAds(IronSourceError a_oError)
	{
		this.ExLateCallFunc((a_oSender) => this.LoadBannerAds(), 5.0f);
	}

	/** 아이언 소스 보상 광고가 로드 되었을 경우 */
	private void OnLoadIronSrcRewardAds(IronSourceAdInfo a_oAdsInfo)
	{
		GameManager.Log($"OnLoadIronSrcRewardAds");
	}

	/** 아이언 소스 보상 광고 로드에 실패했을 경우 */
	private void OnLoadFailIronSrcRewardAds(IronSourceError a_oError)
	{
		GameManager.Log($"OnLoadFailIronSrcRewardAds: {a_oError.getDescription()}");
		this.ExLateCallFuncRealtime((a_oSender) => this.LoadRewardAds(), 5.0f);
	}

	/** 아이언 소스 보상 광고가 닫혔을 경우 */
	private void OnCloseIronSrcRewardAds(IronSourceAdInfo a_oAdsInfo)
	{
		this.ExLateCallFuncRealtime((a_oSender) =>
		{
			this.LoadRewardAds();
			m_oCallbackDictContainer01[EAdsCallback.CLOSE_REWARD_ADS]?.Invoke(this);
		}, 0.5f);
	}

	/** 아이언 소스 광고 보상을 수신했을 경우 */
	private void OnReceiveIronSrcAdsReward(IronSourcePlacement a_oPlacement, IronSourceAdInfo a_oAdsInfo)
	{
		m_oCallbackDictContainer02[EAdsCallback.REWARD_ADS_REWARD]?.Invoke(this, (a_oPlacement != null) ? new STAdsRewardInfo()
		{
			m_oID = a_oPlacement.getRewardName().ExIsValid() ? a_oPlacement.getRewardName() : string.Empty,
			m_oVal = $"{a_oPlacement.getRewardAmount()}"
		} : STAdsRewardInfo.INVALID, true);
	}

	/** 아이언 소스 전면 광고가 로드 되었을 경우 */
	private void OnLoadIronSrcFullscreenAds(IronSourceAdInfo a_oAdsInfo)
	{
		// Do Something
	}

	/** 아이언 소스 전면 광고 로드에 실패했을 경우 */
	private void OnLoadFailIronSrcFullscreenAds(IronSourceError a_oError)
	{
		this.ExLateCallFunc((a_oSender) => this.LoadFullscreenAds(), 5.0f);
	}

	/** 아이언 소스 전면 광고가 닫혔을 경우 */
	private void OnCloseIronSrcFullscreenAds(IronSourceAdInfo a_oAdsInfo)
	{
		this.ExLateCallFunc((a_oSender) => this.LoadFullscreenAds());
		m_oCallbackDictContainer01[EAdsCallback.CLOSE_FULLSCREEN_ADS]?.Invoke(this);
	}

	/** 아이언 소스 배너 광고 로드 결과를 처리한다 */
	private void HandleLoadIronSrcBannerAdsResult()
	{
		this.ExLateCallFunc((a_oSender) => this.ShowBannerAds(null));
	}
	#endregion // 함수

	#region 접근 함수
	/** 배너 광고 로드 여부를 검사한다 */
	public bool IsLoadBannerAds()
	{
		return this.IsInit && this.IsEnableBannerAds && m_bIsLoadIronSrcBannerAds;
	}

	/** 보상 광고 로드 여부를 검사한다 */
	public bool IsLoadRewardAds()
	{
		return this.IsInit && this.IsEnableRewardAds && IronSource.Agent.isRewardedVideoAvailable();
	}

	/** 전면 광고 로드 여부를 검사한다 */
	public bool IsLoadFullscreenAds()
	{
		return this.IsInit && this.IsEnableFullscreenAds && IronSource.Agent.isInterstitialReady();
	}

	/** 배너 광고 가능 여부를 변경한다 */
	public void SetIsEnableBannerAds(bool a_bIsEnable)
	{
		this.IsEnableBannerAds = a_bIsEnable;
	}

	/** 보상 광고 가능 여부를 변경한다 */
	public void SetIsEnableRewardAds(bool a_bIsEnable)
	{
		this.IsEnableRewardAds = a_bIsEnable;
	}

	/** 전면 광고 가능 여부를 변경한다 */
	public void SetIsEnableFullscreenAds(bool a_bIsEnable)
	{
		this.IsEnableFullscreenAds = a_bIsEnable;
	}
	#endregion // 접근 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(string a_oAppKey, List<string> a_oAdsIDList, System.Action<CAdsManager, bool> a_oInitCallback)
	{
		return new STParams()
		{
			m_oAppKey = a_oAppKey,
			m_oAdsIDList = a_oAdsIDList,
			m_oInitCallback = a_oInitCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}

/** 광고 관리자 - 코루틴 */
public partial class CAdsManager : SingletonMono<CAdsManager>
{
	#region 함수
	/** 광고 상태를 갱신한다 */
	private IEnumerator CoUpdateStateAds()
	{
		var oWaitForSeconds = new WaitForSeconds(5.0f);

		do
		{
			yield return oWaitForSeconds;

			// 보상 광고 로드가 필요 할 경우
			if (!this.IsLoadRewardAds())
			{
				this.LoadRewardAds();
			}
		} while (this.gameObject.activeSelf);
	}
	#endregion // 함수
}
#endif // #if ADS_MODULE_ENABLE && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
