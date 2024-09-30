using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AccountLevelTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Level { get { return intValue((int)COLUMN.Level); } }
    public int Exp { get { return intValue((int)COLUMN.Exp); } }
    public int HP { get { return intValue((int)COLUMN.HP); } }
    public int DP { get { return intValue((int)COLUMN.DP); } }
    public uint RewardCharacterKey { get { return GetUINT((int)COLUMN.RewardCharacterKey); } }
    public uint RewardWeaponKey { get { return GetUINT((int)COLUMN.RewardWeaponKey); } }
    public uint RewarditemKey00 { get { return GetUINT((int)COLUMN.RewarditemKey00); } }
    public int Rewarditemcount00 { get { return intValue((int)COLUMN.Rewarditemcount00); } }
    public uint RewarditemKey01 { get { return GetUINT((int)COLUMN.RewarditemKey01); } }
    public int Rewarditemcount01 { get { return intValue((int)COLUMN.Rewarditemcount01); } }
    public uint RewarditemKey02 { get { return GetUINT((int)COLUMN.RewarditemKey02); } }
    public int Rewarditemcount02 { get { return intValue((int)COLUMN.Rewarditemcount02); } }
    public uint RewarditemKey03 { get { return GetUINT((int)COLUMN.RewarditemKey03); } }
    public int Rewarditemcount03 { get { return intValue((int)COLUMN.Rewarditemcount03); } }
    public uint RewarditemKey04 { get { return GetUINT((int)COLUMN.RewarditemKey04); } }
    public int Rewarditemcount04 { get { return intValue((int)COLUMN.Rewarditemcount04); } }
    public uint RewarditemKey05 { get { return GetUINT((int)COLUMN.RewarditemKey05); } }
    public int Rewarditemcount05 { get { return intValue((int)COLUMN.Rewarditemcount05); } }
    public uint RewarditemKey06 { get { return GetUINT((int)COLUMN.RewarditemKey06); } }
    public int Rewarditemcount06 { get { return intValue((int)COLUMN.Rewarditemcount06); } }
    public uint RewarditemKey07 { get { return GetUINT((int)COLUMN.RewarditemKey07); } }
    public int Rewarditemcount07 { get { return intValue((int)COLUMN.Rewarditemcount07); } }
    public int StandardNPCLevel { get { return intValue((int)COLUMN.StandardNPCLevel); } }
    public int TargetQuestPoint { get { return intValue((int)COLUMN.TargetQuestPoint); } }
    public uint QuestRewardsKey00 { get { return GetUINT((int)COLUMN.QuestRewardsKey00); } }
    public int QuestRewardsCount00 { get { return intValue((int)COLUMN.QuestRewardsCount00); } }
    public uint QuestRewardsKey01 { get { return GetUINT((int)COLUMN.QuestRewardsKey01); } }
    public int QuestRewardsCount01 { get { return intValue((int)COLUMN.QuestRewardsCount01); } }
    public int BonusTargetPoint { get { return intValue((int)COLUMN.BonusTargetPoint); } }
    public int Standard { get { return intValue((int)COLUMN.Standard); } }
    public int BonusMaintain { get { return intValue((int)COLUMN.BonusMaintain); } }
    public int BonusChapterMaintain { get { return intValue((int)COLUMN.BonusChapterMaintain); } }
}
