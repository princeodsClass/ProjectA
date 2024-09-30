using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ADSDealTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public string Icon { get { return stringValue((int)COLUMN.Icon); } }
    public int Level { get { return intValue((int)COLUMN.Level); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int SelectionFactor { get { return intValue((int)COLUMN.SelectionFactor); } }
    public int SelectionType { get { return intValue((int)COLUMN.SelectionType); } }
    public uint ItemKey { get { return GetUINT((int)COLUMN.ItemKey); } }
    public int Count { get { return intValue((int)COLUMN.Count); } }
    public int LimitCount { get { return intValue((int)COLUMN.LimitCount); } }
    public int TimeRepurchase { get { return intValue((int)COLUMN.TimeRepurchase); } }
}
