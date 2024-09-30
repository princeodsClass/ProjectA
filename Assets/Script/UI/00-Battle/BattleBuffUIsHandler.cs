using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 전투 버프 UI 처리자 */
public class BattleBuffUIsHandler : MonoBehaviour
{
	#region 변수
	[SerializeField] private Image m_oIconImg = null;
	#endregion // 변수

	#region 프로퍼티
	public Image IconImg => m_oIconImg;
	#endregion // 프로퍼티
}
