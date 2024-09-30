using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

public class DataBaseReader
{
    public static void LoadStringToDataTable(string path, DataBase db)
    {
        try
        {
            path = $"{Application.persistentDataPath}/{ComType.DATA_PATH}/{path}.csv";
            string encryptedData = File.ReadAllText(path);

            if (string.IsNullOrEmpty(encryptedData))
            {
                GameManager.Log($"{path} load fail..", "red");
                return;
            }

            string[] parts = encryptedData.Split(':');
            if (parts.Length < 2)
            {
                GameManager.Log("Invalid encrypted data format", "red");
                return;
            }

            byte[] iv = ComUtil.HexStringToByteArray(parts[0]);
            byte[] encryptedBytes = ComUtil.HexStringToByteArray(parts[1]);

            if (iv == null || encryptedBytes == null)
            {
                GameManager.Log("Invalid encrypted data format", "red");
                return;
            }

            string decodedData = ComUtil.Decrypt(encryptedBytes, iv);
            string[] lines = decodedData.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
            {
                GameManager.Log($"{path} is null", "red");
                return;
            }

            string[] columnName = lines[0].Split(',');
            db.table = new DataValue[lines.Length - 1, columnName.Length];

            for (int l = 1; l < lines.Length; ++l)
            {
                string[] columns = ComUtil.SplitCsvLine(lines[l]);

                for (int c = 0; c < columnName.Length; ++c)
                {
                    if (c >= columns.Length) db.table[l - 1, c] = string.Empty;
                    else if (int.TryParse(columns[c], out int iValue)) db.table[l - 1, c] = iValue;
                    else if (long.TryParse(columns[c], out long lValue)) db.table[l - 1, c] = lValue;
                    else if (float.TryParse(columns[c], out float fValue)) db.table[l - 1, c] = fValue;
                    else db.table[l - 1, c] = columns[c];
                }
            }

            GameManager.Log($"{path} load complete", "green");
        }
        catch (Exception e)
        {
            GameManager.Log($"{path} is not load : {e.Message}", "red");
        }
    }
}
