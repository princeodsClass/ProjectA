using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_IOS && NOTI_MODULE_ENABLE
using Unity.Notifications.iOS;

/** 알림 관리자 - iOS */
public partial class CNotiManager : SingletonMono<CNotiManager>
{
	#region 함수
	/** 초기화 되었을 경우 */
	private void OnInitiOS(bool a_bIsInit)
	{
		this.IsInit = a_bIsInit;
		this.Params.m_oInitCallback?.Invoke(this, a_bIsInit);

		iOSNotificationCenter.OnNotificationReceived += this.OnReceiveiOSNotification;
	}

	/** 알림을 수신했을 경우 */
	private void OnReceiveiOSNotification(iOSNotification a_oNotification)
	{
		// 앱이 활성 상태 일 경우
		if (Application.isFocused)
		{
			iOSNotificationCenter.RemoveAllDeliveredNotifications();
		}
	}

	/** 유저 권한을 요청한다 */
	private IEnumerator CoRequestiOSUerPermission()
	{
		var eAuthorizationOptions = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;

		using (var oRequest = new AuthorizationRequest(eAuthorizationOptions, true))
		{
			do
			{
				yield return null;
			} while (!oRequest.IsFinished);

			this.OnInitiOS(oRequest.Granted);
			this.OnReceiveiOSNotification(null);
		}
	}
	#endregion // 함수
}
#endif // #if UNITY_IOS && NOTI_MODULE_ENABLE
