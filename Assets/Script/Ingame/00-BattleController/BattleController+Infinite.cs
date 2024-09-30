using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;
using DG.Tweening;

/** 전투 제어자 - 무한 */
public partial class BattleController : MonoBehaviour
{
	#region 변수
	[Header("=====> Battle Controller - Infinite (Etc) <=====")]
	[SerializeField] private AudioClip m_oFocusAudioClip = null;
	private string m_oNumKillsStr = string.Empty;

	public int InfinitePoint { get; private set; } = 0;
	public int MaxInfinitePoint { get; private set; } = 0;

	private Tween m_oInfiniteDescAni = null;
	private Tween m_oInfiniteFocusAni = null;
	private List<MissionZombieTable> m_oInfiniteTableList = new List<MissionZombieTable>();

	[Header("=====> Battle Controller - Infinite (UIs) <=====")]
	[SerializeField] private TMP_Text m_oInfiniteDescText = null;
	[SerializeField] private Image m_oInfiniteFocusImg = null;

	[Header("=====> Battle Controller - Infinite (Game Objects) <=====")]
	[SerializeField] private GameObject m_oInfiniteDescUIs = null;
	[SerializeField] private GameObject m_oInfiniteFocusUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public List<NPCGroupTable> InfiniteWaveNPCGroupTableList { get; } = new List<NPCGroupTable>();
	public TMP_Text InfiniteDescText => m_oInfiniteDescText;

	public Image InfiniteFocusImg => m_oInfiniteFocusImg;

	public GameObject InfiniteDescUIs => m_oInfiniteDescUIs;
	public GameObject InfiniteFocusUIs => m_oInfiniteFocusUIs;
	#endregion // 프로퍼티

	#region 함수
	/** 무한 웨이브를 시작한다 */
	public void StartInfiniteWave()
	{
		m_oInfiniteDescUIs.SetActive(false);
		StartCoroutine(this.CoStartInfiniteWave());
	}

	/** 무한 웨이브 상태를 초기화한다 */
	public void InitInfiniteWaveState()
	{
		var oInfiniteTableList = MissionZombieTable.GetList();
		oInfiniteTableList.ExCopyTo(m_oInfiniteTableList, (a_oTable) => a_oTable);

		int nEpisodeID = GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID;
		var oMissionZombieGroupTable = MissionZombieGroupTable.GetDataByEpisode(nEpisodeID + 1);

		m_nWaveOrder = oMissionZombieGroupTable.StartWave - 1;
		this.SetupInfiniteWaveInfo(m_nWaveOrder);

		this.UpdateInfiniteWaveUIsState();
		StartCoroutine(this.CoStartFocusDirecting());
	}

	/** 무한 웨이브 상태를 갱신한다 */
	public void UpdateInfiniteWaveState(float a_fDeltaTime)
	{
		this.UpdateInfiniteWaveUIsState();

		// 웨이브가 모두 완료되었을 경우
		if (m_nWaveOrder >= m_oInfiniteTableList.Count)
		{
			this.TryHandleInfiniteFinishBattle();
			return;
		}

		switch (m_eWaveState)
		{
			case EWaveState.PLAY: this.UpdateInfiniteWaveStatePlay(a_fDeltaTime); break;
			case EWaveState.INTERVAL: this.UpdateInfiniteWaveStateInterval(a_fDeltaTime); break;
		}
	}

	/** 무한 포인트를 증가시킨다 */
	public void IncrInfinitePoint(int a_nPoint)
	{
		// 포인트 증가가 불가능 할 경우
		if(this.MaxInfinitePoint <= 0 || m_eWaveState == EWaveState.FINISH)
		{
			return;
		}

		this.InfinitePoint = Mathf.Clamp(this.InfinitePoint + a_nPoint, 0, this.MaxInfinitePoint);
		this.PageBattle.GaugeInfiniteUIsHandler.IncrPercent(this.InfinitePoint / (float)this.MaxInfinitePoint);

		// 게임 종료가 가능 할 경우
		if(this.InfinitePoint >= this.MaxInfinitePoint)
		{
			m_eWaveState = EWaveState.FINISH;
			this.UpdateInfiniteWaveUIsState();
			
			this.FinishBattle(true, 1.0f);
		}
	}


	/** 무한 웨이브 클리어 연출을 시작한다 */
	public void StartInfiniteWaveClearDirecting(int a_nWaveOrder, float a_fDuration)
	{
		// Do Something
	}

