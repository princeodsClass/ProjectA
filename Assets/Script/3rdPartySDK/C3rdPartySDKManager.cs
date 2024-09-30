using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID
/** 서드 파티 SDK 관리자 */
public class C3rdPartySDKManager : SingletonMono<C3rdPartySDKManager>
{
	#region 변수
#if ADS_MODULE_ENABLE
	private bool m_bIsWatchRewardAds = false;
	private CAdsManager.STAdsRewardInfo m_stRewardAdsRewardInfo = CAdsManager.STAdsRewardInfo.INVALID;
	private System.Action<CAdsManager, CAdsManager.STAdsRewardInfo, bool> m_oRewardAdsCallback = null;
#endif // #if ADS_MODULE_ENABLE
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public virtual void Init()
	{
#if ADS_MODULE_ENABLE
		CAdsManager.Singleton.Init(CAdsManager.MakeParams(ComType.IRON_SRC_APP_KEY, ComType.IRON_SRC_ADS_ID_LIST, this.OnInitAdsManager));
#endif // #if ADS_MODULE_ENABLE

#if FACEBOOK_MODULE_ENABLE
		CFacebookManager.Singleton.Init(CFacebookManager.MakeParams(this.OnInitFacebookManager));
#endif // #if FACEBOOK_MODULE_ENABLE

#if FIREBASE_MODULE_ENABLE
		CFirebaseManager.Singleton.Init(CFirebaseManager.MakeParams(this.OnInitFirebaseManager));
#endif // #if FIREBASE_MODULE_ENABLE

#if APPS_FLYER_MODULE_ENABLE
		CAppsFlyerManager.Singleton.Init(CAppsFlyerManager.MakeParams(ComType.APPS_FLYER_APP_ID, ComType.APPS_FLYER_DEV_KEY, this.OnInitAppsFlyerManager));
#endif // #if APPS_FLYER_MODULE_ENABLE

#if NOTI_MODULE_ENABLE
		CNotiManager.Singleton.Init(CNotiManager.MakeParams(this.OnInitNotiManager));
#endif // #if NOTI_MODULE_ENABLE
	}

	/** 초기화 */
	public virtual void LateInit(Action callback)
	{
#if PURCHASE_MODULE_ENABLE
		CPurchaseManager.Singleton.Init(CPurchaseManager.MakeParams(GameManager.Singleton.GetIAPID(), (a_oSender, a_bIsSuccess) =>
		{
			this.OnInitPurchaseManager(a_oSender, a_bIsSuccess);
			callback?.Invoke();
        }));
#endif // #if PURCHASE_MODULE_ENABLE
	}

#if ADS_MODULE_ENABLE
	/** 보상 광고를 출력한다 */
	public void ShowRewardAds(System.Action<CAdsManager, CAdsManager.STAdsRewardInfo, bool> a_oCallback)
	{
		if (GameManager.Singleton.user.IsVIP())
		{
			a_oCallback?.Invoke(CAdsManager.Singleton, CAdsManager.STAdsRewardInfo.INVALID, true);
			return;
		}

		uint ticketKey = GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey");
		int ticketCount = GameManager.Singleton.invenMaterial.GetItemCount(ticketKey);

		if ( ticketCount > 0 ) // 티켓 보유하고 있으면
		{
			PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

			StartCoroutine(GameManager.Singleton.AddItemCS(ticketKey, -1, () =>
			{
				wait.Close();
				a_oCallback?.Invoke(CAdsManager.Singleton, CAdsManager.STAdsRewardInfo.INVALID, true);
			}));

			return;
		}

		// 보상 광고 출력이 가능 할 경우
		if (CAdsManager.Singleton.IsLoadRewardAds())
		{
			m_bIsWatchRewardAds = false;
			m_oRewardAdsCallback = a_oCallback;

#if DISABLE_THIS
			uint ticketKey = GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey");
			int ticketCount = GameManager.Singleton.invenMaterial.GetItemCount(ticketKey);

			if ( ticketCount > 0 ) // 티켓 보유하고 있으면
            {
				PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

				StartCoroutine(GameManager.Singleton.AddItemCS(ticketKey, -1, () =>
				{
					wait.Close();
					a_oCallback?.Invoke(CAdsManager.Singleton, CAdsManager.STAdsRewardInfo.INVALID, true);
				}));
			}
			else
            {
				CAdsManager.Singleton.ShowRewardAds(this.OnReceiveRewardAdsReward, null, this.OnCloseRewardAds);
            }
#else
			CAdsManager.Singleton.ShowRewardAds(this.OnReceiveRewardAdsReward, null, this.OnCloseRewardAds);
#endif // #if DISABLE_THIS
		}
		else
		{
			PopupSysMessage.ShowWatchRewardAdsFailMsg();
			a_oCallback?.Invoke(CAdsManager.Singleton, CAdsManager.STAdsRewardInfo.INVALID, false);
		}
	}

	/** 광고 관리자가 초기화 되었을 경우 */
	private void OnInitAdsManager(CAdsManager a_oSender, bool a_bIsSuccess)
	{
		GameManager.Log($"C3rdPartySDKManager.OnInitAdsManager: {a_bIsSuccess}");
		CAdsManager.Singleton.LoadRewardAds();
	}

	/** 보상 광고가 닫혔을 경우 */
	private void OnCloseRewardAds(CAdsManager a_oSender)
	{
		m_oRewardAdsCallback?.Invoke(a_oSender, m_stRewardAdsRewardInfo, m_bIsWatchRewardAds);
	}

