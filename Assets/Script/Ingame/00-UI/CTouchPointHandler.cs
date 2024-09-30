using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 터치 위치 처리자 */
public class CTouchPointHandler : MonoBehaviour
{
	#region 변수
	[Header("=====> Touch Point Handler - Game Objects <=====")]
	[SerializeField] private GameObject m_oOnParticle = null;
	[SerializeField] private GameObject m_oOffParticle = null;
	#endregion // 변수

	#region 접근 함수
	/** 활성 여부를 변경한다 */
	public void SetIsOn(bool a_bIsOn)
	{
		m_oOnParticle.SetActive(a_bIsOn);
		m_oOffParticle.SetActive(!a_bIsOn);
	}
	#endregion // 접근 함수
}
