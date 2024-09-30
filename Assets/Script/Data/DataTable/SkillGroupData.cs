using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SkillGroupTable : GameEntityData
{
	public static SkillGroupTable GetData(uint key)
	{
		if (pool.ContainsKey(ENTITY_TYPE.SkillGroupTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.SkillGroupTable.TypeName()];
			SkillGroupTable entity = container.Find(key.ToString()) as SkillGroupTable;

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
		if (pool.ContainsKey(ENTITY_TYPE.SkillGroupTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.SkillGroupTable.TypeName()];
			return container.ContainsKey(nKey.ToString());
		}
		return false;
	}

	public static List<SkillGroupTable> GetList()
	{
		if (pool.ContainsKey(ENTITY_TYPE.SkillGroupTable.TypeName()))
		{
			EntityContainer container = pool[ENTITY_TYPE.SkillGroupTable.TypeName()];
			return container.list.ConvertAll(each => { return each as SkillGroupTable; });
		}

		return null;
	}

	public static List<SkillGroupTable> GetGroup(int group)
	{
		List<SkillGroupTable> returnValue = new List<SkillGroupTable>();
		return GetGroup(group, returnValue);
	}

	public static List<SkillGroupTable> GetGroup(int group, List<SkillGroupTable> a_oOutSkillGroupList)
	{
		foreach (SkillGroupTable reward in GetList())
		{
			if (reward.Group == group)
				a_oOutSkillGroupList.Add(reward);
		}

		return a_oOutSkillGroupList;
	}

	public static List<SkillTable> RandomResultInGroup(int group)
	{
		List<SkillGroupTable> liResult = new List<SkillGroupTable>();
		List<SkillGroupTable> temp = new List<SkillGroupTable>();
		List<SkillGroupTable> add = new List<SkillGroupTable>();
		List<SkillTable> result = new List<SkillTable>();

		liResult = GetGroup(group);

		foreach (SkillGroupTable sg in liResult)
		{
			if (sg.SelectionType == 1)
				result.Add(SkillTable.GetData(sg.Skill));
			else
				temp.Add(sg);
		}

		if (temp.Count > 0)
		{
			add = GetDistinctRandomElements(temp);

			for (int i = 0; i < add.Count; i++)
				result.Add(SkillTable.GetData(add[i].Skill));
		}

		return result;
	}

	public static List<SkillGroupTable> GetDistinctRandomElements(List<SkillGroupTable> list, int count = 1)
	{
		if (list.Count < count)
			count = list.Count;

		List<SkillGroupTable> selectedItems = new List<SkillGroupTable>();

		while (selectedItems.Count < count)
		{
			float totalWeight = list.Sum(item => item.SelectionFactor);
			float randomValue = UnityEngine.Random.Range(0f, totalWeight);

			foreach (var item in list)
			{
				randomValue -= item.SelectionFactor;

				if (randomValue <= 0f && !selectedItems.Contains(item))
				{
					selectedItems.Add(item);
					break;
				}
			}
		}

		return selectedItems;
	}

	public override void OnCreateByDataBase(int fieldid, DataBase database)
	{
		base.OnCreateByDataBase(fieldid, database);
		base.SetKey(string.Format("{0}", PrimaryKey));
	}
}
