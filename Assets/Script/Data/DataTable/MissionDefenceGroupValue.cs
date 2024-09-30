public partial class MissionDefenceGroupTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int Theme { get { return intValue((int)COLUMN.Theme); } }
    public int Difficulty { get { return intValue((int)COLUMN.Difficulty); } }
    public int WaveType { get { return intValue((int)COLUMN.WaveType); } }
    public uint ItemKey { get { return GetUINT((int)COLUMN.ItemKey); } }
    public int ItemCount { get { return intValue((int)COLUMN.ItemCount); } }
    public int BonusBuffGroup { get { return intValue((int)COLUMN.BonusBuffGroup); } }
    public uint AddBonusItemKey { get { return GetUINT((int)COLUMN.AddBonusItemKey); } }
    public int AddBonusItemCount { get { return intValue((int)COLUMN.AddBonusItemCount); } }
    public int MaxCountRevive { get { return intValue((int)COLUMN.MaxCountRevive); } }
    public uint ReviveItemKey { get { return GetUINT((int)COLUMN.ReviveItemKey); } }
    public int ReviveItemCount { get { return intValue((int)COLUMN.ReviveItemCount); } }
}