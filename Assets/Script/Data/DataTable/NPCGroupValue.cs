using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NPCGroupTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Group { get { return intValue((int)COLUMN.Group); } }
    public int SelectionFactor { get { return intValue((int)COLUMN.SelectionFactor); } }
    public uint NPCKey { get { return GetUINT((int)COLUMN.NPCKey); } }
}
