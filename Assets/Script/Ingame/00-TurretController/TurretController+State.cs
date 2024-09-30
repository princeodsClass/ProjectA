using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 터렛 제어자 - 상태 */
public partial class TurretController : NonPlayerController
{
	#region 클래스 함수
	/** 초기 상태를 생성한다 */
	public override CStateUnit CreateInitState()
	{
		return this.CreateIdleState();
	}
	#endregion // 클래스 함수
}
