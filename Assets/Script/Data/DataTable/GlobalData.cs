using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GlobalTable : GameEntityData
{
	public static T GetData<T>(string key) // where T : struct
	{
		if (pool.ContainsKey(ENTITY_TYPE.GlobalTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.GlobalTable.TypeName()];
			GlobalTable entity = container.Find(key) as GlobalTable;
			if(null == entity)
			{
				string msg = $"Invalid Key.. Global.csv == Key:{key}";
				GameManager.Log(msg, "red");
			}

			return (T)Convert.ChangeType(entity.Value, typeof(T));
		}

		return default(T);
	}

	public static List<GlobalTable> GetList()
	{
		if (pool.ContainsKey(ENTITY_TYPE.GlobalTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.GlobalTable.TypeName()];
			return container.list.ConvertAll(each => { return each as GlobalTable; });
		}

		return null;
	}

	public override void OnCreateByDataBase(int fieldid, DataBase database)
	{
		base.OnCreateByDataBase(fieldid, database);
		base.SetKey(string.Format("{0}", sPrimarykey));
	}
}
