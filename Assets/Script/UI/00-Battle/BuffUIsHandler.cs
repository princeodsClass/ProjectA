using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 전투 버프 UI 처리자 */
public class BuffUIsHandler : MonoBehaviour
{
	#region 변수
	[SerializeField] public TMP_Text m_oNameText = null;
	[SerializeField] public TMP_Text m_oDescText = null;
	[SerializeField] public TMP_Text m_oEmptyText = null;

	[SerializeField] public Image m_oIconImg = null;
	[SerializeField] public Image m_oFrameImg = null;

	[SerializeField] public GameObject m_oDescUIs = null;
	[SerializeField] public GameObject m_oEmptyUIs = null;
	[SerializeField] public GameObject m_oActiveUIs = null;
	[SerializeField] public GameObject m_oApplyBuffUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public TMP_Text NameText => m_oNameText;
	public TMP_Text DescText => m_oDescText;
	public TMP_Text EmptyText => m_oEmptyText;

	public Image IconImg => m_oIconImg;
	public Image FrameImg => m_oFrameImg;

	public GameObject DescUIs => m_oDescUIs;
	public GameObject EmptyUIs => m_oEmptyUIs;
	public GameObject ActiveUIs => m_oActiveUIs;
	public GameObject ApplyBuffUIs => m_oApplyBuffUIs;
	#endregion // 프로퍼티
}
