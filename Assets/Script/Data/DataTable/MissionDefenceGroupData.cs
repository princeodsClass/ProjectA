using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MissionDefenceGroupTable : GameEntityData
{
    public static MissionDefenceGroupTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionDefenceGroupTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionDefenceGroupTable.TypeName()];
            MissionDefenceGroupTable entity = container.Find(key.ToString()) as MissionDefenceGroupTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. MissionDefenceGroup.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionDefenceGroupTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionDefenceGroupTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<MissionDefenceGroupTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionDefenceGroupTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionDefenceGroupTable.TypeName()];
            return container.list.ConvertAll(each => { return each as MissionDefenceGroupTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
