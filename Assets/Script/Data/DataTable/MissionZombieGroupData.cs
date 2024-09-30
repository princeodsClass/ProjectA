using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MissionZombieGroupTable : GameEntityData
{
    public static MissionZombieGroupTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionZombieGroupTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionZombieGroupTable.TypeName()];
            MissionZombieGroupTable entity = container.Find(key.ToString()) as MissionZombieGroupTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. MissionZombieGroup.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionZombieGroupTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionZombieGroupTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

	public static MissionZombieGroupTable GetDataByEpisode(int a_nEpisode)
	{
		var oGroupTableList = MissionZombieGroupTable.GetList();

		for(int i = 0; i < oGroupTableList.Count; ++i)
		{
			// 동일한 에피소드 일 경우
			if(oGroupTableList[i].Episode == a_nEpisode)
			{
				return oGroupTableList[i];
			}
		}

		return null;
	}

    public static List<MissionZombieGroupTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionZombieGroupTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionZombieGroupTable.TypeName()];
            return container.list.ConvertAll(each => { return each as MissionZombieGroupTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
