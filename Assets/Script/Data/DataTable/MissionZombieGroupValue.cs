public partial class MissionZombieGroupTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int Episode { get { return intValue((int)COLUMN.Episode); } }
    public int StartWave { get { return intValue((int)COLUMN.StartWave); } }
    public uint ItemKey { get { return GetUINT((int)COLUMN.ItemKey); } }
    public int ItemCount { get { return intValue((int)COLUMN.ItemCount); } }
    public int MaxCountRevive { get { return intValue((int)COLUMN.MaxCountRevive); } }
    public uint ReviveItemKey { get { return GetUINT((int)COLUMN.ReviveItemKey); } }
    public int ReviveItemCount { get { return intValue((int)COLUMN.ReviveItemCount); } }
}