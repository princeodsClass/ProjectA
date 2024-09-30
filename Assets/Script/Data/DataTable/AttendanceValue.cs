using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AttendanceTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public string Title { get { return stringValue((int)COLUMN.Title); } }
    public int Level { get { return intValue((int)COLUMN.Level); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public uint RewardKey { get { return GetUINT((int)COLUMN.RewardKey); } }
    public int RewardCount { get { return intValue((int)COLUMN.RewardCount); } }
}
