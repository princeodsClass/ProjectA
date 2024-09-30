using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UIStringTable : GameEntityData
{
    public static UIStringTable GetData(string key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.UIStringTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.UIStringTable.TypeName()];
            UIStringTable entity = container.Find(key) as UIStringTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. UIString.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }
    public static string GetValue(string key)
    {
        return GetData(key).stringValue(GameManager.Singleton._curLanguage);
    }

    public static bool IsContainsKey(string Key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.UIStringTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.UIStringTable.TypeName()];
            return container.ContainsKey(Key.ToString());
        }
        return false;
    }

    public static List<UIStringTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.UIStringTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.UIStringTable.TypeName()];
            return container.list.ConvertAll(each => { return each as UIStringTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", sPrimaryKey));
    }
}
