using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ContentsStringTable : GameEntityData
{
    public static ContentsStringTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.ContentsStringTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.ContentsStringTable.TypeName()];
            ContentsStringTable entity = container.Find(key.ToString()) as ContentsStringTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. ContentsString.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }
    public static string GetValue(uint key)
    {
        /*
        string languageString = "Korean";
        ELanguage language = (ELanguage)Enum.Parse(typeof(ELanguage), languageString);
        int index = (int)language;
        */

        return GetData(key).stringValue(GameManager.Singleton._curLanguage);
    }

    public static bool IsContainsKey(int nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.ContentsStringTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.ContentsStringTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<ContentsStringTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.ContentsStringTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.ContentsStringTable.TypeName()];
            return container.list.ConvertAll(each => { return each as ContentsStringTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
