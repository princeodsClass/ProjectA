using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/** 전투 제어자 - 방어전 */
public partial class BattleController : MonoBehaviour
{
	#region 변수
	private int m_nMaxNumMakeAppearNPCs = 0;
	private string m_oDefenceName = string.Empty;

	private MissionDefenceGroupTable m_oDefenceGroupTable = null;
	private List<MissionDefenceTable> m_oDefenceTableList = new List<MissionDefenceTable>();
	#endregion // 변수

	#region 프로퍼티
	public List<NPCGroupTable> DefenceWaveNPCGroupTableList { get; } = new List<NPCGroupTable>();
	public string DefenceName => m_oDefenceName;

	public List<MissionDefenceTable> DefenceTableList => m_oDefenceTableList;
	#endregion // 프로퍼티

	#region 함수
	/** 방어전 웨이브를 시작한다 */
	public void StartDefenceWave()
	{
		this.PageBattle.StartDefenceWaveStartDirecting(m_nWaveOrder,
			m_oDefenceTableList[m_nWaveOrder].IntervalWave * ComType.G_UNIT_MS_TO_S);

		this.PlayerController.OnBattlePlay();
	}

	/** 방어전 웨이브 상태를 초기화한다 */
	public void InitDefenceWaveState()
	{
		var oDefenceTableList = MissionDefenceTable.GetDifficulty(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nChapterID);
		oDefenceTableList.ExCopyTo(m_oDefenceTableList, (a_oTable) => a_oTable);

		uint nDefenceGroupTableKey = ComUtil.GetDefenceGroupKey(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nChapterID);
		
		m_oDefenceGroupTable = MissionDefenceGroupTable.GetData(nDefenceGroupTableKey);
		m_oDefenceName = NameTable.GetValue(m_oDefenceGroupTable.NameKey);

		this.SetupDefenceWaveInfo(m_nWaveOrder, false);
		this.UpdateDefenceWaveUIsState();

		this.PageBattle.DefenceWarningUIs.SetActive(false);
		this.PageBattle.DefenceWaveNotiUIs.SetActive(false);
		this.PageBattle.DefenceWaveNotiUIs.transform.localScale = Vector3.one;
	}

	/** 방어전 웨이브 상태를 갱신한다 */
	public void UpdateDefenceWaveState(float a_fDeltaTime)
	{
		this.PageBattle.DefenceWarningUIs.SetActive(false);
		this.UpdateDefenceWaveUIsState();

		// 웨이브가 모두 완료되었을 경우
		if (m_nWaveOrder >= m_oDefenceTableList.Count)
		{
			TryHandleDefenceFinishBattle();
			return;
		}

		switch (m_eWaveState)
		{
			case EWaveState.PLAY: this.UpdateDefenceWaveStatePlay(a_fDeltaTime); break;
			case EWaveState.INTERVAL: this.UpdateDefenceWaveStateInterval(a_fDeltaTime); break;
		}
	}

	/** 방어전 웨이브 UI 상태를 갱신한다 */
	private void UpdateDefenceWaveUIsState()
	{
		int nWaveOrder = Mathf.Min(m_nWaveOrder + 1, this.DefenceTableList.Count);
		this.PageBattle.DefenceTitleText.text = $"{m_oDefenceName}\nWave {nWaveOrder}";

		int nTotalSecs = Mathf.CeilToInt(m_fMaxWaveSkipTime - m_fWaveSkipTime);
		nTotalSecs = Mathf.Max(0, nTotalSecs);

		bool bIsFinishWave = m_nWaveOrder >= m_oDefenceTableList.Count;
		bool bIsDefenceWarning = this.NonPlayerControllerList.Count >= m_nMaxNumMakeAppearNPCs;

		// 대기 상태 일 경우
		if (!bIsFinishWave && m_eWaveState == EWaveState.INTERVAL)
		{
			nTotalSecs = Mathf.CeilToInt(m_fMaxWaveIntervalSkipTime - m_fWaveIntervalSkipTime);
			nTotalSecs = Mathf.Max(0, nTotalSecs);

			this.PageBattle.RemainTimeText.color = Color.yellow;
		}
		else
		{
			bool bIsActiveWarningUIs = m_eWaveState != EWaveState.FINISH && (bIsFinishWave || bIsDefenceWarning);
			this.PageBattle.DefenceWarningUIs.SetActive(bIsActiveWarningUIs);

			this.PageBattle.RemainTimeText.color = this.PageBattle.DefenceWarningUIs.activeSelf ? 
				Color.red : Color.white;
		}

		this.PageBattle.RemainTimeText.text = $"{nTotalSecs / 60:00}:{nTotalSecs % 60:00}";
	}

