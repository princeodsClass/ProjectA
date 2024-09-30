using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AbyssTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int Group { get { return intValue((int)COLUMN.Group); } }
    public int Theme { get { return intValue((int)COLUMN.Theme); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int LimitTime { get { return intValue((int)COLUMN.LimitTime); } }
    public int RewardExp { get { return intValue((int)COLUMN.RewardExp); } }
    public int RewardPassExp { get { return intValue((int)COLUMN.RewardPassExp); } }
    public int RewardBattlePassExp { get { return intValue((int)COLUMN.RewardBattlePassExp); } }
    public int BonusBuffGroup { get { return intValue((int)COLUMN.BonusBuffGroup); } }
    public int MaxCountRevive { get { return intValue((int)COLUMN.MaxCountRevive); } }
    public uint ReviveItemKey { get { return GetUINT((int)COLUMN.ReviveItemKey); } }
    public int ReviveItemCount { get { return intValue((int)COLUMN.ReviveItemCount); } }
    public int StandardNPCLevel { get { return intValue((int)COLUMN.StandardNPCLevel); } }
    public int TargetPoint { get { return intValue((int)COLUMN.TargetPoint); } }    
}