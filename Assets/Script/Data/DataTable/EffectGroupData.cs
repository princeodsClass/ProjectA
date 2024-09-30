using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EffectGroupTable : GameEntityData
{
	public static EffectGroupTable GetData(uint key)
	{
		if (pool.ContainsKey(ENTITY_TYPE.EffectGroupTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.EffectGroupTable.TypeName()];
			EffectGroupTable entity = container.Find(key.ToString()) as EffectGroupTable;

			if (null == entity)
			{
				string msg = $"Invalid Key.. EffectGroup.csv == Key:{key}";
				GameManager.Log(msg, "red");
			}

			return entity;
		}

		return null;
	}

	public static bool IsContainsKey(uint nKey)
	{
		if (pool.ContainsKey(ENTITY_TYPE.EffectGroupTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.EffectGroupTable.TypeName()];
			return container.ContainsKey(nKey.ToString());
		}
		return false;
	}

	public static List<EffectGroupTable> GetList()
	{
		if (pool.ContainsKey(ENTITY_TYPE.EffectGroupTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.EffectGroupTable.TypeName()];
			return container.list.ConvertAll(each => { return each as EffectGroupTable; });
		}

		return null;
	}

	public override void OnCreateByDataBase(int fieldid, DataBase database)
	{
		base.OnCreateByDataBase(fieldid, database);
		base.SetKey(string.Format("{0}", PrimaryKey));
	}

	/** 그룹 테이블 정보를 반환한다 */
	public static List<EffectGroupTable> GetGroup(int group)
	{
		List<EffectGroupTable> result = new List<EffectGroupTable>();

		foreach (EffectGroupTable ef in GetList())
			if (ef.Group == group)	result.Add(ef);

		return result;
	}

	public static List<EffectGroupTable> RandomResultByFactorInGroup(List<EffectGroupTable> list, int count)
	{
		List<EffectGroupTable> remainingItems = new List<EffectGroupTable>(list);
		List<EffectGroupTable> result = new List<EffectGroupTable>();

		float totalWeight = 0f;
		remainingItems.ForEach(i => totalWeight += i.SelectionFactor);

		for (int i = 0; i < count && remainingItems.Count > 0; i++)
		{
			float randomValue = UnityEngine.Random.Range(0f, totalWeight);

			for (int j = 0; j < remainingItems.Count; j++)
			{
				if (randomValue < remainingItems[j].SelectionFactor)
				{
					result.Add(remainingItems[j]);
					totalWeight -= remainingItems[j].SelectionFactor;
					remainingItems.RemoveAt(j);
					break;
				}

				randomValue -= remainingItems[j].SelectionFactor;
			}
		}

		return result;
	}

	public static Dictionary<EffectTable, float> RandomEffectInGroup(int group, int selectionCount)
	{
		List<EffectGroupTable> effectGroup = new List<EffectGroupTable>();
		List<EffectGroupTable> temp = new List<EffectGroupTable>();
		List<EffectGroupTable> add = new List<EffectGroupTable>();
		Dictionary<EffectTable, float> result = new Dictionary<EffectTable, float>();

		effectGroup = GetGroup(group);

		foreach (EffectGroupTable efg in effectGroup)
		{
			if (efg.SelectionType == 1)
				result.Add(EffectTable.GetData(efg.EffectKey), EffectTable.RandomEffectValue(efg.EffectKey));
			else
				temp.Add(efg);
		}

		if (temp.Count > 0)
			add = RandomResultByFactorInGroup(temp, selectionCount - result.Count);

		foreach (EffectGroupTable ef in add)
			result.Add(EffectTable.GetData(ef.EffectKey), EffectTable.RandomEffectValue(ef.EffectKey));

		return result;
	}
}
