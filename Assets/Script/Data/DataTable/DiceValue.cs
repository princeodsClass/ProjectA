using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DiceTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Grade { get { return intValue((int)COLUMN.Grade); } }
    public string Icon { get { return stringValue((int)COLUMN.Icon); } }
    public int AbleToAgain { get { return intValue((int)COLUMN.AbleToAgain); } }
    public uint AgainMaterialKey { get { return GetUINT((int)COLUMN.AgainMaterialKey); } }
    public int AgainMaterialCount { get { return intValue((int)COLUMN.AgainMaterialCount); } }
    public int AbleToADAgain { get { return intValue((int)COLUMN.AbleToADAgain); } }
    public int RewardGroup { get { return intValue((int)COLUMN.RewardGroup); } }
}
