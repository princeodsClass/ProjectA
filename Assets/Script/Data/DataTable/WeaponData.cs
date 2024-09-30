using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WeaponTable : GameEntityData
{
    public static WeaponTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.WeaponTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.WeaponTable.TypeName()];
            WeaponTable entity = container.Find(key.ToString()) as WeaponTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Weapon.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static WeaponTable GetData(int order)
    {
        List<WeaponTable> list = GetList();
        return list[order];
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.WeaponTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.WeaponTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<WeaponTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.WeaponTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.WeaponTable.TypeName()];
            return container.list.ConvertAll(each => { return each as WeaponTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
