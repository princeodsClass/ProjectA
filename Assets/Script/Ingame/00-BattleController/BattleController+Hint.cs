using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 전투 제어자 - 힌트 */
public partial class BattleController : MonoBehaviour
{
	/** 힌트 정보 */
	public struct STHintInfo
	{
		public uint m_nHintGroup;
		public UnitController m_oTarget;
	}

	#region 변수
	[Header("=====> Battle Controller - Etc (Hint) <=====")]
	private bool m_bIsEnableHintDirecting = true;

	private float m_fOriginCameraHeight = 0.0f;
	private float m_fOriginCameraForward = 0.0f;
	private float m_fOriginCamDummyDistance = 0.0f;

	private Queue<STHintInfo> m_oHintInfoQueue = new Queue<STHintInfo>();
	#endregion // 변수

	#region 프로퍼티
	public bool IsEnableHintDirecting => m_bIsEnableHintDirecting;
	public Queue<STHintInfo> HintInfoQueue => m_oHintInfoQueue;
	#endregion // 프로퍼티

	#region 함수
	/** 힌트 정보를 추가한다 */
	public void AddHintInfo(UnitController a_oTarget, uint a_nHintGroup)
	{
		// 출력한 힌트 일 경우
		if(GameDataManager.Singleton.HintGroupKeyList.Contains(a_nHintGroup))
		{
			return;
		}

		var stHintInfo = new STHintInfo()
		{
			m_oTarget = a_oTarget,
			m_nHintGroup = a_nHintGroup
		};

		m_oHintInfoQueue.Enqueue(stHintInfo);
		GameDataManager.Singleton.HintGroupKeyList.ExAddVal(a_nHintGroup);
	}

	/** 힌트 연출을 처리한다 */
	private void TryHandleHintDirecting()
	{
		// 힌트 연출이 불가능 할 경우
		if(!m_bIsEnableHintDirecting || m_oHintInfoQueue.Count <= 0)
		{
			return;
		}

		m_bIsEnableHintDirecting = false;
		var stHintInfo = m_oHintInfoQueue.Dequeue();

		var oCamDummy = this.CamDummy.GetComponent<CamDummy>();
		oCamDummy.bIsRealtime = true;

		var oHintGroupTableList = NPCHintGroupTable.GetGroup((int)stHintInfo.m_nHintGroup);

		// 힌트 테이블이 없을 경우
		if(!oHintGroupTableList.ExIsValid())
		{
			this.OnCompleteHintDirecting(stHintInfo);
			return;
		}

		this.PageBattle.ShowTalk(NPCHintStringTable.GetValue(oHintGroupTableList[0].MySpeechKey), true);
		ComUtil.SetTimeScale(oHintGroupTableList[0].TargetTimeScale, true);

		var oNonPlayerController = stHintInfo.m_oTarget as NonPlayerController;
		oNonPlayerController?.ShowTalk(NPCHintStringTable.GetValue(oHintGroupTableList[0].EnemySpeechKey), true);

		this.StartCameraDirecting(ComType.G_OFFSET_CAMERA_HEIGHT_FOR_FOCUS,
			ComType.G_OFFSET_CAMERA_FORWARD_FOR_FOCUS, ComType.G_OFFSET_CAMERA_DISTANCE_FOR_FOCUS, stHintInfo.m_oTarget.gameObject, true, true);

		GameDataManager.Singleton.StopCoroutine("CoStartCameraFocusDirecting");
		GameDataManager.Singleton.StartCoroutine(this.CoStartCameraFocusDirecting(stHintInfo));
	}

	/** 힌트 연출이 완료되었을 경우 */
	private void OnCompleteHintDirecting(STHintInfo a_stHintInfo)
	{
		// 전투 종료 상태 일 경우
		if(MenuManager.Singleton.CurScene != ESceneType.Battle || this.StateMachine.State is CStateBattleControllerFinish)
		{
			return;
		}

		m_bIsEnableHintDirecting = true;

		// 남은 연출이 존재 할 경우
		if(m_oHintInfoQueue.Count > 0)
		{
			this.TryHandleHintDirecting();
		}
		else
		{
			this.StartCameraDirecting(m_fOriginCameraHeight, 
				m_fOriginCameraForward, m_fOriginCamDummyDistance, this.PlayerController.gameObject, true);
		}
	}
	#endregion // 함수
}

/** 전투 제어자 - 힌트 (코루틴) */
public partial class BattleController : MonoBehaviour
{
	#region 함수
	/** 코루틴을 설정한다 */
	private IEnumerator CoStartCameraFocusDirecting(STHintInfo a_stHintInfo)
	{
		yield return new WaitForSecondsRealtime(4.5f);

		// 전투 씬이 아닐 경우
		if(MenuManager.Singleton.CurScene != ESceneType.Battle)
		{
			yield break;
		}

		ComUtil.SetTimeScale(this.PageBattle.IsBoost ? 2.0f : 1.0f, true);
		this.OnCompleteHintDirecting(a_stHintInfo);
	}
	#endregion // 함수
}
