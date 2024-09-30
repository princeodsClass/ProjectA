using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class VIPDealTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int NameKey { get { return intValue((int)COLUMN.NameKey); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public string ImageBG { get { return stringValue((int)COLUMN.ImageBG); } }
    public string ImageIcon { get { return stringValue((int)COLUMN.ImageIcon); } }
    public int BuyType { get { return intValue((int)COLUMN.BuyType); } }
    public int IAPAOS_Discount { get { return intValue((int)COLUMN.IAPAOS_Discount); } }
    public int IAPiOS_Discount { get { return intValue((int)COLUMN.IAPiOS_Discount); } }
    public string IAPAOS_Original { get { return stringValue((int)COLUMN.IAPAOS_Original); } }
    public string IAPiOS_Original { get { return stringValue((int)COLUMN.IAPiOS_Original); } }
    public string IAPAOS { get { return stringValue((int)COLUMN.IAPAOS); } }
    public string IAPiOS { get { return stringValue((int)COLUMN.IAPiOS); } }
    public int Period { get { return intValue((int)COLUMN.Period); } }
    public uint RewardItemKey0 { get { return GetUINT((int)COLUMN.RewardItemKey0); } }
    public int RewardItemCount0 { get { return intValue((int)COLUMN.RewardItemCount0); } }
    public uint RewardItemKey1 { get { return GetUINT((int)COLUMN.RewardItemKey1); } }
    public int RewardItemCount1 { get { return intValue((int)COLUMN.RewardItemCount1); } }
    public uint RewardItemKey2 { get { return GetUINT((int)COLUMN.RewardItemKey2); } }
    public int RewardItemCount2 { get { return intValue((int)COLUMN.RewardItemCount2); } }
    public int RewardsBonus { get { return intValue((int)COLUMN.RewardsBonus); } }
}