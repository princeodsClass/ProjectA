using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class LeagueTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public int Grade { get { return intValue((int)COLUMN.Grade); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public string Icon { get { return stringValue((int)COLUMN.Icon); } }
    public int RewardsGroup { get { return intValue((int)COLUMN.RewardsGroup); } }
    public int MinRank { get { return intValue((int)COLUMN.MinRank); } }
    public int MaxRank { get { return intValue((int)COLUMN.MaxRank); } }
}