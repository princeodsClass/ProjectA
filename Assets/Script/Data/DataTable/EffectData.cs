using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EffectTable : GameEntityData
{
	public static EffectTable GetData(uint key)
	{
		if (pool.ContainsKey(ENTITY_TYPE.EffectTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.EffectTable.TypeName()];
			EffectTable entity = container.Find(key.ToString()) as EffectTable;

			if (null == entity)
			{
				string msg = $"Invalid Key.. Effect.csv == Key:{key}";
				GameManager.Log(msg, "red");
			}

			return entity;
		}

		return null;
	}

	public static bool IsContainsKey(uint nKey)
	{
		if (pool.ContainsKey(ENTITY_TYPE.EffectTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.EffectTable.TypeName()];
			return container.ContainsKey(nKey.ToString());
		}
		return false;
	}

	public static List<EffectTable> GetList()
	{
		if (pool.ContainsKey(ENTITY_TYPE.EffectTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.EffectTable.TypeName()];
			return container.list.ConvertAll(each => { return each as EffectTable; });
		}

		return null;
	}

	public override void OnCreateByDataBase(int fieldid, DataBase database)
	{
		base.OnCreateByDataBase(fieldid, database);
		base.SetKey(string.Format("{0}", PrimaryKey));
	}

	/** 그룹 테이블 정보를 반환한다 */
	public static List<EffectTable> GetGroup(int group)
	{
		return GetGroup(group, new List<EffectTable>());
	}

	/** 그룹 테이블 정보를 반환한다 */
	public static List<EffectTable> GetGroup(int group, List<EffectTable> a_oOutEffectTableList)
	{
		foreach (EffectTable reward in GetList())
		{
			if (reward.Group == group)
				a_oOutEffectTableList.Add(reward);
		}

		return a_oOutEffectTableList;
	}

	public static float RandomEffectValue(uint key)
	{
		EffectTable eft = GetData(key);

		return Random.Range(eft.ValueMin, eft.ValueMax);
	}
}
