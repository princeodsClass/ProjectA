using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class LeagueTable : GameEntityData
{
    public static LeagueTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.LeagueTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.LeagueTable.TypeName()];
            LeagueTable entity = container.Find(key.ToString()) as LeagueTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. League.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static LeagueTable GetDataWithRank(int rank)
    {
        List<LeagueTable> table = GetList();
        LeagueTable rv = null;

        table.ForEach(el =>
        {
            if ( el.MinRank <= rank && rank <= el.MaxRank )
                rv = el;
        });

        return rv;
    }

    public static Sprite GetGradeIcon(int rank)
    {
        LeagueTable te = GetDataWithRank(rank);
        return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, te.Icon);
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.LeagueTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.LeagueTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<LeagueTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.LeagueTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.LeagueTable.TypeName()];
            return container.list.ConvertAll(each => { return each as LeagueTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
