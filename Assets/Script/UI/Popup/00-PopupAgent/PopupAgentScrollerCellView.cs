using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 에이전트 팝업 스크롤러 셀 뷰 */
public class PopupAgentScrollerCellView : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public int m_nIdx;
		public PopupAgent m_oPopupAgent;
		public System.Action<PopupAgentScrollerCellView, int> m_oSelCallback;
	}

	#region 프로퍼티
	public STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		ComUtil.RebuildLayouts(this.gameObject);
	}

	/** 선택 버튼을 눌렀을 경우 */
	public void OnTouchSelBtn()
	{
		this.Params.m_oSelCallback?.Invoke(this, this.Params.m_nIdx);
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(int a_nIdx,
		PopupAgent a_oPopupAgent, System.Action<PopupAgentScrollerCellView, int> a_oSelCallback)
	{
		return new STParams()
		{
			m_nIdx = a_nIdx,
			m_oPopupAgent = a_oPopupAgent,
			m_oSelCallback = a_oSelCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}
