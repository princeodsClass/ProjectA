using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/** 최상단 객체 */
public partial class CObj : IUpdatable
{
	#region 프로퍼티
	public bool IsEnable { get; private set; } = true;
	public bool IsPooling { get; private set; } = false;

	public object Owner { get; private set; } = null;
	#endregion // 프로퍼티

	#region IUpdatable
	/** 상태를 갱신한다 */
	public virtual void OnUpdate(float a_fDeltaTime)
	{
		// Do Something
	}

	/** 상태를 갱신한다 */
	public virtual void OnLateUpdate(float a_fDeltaTime)
	{
		// Do Something
	}

	/** 상태를 갱신한다 */
	public virtual void OnFixedUpdate(float a_fDeltaTime)
	{
		// Do Something
	}

	/** 상태를 갱신한다 */
	public virtual void OnCustomUpdate(float a_fDeltaTime)
	{
		// Do Something
	}
	#endregion // IUpdatable

	#region 함수
	/** 초기화 */
	public virtual void Init()
	{
		// Do Something
	}

	/** 상태를 리셋한다 */
	public virtual void Reset()
	{
		// Do Something
	}
	#endregion // 함수

	#region 접근 함수
	/** 유효 여부를 변경한다 */
	public void SetIsEnable(bool a_bIsEnable)
	{
		this.IsEnable = a_bIsEnable;
	}

	/** 풀링 여부를 변경한다 */
	public void SetIsPooling(bool a_bIsPooling)
	{
		this.IsPooling = a_bIsPooling;
	}

	/** 소유자를 변경한다 */
	public void SetOwner(object a_oOwner)
	{
		this.Owner = a_oOwner;
	}
	#endregion // 접근 함수

	#region 제네릭 접근 함수
	/** 소유자를 반환한다 */
	public T GetOwner<T>() where T : class
	{
		return (this.Owner != null) ? this.Owner as T : null;
	}
	#endregion // 제네릭 접근 함수
}
