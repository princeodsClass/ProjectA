using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 전투 제어자 - 보너스 */
public partial class BattleController : MonoBehaviour
{
	#region 함수
	/** 보너스 포인트를 증가시킨다 */
	public void IncrGoldenPoint(float a_fPoint)
	{
		this.GoldenPoint = Mathf.Max(0.0f, this.GoldenPoint + a_fPoint);
	}
	#endregion // 함수

	#region 접근 함수
	/** 보너스 포인트를 변경한다 */
	public void SetGoldenPoint(float a_fPoint)
	{
		this.GoldenPoint = Mathf.Max(0.0f, a_fPoint);
	}
	#endregion // 접근 함수
}
