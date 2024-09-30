using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if NOTI_MODULE_ENABLE && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
#if UNITY_IOS
using Unity.Notifications.iOS;
#elif UNITY_ANDROID
using UnityEngine.Android;
using Unity.Notifications.Android;
#endif // #if UNITY_IOS

/** 알림 관리자 */
public partial class CNotiManager : SingletonMono<CNotiManager>
{
	/** 매개 변수 */
	public struct STParams
	{
		public System.Action<CNotiManager, bool> m_oInitCallback;
	}

	#region 프로퍼티
	public STParams Params { get; private set; }
	public bool IsInit { get; private set; } = false;
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

#if UNITY_IOS
			StartCoroutine(this.CoRequestiOSUerPermission());
#elif UNITY_ANDROID
			var stChannel = new AndroidNotificationChannel(Application.identifier, Application.productName, Application.identifier, Importance.Default);
			AndroidNotificationCenter.RegisterNotificationChannel(stChannel);

			StartCoroutine(this.CoRequestAndroidUerPermission());
#endif // #if UNITY_IOS
		}
	}

	/** 앱이 정지 되었을 경우 */
	public virtual void OnApplicationPause(bool a_bIsPause)
	{
		// 일시 정지 상태 일 경우
		if (a_bIsPause || !this.IsInit)
		{
			return;
		}

#if UNITY_IOS
		this.OnReceiveiOSNotification(null);
#elif UNITY_ANDROID
		this.OnReceiveAndroidNotification(null);
#endif // #if UNITY_IOS
	}

	/** 알림을 추가한다 */
	public int AddNoti(string a_oTitle, string a_oMsg, System.DateTime a_stFireTime, int a_nID)
	{
		// 초기화 되지 않았을 경우
		if (!this.IsInit)
		{
			return -1;
		}

#if UNITY_IOS
		var oTrigger = new iOSNotificationCalendarTrigger();
		oTrigger.Year = a_stFireTime.Year;
		oTrigger.Month = a_stFireTime.Month;
		oTrigger.Day = a_stFireTime.Day;
		oTrigger.Hour = a_stFireTime.Hour;
		oTrigger.Minute = a_stFireTime.Minute;
		oTrigger.Second = a_stFireTime.Second;

		var oNotification = new iOSNotification($"{a_nID}");
		oNotification.Title = a_oTitle;
		oNotification.Body = a_oMsg;
		oNotification.Trigger = oTrigger;
		oNotification.CategoryIdentifier = Application.identifier;
		oNotification.ThreadIdentifier = $"{System.Threading.Thread.CurrentThread.ManagedThreadId}";

		iOSNotificationCenter.ScheduleNotification(oNotification);

#elif UNITY_ANDROID
		var oNotification = new AndroidNotification(a_oTitle, a_oMsg, a_stFireTime);
		oNotification.Group = Application.identifier;
		oNotification.SmallIcon = "icon_0";
		
		AndroidNotificationCenter.SendNotificationWithExplicitID(oNotification, Application.identifier, a_nID);
#endif // #if UNITY_IOS

		return a_nID;
	}

	/** 알림을 제거한다 */
	public void RemoveNoti(int a_nID)
	{
		// 초기화 되지 않았을 경우
		if (!this.IsInit)
		{
			return;
		}

#if UNITY_IOS
		iOSNotificationCenter.RemoveDeliveredNotification($"{a_nID}");
		iOSNotificationCenter.RemoveScheduledNotification($"{a_nID}");
#elif UNITY_ANDROID
		AndroidNotificationCenter.CancelNotification(a_nID);
		AndroidNotificationCenter.CancelDisplayedNotification(a_nID);
		AndroidNotificationCenter.CancelScheduledNotification(a_nID);
#endif // #if UNITY_IOS
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(System.Action<CNotiManager, bool> a_oInitCallback)
	{
		return new STParams()
		{
			m_oInitCallback = a_oInitCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}
#endif // #if NOTI_MODULE_ENABLE && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
