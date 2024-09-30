using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DescTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public string Korea { get { return stringValue((int)COLUMN.Korea); } }
    public string English { get { return stringValue((int)COLUMN.English); } }
    public string Japanese { get { return stringValue((int)COLUMN.Japanese); } }
    public string Chinese { get { return stringValue((int)COLUMN.Chinese); } }
    public string German { get { return stringValue((int)COLUMN.German); } }
    public string Spanish { get { return stringValue((int)COLUMN.Spanish); } }
    public string Portuguese { get { return stringValue((int)COLUMN.Portuguese); } }
    public string French { get { return stringValue((int)COLUMN.French); } }
    public string Italian { get { return stringValue((int)COLUMN.Italian); } }
    public string Dutch { get { return stringValue((int)COLUMN.Dutch); } }
    public string Russian { get { return stringValue((int)COLUMN.Russian); } }
    public string Hungarian { get { return stringValue((int)COLUMN.Hungarian); } }
    public string Greek { get { return stringValue((int)COLUMN.Greek); } }
    public string Turkish { get { return stringValue((int)COLUMN.Turkish); } }
    public string Vietnamese { get { return stringValue((int)COLUMN.Vietnamese); } }
    //public string Persian { get { return stringValue((int)COLUMN.Persian); } }
    public string Malay { get { return stringValue((int)COLUMN.Malay); } }
    //public string Arabic { get { return stringValue((int)COLUMN.Arabic); } }
    public string Indonesian { get { return stringValue((int)COLUMN.Indonesian); } }
    //public string Hebrew { get { return stringValue((int)COLUMN.Hebrew); } }
}
