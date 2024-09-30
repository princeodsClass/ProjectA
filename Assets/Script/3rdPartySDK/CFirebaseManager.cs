using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_MODULE_ENABLE
using System.Threading.Tasks;

using Firebase;
using Firebase.Analytics;
using Firebase.Messaging;

/** 파이어 베이스 관리자 */
public partial class CFirebaseManager : SingletonMono<CFirebaseManager>
{
	/** 매개 변수 */
	public struct STParams
	{
		public System.Action<CFirebaseManager, bool> m_oInitCallback;
	}

	#region 변수
	private FirebaseApp m_oFirebaseApp = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }

	public bool IsInit { get; private set; } = false;
	public string MsgToken { get; private set; } = string.Empty;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual async void Init(STParams a_stParams)
	{
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.IsInit) {
			a_stParams.m_oInitCallback?.Invoke(this, this.IsInit);
		} else {
			this.Params = a_stParams;
			
			var oAsyncTask = FirebaseApp.CheckAndFixDependenciesAsync();
			await oAsyncTask;

			this.OnInit(oAsyncTask);
		}
#else
		await Task.Yield();
		a_stParams.m_oInitCallback?.Invoke(this, false);
#endif // #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
	}

	// 로그를 전송한다
	public void SendLog(string a_oName, Dictionary<string, string> a_oDataDict)
	{
		// 초기화가 필요 할 경우
		if (!this.IsInit)
		{
			return;
		}

#if UNITY_IOS || UNITY_ANDROID
		var oDataDict = a_oDataDict ?? new Dictionary<string, string>();
		oDataDict.TryAdd("Platform", Application.platform.ToString());

		FirebaseAnalytics.LogEvent(a_oName, this.MakeLogParams(oDataDict).ToArray());
#endif // #if UNITY_IOS || UNITY_ANDROID
	}

#if UNITY_IOS || UNITY_ANDROID
	// 초기화 되었을 경우
	private void OnInit(Task<DependencyStatus> a_oTask)
	{
		this.IsInit = a_oTask.ExIsCompleteSuccess() && a_oTask.Result == DependencyStatus.Available;
		string oErrorMsg = (a_oTask.Exception != null) ? a_oTask.Exception.Message : string.Empty;

		// 초기화 되었을 경우
		if (this.IsInit)
		{
			m_oFirebaseApp = FirebaseApp.DefaultInstance;
			FirebaseAnalytics.SetSessionTimeoutDuration(new System.TimeSpan(0, 0, 60));

#if DEBUG || DEVELOPMENT_BUILD
			FirebaseAnalytics.SetAnalyticsCollectionEnabled(false);
#else
			FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
#endif // #if DEBUG || DEVELOPMENT_BUILD

			FirebaseMessaging.TokenRegistrationOnInitEnabled = true;

			FirebaseMessaging.TokenReceived += this.OnReceiveMsgToken;
			FirebaseMessaging.MessageReceived += this.OnReceiveNotiMsg;

			this.SendLog("Launch", null);
		}

		this.Params.m_oInitCallback?.Invoke(this, this.IsInit);
	}

	/** 메세지 토큰을 수신했을 경우 */
	private void OnReceiveMsgToken(object a_oSender, TokenReceivedEventArgs a_oArgs)
	{
		this.MsgToken = a_oArgs.Token;
		GameManager.Log($"CFirebaseManager.OnReceiveMsgToken: {a_oArgs.Token}");
	}

	/** 알림 메세지를 수신했을 경우 */
	private void OnReceiveNotiMsg(object a_oSender, MessageReceivedEventArgs a_oArgs)
	{
		// Do Something
	}
#endif // #if UNITY_IOS || UNITY_ANDROID
	#endregion // 함수

	#region 팩토리 함수
#if UNITY_IOS || UNITY_ANDROID
	/** 로그 매개 변수를 생성한다 */
	private List<Parameter> MakeLogParams(Dictionary<string, string> a_oDataDict)
	{
		var oLogParamsList = new List<Parameter>();

		foreach (var stKeyVal in a_oDataDict)
		{
			oLogParamsList.ExAddVal(new Parameter(stKeyVal.Key, stKeyVal.Value));
		}

		return oLogParamsList;
	}
#endif // #if UNITY_IOS || UNITY_ANDROID
	#endregion // 팩토리 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(System.Action<CFirebaseManager, bool> a_oInitCallback)
	{
		return new STParams()
		{
			m_oInitCallback = a_oInitCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}
#endif // #if FIREBASE_MODULE_ENABLE
