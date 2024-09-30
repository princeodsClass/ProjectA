using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AttendanceTable : GameEntityData
{
    public static AttendanceTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.AttendanceTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.AttendanceTable.TypeName()];
            AttendanceTable entity = container.Find(key.ToString()) as AttendanceTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Attendance.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.AttendanceTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.AttendanceTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<AttendanceTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.AttendanceTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.AttendanceTable.TypeName()];
            return container.list.ConvertAll(each => { return each as AttendanceTable; });
        }

        return null;
    }

    public static List<AttendanceTable> GetList(int level)
    {
        List<AttendanceTable> list = new List<AttendanceTable>();
        List<AttendanceTable> t = new List<AttendanceTable>();

        list = GetList();

        int tar = FindClosestValue(list, level);

        list.ForEach(each => { if (each.Level == tar) t.Add(each); });

        return t;
    }

    public static int FindClosestValue(List<AttendanceTable> values, int targetValue)
    {
        int closestValue = values[0].Level;
        int minDifference = Math.Abs(targetValue - closestValue);

        foreach (AttendanceTable item in values)
        {
            int difference = Math.Abs(targetValue - item.Level);
            if (difference < minDifference)
            {
                minDifference = difference;
                closestValue = item.Level;
            }
        }

        return closestValue;
    }

    public static Sprite GetAttendanceSlotFrameSprite(uint key)
    {
        switch ( ComUtil.GetItemGrade(key) )
        {            
            case 2: return GameResourceManager.Singleton.LoadSprite(EAtlasType.Outgame, "Attendance_Frame_G02");
            case 3: return GameResourceManager.Singleton.LoadSprite(EAtlasType.Outgame, "Attendance_Frame_G03");
            case 4: return GameResourceManager.Singleton.LoadSprite(EAtlasType.Outgame, "Attendance_Frame_G04");
            case 5: return GameResourceManager.Singleton.LoadSprite(EAtlasType.Outgame, "Attendance_Frame_G05");
            default: return GameResourceManager.Singleton.LoadSprite(EAtlasType.Outgame, "Attendance_Frame_G01");
        }
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
