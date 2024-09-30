using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RewardListTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint Group { get { return GetUINT((int)COLUMN.Group); } }
    public int SelectionFactor { get { return intValue((int)COLUMN.SelectionFactor); } }
    public uint RewardKey { get { return GetUINT((int)COLUMN.RewardKey); } }
    public int RewardCountMin { get { return intValue((int)COLUMN.RewardCountMin); } }
    public int RewardCountMax { get { return intValue((int)COLUMN.RewardCountMax); } }
}
