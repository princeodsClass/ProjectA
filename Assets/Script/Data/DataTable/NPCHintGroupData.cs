using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NPCHintGroupTable : GameEntityData
{
	public static NPCHintGroupTable GetData(uint key)
	{
		if (pool.ContainsKey(ENTITY_TYPE.NPCHintGroupTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.NPCHintGroupTable.TypeName()];
			NPCHintGroupTable entity = container.Find(key.ToString()) as NPCHintGroupTable;

			if (null == entity)
			{
				string msg = $"Invalid Key.. SkillGroup.csv == Key:{key}";
				GameManager.Log(msg, "red");
			}

			return entity;
		}

		return null;
	}

	public static bool IsContainsKey(uint nKey)
	{
		if (pool.ContainsKey(ENTITY_TYPE.NPCHintGroupTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.NPCHintGroupTable.TypeName()];
			return container.ContainsKey(nKey.ToString());
		}
		return false;
	}

	public static List<NPCHintGroupTable> GetList()
	{
		if (pool.ContainsKey(ENTITY_TYPE.NPCHintGroupTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.NPCHintGroupTable.TypeName()];
			return container.list.ConvertAll(each => { return each as NPCHintGroupTable; });
		}

		return null;
	}

	public static List<NPCHintGroupTable> GetGroup(int group)
	{
		List<NPCHintGroupTable> returnValue = new List<NPCHintGroupTable>();
		return GetGroup(group, returnValue);
	}

	public static List<NPCHintGroupTable> GetGroup(int group, List<NPCHintGroupTable> a_oOutSkillGroupList)
	{
		foreach (NPCHintGroupTable reward in GetList())
		{
			if (reward.Group == group)
				a_oOutSkillGroupList.Add(reward);
		}

		return a_oOutSkillGroupList;
	}

	public override void OnCreateByDataBase(int fieldid, DataBase database)
	{
		base.OnCreateByDataBase(fieldid, database);
		base.SetKey(string.Format("{0}", PrimaryKey));
	}
}
