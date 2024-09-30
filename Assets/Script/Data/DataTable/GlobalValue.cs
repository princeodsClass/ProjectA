using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GlobalTable
{
	private string sPrimarykey { get { return this[(int)COLUMN.sPrimarykey]; } }
	public int VariableType { get { return this[(int)COLUMN.VariableType]; } }
	public string Value { get { return stringValue((int)COLUMN.Value); } }
}
