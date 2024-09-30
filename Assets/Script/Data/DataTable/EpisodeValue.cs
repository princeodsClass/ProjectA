using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EpisodeTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int MaxChapter { get { return intValue((int)COLUMN.MaxChapter); } }
    public string PrefebMini { get { return stringValue((int)COLUMN.PrefebMini); } }
    public string Icon { get { return stringValue((int)COLUMN.Icon); } }
    public int RewardGroup { get { return intValue((int)COLUMN.RewardGroup); } }
    public string BGMLobby { get { return stringValue((int)COLUMN.BGMLobby); } }
    public string BGMBattle { get { return stringValue((int)COLUMN.BGMBattle); } }
    public string BGMMidBoss { get { return stringValue((int)COLUMN.BGMMidBoss); } }
    public string BGMBoss { get { return stringValue((int)COLUMN.BGMBoss); } }
}
