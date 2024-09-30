using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CharacterTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public string Icon { get { return stringValue((int)COLUMN.Icon); } }
    public string Prefab { get { return stringValue((int)COLUMN.Prefab); } }
    public int Grade { get { return intValue((int)COLUMN.Grade); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public uint RequireItemKey { get { return GetUINT((int)COLUMN.RequireItemKey); } }
    public int RequireItemCount { get { return intValue((int)COLUMN.RequireItemCount); } }
    public uint PreRequireItemKey { get { return GetUINT((int)COLUMN.PreRequireItemKey); } }
    public int PreRequireItemCount { get { return intValue((int)COLUMN.PreRequireItemCount); } }
    public uint Skill0501 { get { return GetUINT((int)COLUMN.Skill0501); } }
    public uint Skill1001 { get { return GetUINT((int)COLUMN.Skill1001); } }
    public uint Skill1501 { get { return GetUINT((int)COLUMN.Skill1501); } }
    public uint Skill1502 { get { return GetUINT((int)COLUMN.Skill1502); } }
    public uint Skill2001 { get { return GetUINT((int)COLUMN.Skill2001); } }
    public uint Skill2501 { get { return GetUINT((int)COLUMN.Skill2501); } }
    public uint Skill2502 { get { return GetUINT((int)COLUMN.Skill2502); } }
    public uint Skill3001 { get { return GetUINT((int)COLUMN.Skill3001); } }
    public uint Skill3501 { get { return GetUINT((int)COLUMN.Skill3501); } }
    public uint Skill3502 { get { return GetUINT((int)COLUMN.Skill3502); } }
    public uint Skill4001 { get { return GetUINT((int)COLUMN.Skill4001); } }
    public uint Skill4501 { get { return GetUINT((int)COLUMN.Skill4501); } }
    public uint Skill4502 { get { return GetUINT((int)COLUMN.Skill4502); } }
    public uint Skill5001 { get { return GetUINT((int)COLUMN.Skill5001); } }
    public uint Skill5501 { get { return GetUINT((int)COLUMN.Skill5501); } }
    public uint Skill5502 { get { return GetUINT((int)COLUMN.Skill5502); } }
    public uint Skill6001 { get { return GetUINT((int)COLUMN.Skill6001); } }
    public uint Skill6501 { get { return GetUINT((int)COLUMN.Skill6501); } }
    public uint Skill6502 { get { return GetUINT((int)COLUMN.Skill6502); } }
}