	/** 무한 웨이브 UI 상태를 갱신한다 */
	private void UpdateInfiniteWaveUIsState()
	{
		this.PageBattle.InfiniteNumKillsText.text = $"{m_oNumKillsStr} : {m_nNumKillNonPlayers}";

#if DEBUG || DEVELOPMENT_BUILD
		this.PageBattle.WaveText.text = $"{m_nWaveOrder + 1}";
#endif // #if DEBUG || DEVELOPMENT_BUILD
	}

	/** 플레이 웨이브 상태를 갱신한다 */
	private void UpdateInfiniteWaveStatePlay(float a_fDeltaTime)
	{
		m_fWaveSkipTime += a_fDeltaTime;
		m_fWaveNPCAppearSkipTime += a_fDeltaTime;

		// NPC 등장 처리가 필요 할 경우
		if (m_fWaveNPCAppearSkipTime.ExIsGreatEquals(m_oInfiniteTableList[m_nWaveOrder].IntervalNPC * ComType.G_UNIT_MS_TO_S))
		{
			m_fWaveNPCAppearSkipTime = 0.0f;

			var stBattlePlayInfo = this.BattlePlayInfo;
			stBattlePlayInfo.m_nStandardNPCLevel = m_oInfiniteTableList[m_nWaveOrder].StandardNPCLevel;

			this.BattlePlayInfo = stBattlePlayInfo;
			this.MakeAppearInfiniteWaveNPCs();
		}

#if DISABLE_THIS
		bool bIsMoveToNextWaveA = m_nNumMakeAppearWaveNPCs >= m_oInfiniteTableList[m_nWaveOrder].CountNPC && this.NonPlayerControllerList.Count <= 0;
		bool bIsMoveToNextWaveB = m_fWaveSkipTime.ExIsGreatEquals(m_oInfiniteTableList[m_nWaveOrder].timeWave * ComType.G_UNIT_MS_TO_S);
#else
		bool bIsMoveToNextWaveA = m_nNumMakeAppearWaveNPCs >= m_oInfiniteTableList[m_nWaveOrder].CountNPC;
		bool bIsMoveToNextWaveB = false;
#endif // #if DISABLE_THIS

		// 다음 웨이브 처리가 필요 할 경우
		if (bIsMoveToNextWaveA || bIsMoveToNextWaveB)
		{
			m_nWaveOrder += 1;
			m_eWaveState = EWaveState.INTERVAL;

			this.SetupInfiniteWaveInfo(m_nWaveOrder);
		}
	}

	/** 대기 웨이브 상태를 갱신한다 */
	private void UpdateInfiniteWaveStateInterval(float a_fDeltaTime)
	{
		m_fWaveIntervalSkipTime += a_fDeltaTime;

#if DISABLE_THIS
		// 다음 웨이브 플레이가 불가능 할 경우
		if (m_fWaveIntervalSkipTime.ExIsLess(m_oInfiniteTableList[m_nWaveOrder].IntervalWave * ComType.G_UNIT_MS_TO_S))
		{
			return;
		}
#endif // #if DISABLE_THIS

		m_fWaveSkipTime = 0.0f;
		m_fWaveIntervalSkipTime = 0.0f;
		m_fWaveNPCAppearSkipTime = 0.0f;

		m_eWaveState = EWaveState.PLAY;
	}

	/** 무한 웨이브 정보를 설정한다 */
	private void SetupInfiniteWaveInfo(int a_nWaveOrder)
	{
#if DISABLE_THIS
		m_fWaveSkipTime = m_fMaxWaveSkipTime = m_oInfiniteTableList[a_nWaveOrder].timeWave * ComType.G_UNIT_MS_TO_S;
		m_fMaxWaveIntervalSkipTime = m_oInfiniteTableList[a_nWaveOrder].IntervalWave * ComType.G_UNIT_MS_TO_S;
#else
		m_fWaveSkipTime = m_fMaxWaveSkipTime = 0.0f;
		m_fMaxWaveIntervalSkipTime = 0.0f;

		this.InstEXPRatio = m_oInfiniteTableList[a_nWaveOrder].IntanceExpRatio;
#endif // #if DISABLE_THIS

		m_nNumMakeAppearWaveNPCs = 0;
		this.InfiniteWaveNPCGroupTableList.Clear();

		int nNPCGroup = m_oInfiniteTableList[a_nWaveOrder].NPCGroup;
		NPCGroupTable.GetGroup(nNPCGroup, this.InfiniteWaveNPCGroupTableList);
	}

