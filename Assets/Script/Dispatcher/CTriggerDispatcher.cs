using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 충돌 전달자 */
public partial class CTriggerDispatcher : MonoBehaviour
{
	#region 프로퍼티
	public System.Action<CTriggerDispatcher, Collider> EnterCallback { get; private set; } = null;
	public System.Action<CTriggerDispatcher, Collider> StayCallback { get; private set; } = null;
	public System.Action<CTriggerDispatcher, Collider> ExitCallback { get; private set; } = null;
	#endregion // 프로퍼티

	#region 함수
	/** 충돌이 시작 되었을 경우 */
	public void OnTriggerEnter(Collider a_oCollider)
	{
		this.EnterCallback?.Invoke(this, a_oCollider);
	}

	/** 충돌이 진행 중 일 경우 */
	public void OnTriggerStay(Collider a_oCollider)
	{
		this.StayCallback?.Invoke(this, a_oCollider);
	}

	/** 충돌이 종료 되었을 경우 */
	public void OnTriggerExit(Collider a_oCollider)
	{
		this.ExitCallback?.Invoke(this, a_oCollider);
	}
	#endregion // 함수
}

/** 충돌 전달자 - 접근 */
public partial class CTriggerDispatcher : MonoBehaviour
{
	#region 함수
	/** 시작 콜백을 변경한다 */
	public void SetEnterCallback(System.Action<CTriggerDispatcher, Collider> a_oCallback)
	{
		this.EnterCallback = a_oCallback;
	}

	/** 진행 콜백을 변경한다 */
	public void SetStayCallback(System.Action<CTriggerDispatcher, Collider> a_oCallback)
	{
		this.StayCallback = a_oCallback;
	}

	/** 종료 콜백을 변경한다 */
	public void SetExitCallback(System.Action<CTriggerDispatcher, Collider> a_oCallback)
	{
		this.ExitCallback = a_oCallback;
	}
	#endregion // 함수	
}
