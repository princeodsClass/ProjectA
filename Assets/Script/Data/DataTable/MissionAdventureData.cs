using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MissionAdventureTable : GameEntityData
{
    public static MissionAdventureTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionAdventureTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionAdventureTable.TypeName()];
            MissionAdventureTable entity = container.Find(key.ToString()) as MissionAdventureTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. MissionAdventure.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionAdventureTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionAdventureTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<MissionAdventureTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionAdventureTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionAdventureTable.TypeName()];
            return container.list.ConvertAll(each => { return each as MissionAdventureTable; });
        }

        return null;
    }

	public static List<MissionAdventureTable> GetGroup(int a_nGroup) 
	{
		var oTableList = MissionAdventureTable.GetList();
		var oTableGroupList = new List<MissionAdventureTable>();

		for(int i = 0; i < oTableList.Count; ++i) {
			if(oTableList[i].Group == a_nGroup) oTableGroupList.Add(oTableList[i]);
		}

		return oTableGroupList;
	}

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
