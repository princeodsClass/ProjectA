using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 선택 에이전트 팝업 스크롤러 셀 뷰 */
public class PopupAgentScrollerCellViewSel : PopupAgentScrollerCellView
{
	/** 매개 변수 */
	public new struct STParams
	{
		public PopupAgentScrollerCellView.STParams m_stBase;
		public List<CharacterTable> m_oCharacterTableList;
	}

	#region 변수
	[Header("=====> Popup Agent Scroller Cell View Sel - Etc")]
	[SerializeField] private List<PopupAgentScrollerCellSel> m_oScrollerCellSelList = new List<PopupAgentScrollerCellSel>();
	#endregion // 변수

	#region 프로퍼티
	public new STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		base.Init(a_stParams.m_stBase);
		this.Params = a_stParams;

		for (int i = 0; i < m_oScrollerCellSelList.Count; ++i)
		{
			int nIdx = a_stParams.m_stBase.m_nIdx + i;

			var stParams = PopupAgentScrollerCellSel.MakeParams(a_stParams.m_stBase.m_oPopupAgent,
				a_stParams.m_oCharacterTableList[nIdx], (a_oSender) => this.Params.m_stBase.m_oSelCallback(this, nIdx));

			m_oScrollerCellSelList[i].Init(stParams);
		}
	}

	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		for (int i = 0; i < m_oScrollerCellSelList.Count; ++i)
		{
			m_oScrollerCellSelList[i].UpdateUIsState();
		}
	}
	#endregion // 함수

	#region 클래스 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(int a_nIdx,
		PopupAgent a_oPopupAgent, List<CharacterTable> a_oCharacterTableList, System.Action<PopupAgentScrollerCellView, int> a_oCallback)
	{
		return new STParams()
		{
			m_stBase = PopupAgentScrollerCellView.MakeParams(a_nIdx, a_oPopupAgent, a_oCallback),
			m_oCharacterTableList = a_oCharacterTableList
		};
	}
	#endregion // 클래스 함수
}
