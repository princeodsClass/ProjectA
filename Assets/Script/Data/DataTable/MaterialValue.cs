using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MaterialTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public uint ContentsForGetKey00 { get { return GetUINT((int)COLUMN.ContentsForGetKey00); } }
    public uint ContentsForGetKey01 { get { return GetUINT((int)COLUMN.ContentsForGetKey01); } }
    public uint ContentsForGetKey02 { get { return GetUINT((int)COLUMN.ContentsForGetKey02); } }
    public uint ContentsForGetKey03 { get { return GetUINT((int)COLUMN.ContentsForGetKey03); } }
    public uint ContentsForGetKey04 { get { return GetUINT((int)COLUMN.ContentsForGetKey04); } }
    public uint ContentsForGetKey05 { get { return GetUINT((int)COLUMN.ContentsForGetKey05); } }
    public uint ContentsForGetKey06 { get { return GetUINT((int)COLUMN.ContentsForGetKey06); } }
    public uint ContentsForGetKey07 { get { return GetUINT((int)COLUMN.ContentsForGetKey07); } }
    public uint ContentsForGetKey08 { get { return GetUINT((int)COLUMN.ContentsForGetKey08); } }
    public uint ContentsForGetKey09 { get { return GetUINT((int)COLUMN.ContentsForGetKey09); } }
    public uint ContentsForUseKey00 { get { return GetUINT((int)COLUMN.ContentsForUseKey00); } }
    public uint ContentsForUseKey01 { get { return GetUINT((int)COLUMN.ContentsForUseKey01); } }
    public uint ContentsForUseKey02 { get { return GetUINT((int)COLUMN.ContentsForUseKey02); } }
    public uint ContentsForUseKey03 { get { return GetUINT((int)COLUMN.ContentsForUseKey03); } }
    public uint ContentsForUseKey04 { get { return GetUINT((int)COLUMN.ContentsForUseKey04); } }
    public uint ContentsForUseKey05 { get { return GetUINT((int)COLUMN.ContentsForUseKey05); } }
    public uint ContentsForUseKey06 { get { return GetUINT((int)COLUMN.ContentsForUseKey06); } }
    public uint ContentsForUseKey07 { get { return GetUINT((int)COLUMN.ContentsForUseKey07); } }
    public uint ContentsForUseKey08 { get { return GetUINT((int)COLUMN.ContentsForUseKey08); } }
    public uint ContentsForUseKey09 { get { return GetUINT((int)COLUMN.ContentsForUseKey09); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int SubType { get { return intValue((int)COLUMN.SubType); } }
    public int Grade { get { return intValue((int)COLUMN.Grade); } }
    public string Icon { get { return stringValue((int)COLUMN.Icon); } }
    public string IconSymbol { get { return stringValue((int)COLUMN.IconSymbol); } }
    public string Prefab { get { return stringValue((int)COLUMN.Prefab); } }
    public int MaxStackCount { get { return intValue((int)COLUMN.MaxStackCount); } }
}
