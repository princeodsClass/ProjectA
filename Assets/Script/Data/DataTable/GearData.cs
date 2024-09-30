using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GearTable : GameEntityData
{
    public static GearTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.GearTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.GearTable.TypeName()];
            GearTable entity = container.Find(key.ToString()) as GearTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Gear.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static GearTable GetData(int order)
    {
        List<GearTable> list = GetList();
        return list[order];
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.GearTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.GearTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<GearTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.GearTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.GearTable.TypeName()];
            return container.list.ConvertAll(each => { return each as GearTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
