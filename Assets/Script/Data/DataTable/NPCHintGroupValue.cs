public partial class NPCHintGroupTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Group { get { return intValue((int)COLUMN.Group); } }
    public uint EnemySpeechKey { get { return GetUINT((int)COLUMN.EnemySpeechKey); } }
    public uint MySpeechKey { get { return GetUINT((int)COLUMN.MySpeechKey); } }
    public float TargetTimeScale { get { return floatValue((int)COLUMN.TargetTimeScale); } }
    public bool isFocusing { get { return intValue((int)COLUMN.isFocusing) == 1; } }
    public bool isRepeat { get { return intValue((int)COLUMN.isRepeat) == 1; } }
}
