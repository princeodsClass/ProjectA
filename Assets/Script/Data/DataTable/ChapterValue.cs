using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ChapterTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    // public int RewardExp { get { return intValue((int)COLUMN.RewardExp); } }
    // public int RewardPassExp { get { return intValue((int)COLUMN.RewardPassExp); } }
    public int MaxCountRevive { get { return intValue((int)COLUMN.MaxCountRevive); } }
    public uint ReviveItemKey { get { return GetUINT((int)COLUMN.ReviveItemKey); } }
    public int ReviveItemCount { get { return intValue((int)COLUMN.ReviveItemCount); } }
    public int Episode { get { return intValue((int)COLUMN.Episode); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int TargetEndPoint { get { return intValue((int)COLUMN.TargetEndPoint); } }
    public int StandardLevel { get { return intValue((int)COLUMN.StandardLevel); } }
    public int RewardGroup { get { return intValue((int)COLUMN.RewardGroup); } }
    public uint RewardItemKey { get { return GetUINT((int)COLUMN.RewardItemKey); } }
    public int RewardItemCount { get { return intValue((int)COLUMN.RewardItemCount); } }
}
