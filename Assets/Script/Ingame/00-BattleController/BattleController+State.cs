using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 전투 제어자 - 상태 */
public partial class BattleController : MonoBehaviour
{
	#region 팩토리 함수
	/** 준비 상태를 생성한다 */
	public CStateBattleControllerReady CreateReadyState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateBattleControllerReady>();
	}

	/** 플레이 상태를 생성한다 */
	public CStateBattleControllerPlay CreatePlayState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateBattleControllerPlay>();
	}

	/** 이어하기 상태를 생성한다 */
	public CStateBattleControllerContinue CreateContinueState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateBattleControllerContinue>();
	}

	/** 종료 상태를 생성한다 */
	public CStateBattleControllerFinish CreateFinishState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateBattleControllerFinish>();
	}
	#endregion // 팩토리 함수
}