	/** 무한 웨이브 전투 종료를 처리한다 */
	private void TryHandleInfiniteFinishBattle()
	{
		// NPC 가 존재 할 경우
		if (this.NonPlayerControllerList.Count >= 1 || m_eWaveState == EWaveState.FINISH)
		{
			return;
		}

		m_eWaveState = EWaveState.FINISH;
		this.UpdateInfiniteWaveUIsState();

		this.FinishBattle(false, 1.0f);
	}

	/** 무한 웨이브 NPC 를 등장시킨다 */
	private void MakeAppearInfiniteWaveNPCs()
	{
		// 웨이브 정보가 없을 경우
		if (m_nWaveOrder >= m_oInfiniteTableList.Count)
		{
			return;
		}

		for (int i = 0; i < m_oInfiniteTableList[m_nWaveOrder].CountAppearNPC; ++i)
		{
			bool bIsValidA = m_nNumMakeAppearWaveNPCs < m_oInfiniteTableList[m_nWaveOrder].CountNPC;
			bool bIsValidB = this.NonPlayerControllerList.Count < m_nMaxNumMakeAppearNPCs;

			// NPC 생성이 불가능 할 경우
			if (!bIsValidA || !bIsValidB)
			{
				return;
			}

			int nSelectionFactor = 0;
			int nSumSelectionFactor = this.InfiniteWaveNPCGroupTableList.Sum((a_oGroupTable) => a_oGroupTable.SelectionFactor);

			float fRandomSelectionFactor = Random.Range(0.0f, (float)nSumSelectionFactor);

			for (int j = 0; j < this.InfiniteWaveNPCGroupTableList.Count; ++j)
			{
				nSelectionFactor += this.InfiniteWaveNPCGroupTableList[j].SelectionFactor;

				// NPC 생성이 가능 할 경우
				if (fRandomSelectionFactor.ExIsLess((float)nSelectionFactor))
				{
					this.MakeAppearInfiniteWaveNPCs(this.InfiniteWaveNPCGroupTableList[j]);
					break;
				}
			}
		}
	}

	/** 무한 웨이브 NPC 를 등장시킨다 */
	private void MakeAppearInfiniteWaveNPCs(NPCGroupTable a_oGroupTable)
	{
		m_nNumMakeAppearWaveNPCs += 1;

		var oNPCTable = NPCTable.GetData(a_oGroupTable.NPCKey);
		var stSpawnPosInfo = this.GetInfiniteWaveSpawnPos();

		var oNonPlayerController = this.CreateNonPlayer(oNPCTable,
			stSpawnPosInfo.Item2, this.PlayMapObjsRoot, null, a_oEffectTableList: this.NPCEffectTableList);

		oNonPlayerController.SetIsWaveNPC(true);
		oNonPlayerController.SetIsNeedUIs(false);
		oNonPlayerController.SetIsNeedAppear(stSpawnPosInfo.Item1 >= 0);

		oNonPlayerController.Canvas.SetActive(false);

		// 시작 위치가 존재 할 경우
		if(stSpawnPosInfo.Item1 >= 0)
		{
			var oSpawnPosHandler = this.SpawnPosHandlerList[stSpawnPosInfo.Item1];
			bool bIsValid = NavMesh.SamplePosition(oSpawnPosHandler.ObjInfo.m_oWayPointInfoList[0].m_stTransInfo.m_stPos + (Vector3.up * 1.5f), out NavMeshHit stNavMeshHit, float.MaxValue / 2.0f, m_nWalkableAreaMask);

			oNonPlayerController.SetStartPos(stNavMeshHit.position);
		}

		oNonPlayerController.LookTarget(this.PlayerController);
		oNonPlayerController.SetTrackingTarget(this.PlayerController);

		this.ExLateCallFunc((a_oSender) =>
		{
			float fStandardHP = oNonPlayerController.CurStandardAbilityValDict.GetValueOrDefault(EEquipEffectType.HP);
			float fStandardATK = oNonPlayerController.CurStandardAbilityValDict.GetValueOrDefault(EEquipEffectType.ATK);
			float fStandardAimingDelay = oNonPlayerController.CurStandardAbilityValDict.GetValueOrDefault(EEquipEffectType.AIMING_DELAY);

			string oPrefabPath = GameResourceManager.Singleton.GetPrefabPath(EResourceType.Character_NPC, 
				oNPCTable.Prefab, oNPCTable.Theme - 1);

			var oPrefab = Resources.Load<GameObject>(oPrefabPath);

			oNonPlayerController.transform.position = stSpawnPosInfo.Item2;
			oNonPlayerController.transform.localScale = oPrefab.transform.localScale;

			oNonPlayerController.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.HP, fStandardHP * m_oInfiniteTableList[m_nWaveOrder].RatioHP);
			oNonPlayerController.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.ATK, fStandardATK * m_oInfiniteTableList[m_nWaveOrder].RatioPowerAttack);
			oNonPlayerController.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.AIMING_DELAY, fStandardAimingDelay * m_oInfiniteTableList[m_nWaveOrder].RatioAimTime);

