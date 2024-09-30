using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/** NPC 제어자 - 스킬 */
public partial class NonPlayerController : UnitController
{
	#region 함수
	/** 스킬을 적용한다 */
	public override void ApplySkill(SkillTable a_oSkillTable, bool a_bIsContinuing)
	{
		// FIXME: 스킬 사용 처리 구문 처리 필요

		this.ApplySkill(a_oSkillTable, 
			this.BattleController.PlayerController.transform.position, this.BattleController.PlayerController, a_bIsContinuing);
	}

	/** 스킬 그룹을 적용한다 */
	public override void ApplySkillGroup(int a_nGroup, bool a_bIsContinuing, bool a_bIsIgnoreNextSelectionSkill = false)
	{
		base.ApplySkillGroup(a_nGroup, a_bIsContinuing, a_bIsIgnoreNextSelectionSkill);

		// 다음 선택 발동 스킬이 없을 경우
		if(a_bIsIgnoreNextSelectionSkill || this.NextSelectionSkillGroupTable == null)
		{
			return;
		}

		var oSkillTable = SkillTable.GetData(this.NextSelectionSkillGroupTable.Skill);
		this.ApplySkillGroup(this.NextSelectionSkillGroupTable, false);

		// 스킬 이름이 존재 할 경우
		if(oSkillTable.NameKey > 0)
		{
			this.ShowTalk(NameTable.GetValue(oSkillTable.NameKey), true);
		}

		this.SetupApplySkillInfo(oSkillTable, a_bIsContinuing);
	}

	/** 스킬 적용이 완료 되었을 경우 */
	protected override void OnCompleteApplySkill(SkillController a_oSender)
	{
		base.OnCompleteApplySkill(a_oSender);

		// 지속 적용 스킬이 아닐 경우
		if (!a_oSender.Params.m_bIsContinuing || this.BattleController.TargetNonPlayerControllerList.Count <= 0)
		{
			return;
		}

		this.ApplySkill(a_oSender.Params.m_oTable, true);
	}

	/** 다음 선택 스킬을 설정한다 */
	private void SetupNextSelectionSkillGroupTable()
	{
		var oGroupTableList = CCollectionPoolManager.Singleton.SpawnList<SkillGroupTable>();
		var oSelectionGroupTable = CCollectionPoolManager.Singleton.SpawnList<SkillGroupTable>();

		try
		{
			SkillGroupTable.GetGroup(this.StatTable.AttackSkillGroup, oGroupTableList);

			for (int i = 0; i < oGroupTableList.Count; ++i)
			{
				// 상시 발동 스킬 일 경우
				if (oGroupTableList[i].SelectionType >= 1)
				{
					continue;
				}

				oSelectionGroupTable.Add(oGroupTableList[i]);
			}

			int nSelectionFactor = 0;
			int nSumSelectionFactor = oSelectionGroupTable.Sum((a_oGroupTable) => a_oGroupTable.SelectionFactor);

			float fRandomSelectionFactor = Random.Range(0.0f, (float)nSumSelectionFactor);

			for (int i = 0; i < oSelectionGroupTable.Count; ++i)
			{
				nSelectionFactor += oSelectionGroupTable[i].SelectionFactor;

				// 스킬 발동이 가능 할 경우
				if (fRandomSelectionFactor.ExIsLess((float)nSelectionFactor))
				{
					this.NextSelectionSkillGroupTable = oSelectionGroupTable[i];
					break;
				}
			}
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oGroupTableList);
			CCollectionPoolManager.Singleton.DespawnList(oSelectionGroupTable);
		}
	}

	/** 피격 스킬을 적용한다 */
	private void TryApplyHitSkill()
	{
		// 체력이 없을 경우
		if (this.HP <= 0)
		{
			return;
		}

		int nPlayEpisodeID = GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID;
		var oSkillInfoDict = CCollectionPoolManager.Singleton.SpawnDict<string, (int, float)>();

		try
		{
			if (StatTable.HPSkillGroup25 > 0) oSkillInfoDict.Add(ComType.G_HP_SKILL_GROUP_25, (this.StatTable.HPSkillGroup25, 0.25f));
			if (StatTable.HPSkillGroup50 > 0) oSkillInfoDict.Add(ComType.G_HP_SKILL_GROUP_50, (this.StatTable.HPSkillGroup50, 0.5f));
			if (StatTable.HPSkillGroup75 > 0) oSkillInfoDict.Add(ComType.G_HP_SKILL_GROUP_75, (this.StatTable.HPSkillGroup75, 0.75f));

			bool bIsApplyHitSkill = false;
			float fPercent = this.HP / (float)this.MaxHP;

			foreach (var stKeyVal in oSkillInfoDict)
			{
				// 스킬 사용이 가능 할 경우
				if (!m_oApplyHitSkillList.Contains(stKeyVal.Key) && fPercent.ExIsLessEquals(stKeyVal.Value.Item2))
				{
					bIsApplyHitSkill = true;
					m_oApplyHitSkillList.Add(stKeyVal.Key);

					this.GaugeUIsHandler.SetPercent(fPercent);
					this.ExtraGaugeUIsHandler?.SetPercent(fPercent);

					this.ApplySkillGroup(stKeyVal.Value.Item1, false);
				}
			}

#if DISABLE_THIS
			// 초반 에피소드 일 경우
			if (bIsApplyHitSkill && nPlayEpisodeID <= 0)
			{
				int nIsCompleteNPCSummonSkillTutorial = PlayerPrefs.GetInt(ComType.HelpTutorialKindsKeyDict[EHelpTutorialKinds.NPC_SUMMON_SKILL]);

				// NPC 소환 스킬 튜토리얼이 가능 할 경우
				if (nIsCompleteNPCSummonSkillTutorial <= 0)
				{
					ComUtil.SetTimeScale(0.0f);
					this.PageBattle.ShowHelpTutorialUIs(EHelpTutorialKinds.NPC_SUMMON_SKILL);
				}
			}
#endif // #if DISABLE_THIS
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnDict(oSkillInfoDict);
		}
	}
	#endregion // 함수
}
