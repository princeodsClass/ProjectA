using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EpisodeTable : GameEntityData
{
    public enum EBGMType
    {
        Title,
        Lobby,
        Battle
    }

    public static EpisodeTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.EpisodeTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.EpisodeTable.TypeName()];
            EpisodeTable entity = container.Find(key.ToString()) as EpisodeTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Episode.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.EpisodeTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.EpisodeTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<EpisodeTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.EpisodeTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.EpisodeTable.TypeName()];
            return container.list.ConvertAll(each => { return each as EpisodeTable; });
        }

        return null;
    }

    public static int GetMax()
    {
        List<EpisodeTable> epl = GetList();

        return epl.Max(ep => ep.Order);
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
