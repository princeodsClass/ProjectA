using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 에이전트 팝업 정보 스크롤러 셀 뷰 */
public class PopupAgentScrollerCellViewInfo : PopupAgentScrollerCellView
{
	#region 변수
	[Header("=====> Popup Agent Scroller Cell View Info - UIs <=====")]
	[SerializeField] private TMP_Text m_oValText = null;
	[SerializeField] private TMP_Text m_oNameText = null;
	[SerializeField] private TMP_Text m_oDescText = null;

	[SerializeField] private Image m_oIconImg = null;

	[Header("=====> Popup Agent Scroller Cell View Info - Game Objects <=====")]
	[SerializeField] private GameObject m_oEnhanceUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public TMP_Text ValText => m_oValText;
	public TMP_Text NameText => m_oNameText;
	public TMP_Text DescText => m_oDescText;

	public Image IconImg => m_oIconImg;
	public GameObject EnhanceUIs => m_oEnhanceUIs;
	#endregion // 프로퍼티
}
