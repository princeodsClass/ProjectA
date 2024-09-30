using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PassDealTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public string Image { get { return stringValue((int)COLUMN.Image); } }
    public string IAPAOS { get { return stringValue((int)COLUMN.IAPAOS); } }
    public string IAPiOS { get { return stringValue((int)COLUMN.IAPiOS); } }
    public string TargetCode { get { return stringValue((int)COLUMN.TargetCode); } }
    public int RewardsBonus { get { return intValue((int)COLUMN.RewardsBonus); } }
}