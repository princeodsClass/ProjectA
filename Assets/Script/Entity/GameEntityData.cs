
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if (NET_4_6 || !NET_STANDARD_2_0)
using System.Runtime.Remoting;
#endif

/// <summary>
/// data table for entity 
/// </summary>
public partial class GameEntityData : Entity
{
	/// <summary>
	/// table field id ( 0 ~ n )
	/// </summary>
	public int fieldid { protected set; get; }

	/// <summary>
	/// database
	/// </summary>
	/// <value>The database.</value>
	DataBase database { set; get; }

	public string stringValue(int column)
	{
		return this[column];
	}

	protected float floatValue(int column)
	{
		string v = stringValue(column);
		if (true == string.IsNullOrEmpty(v))
			return 0f;

		return float.Parse(v);
	}

	protected int intValue(int column)
	{
		string v = stringValue(column);
		if (true == string.IsNullOrEmpty(v))
			return 0;
			
		return int.Parse(v);
	}

	protected uint GetUINT(int nColumn)
	{
		string v = stringValue(nColumn);
		if (true == string.IsNullOrEmpty(v) || v == "NaN")
			return 0;

		return uint.Parse(v);
	}

	protected long longValue(int column)
	{
		string v = stringValue(column);
		if (true == string.IsNullOrEmpty(v))
			return 0;

		return long.Parse(v);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="column"></param>
	/// <returns></returns>
	protected DataValue this[int columns]
	{
		get
		{
			if(database == null)
			{
				database = DataBase.GetDB(GetType());
			}

			//if(database==null)
			//	database = DataBase.dataDBs[tablename];

			return database.table[this.fieldid, columns];
		}
	}

	/// <summary>
	/// override oncreate
	/// </summary>
	/// <param name="p">P.</param>
	public virtual void OnCreateByDataBase (int fieldid, DataBase database)
	{
		this.fieldid = fieldid;
		this.database = database;
	}
}

