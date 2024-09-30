using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FACEBOOK_MODULE_ENABLE && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
using Facebook.Unity;

/** 페이스 북 관리자 */
public class CFacebookManager : SingletonMono<CFacebookManager>
{
	/** 매개 변수 */
	public struct STParams
	{
		public System.Action<CFacebookManager, bool> m_oInitCallback;
	}

	#region 프로퍼티
	public STParams Params { get; private set; }

	public bool IsLogin
	{
		get
		{
			// 초기화 되었을 경우
			if (this.IsInit)
			{
				var oToken = Facebook.Unity.AccessToken.CurrentAccessToken;
				var stExpirationTime = (oToken != null) ? oToken.ExpirationTime : System.DateTime.Now;

				return stExpirationTime.ExGetDeltaTimePerDays(System.DateTime.Now).ExIsGreat(0.0f);
			}

			return false;
		}
	}

	public bool IsInit => FB.IsInitialized;
	public string UserID => this.IsLogin ? Facebook.Unity.AccessToken.CurrentAccessToken.UserId : string.Empty;
	public string AccessToken => this.IsLogin ? Facebook.Unity.AccessToken.CurrentAccessToken.TokenString : string.Empty;
	#endregion // 프로퍼티

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
			FB.Init(this.OnInit, this.OnChangeViewState);
		}
	}

	// 초기화 되었을 경우
	private void OnInit()
	{
		FB.Mobile.SetAutoLogAppEventsEnabled(false);

#if DEBUG || DEVELOPMENT_BUILD
		FB.Mobile.SetAdvertiserTrackingEnabled(false);
		FB.Mobile.SetAdvertiserIDCollectionEnabled(false);
#else
		FB.Mobile.SetAdvertiserTrackingEnabled(true);
		FB.Mobile.SetAdvertiserIDCollectionEnabled(true);
#endif // #if DEBUG || DEVELOPMENT_BUILD

		FB.ActivateApp();
		this.Params.m_oInitCallback?.Invoke(this, this.IsInit);
	}

	/** 뷰 상태가 변경 되었을 경우 */
	private void OnChangeViewState(bool a_bIsShow)
	{
		// Do Something
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(System.Action<CFacebookManager, bool> a_oInitCallback)
	{
		return new STParams()
		{
			m_oInitCallback = a_oInitCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}
#endif // #if FACEBOOK_MODULE_ENABLE && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
