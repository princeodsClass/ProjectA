using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 의상 에이전트 팝업 스크롤러 셀 뷰 */
public class PopupAgentScrollerCellViewCostume : PopupAgentScrollerCellView
{
	/** 매개 변수 */
	public new struct STParams
	{
		public PopupAgentScrollerCellView.STParams m_stBase;
		public CharacterTable m_oCharacterTable;
	}

	#region 프로퍼티
	public new STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		// Do Something
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		base.Init(a_stParams.m_stBase);
		this.Params = a_stParams;

		this.UpdateUIsState();
	}

	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		// Do Something
	}
	#endregion // 함수

	#region 클래스 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(int a_nIdx,
		PopupAgent a_oPopupAgent, CharacterTable a_oCharacterTable, System.Action<PopupAgentScrollerCellView, int> a_oCallback)
	{
		return new STParams()
		{
			m_stBase = PopupAgentScrollerCellView.MakeParams(a_nIdx, a_oPopupAgent, a_oCallback),
			m_oCharacterTable = a_oCharacterTable
		};
	}
	#endregion // 클래스 함수
}
