using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MissionZombieTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int WaveType { get { return intValue((int)COLUMN.WaveType); } }
    public float IntanceExpRatio { get { return floatValue((int)COLUMN.IntanceExpRatio); } }
    public int CountAppearNPC { get { return intValue((int)COLUMN.CountAppearNPC); } }
    public int NPCGroup { get { return intValue((int)COLUMN.NPCGroup); } }
    public int StandardNPCLevel { get { return intValue((int)COLUMN.StandardNPCLevel); } }
    public int CountNPC { get { return intValue((int)COLUMN.CountNPC); } }
    public float RatioHP { get { return floatValue((int)COLUMN.RatioHP); } }
    public float RatioAimTime { get { return floatValue((int)COLUMN.RatioAimTime); } }
    public float RatioPowerAttack { get { return floatValue((int)COLUMN.RatioPowerAttack); } }
    public int IntervalNPC { get { return intValue((int)COLUMN.IntervalNPC); } }
}