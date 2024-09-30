using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 팝업 로딩 처리자 */
public abstract class PopupLoadingHandler
{
	#region 프로퍼티
	public PopupLoading Owner { get; private set; } = null;
	#endregion // 프로퍼티

	#region 함수
	/** 로딩을 시작한다 */
	public virtual void StartLoading()
	{
		// Do Something
	}
	#endregion // 함수

	#region 접근 함수
	/** 소유자를 변경한다 */
	public void SetOwner(PopupLoading a_oOwner)
	{
		this.Owner = a_oOwner;
	}
	#endregion // 접근 함수
}
