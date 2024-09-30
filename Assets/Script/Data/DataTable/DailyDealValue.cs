using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DailyDealTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Level { get { return intValue((int)COLUMN.Level); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int SelectionFactor { get { return intValue((int)COLUMN.SelectionFactor); } }
    public int SelectionType { get { return intValue((int)COLUMN.SelectionType); } }
    public uint ItemKey { get { return GetUINT((int)COLUMN.ItemKey); } }
    public int Count { get { return intValue((int)COLUMN.Count); } }
    public uint CostItemKey { get { return GetUINT((int)COLUMN.CostItemKey); } }
    public int CostItemCount { get { return intValue((int)COLUMN.CostItemCount); } }
}
