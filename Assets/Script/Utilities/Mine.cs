using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Mine
{
	namespace NineTap
	{
		public class Test
		{
			public static string GetDecToColumn(int col)
			{
				string result = string.Empty;

				while (col > 0)
				{
					int index = (col - 1) % 26;
					result = (char)(index + 'A') + result;
					col = (col - 1) / 26;
				}

				return result;
			}

			public static string GetDecToCellAddress(int col, int row)
			{
				string result = string.Empty;

				result = GetDecToColumn(col) + row.ToString();

				return result;
			}
		}
	}
}