using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NPCGroupTable : GameEntityData
{
	public static NPCGroupTable GetData(uint key)
	{
		if (pool.ContainsKey(ENTITY_TYPE.NPCGroupTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.NPCGroupTable.TypeName()];
			NPCGroupTable entity = container.Find(key.ToString()) as NPCGroupTable;

			if (null == entity)
			{
				string msg = $"Invalid Key.. NPCGroup.csv == Key:{key}";
				GameManager.Log(msg, "red");
			}

			return entity;
		}

		return null;
	}

	public static bool IsContainsKey(uint nKey)
	{
		if (pool.ContainsKey(ENTITY_TYPE.NPCGroupTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.NPCGroupTable.TypeName()];
			return container.ContainsKey(nKey.ToString());
		}
		return false;
	}

	public static List<NPCGroupTable> GetList()
	{
		if (pool.ContainsKey(ENTITY_TYPE.NPCGroupTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.NPCGroupTable.TypeName()];
			return container.list.ConvertAll(each => { return each as NPCGroupTable; });
		}

		return null;
	}

	public static List<NPCGroupTable> GetGroup(int a_nGroup, List<NPCGroupTable> a_oOutNPCGroupTableList)
	{
		foreach (NPCGroupTable oNPCGroupTable in GetList())
		{
			if (oNPCGroupTable.Group == a_nGroup)
				a_oOutNPCGroupTableList.Add(oNPCGroupTable);
		}

		return a_oOutNPCGroupTableList;
	}

	public override void OnCreateByDataBase(int fieldid, DataBase database)
	{
		base.OnCreateByDataBase(fieldid, database);
		base.SetKey(string.Format("{0}", PrimaryKey));
	}
}
