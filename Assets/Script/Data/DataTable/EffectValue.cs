using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EffectTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public int Group { get { return intValue((int)COLUMN.Group); } }
    public string Icon { get { return stringValue((int)COLUMN.Icon); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int Category { get { return intValue((int)COLUMN.Category); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int OperationType { get { return intValue((int)COLUMN.OperationType); } }
    public int RangeType { get { return intValue((int)COLUMN.RangeType); } }
    public float Value { get { return floatValue((int)COLUMN.Value); } }
    public float ValueMin { get { return floatValue((int)COLUMN.ValueMin); } }
    public float ValueMax { get { return floatValue((int)COLUMN.ValueMax); } }
    public int Duration { get { return intValue((int)COLUMN.Duration); } }
    public int MaxStackCount { get { return intValue((int)COLUMN.MaxStackCount); } }
    public int WaitTime { get { return intValue((int)COLUMN.WaitTime); } }
    public int Inteval { get { return intValue((int)COLUMN.Inteval); } }
    public string PrefabFX { get { return stringValue((int)COLUMN.PrefabFX); } }
}
