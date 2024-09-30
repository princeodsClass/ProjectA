using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryData<T> where T : ItemBase
{
    // private Dictionary<long, ItemMaterial> m_dicItem = new Dictionary<long, ItemMaterial>();
    private Dictionary<long, T> m_dicItem = new Dictionary<long, T>() ;

    private int m_nNewItemCount = 0;

    public InventoryData()
    {
        // 서버에서 받아서 넣자.
    }

    ~InventoryData()
    {
        Clear();
    }

    //임시
    void LoadMyInventoryData()
    {

    }

    public enum EItemModifyType
    {
        Volume,
        Upgrade,
        Reinforce,
        Limitbreak,
    }

    public void SortItem()
    {
        m_dicItem = m_dicItem.OrderByDescending(item => item.Value.nGrade)
                             .ThenByDescending(item => item.Value.nCP)
                             .ThenBy(item => item.Value.strName)
                             .ThenBy(item => item.Value.nKey)
                             .ToDictionary(x => x.Key, x => x.Value);
    }

    public void ModifyItem(long id, EItemModifyType type, int value)
    {
		bool bIsValid = m_dicItem.TryGetValue(id, out T cItem);

		// 아이템 변경이 불가능 할 경우
		if(!bIsValid)
		{
			GameManager.Log("ModifyItem was deleted");
			return;
		}

        switch ( type )
        {
            case EItemModifyType.Volume:
                cItem.nVolume = value;
                break;
            case EItemModifyType.Upgrade:
                cItem.nCurUpgrade = value;
                break;
            case EItemModifyType.Reinforce:
                cItem.nCurReinforce = value;
                break;
            case EItemModifyType.Limitbreak:
                cItem.nCurLimitbreak = value;
                break;
        }
    }

    public Sprite GetIconSprite(long id)
    {
        m_dicItem.TryGetValue(id, out T cItem);

        return cItem.GetIcon();
    }

    public string GetIconString(long id)
    {
        m_dicItem.TryGetValue(id, out T cItem);

        return cItem.strIcon;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return m_dicItem.Values.GetEnumerator();
    }

    public void Clear()
    {
        m_dicItem.Clear();

        m_nNewItemCount = 0;
    }

    public long GetItemID(uint nKey)
    {
        Dictionary<long, T>.Enumerator it = m_dicItem.GetEnumerator();

        while (it.MoveNext())
        {
            if (nKey == it.Current.Value.nKey)
                return it.Current.Value.id;
        }

        return 0;
    }

    public long ContainKey(uint key)
    {
        Dictionary<long, T>.Enumerator it = m_dicItem.GetEnumerator();

        while (it.MoveNext())
        {
            if (key == it.Current.Value.nKey)
                return it.Current.Value.id;
        }

        return -1;
    }

    public bool ContainID(long id)
    {
        Dictionary<long, T>.Enumerator it = m_dicItem.GetEnumerator();

        while (it.MoveNext())
        {
            if (id == it.Current.Value.id)
                return true;
        }

        return false;
    }

    public bool CheckCost(uint key, int count)
    {
        int reserveCount = 0;

        if (key == ComType.KEY_ITEM_CRYSTAL_FREE || key == ComType.KEY_ITEM_CRYSTAL_PAY)
        {
            reserveCount = GetItemCount(ComType.KEY_ITEM_CRYSTAL_FREE);
            reserveCount += GetItemCount(ComType.KEY_ITEM_CRYSTAL_PAY);
        }
        else
            reserveCount = GetItemCount(key);

        return reserveCount >= count;
    }

    public int GetItemCount(uint nKey)
    {
        Dictionary<long, T>.Enumerator it = m_dicItem.GetEnumerator();

        while (it.MoveNext())
        {
            if (nKey == it.Current.Value.nKey)
                return it.Current.Value.nVolume;
        }

        return 0;
    }

    public int GetItemCount(long id)
    {
        Dictionary<long, T>.Enumerator it = m_dicItem.GetEnumerator();

        while (it.MoveNext())
        {
            if (id == it.Current.Value.id)
                return it.Current.Value.nVolume;
        }

        return 0;
    }

    public T GetItem(long id)
    {
        Dictionary<long, T>.Enumerator it = m_dicItem.GetEnumerator();

        while (it.MoveNext())
        {
            if (id == it.Current.Value.id)
                return it.Current.Value;
        }

        return null;
    }

    public void ClearNewItem()
    {
        m_nNewItemCount = 0;
    }

    public int CalcTotalCrystal()
    {
        return GetItemCount(ComType.KEY_ITEM_CRYSTAL_FREE) + GetItemCount(ComType.KEY_ITEM_CRYSTAL_PAY);
    }

    public int CalcTotalMoney()
    {
        return GetItemCount(ComType.KEY_ITEM_GOLD);
    }

    public int CalcMissionTicket()
    {
        return GetItemCount(GlobalTable.GetData<uint>("valueDefenceTicketDecKey"));
    }

    public IEnumerator ConsumeCrystal(int cost)
    {
        int t = GameManager.Singleton.invenMaterial.GetItemCount(ComType.KEY_ITEM_CRYSTAL_FREE) - cost;

        if (t < 0)
        {
            yield return GameManager.Singleton.StartCoroutine(GameManager.Singleton.AddItemCS(ComType.KEY_ITEM_CRYSTAL_PAY, t));

            if (cost != -t)
                yield return GameManager.Singleton.StartCoroutine(GameManager.Singleton.AddItemCS(ComType.KEY_ITEM_CRYSTAL_FREE, -(cost + t)));
        }
        else
        {
            yield return GameManager.Singleton.StartCoroutine(GameManager.Singleton.AddItemCS(ComType.KEY_ITEM_CRYSTAL_FREE, -cost));
        }
    }

    public void AddItem(long id, uint nKey, int nCount, int upgrade = 0, int nReinforce = 0, int nLimitbreak = 0, bool isLock = false, DateTime dateTime = default(DateTime), bool isNew = false, uint[] effectkey = null, float[] effectvalue = null, int[] effectfixxed = null)
    {
        if (0 == nKey) return;

        if ( m_dicItem.TryGetValue(id, out T cItem) )
        {
            if (typeof(T) != typeof(ItemBox))
            {
                cItem.nVolume = nCount;

                if (0 >= nCount)
                    RemoveItem(id);
            }
        }
        else
        {
            cItem = null;
            if (typeof(T) == typeof(ItemMaterial))
            {
                cItem = new ItemMaterial(id, nKey, nCount) as T;
            }
            else if (typeof(T) == typeof(ItemWeapon))
            {
                cItem = new ItemWeapon(id, nKey, nCount, upgrade, nReinforce, nLimitbreak, isLock, isNew, effectkey, effectvalue, effectfixxed) as T;
            }
            else if (typeof(T) == typeof(ItemGear))
            {
                cItem = new ItemGear(id, nKey, nCount, upgrade, nReinforce, nLimitbreak, isLock, isNew, effectkey, effectvalue, effectfixxed) as T;
            }
            else if (typeof(T) == typeof(ItemBox))
            {
                cItem = new ItemBox(id, nKey, nCount, dateTime, isNew) as T;
            }

            cItem.bNew = isNew;

            m_dicItem.Add(id, cItem);
        }
    }

    public void AddCharacter(long id, uint nKey, int upgrade, ItemCharacter.stSkillUpgrade upInfo)
    {
        if ( 0 == id || false == CharacterTable.IsContainsKey(nKey) || m_dicItem.ContainsKey(id) ) return;

        T character = new ItemCharacter(id, nKey, upgrade, upInfo) as T;

        m_dicItem.Add(id, character);
    }

    public void RemoveItem(long id)
    {
        if ( m_dicItem.ContainsKey(id) )
        {
            m_dicItem.Remove(id);
        }
    }

    public void DeleteBox(long id)
    {
        if (!m_dicItem.ContainsKey(id))
        {
            GameManager.Log($"{id} is not exsit", "red");
            return;
        }

        GameDataManager.Singleton.DeleteItem(id, EDatabaseType.box);
    }


    public int GetItemCount()
    {
        return m_dicItem.Count;
    }
}
