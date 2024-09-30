using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoxDealTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint BoxKey { get { return GetUINT((int)COLUMN.BoxKey); } }
    public int Group { get { return intValue((int)COLUMN.Group); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int SelectionFactor { get { return intValue((int)COLUMN.SelectionFactor); } }
    public uint CostItemKey { get { return GetUINT((int)COLUMN.CostItemKey); } }
    public int CostItemCount { get { return intValue((int)COLUMN.CostItemCount); } }
    public uint CostTokenKey { get { return GetUINT((int)COLUMN.CostTokenKey); } }
    public int CostTokenCount { get { return intValue((int)COLUMN.CostTokenCount); } }
    public uint CostResetKey { get { return GetUINT((int)COLUMN.CostResetKey); } }
    public int CostResetStandardCount { get { return intValue((int)COLUMN.CostResetStandardCount); } }
    public int CostResetAdditionalCount { get { return intValue((int)COLUMN.CostResetAdditionalCount); } }
    public float PercentageSymbol { get { return floatValue((int)COLUMN.PercentageSymbol); } }
    public float Percentage0 { get { return floatValue((int)COLUMN.Percentage0); } }
    public float Percentage1 { get { return floatValue((int)COLUMN.Percentage1); } }
    public float Percentage2 { get { return floatValue((int)COLUMN.Percentage2); } }
    public float Percentage3 { get { return floatValue((int)COLUMN.Percentage3); } }
    public float Percentage4 { get { return floatValue((int)COLUMN.Percentage4); } }
    public float Percentage5 { get { return floatValue((int)COLUMN.Percentage5); } }
    public float Percentage6 { get { return floatValue((int)COLUMN.Percentage6); } }

}