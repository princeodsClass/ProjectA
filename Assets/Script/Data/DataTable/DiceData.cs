using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DiceTable : GameEntityData
{
    public static DiceTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.DiceTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.DiceTable.TypeName()];
            DiceTable entity = container.Find(key.ToString()) as DiceTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Dice.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.DiceTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.DiceTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<DiceTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.DiceTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.DiceTable.TypeName()];
            return container.list.ConvertAll(each => { return each as DiceTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));

        //GameManager.Log($"Box : {fieldid} /  {PrimaryKey}  / {NameKey} / {DescKey}", "green");
    }
}
