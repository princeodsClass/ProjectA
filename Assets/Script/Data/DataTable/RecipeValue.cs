using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RecipeTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Category { get { return intValue((int)COLUMN.Category); } }
    public uint TypeKey { get { return GetUINT((int)COLUMN.TypeKey); } }
    public uint TitleKey { get { return GetUINT((int)COLUMN.TitleKey); } }
    public int RewardExp { get { return intValue((int)COLUMN.RewardExp); } }
    public int RewardWorkshopExp { get { return intValue((int)COLUMN.RewardWorkshopExp); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int AvailableWorkshopLevel { get { return intValue((int)COLUMN.AvailableWorkshopLevel); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public uint SymbolItemKey { get { return GetUINT((int)COLUMN.SymbolItemKey); } }
    public int IsRandom { get { return intValue((int)COLUMN.IsRandom); } }
    public int isMassiveMaterial { get { return intValue((int)COLUMN.isMassiveMaterial); } }
    public int TypeHigh { get { return intValue((int)COLUMN.TypeHigh); } }
    public int TypeLow { get { return intValue((int)COLUMN.TypeLow); } }
    public int TypeGrade { get { return intValue((int)COLUMN.TypeGrade); } }
    public int NeedCount { get { return intValue((int)COLUMN.NeedCount); } }
    public uint CostItemKey { get { return GetUINT((int)COLUMN.CostItemKey); } }
    public int CostItemCount { get { return intValue((int)COLUMN.CostItemCount); } }
    public int OutputGroup { get { return intValue((int)COLUMN.OutputGroup); } }
    public int OutputCount { get { return intValue((int)COLUMN.OutputCount); } }
}