	/** 플레이 웨이브 상태를 갱신한다 */
	private void UpdateDefenceWaveStatePlay(float a_fDeltaTime)
	{
		// 웨이브 진행이 가능 할 경우
		if (this.NonPlayerControllerList.Count < m_nMaxNumMakeAppearNPCs)
		{
			m_fWaveSkipTime += a_fDeltaTime;
			m_fWaveNPCAppearSkipTime += a_fDeltaTime;
		}

		// NPC 등장 처리가 필요 할 경우
		if (m_fWaveNPCAppearSkipTime.ExIsGreatEquals(m_oDefenceTableList[m_nWaveOrder].IntervalNPC * ComType.G_UNIT_MS_TO_S))
		{
			m_fWaveNPCAppearSkipTime = 0.0f;

			var stBattlePlayInfo = this.BattlePlayInfo;
			stBattlePlayInfo.m_nStandardNPCLevel = m_oDefenceTableList[m_nWaveOrder].StandardNPCLevel;

			this.BattlePlayInfo = stBattlePlayInfo;
			this.MakeAppearDefenceWaveNPCs();
		}

		bool bIsMoveToNextWaveA = m_nNumMakeAppearWaveNPCs >= m_oDefenceTableList[m_nWaveOrder].CountNPC && this.NonPlayerControllerList.Count <= 0;
		bool bIsMoveToNextWaveB = m_fWaveSkipTime.ExIsGreatEquals(m_oDefenceTableList[m_nWaveOrder].timeWave * ComType.G_UNIT_MS_TO_S);

		// 다음 웨이브 처리가 필요 할 경우
		if (bIsMoveToNextWaveA || bIsMoveToNextWaveB)
		{
			m_nWaveOrder += 1;
			m_eWaveState = EWaveState.INTERVAL;

			this.SetupDefenceWaveInfo(m_nWaveOrder);
		}
	}

	/** 대기 웨이브 상태를 갱신한다 */
	private void UpdateDefenceWaveStateInterval(float a_fDeltaTime)
	{
		m_fWaveIntervalSkipTime += a_fDeltaTime;

		// 다음 웨이브 플레이가 불가능 할 경우
		if (m_fWaveIntervalSkipTime.ExIsLess(m_oDefenceTableList[m_nWaveOrder].IntervalWave * ComType.G_UNIT_MS_TO_S))
		{
			return;
		}

		m_fWaveSkipTime = 0.0f;
		m_fWaveIntervalSkipTime = 0.0f;
		m_fWaveNPCAppearSkipTime = 0.0f;

		m_eWaveState = EWaveState.PLAY;
		this.PageBattle.DefenceWaveNotiUIs.SetActive(false);
	}

	/** 방어전 웨이브 정보를 설정한다 */
	private void SetupDefenceWaveInfo(int a_nWaveOrder, bool a_bIsEnableStartDirecting = true)
	{
		// 웨이브 정보가 없을 경우
		if (a_nWaveOrder >= m_oDefenceTableList.Count)
		{
			return;
		}

		m_fWaveSkipTime = m_fMaxWaveSkipTime = m_oDefenceTableList[a_nWaveOrder].timeWave * ComType.G_UNIT_MS_TO_S;
		m_fMaxWaveIntervalSkipTime = m_oDefenceTableList[a_nWaveOrder].IntervalWave * ComType.G_UNIT_MS_TO_S;
		m_nNumMakeAppearWaveNPCs = 0;

		this.InstEXPRatio = m_oDefenceTableList[a_nWaveOrder].IntanceExpRatio;
		this.DefenceWaveNPCGroupTableList.Clear();

		int nNPCGroup = m_oDefenceTableList[a_nWaveOrder].NPCGroup;
		NPCGroupTable.GetGroup(nNPCGroup, this.DefenceWaveNPCGroupTableList);

		// 시작 연출 처리 모드 일 경우
		if (a_bIsEnableStartDirecting)
		{
			this.PageBattle.StartDefenceWaveStartDirecting(a_nWaveOrder,
				m_oDefenceTableList[a_nWaveOrder].IntervalWave * ComType.G_UNIT_MS_TO_S);
		}
	}

