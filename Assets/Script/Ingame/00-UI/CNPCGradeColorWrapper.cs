using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** NPC 등급 색상 */
public class CNPCGradeColorWrapper : MonoBehaviour
{
	#region 변수
	[SerializeField]
	private List<Color> m_oColorList = new List<Color>();
	#endregion // 변수

	#region 함수
	/** 상태를 리셋한다 */
	public void Reset()
	{
		m_oColorList = new List<Color>() {
			ComUtil.ConvertToColor($"#5E6C76FF"),
			ComUtil.ConvertToColor($"#9BA8B0FF"),
			ComUtil.ConvertToColor($"#47E261FF"),
			ComUtil.ConvertToColor($"#FFF51CFF"),
			ComUtil.ConvertToColor($"#1798FFFF"),
			ComUtil.ConvertToColor($"#FF992DFF"),
			ComUtil.ConvertToColor($"#FF514CFF"),
			ComUtil.ConvertToColor($"#B42DF5FF")
		};
	}
	#endregion // 함수

	#region 접근 함수
	/** 색상을 반환한다 */
	public Color GetColor(int a_nGrade)
	{
		return (a_nGrade >= 0 && a_nGrade < m_oColorList.Count) ? m_oColorList[a_nGrade] : Color.white;
	}
	#endregion // 접근 함수
}
