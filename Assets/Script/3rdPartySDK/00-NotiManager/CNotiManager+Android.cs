using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID && NOTI_MODULE_ENABLE
using Unity.Notifications.Android;

/** 알림 관리자 - 안드로이드 */
public partial class CNotiManager : SingletonMono<CNotiManager>
{
	#region 함수
	/** 초기화 되었을 경우 */
	private void OnInitAndroid(bool a_bIsInit)
	{
		this.IsInit = a_bIsInit;
		this.Params.m_oInitCallback?.Invoke(this, a_bIsInit);

		AndroidNotificationCenter.OnNotificationReceived += this.OnReceiveAndroidNotification;
	}

	/** 알림을 수신했을 경우 */
	private void OnReceiveAndroidNotification(AndroidNotificationIntentData a_oData)
	{
		// 앱이 활성 상태 일 경우
		if (Application.isFocused)
		{
			AndroidNotificationCenter.CancelAllDisplayedNotifications();
		}
	}

	/** 유저 권한을 요청한다 */
	private IEnumerator CoRequestAndroidUerPermission()
	{
		var oRequest = new PermissionRequest();

		do
		{
			yield return null;
		} while (oRequest.Status == PermissionStatus.RequestPending);
		
		this.OnInitAndroid(oRequest.Status == PermissionStatus.Allowed);
		this.OnReceiveAndroidNotification(null);
	}
	#endregion // 함수
}
#endif // #if UNITY_ANDROID && NOTI_MODULE_ENABLE
