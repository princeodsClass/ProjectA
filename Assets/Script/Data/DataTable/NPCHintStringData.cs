using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NPCHintStringTable : GameEntityData
{
    public static NPCHintStringTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.NPCHintStringTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.NPCHintStringTable.TypeName()];
            NPCHintStringTable entity = container.Find(key.ToString()) as NPCHintStringTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Desc.csv == Key:{key}";
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

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.NPCHintStringTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.NPCHintStringTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<NPCHintStringTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.NPCHintStringTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.NPCHintStringTable.TypeName()];
            return container.list.ConvertAll(each => { return each as NPCHintStringTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
