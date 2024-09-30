using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NPCStatTable : GameEntityData
{
    public static NPCStatTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.NPCStatTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.NPCStatTable.TypeName()];
            NPCStatTable entity = container.Find(key.ToString()) as NPCStatTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. NPCStat.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.NPCStatTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.NPCStatTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<NPCStatTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.NPCStatTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.NPCStatTable.TypeName()];
            return container.list.ConvertAll(each => { return each as NPCStatTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
