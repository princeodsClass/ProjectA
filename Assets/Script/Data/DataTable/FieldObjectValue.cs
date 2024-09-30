using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class FieldObjectTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public string Prefab { get { return stringValue((int)COLUMN.Prefab); } }
    public int InteractionRange { get { return intValue((int)COLUMN.InteractionRange); } }
    public int CastingTime { get { return intValue((int)COLUMN.CastingTime); } }
    public int GainEffectGroup { get { return intValue((int)COLUMN.GainEffectGroup); } }
}
