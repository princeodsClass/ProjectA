using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MissionAdventureTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int Group { get { return intValue((int)COLUMN.Group); } }
    public int Theme { get { return intValue((int)COLUMN.Theme); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int ChapterType { get { return intValue((int)COLUMN.ChapterType); } }
    public int RewardExp { get { return intValue((int)COLUMN.RewardExp); } }
    public int RewardPassExp { get { return intValue((int)COLUMN.RewardPassExp); } }
    public int RewardBattlePassExp { get { return intValue((int)COLUMN.RewardBattlePassExp); } }
    public uint ItemKey { get { return GetUINT((int)COLUMN.ItemKey); } }
    public int ItemCount { get { return intValue((int)COLUMN.ItemCount); } }
    public int BonusBuffGroup { get { return intValue((int)COLUMN.BonusBuffGroup); } }
    public uint AddBonusItemKey { get { return GetUINT((int)COLUMN.AddBonusItemKey); } }
    public int AddBonusItemCount { get { return intValue((int)COLUMN.AddBonusItemCount); } }
    public uint AutoCompItemKey { get { return GetUINT((int)COLUMN.AutoCompItemKey); } }
    public int AutoCompItemCount { get { return intValue((int)COLUMN.AutoCompItemCount); } }
    public int MaxCountRevive { get { return intValue((int)COLUMN.MaxCountRevive); } }
    public int ReviveItemCount { get { return intValue((int)COLUMN.ReviveItemCount); } }
    public int CorrectValueStandardLevel { get { return intValue((int)COLUMN.CorrectValueStandardLevel); } }
    public int RewardGroup { get { return intValue((int)COLUMN.RewardGroup); } }
    public uint RewardMain { get { return GetUINT((int)COLUMN.RewardMain); } }
}