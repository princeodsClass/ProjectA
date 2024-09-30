using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 상태 머신 행동 */
public partial class CStateMachineBehaviour : StateMachineBehaviour
{
	#region 프로퍼티
	public System.Action<Animator, AnimatorStateInfo, int> ExitCallback { get; private set; } = null;
	#endregion // 프로퍼티

	#region 함수
	/** 상태가 종료 되었을 경우 */
	public override void OnStateExit(Animator a_oSender, AnimatorStateInfo a_stStateInfo, int a_nLayerIdx)
	{
		this.ExitCallback?.Invoke(a_oSender, a_stStateInfo, a_nLayerIdx);
	}
	#endregion // 함수

	#region 접근 함수
	/** 종료 콜백을 변경한다 */
	public void SetExitCallback(System.Action<Animator, AnimatorStateInfo, int> a_oCallback)
	{
		this.ExitCallback = a_oCallback;
	}
	#endregion // 접근 함수
}