	/** 보상 광고 보상을 수신했을 경우 */
	private void OnReceiveRewardAdsReward(CAdsManager a_oSender, CAdsManager.STAdsRewardInfo a_stRewardInfo, bool a_bIsSuccess)
	{
		m_bIsWatchRewardAds = a_bIsSuccess;
		m_stRewardAdsRewardInfo = a_stRewardInfo;
	}
#endif // #if ADS_MODULE_ENABLE

#if PURCHASE_MODULE_ENABLE
	/** 상품을 반환한다 */
	public Product GetProduct(string a_oID)
	{
		return CPurchaseManager.Singleton.GetProduct(a_oID);
	}

	/** 상품을 결제한다 */
	public void PurchaseProduct(string a_oID, System.Action<CPurchaseManager, string, bool, string> a_oCallback)
	{
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		CPurchaseManager.Singleton.PurchaseProduct(a_oID, (a_oSender, a_oProductID, a_bIsSuccess, a_oReceipt) =>
		{
			oWaitPopup.Close();
			a_oCallback?.Invoke(a_oSender, a_oProductID, a_bIsSuccess, a_oReceipt);
		});
	}

	/** 결제를 확정한다 */
	public void ConfirmPurchase(string a_oID, System.Action<CPurchaseManager, string, bool> a_oCallback = null)
	{
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		CPurchaseManager.Singleton.ConfirmPurchase(a_oID, (a_oSender, a_oProductID, a_bIsSuccess) =>
		{
			oWaitPopup.Close();
			a_oCallback?.Invoke(a_oSender, a_oProductID, a_bIsSuccess);
		});
	}

	/** 결제를 거부한다 */
	public void RejectPurchase(string a_oID, System.Action<CPurchaseManager, string, bool> a_oCallback = null)
	{
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		CPurchaseManager.Singleton.RejectPurchase(a_oID, (a_oSender, a_oProductID, a_bIsSuccess) =>
		{
			oWaitPopup.Close();
			a_oCallback?.Invoke(a_oSender, a_oProductID, a_bIsSuccess);
		});
	}

	/** 결제 관리자가 초기화 되었을 경우 */
	private void OnInitPurchaseManager(CPurchaseManager a_oSender, bool a_bIsSuccess)
	{
		GameManager.Log($"C3rdPartySDKManager.OnInitPurchaseManager: {a_bIsSuccess}");
	}
#endif // PURCHASE_MODULE_ENABLE

#if FACEBOOK_MODULE_ENABLE
	/** 페이스 북 관리자가 초기화 되었을 경우 */
	private void OnInitFacebookManager(CFacebookManager a_oSender, bool a_bIsSuccess)
	{
		GameManager.Log($"C3rdPartySDKManager.OnInitFacebookManager: {a_bIsSuccess}");
	}
#endif // #if FACEBOOK_MODULE_ENABLE

#if FIREBASE_MODULE_ENABLE
	/** 로그를 전송한다 */
	public void SendLog(string a_oLogName, Dictionary<string, string> a_oDataDict)
	{
		CFirebaseManager.Singleton.SendLog(a_oLogName, a_oDataDict);
	}

	/** 파이어 베이스 관리자가 초기화 되었을 경우 */
	private void OnInitFirebaseManager(CFirebaseManager a_oSender, bool a_bIsSuccess)
	{
		GameManager.Log($"C3rdPartySDKManager.OnInitFirebaseManager: {a_bIsSuccess}");
	}

	/** 파이어 베이스 메세지 토큰을 수신했을 경우 */
	private void OnLoadFirebaseMsgToken(CFirebaseManager a_oSender, string a_oMsgToken, bool a_bIsSuccess)
	{
		GameManager.Log($"C3rdPartySDKManager.OnLoadFirebaseMsgToken: {a_oMsgToken}, {a_bIsSuccess}");
	}
#endif // #if FIREBASE_MODULE_ENABLE

#if APPS_FLYER_MODULE_ENABLE
	/** 결제 로그를 전송한다 */
	public void SendPurchaseLog(Product a_oProduct, int a_nNumProducts = 1)
	{
		CAppsFlyerManager.Singleton.SendPurchaseLog(a_oProduct, a_nNumProducts);
	}

	/** 앱스 플라이어 관리자가 초기화 되었을 경우 */
	private void OnInitAppsFlyerManager(CAppsFlyerManager a_oSender, bool a_bIsSuccess)
	{
		GameManager.Log($"C3rdPartySDKManager.OnInitAppsFlyerManager: {a_bIsSuccess}");
	}
#endif // #if APPS_FLYER_MODULE_ENABLE

#if NOTI_MODULE_ENABLE
	/** 알림을 추가한다 */
	public int AddNoti(string a_oTitle, string a_oMsg, System.DateTime a_stFireTime, int a_nID = -1)
	{
		int nID = (a_nID < 0) ? (int)System.DateTime.Now.Ticks : a_nID;
		return CNotiManager.Singleton.AddNoti(a_oTitle, a_oMsg, a_stFireTime, nID);
	}

	/** 알림을 제거한다 */
	public void RemoveNoti(int a_nID)
	{
		CNotiManager.Singleton.RemoveNoti(a_nID);
	}

	/** 초기화 되었을 경우 */
	private void OnInitNotiManager(CNotiManager a_oSender, bool a_bIsSuccess)
	{
		GameManager.Log($"C3rdPartySDKManager.OnInitNotiManager: {a_bIsSuccess}");
	}
#endif // #if NOTI_MODULE_ENABLE
	#endregion // 함수

	#region 접근 함수
#if ADS_MODULE_ENABLE
	/** 보상 광고 가능 여부를 변경한다 */
	public void SetIsEnableRewardAds(bool a_bIsEnable)
	{
		CAdsManager.Singleton.SetIsEnableRewardAds(a_bIsEnable);
	}
#endif // #if ADS_MODULE_ENABLE
	#endregion // 접근 함수
}
#endif // #if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID
