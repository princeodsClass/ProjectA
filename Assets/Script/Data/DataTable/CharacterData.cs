using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CharacterTable : GameEntityData
{
    public static CharacterTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.CharacterTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.CharacterTable.TypeName()];
            CharacterTable entity = container.Find(key.ToString()) as CharacterTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Character.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.CharacterTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.CharacterTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<CharacterTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.CharacterTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.CharacterTable.TypeName()];
            return container.list.ConvertAll(each => { return each as CharacterTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
