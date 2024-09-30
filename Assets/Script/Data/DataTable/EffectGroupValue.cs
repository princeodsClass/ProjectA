using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EffectGroupTable
{
    public int PrimaryKey { get { return intValue((int)COLUMN.PrimaryKey); } }
    public int Group { get { return intValue((int)COLUMN.Group); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int SelectionType { get { return intValue((int)COLUMN.SelectionType); } }
    public int SelectionFactor { get { return intValue((int)COLUMN.SelectionFactor); } }
    public uint EffectKey { get { return GetUINT((int)COLUMN.EffectKey); } }
}
