using System;
using System.Collections.Generic;

public partial class MissionZombieTable : GameEntityData
{
    public static MissionZombieTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionZombieTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionZombieTable.TypeName()];
            MissionZombieTable entity = container.Find(key.ToString()) as MissionZombieTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. MissionZombie.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionZombieTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionZombieTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<MissionZombieTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionZombieTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionZombieTable.TypeName()];
            return container.list.ConvertAll(each => { return each as MissionZombieTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
