using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CrystalDealTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public string Icon { get { return stringValue((int)COLUMN.Icon); } }
    public uint CostItemKey { get { return GetUINT((int)COLUMN.CostItemKey); } }
    public int CostItemCount { get { return intValue((int)COLUMN.CostItemCount); } }
    public string IAPAOS { get { return stringValue((int)COLUMN.IAPAOS); } }
    public string IAPiOS { get { return stringValue((int)COLUMN.IAPiOS); } }
    public uint RewardItemKey { get { return GetUINT((int)COLUMN.RewardItemKey); } }
    public int RewardItemCount { get { return intValue((int)COLUMN.RewardItemCount); } }
    public int RewardsBonus { get { return intValue((int)COLUMN.RewardsBonus); } }
}