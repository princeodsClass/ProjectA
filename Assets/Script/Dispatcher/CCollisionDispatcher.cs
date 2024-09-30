using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 충돌 전달자 */
public partial class CCollisionDispatcher : MonoBehaviour
{
	#region 프로퍼티
	public System.Action<CCollisionDispatcher, Collision> EnterCallback { get; private set; } = null;
	public System.Action<CCollisionDispatcher, Collision> StayCallback { get; private set; } = null;
	public System.Action<CCollisionDispatcher, Collision> ExitCallback { get; private set; } = null;
	#endregion // 프로퍼티

	#region 함수
	/** 충돌이 시작 되었을 경우 */
	public void OnCollisionEnter(Collision a_oCollision)
	{
		this.EnterCallback?.Invoke(this, a_oCollision);
	}

	/** 충돌이 진행 중 일 경우 */
	public void OnCollisionStay(Collision a_oCollision)
	{
		this.StayCallback?.Invoke(this, a_oCollision);
	}

	/** 충돌이 종료 되었을 경우 */
	public void OnCollisionExit(Collision a_oCollision)
	{
		this.ExitCallback?.Invoke(this, a_oCollision);
	}
	#endregion // 함수
}

/** 충돌 전달자 - 접근 */
public partial class CCollisionDispatcher : MonoBehaviour
{
	#region 함수
	/** 시작 콜백을 변경한다 */
	public void SetEnterCallback(System.Action<CCollisionDispatcher, Collision> a_oCallback)
	{
		this.EnterCallback = a_oCallback;
	}

	/** 진행 콜백을 변경한다 */
	public void SetStayCallback(System.Action<CCollisionDispatcher, Collision> a_oCallback)
	{
		this.StayCallback = a_oCallback;
	}

	/** 종료 콜백을 변경한다 */
	public void SetExitCallback(System.Action<CCollisionDispatcher, Collision> a_oCallback)
	{
		this.ExitCallback = a_oCallback;
	}
	#endregion // 함수	
}