	/** 방어전 전투 종료를 처리한다 */
	private void TryHandleDefenceFinishBattle()
	{
		// NPC 가 존재 할 경우
		if (this.NonPlayerControllerList.Count >= 1 || m_eWaveState == EWaveState.FINISH)
		{
			return;
		}

		m_eWaveState = EWaveState.FINISH;
		this.UpdateDefenceWaveUIsState();

		this.FinishBattle(false, 1.0f);
		this.PageBattle.StartDefenceWaveClearDirecting(m_nWaveOrder, 1.0f);
	}

	/** 방어전 웨이브 NPC 를 등장시킨다 */
	private void MakeAppearDefenceWaveNPCs()
	{
		// 웨이브 정보가 없을 경우
		if (m_nWaveOrder >= m_oDefenceTableList.Count)
		{
			return;
		}

		for (int i = 0; i < m_oDefenceTableList[m_nWaveOrder].CountAppearNPC; ++i)
		{
			bool bIsValidA = m_nNumMakeAppearWaveNPCs < m_oDefenceTableList[m_nWaveOrder].CountNPC;
			bool bIsValidB = this.NonPlayerControllerList.Count < m_nMaxNumMakeAppearNPCs;

			// NPC 생성이 불가능 할 경우
			if (!bIsValidA || !bIsValidB)
			{
				return;
			}

			int nSelectionFactor = 0;
			int nSumSelectionFactor = this.DefenceWaveNPCGroupTableList.Sum((a_oGroupTable) => a_oGroupTable.SelectionFactor);

			float fRandomSelectionFactor = Random.Range(0.0f, (float)nSumSelectionFactor);

			for (int j = 0; j < this.DefenceWaveNPCGroupTableList.Count; ++j)
			{
				nSelectionFactor += this.DefenceWaveNPCGroupTableList[j].SelectionFactor;

				// NPC 생성이 가능 할 경우
				if (fRandomSelectionFactor.ExIsLess((float)nSelectionFactor))
				{
					this.MakeAppearDefenceWaveNPCs(this.DefenceWaveNPCGroupTableList[j]);
					break;
				}
			}
		}
	}

	/** 방어전 웨이브 NPC 를 등장시킨다 */
	private void MakeAppearDefenceWaveNPCs(NPCGroupTable a_oGroupTable)
	{
		m_nNumMakeAppearWaveNPCs += 1;

		var oNonPlayerController = this.CreateNonPlayer(NPCTable.GetData(a_oGroupTable.NPCKey),
			this.GetDefenceWaveSpawnPos(), this.PlayMapObjsRoot, null, a_oEffectTableList: this.NPCEffectTableList);

		oNonPlayerController.LookTarget(this.PlayerController);
		oNonPlayerController.SetTrackingTarget(this.PlayerController);

		this.ExLateCallFunc((a_oSender) =>
		{
			float fStandardHP = oNonPlayerController.CurStandardAbilityValDict.GetValueOrDefault(EEquipEffectType.HP);
			float fStandardATK = oNonPlayerController.CurStandardAbilityValDict.GetValueOrDefault(EEquipEffectType.ATK);
			float fStandardAimingDelay = oNonPlayerController.CurStandardAbilityValDict.GetValueOrDefault(EEquipEffectType.AIMING_DELAY);

			oNonPlayerController.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.HP, fStandardHP * m_oDefenceTableList[m_nWaveOrder].RatioHP);
			oNonPlayerController.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.ATK, fStandardATK * m_oDefenceTableList[m_nWaveOrder].RatioPowerAttack);
			oNonPlayerController.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.AIMING_DELAY, fStandardAimingDelay * m_oDefenceTableList[m_nWaveOrder].RatioAimTime);

			oNonPlayerController.SetupAbilityValues(true);
			oNonPlayerController.OnBattlePlay();
		});
	}
	#endregion // 함수

	#region 접근 함수
	/** 방어전 웨이브 등장 위치를 반환한다 */
	private Vector3 GetDefenceWaveSpawnPos()
	{
		int nIdx = Random.Range(0, this.SpawnPosHandlerList.Count);
		var stSpawnPos = this.SpawnPosHandlerList[nIdx].transform.position;

		// 내비게이션 영역이 존재 할 경우
		if (NavMesh.SamplePosition(stSpawnPos + (Vector3.up * 1.5f), out NavMeshHit stNavMeshHit, float.MaxValue / 2.0f, m_nWalkableAreaMask))
		{
			return stNavMeshHit.position;
		}

		return stSpawnPos;
	}
	#endregion // 접근 함수
}
