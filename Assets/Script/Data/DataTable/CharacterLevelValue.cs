using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CharacterLevelTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint CharacterKey { get { return GetUINT((int)COLUMN.CharacterKey); } }
    public int Lv { get { return intValue((int)COLUMN.Lv); } }
    public uint RequireItemKey00 { get { return GetUINT((int)COLUMN.RequireItemKey00); } }
    public int RequireItemCount00 { get { return intValue((int)COLUMN.RequireItemCount00); } }
    public uint RequireItemKey01 { get { return GetUINT((int)COLUMN.RequireItemKey01); } }
    public int RequireItemCount01 { get { return intValue((int)COLUMN.RequireItemCount01); } }
    public int HP { get { return intValue((int)COLUMN.HP); } }
    public int DP { get { return intValue((int)COLUMN.DP); } }
    public float MoveSpeed { get { return floatValue((int)COLUMN.MoveSpeed); } }
}