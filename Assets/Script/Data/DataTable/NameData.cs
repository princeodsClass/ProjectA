using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NameTable : GameEntityData
{
    public static NameTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.NameTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.NameTable.TypeName()];
            NameTable entity = container.Find(key.ToString()) as NameTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Name.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static string GetValue(uint key)
    {
        return GetData(key).stringValue(GameManager.Singleton._curLanguage);
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.NameTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.NameTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<NameTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.NameTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.NameTable.TypeName()];
            return container.list.ConvertAll(each => { return each as NameTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
