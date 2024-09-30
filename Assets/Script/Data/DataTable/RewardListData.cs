using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RewardListTable : GameEntityData
{
    public static RewardListTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.RewardListTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.RewardListTable.TypeName()];
            RewardListTable entity = container.Find(key.ToString()) as RewardListTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. RewardList.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(int nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.RewardListTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.RewardListTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<RewardListTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.RewardListTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.RewardListTable.TypeName()];
            return container.list.ConvertAll(each => { return each as RewardListTable; });
        }

        return null;
    }

    public static List<RewardListTable> GetGroup(uint group)
    {
        List<RewardListTable> returnValue = new List<RewardListTable>();

        foreach (RewardListTable reward in GetList())
        {
            if (reward.Group == group)
                returnValue.Add(reward);
        }

        return returnValue;
    }

    public static List<RewardListTable> RandomResultByFactorInGroup(uint group, int count = 1)
    {
        List<RewardListTable> list = GetGroup(group);
        List<RewardListTable> selectedItems = new List<RewardListTable>();

        if (count >= list.Count)
        {
            return list;
        }

        float totalWeight = list.Sum(i => i.SelectionFactor);

        while (selectedItems.Count < count)
        {
            float randomValue = Random.Range(0f, totalWeight);

            for (int i = 0; i < list.Count; i++)
            {
                if (!selectedItems.Contains(list[i]) && randomValue < list[i].SelectionFactor)
                {
                    selectedItems.Add(list[i]);
                    totalWeight -= list[i].SelectionFactor;
                    break;
                }
                randomValue -= list[i].SelectionFactor;
            }
        }

        return selectedItems;
    }

    public static Dictionary<uint, int> RandomResultByFactorInGroup(List<RewardTable> list, bool isReduce = false)
    {
        Dictionary<uint, int> results = new Dictionary<uint, int>();
        List<RewardListTable> candidate = new List<RewardListTable>();

        bool shouldAddReward = false;

        float totalWeight = default;
        float r = 0f;
        float temp = 0f;

        float fRatioWeapon = GlobalTable.GetData<float>("ratioZombieWeaponDrop");
        float fRatioHealth = GlobalTable.GetData<float>("ratioZombieHealthDrop");
        float fRatioGold = GlobalTable.GetData<float>("ratioZombieMoneyDrop");

        EBattleType nowType = GameDataManager.Singleton.BattleType;
        EPlayMode nowMode = GameDataManager.Singleton.PlayMode;
        EMapInfoType nowMapType = GameDataManager.Singleton.PlayMapInfoType;

        for (int i = 0; i < list.Count; i++)
        {
            candidate = GetGroup(list[i].RewardListGroup);
            totalWeight = candidate.Sum(i => i.SelectionFactor);

            temp = 0f;
            r = Random.Range(0f, totalWeight);

            for (int j = 0; j < candidate.Count; j++)
            {
                temp += candidate[j].SelectionFactor;

                if (r < temp && candidate[j].RewardKey > 0)
                {
                    int volume = Random.Range(candidate[j].RewardCountMin, candidate[j].RewardCountMax + 1);

                    // if (nowMapType == EMapInfoType.INFINITE &&
                    //     nowType == EBattleType.INFINITE &&
                    //     MenuManager.Singleton.CurScene != ESceneType.Lobby)
                    if ( isReduce )
                    {
                        shouldAddReward = false;

                        if (ComUtil.GetItemType(candidate[j].RewardKey) == EItemType.Weapon)
                            shouldAddReward = Random.Range(0f, 1f) <= fRatioWeapon;
                        else if (ComUtil.GetItemType(candidate[j].RewardKey) == EItemType.FieldObject)
                            shouldAddReward = Random.Range(0f, 1f) <= fRatioHealth;
                        else if (candidate[j].RewardKey == ComType.KEY_ITEM_GOLD)
                            shouldAddReward = Random.Range(0f, 1f) <= fRatioGold;
                        else
                            shouldAddReward = true;

                        if (shouldAddReward)
                            if (results.ContainsKey(candidate[j].RewardKey))
                                results[candidate[j].RewardKey] += volume;
                            else
                                results.Add(candidate[j].RewardKey, volume);
                    }
                    else
                    {
                        if (results.ContainsKey(candidate[j].RewardKey))
                            results[candidate[j].RewardKey] += volume;
                        else
                            results.Add(candidate[j].RewardKey, volume);
                    }

                    /*
                    if ( results.ContainsKey(candidate[j].RewardKey) )
                        results[candidate[j].RewardKey] += volume;
                    else
                        results.Add(candidate[j].RewardKey, volume);
                    */

                    break;
                }
            }
        }

        return results;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
