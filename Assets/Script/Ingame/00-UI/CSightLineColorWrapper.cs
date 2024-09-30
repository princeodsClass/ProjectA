using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 조준선 색상 */
public class CSightLineColorWrapper : MonoBehaviour
{
	#region 변수
	[SerializeField] private List<Color> m_oColorList = new List<Color>();
	#endregion // 변수

	#region 접근 함수
	/** 색상을 반환한다 */
	public Color GetColor(int a_nIdx)
	{
		return (a_nIdx >= 0 && a_nIdx < m_oColorList.Count) ? m_oColorList[a_nIdx] : Color.white;
	}
	#endregion // 접근 함수
}
