using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoxTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int SubType { get { return intValue((int)COLUMN.SubType); } }
    public int Grade { get { return intValue((int)COLUMN.Grade); } }
    public int Level { get { return intValue((int)COLUMN.Level); } }
    public int AbleToBoost { get { return intValue((int)COLUMN.AbleToBoost); } }
    public int AbleToBoostFree { get { return intValue((int)COLUMN.AbleToBoostFree); } }
    public int AbleToADOpen { get { return intValue((int)COLUMN.AbleToADOpen); } }
    public int BoostTutorial { get { return intValue((int)COLUMN.BoostTutorial); } }
    public string Prefeb { get { return stringValue((int)COLUMN.Prefeb); } }
    public string Icon { get { return stringValue((int)COLUMN.Icon); } }
    public int OpenTime { get { return intValue((int)COLUMN.OpenTime); } }
    public uint OpenMaterialKey { get { return GetUINT((int)COLUMN.OpenMaterialKey); } }
    public int OpenMaterialCount { get { return intValue((int)COLUMN.OpenMaterialCount); } }
    public int RewardGroup { get { return intValue((int)COLUMN.RewardGroup); } }
}