			oNonPlayerController.SetupAbilityValues(true);
			oNonPlayerController.OnBattlePlay();
		});
	}
	#endregion // 함수

	#region 접근 함수
	/** 무한 웨이브 등장 위치를 반환한다 */
	private (int, Vector3) GetInfiniteWaveSpawnPos()
	{
		int nIdx = Random.Range(0, this.SpawnPosHandlerList.Count);
		var stSpawnPos = this.SpawnPosHandlerList[nIdx].transform.position;

		bool bIsValid = NavMesh.SamplePosition(stSpawnPos + (Vector3.up * 1.5f), out NavMeshHit stNavMeshHit, float.MaxValue / 2.0f, m_nWalkableAreaMask);
		bIsValid = bIsValid && this.SpawnPosHandlerList[nIdx].ObjInfo.m_oWayPointInfoList.Count <= 0;

		// 내비게이션 영역이 존재 할 경우
		if (bIsValid)
		{
			return (-1, stNavMeshHit.position);
		}

		return (nIdx, stSpawnPos);
	}
	#endregion // 접근 함수
}

/** 전투 제어자 - 무한 (코루틴) */
public partial class BattleController : MonoBehaviour
{
	#region 함수
	/** 무한 웨이브를 시작한다 */
	private IEnumerator CoStartInfiniteWave()
	{
		yield return new WaitForSeconds(1.0f);
		this.PlayerController.OnBattlePlay();

		this.PageBattle.SetIsVisibleUIs(true);
		this.PageBattle.UpdateUIsState();
	}

	/** 포커스 연출을 시작한다 */
	private IEnumerator CoStartFocusDirecting()
	{
		this.IsPlaySecneDirecting = true;
		this.PageBattle.SetIsVisibleUIs(false);
		
		yield return YieldInstructionCache.WaitForSeconds(0.5f);

		this.PlaySound(m_oFocusAudioClip, null);

		this.PageBattle.StartCameraShakeDirecting(Vector3.zero, 1.5f, 0.25f);
		yield return YieldInstructionCache.WaitForSeconds(2.0f);

		this.InfiniteFocusUIs.SetActive(true);
		this.InfiniteFocusImg.color = Color.clear;

		var oAni = this.InfiniteFocusImg.DOColor(new Color(0.0f, 0.0f, 0.0f, 1.0f), 1.5f);
		ComUtil.AssignVal(ref m_oInfiniteFocusAni, oAni);

		yield return YieldInstructionCache.WaitForSeconds(1.0f);
		bool bIsNeedsFocusDirecting = PlayerPrefs.GetInt(ComType.G_KEY_IS_NEEDS_FOCUS_DIRECTING, 1) != 0;

		// 포커스 연출이 필요 없을 경우
		if(!bIsNeedsFocusDirecting)
		{
			this.IsPlaySecneDirecting = false;
			this.StateMachine.SetState(this.CreatePlayState());

			yield break;
		}

		this.InfiniteDescUIs.SetActive(true);
		this.InfiniteDescUIs.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f);

		this.InfiniteDescUIs.transform.DOScaleY(1.0f, 0.25f);
		yield return YieldInstructionCache.WaitForSeconds(0.25f);

		string oDescText = UIStringTable.GetValue("ui_hint_mission_zombie_desc_3");
		m_oInfiniteDescText.gameObject.SetActive(true);

		var oDescAni = m_oInfiniteDescText.DOText(oDescText, 3.0f);
		ComUtil.AssignVal(ref m_oInfiniteDescAni, oDescAni);

		yield return YieldInstructionCache.WaitForSeconds(3.5f);
		this.IsPlaySecneDirecting = false;

		yield return YieldInstructionCache.WaitForSeconds(0.5f);
		this.StateMachine.SetState(this.CreatePlayState());

		PlayerPrefs.SetInt(ComType.G_KEY_IS_NEEDS_FOCUS_DIRECTING, 0);
		PlayerPrefs.Save();
	}
	#endregion // 함수
}
