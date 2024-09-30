public partial class QuestTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Level { get { return intValue((int)COLUMN.Level); } }
    public uint TitleKey { get { return GetUINT((int)COLUMN.TitleKey); } }
    public int Action { get { return intValue((int)COLUMN.Action); } }
    public int RequireCount { get { return intValue((int)COLUMN.RequireCount); } }
    public int SelectionFactor { get { return intValue((int)COLUMN.SelectionFactor); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int Point { get { return intValue((int)COLUMN.Point); } }
    public uint RewardsKey00 { get { return GetUINT((int)COLUMN.RewardsKey00); } }
    public int RewardsCount00 { get { return intValue((int)COLUMN.RewardsCount00); } }
    public uint RewardsKey01 { get { return GetUINT((int)COLUMN.RewardsKey01); } }
    public int RewardsCount01 { get { return intValue((int)COLUMN.RewardsCount01); } }
}
