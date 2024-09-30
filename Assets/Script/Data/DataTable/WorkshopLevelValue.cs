using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WorkshopLevelTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Level { get { return intValue((int)COLUMN.Level); } }
    public int Exp { get { return intValue((int)COLUMN.exp); } }
}
