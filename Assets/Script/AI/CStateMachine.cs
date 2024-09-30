using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 상태 */
public partial class CState<T> : CObj where T : class
{
	#region 함수
	/** 상태가 시작 되었을 경우 */
	public virtual void OnStateEnter()
	{
		this.SetIsEnable(true);
	}

	/** 상태가 종료 되었을 경우 */
	public virtual void OnStateExit()
	{
		this.SetIsEnable(false);
	}

	/** 상태를 갱신한다 */
	public virtual void OnStateUpdate(float a_fDeltaTime)
	{
		// Do Something
	}

	/** 상태를 갱신한다 */
	public virtual void OnStateLateUpdate(float a_fDeltaTime)
	{
		// Do Something
	}

	/** 상태를 갱신한다 */
	public virtual void OnStateFixedUpdate(float a_fDeltaTime)
	{
		// Do Something
	}

	/** 상태를 갱신한다 */
	public virtual void OnStateCustomUpdate(float a_fDeltaTime)
	{
		// Do Something
	}
	#endregion // 함수
}

/** 상태 머신 */
public partial class CStateMachine<T> : CObj where T : class
{
	#region 프로퍼티
	public CState<T> State { get; private set; } = null;
	#endregion // 프로퍼티

	#region IUpdatable
	/** 상태를 갱신한다 */
	public override void OnUpdate(float a_fDeltaTime)
	{
		base.OnUpdate(a_fDeltaTime);

		// 활성 상태가 아닐 경우
		if (!this.IsEnable)
		{
			return;
		}

		this.State?.OnStateUpdate(a_fDeltaTime);
	}

	/** 상태를 갱신한다 */
	public override void OnLateUpdate(float a_fDeltaTime)
	{
		base.OnLateUpdate(a_fDeltaTime);

		// 활성 상태가 아닐 경우
		if (!this.IsEnable)
		{
			return;
		}

		this.State?.OnStateLateUpdate(a_fDeltaTime);
	}

	/** 상태를 갱신한다 */
	public override void OnFixedUpdate(float a_fDeltaTime)
	{
		base.OnFixedUpdate(a_fDeltaTime);

		// 활성 상태가 아닐 경우
		if (!this.IsEnable)
		{
			return;
		}

		this.State?.OnStateFixedUpdate(a_fDeltaTime);
	}

	/** 상태를 갱신한다 */
	public override void OnCustomUpdate(float a_fDeltaTime)
	{
		base.OnCustomUpdate(a_fDeltaTime);

		// 활성 상태가 아닐 경우
		if (!this.IsEnable)
		{
			return;
		}

		this.State?.OnStateCustomUpdate(a_fDeltaTime);
	}
	#endregion // IUpdatable

	#region 접근 함수
	/** 상태를 변경한다 */
	public void SetState(CState<T> a_oState)
	{
		// 상태 변경이 불가능 할 경우
		if (this.State == a_oState)
		{
			return;
		}

		a_oState?.SetOwner(this.Owner);

		var oPrevState = this.State;
		oPrevState?.OnStateExit();

		this.State = a_oState;
		this.State?.OnStateEnter();

		// 이전 상태가 존재 할 경우
		if (oPrevState != null && oPrevState.IsPooling)
		{
			CObjsPoolManager.Singleton.DespawnObj(oPrevState);
		}
	}
	#endregion // 접근 함수
}
