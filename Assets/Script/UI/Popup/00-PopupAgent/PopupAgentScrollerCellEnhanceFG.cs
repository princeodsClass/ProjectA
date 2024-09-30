using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 강화 에이전트 팝업 상단 스크롤러 셀 */
public class PopupAgentScrollerCellEnhanceFG : MonoBehaviour
{
	#region 변수
	[Header("=====> Popup Agent Scroller Cell Enhance FG - UIs <=====")]
	[SerializeField] private TMP_Text m_oNameText = null;

	[SerializeField] private Image m_oIconImg = null;
	[SerializeField] private Image m_oCheckImg = null;

	[Header("=====> Popup Agent Scroller Cell Enhance FG - Game Objects <=====")]
	[SerializeField] private GameObject m_oLockUIs = null;
	[SerializeField] private GameObject m_oOpenUIs = null;
	[SerializeField] private GameObject m_oDescUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public TMP_Text NameText => m_oNameText;

	public Image IconImg => m_oIconImg;
	public Image CheckImg => m_oCheckImg;

	public GameObject LockUIs => m_oLockUIs;
	public GameObject OpenUIs => m_oOpenUIs;
	public GameObject DescUIs => m_oDescUIs;
	#endregion // 프로퍼티
}
