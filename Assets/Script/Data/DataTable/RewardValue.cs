using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RewardTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Group { get { return intValue((int)COLUMN.Group); } }
    public int SelectionCount { get { return intValue((int)COLUMN.SelectionCount); } }
    public int SelectionType { get { return intValue((int)COLUMN.SelectionType); } }
    public int SelectionFactor { get { return intValue((int)COLUMN.SelectionFactor); } }
    public uint RewardListGroup { get { return GetUINT((int)COLUMN.RewardListGroup); } }
    public uint ItemKey { get { return GetUINT((int)COLUMN.ItemKey); } }
    public string Icon { get { return stringValue((int)COLUMN.Icon); } }
    public int MinCount { get { return intValue((int)COLUMN.MinCount); } }
    public int MaxCount { get { return intValue((int)COLUMN.MaxCount); } }
}
