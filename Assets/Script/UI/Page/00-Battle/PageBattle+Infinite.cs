using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 전투 페이지 - 무한 */
public partial class PageBattle : UIDialog
{
	#region 변수
	[Header("=====> Battle Controller - UIs (Infinite) <=====")]
	[SerializeField] private TMP_Text m_oInfiniteNumKillsText = null;

	[Header("=====> Page Battle - Game Objects (Infinite) <=====")]
	[SerializeField] private GameObject m_oInfiniteUIs = null;
	[SerializeField] private GameObject m_oInfiniteGaugeUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public TMP_Text InfiniteNumKillsText => m_oInfiniteNumKillsText;

	public GameObject InfiniteUIs => m_oInfiniteUIs;
	public GameObject InfiniteGaugeUIs => m_oInfiniteGaugeUIs;
	#endregion // 프로퍼티
}
