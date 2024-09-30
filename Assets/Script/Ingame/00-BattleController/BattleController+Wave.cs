using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/** 전투 제어자 - 웨이브 */
public partial class BattleController : MonoBehaviour
{
	#region 변수
	private int m_nWaveOrder = 0;
	private int m_nNumMakeAppearWaveNPCs = 0;

	private float m_fWaveSkipTime = 0.0f;
	private float m_fMaxWaveSkipTime = 0.0f;

	private float m_fWaveIntervalSkipTime = 0.0f;
	private float m_fMaxWaveIntervalSkipTime = 0.0f;

	private float m_fWaveNPCAppearSkipTime = 0.0f;
	private EWaveState m_eWaveState = EWaveState.INTERVAL;
	#endregion // 변수

	#region 프로퍼티
	public List<CSpawnPosHandler> SpawnPosHandlerList { get; } = new List<CSpawnPosHandler>();
	public int WaveOrder => m_nWaveOrder;
	#endregion // 프로퍼티

	#region 함수
	/** 웨이브를 시작한다 */
	public void StartWave()
	{
		switch(GameDataManager.Singleton.PlayMapInfoType)
		{
			case EMapInfoType.DEFENCE: this.StartDefenceWave(); break;
			default: this.StartInfiniteWave(); break;
		}	
	}

	/** 웨이브 상태를 초기화한다 */
	public void InitWaveState()
	{
		switch(GameDataManager.Singleton.PlayMapInfoType)
		{
			case EMapInfoType.DEFENCE: this.InitDefenceWaveState(); break;
			default: this.InitInfiniteWaveState(); break;
		}

		m_nMaxNumMakeAppearNPCs = GlobalTable.GetData<int>(ComType.G_COUNT_NPC_CORPSE);
	}

	/** 웨이브 상태를 갱신한다 */
	public void UpdateWaveState(float a_fDeltaTime)
	{
		switch (GameDataManager.Singleton.PlayMapInfoType)
		{
			case EMapInfoType.DEFENCE: this.UpdateDefenceWaveState(a_fDeltaTime); break;
			default: this.UpdateInfiniteWaveState(a_fDeltaTime); break;
		}
	}
	#endregion // 함수
}
