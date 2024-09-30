using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 에이전트 강화 팝업 보너스 UI */
public class PopupAgentEnhanceBonusUIs : MonoBehaviour
{
	#region 변수
	[Header("=====> Popup Agent Enhance Bonus UIs - UI <=====")]
	[SerializeField] private TMP_Text m_oValText = null;
	[SerializeField] private TMP_Text m_oNameText = null;
	[SerializeField] private TMP_Text m_oDescText = null;

	[SerializeField] private Image m_oIconImg = null;
	[SerializeField] private Image m_oCheckImg = null;
	[SerializeField] private Image m_oSeparateImg = null;

	[SerializeField] private SoundButton m_oActiveBtn = null;

	[Header("=====> Popup Agent Enhance Bonus UIs - Game Objects <=====")]
	[SerializeField] private GameObject m_oActiveUIs = null;
	[SerializeField] private GameObject m_oEnhanceUIs = null;

	[SerializeField] private GameObject m_oActiveUIsSel = null;
	[SerializeField] private GameObject m_oActiveUIsUnsel = null;
	#endregion // 변수

	#region 프로퍼티
	public TMP_Text ValText => m_oValText;
	public TMP_Text NameText => m_oNameText;
	public TMP_Text DescText => m_oDescText;

	public Image IconImg => m_oIconImg;
	public Image CheckImg => m_oCheckImg;
	public Image SeparateImg => m_oSeparateImg;

	public SoundButton ActiveBtn => m_oActiveBtn;

	public GameObject ActiveUIs => m_oActiveUIs;
	public GameObject EnhanceUIs => m_oEnhanceUIs;

	public GameObject ActiveUIsSel => m_oActiveUIsSel;
	public GameObject ActiveUIsUnsel => m_oActiveUIsUnsel;
	#endregion // 프로퍼티
}
