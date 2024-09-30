using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NPCTable : GameEntityData
{
	public static NPCTable GetData(uint key)
	{
		if (pool.ContainsKey(ENTITY_TYPE.NPCTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.NPCTable.TypeName()];
			NPCTable entity = container.Find(key.ToString()) as NPCTable;

			if (null == entity)
			{
				string msg = $"Invalid Key.. NPC.csv == Key:{key}";
				GameManager.Log(msg, "red");
			}

			return entity;
		}

		return null;
	}

	public static bool IsContainsKey(uint nKey)
	{
		if (pool.ContainsKey(ENTITY_TYPE.NPCTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.NPCTable.TypeName()];
			return container.ContainsKey(nKey.ToString());
		}
		return false;
	}

	public static List<NPCTable> GetList()
	{
		if (pool.ContainsKey(ENTITY_TYPE.NPCTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.NPCTable.TypeName()];
			return container.list.ConvertAll(each => { return each as NPCTable; });
		}

		return null;
	}

	public static List<NPCTable> GetDefenceGroup(int a_nGroup, List<NPCTable> a_oOutNPCTableList)
	{
		foreach (NPCTable oNPCTable in GetList())
		{
			if (oNPCTable.DefenceGroup == a_nGroup)
				a_oOutNPCTableList.Add(oNPCTable);
		}

		return a_oOutNPCTableList;
	}

	public override void OnCreateByDataBase(int fieldid, DataBase database)
	{
		base.OnCreateByDataBase(fieldid, database);
		base.SetKey(string.Format("{0}", PrimaryKey));
	}
}
