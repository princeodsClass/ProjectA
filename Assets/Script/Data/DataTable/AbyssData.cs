using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AbyssTable : GameEntityData
{
    public static AbyssTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.AbyssTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.AbyssTable.TypeName()];
            AbyssTable entity = container.Find(key.ToString()) as AbyssTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Abyss.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.AbyssTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.AbyssTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<AbyssTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.AbyssTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.AbyssTable.TypeName()];
            return container.list.ConvertAll(each => { return each as AbyssTable; });
        }

        return null;
    }

	public static List<AbyssTable> GetGroup(int a_nGroup) 
	{
		var oTableList = AbyssTable.GetList();
		var oTableGroupList = new List<AbyssTable>();

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
