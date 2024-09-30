using System.Collections.Generic;

public class DataBase
{
	public DataValue[,] table = null;

	public int row { get { return table != null ? table.GetLength(0) : 0; } }
	public int col { get { return table != null ? table.GetLength(1) : 0; } }

	/// <summary>
	/// data base name
	/// </summary>
	public string databaseKey;


	/// <summary>
	/// data base list
	/// </summary>
    public static Dictionary<string, DataBase> dataDBs = new Dictionary<string, DataBase>();

	public static void Clear()
    {
		dataDBs.Clear();
	}


	/// <summary>
	/// return data base
	/// </summary>
	/// <param name="keyname"></param>
	/// <returns></returns>
	public static DataBase GetDB(string keyname)
	{
        if (dataDBs.ContainsKey(keyname))
            return dataDBs[keyname];
        else
        {
            DataBase db = new DataBase(keyname);
            if( db != null )
            {
                dataDBs.Add(keyname, db);
            }
        }

        return dataDBs[keyname];
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns><c>true</c> if is exist D the specified keyname; otherwise, <c>false</c>.</returns>
	/// <param name="keyname">Keyname.</param>
	public static DataBase IsExistDB(string keyname)
	{
		if (dataDBs.ContainsKey(keyname))
			return dataDBs[keyname];

		return null;
	}

    protected DataBase(string databaseKey)
	{
        this.databaseKey = databaseKey;
	}
}