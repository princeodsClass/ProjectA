public partial class SkillGroupTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public int Group { get { return intValue((int)COLUMN.Group); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int SelectionType { get { return intValue((int)COLUMN.SelectionType); } }
    public int SelectionFactor { get { return intValue((int)COLUMN.SelectionFactor); } }
    public uint Skill { get { return GetUINT((int)COLUMN.Skill); } }
}
