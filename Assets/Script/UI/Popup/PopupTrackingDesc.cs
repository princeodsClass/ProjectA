using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 추적 설명 팝업 */
public partial class PopupTrackingDesc : UIDialog
{
	/** 매개 변수 */
	public struct STParams
	{
		public System.Action<PopupTrackingDesc> m_oCallback;
	}

	#region 프로퍼티
	public STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	private void Awake()
	{
		Initialize();
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		this.UpdateUIsState();
	}

	/** 백 키를 처리한다 */
	public override void Escape()
	{
		// Do Something
	}

	/** 다음 버튼을 눌렀을 경우 */
	public void OnTouchNextBtn()
	{
		this.Params.m_oCallback?.Invoke(this);
	}

	/** UI 상태를 갱신한다 */
	private void UpdateUIsState()
	{
		// Do Something
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(System.Action<PopupTrackingDesc> a_oCallback)
	{
		return new STParams()
		{
			m_oCallback = a_oCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}
