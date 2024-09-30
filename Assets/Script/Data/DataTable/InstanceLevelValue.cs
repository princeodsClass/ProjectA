using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InstanceLevelTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Level { get { return intValue((int)COLUMN.Level); } }
    public int Exp { get { return intValue((int)COLUMN.Exp); } }
    public int AdditionHP { get { return intValue((int)COLUMN.AdditionHP); } }
    public int AdditionDP { get { return intValue((int)COLUMN.AdditionDP); } }
    public int HPRecovery { get { return intValue((int)COLUMN.HPRecovery); } }
    public int BonusEffectGroup { get { return intValue((int)COLUMN.BonusEffectGroup); } }
}
