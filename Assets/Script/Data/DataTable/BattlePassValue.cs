using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattlePassTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Lv { get { return intValue((int)COLUMN.Lv); } }
    public int Exp { get { return intValue((int)COLUMN.Exp); } }
    public uint NormalRewardItemKey { get { return GetUINT((int)COLUMN.NormalRewardItemKey); } }
    public int NormalRewardItemCount { get { return intValue((int)COLUMN.NormalRewardItemCount); } }
    public uint EliteRewardItemKey { get { return GetUINT((int)COLUMN.EliteRewardItemKey); } }
    public int EliteRewardItemCount { get { return intValue((int)COLUMN.EliteRewardItemCount); } }
    public uint PlusRewardItemKey { get { return GetUINT((int)COLUMN.PlusRewardItemKey); } }
    public int PlusRewardItemCount { get { return intValue((int)COLUMN.PlusRewardItemCount); } }
    public uint SkipItemKey { get { return GetUINT((int)COLUMN.SkipItemKey); } }
    public int SkipItemCount { get { return intValue((int)COLUMN.SkipItemCount); } }
}
