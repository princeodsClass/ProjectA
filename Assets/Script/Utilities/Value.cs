using System;


public enum VALUE_TYPE
{
	INT32,
	INT64,
	STRING,
	FLOAT,
	BYTE,
	ARRAY,
}

public abstract class Value
{
	public abstract new Type GetType();

	public static implicit operator int(Value v)
	{
		if (v == null || !(v is Value<int>)) return 0;
		return ((Value<int>)v).value;
	}

	public static implicit operator float(Value v)
	{
		if (v == null || !(v is Value<float>)) return 0f;
		return ((Value<float>)v).value;
	}

	public static implicit operator string(Value v)
	{
		if (v == null) return string.Empty;
		return v.ToString();
	}

	public static implicit operator long(Value v)
	{
		if (v == null || !(v is Value<long>)) return 0L;
		return ((Value<long>)v).value;
	}

	public static implicit operator byte(Value v)
	{
		if (v == null || !(v is Value<byte>)) return 0;
		return ((Value<byte>)v).value;
	}

	public static implicit operator int[](Value v)
	{
		if (v == null || !(v is Value<int[]>)) return null;
		return ((Value<int[]>)v).value;
	}

	public static implicit operator Value(int v)
	{
		return new Value<int>(v);
	}

	public static implicit operator Value(float v)
	{
		return new Value<float>(v);
	}

	public static implicit operator Value(string v)
	{
		return new Value<string>(v);
	}

	public static implicit operator Value(long v)
	{
		return new Value<long>(v);
	}

	public static implicit operator Value(byte v)
	{
		return new Value<byte>(v);
	}

	public static implicit operator Value(int[] v)
	{
		return new Value<int[]>(v);
	}

	public static int TryParseInt32(string text)
	{
		int value;
		int.TryParse(text, out value);
		return value;
	}

	public static long TryParseInt64(string text)
	{
		long value;
		long.TryParse(text, out value);
		return value;
	}

	public static float TryParseFloat(string text)
	{
		float value;
		float.TryParse(text, out value);
		return value;
	}

	//-----------------------------------------------------------------------------
	//      
	//-----------------------------------------------------------------------------
	public static Value ParsingToData(string type, object data)
	{
		VALUE_TYPE valuetype = (VALUE_TYPE)Enum.Parse(typeof(VALUE_TYPE), type);
		switch (valuetype)
		{
			case VALUE_TYPE.INT32:
				{
					int value;
					Int32.TryParse(data.ToString(), out value);

					return value;
				}

			case VALUE_TYPE.INT64:
				{
					long value;
					Int64.TryParse(data.ToString(), out value);

					return value;
				}
				  
			case VALUE_TYPE.STRING:
				{
					return data.ToString();
				}

			case VALUE_TYPE.FLOAT:
				{
					float value;
					float.TryParse(data.ToString(), out value);

					return value;
				}

			case VALUE_TYPE.BYTE:
				{
					byte value;
					byte.TryParse(data.ToString(), out value);

					return value;
				}

			case VALUE_TYPE.ARRAY:
				{
					string[] array = ComUtil.DivideString(data.ToString());
					int[] arrayInt32 = System.Array.ConvertAll<string, int>(array, Value.TryParseInt32);

					return arrayInt32;
				}
		}

		return 0;
	}
}
//==========================================================================================================================

public class Value<T> : Value
{
	public T value;

	public Value()
	{
		this.value = default(T);
	}

	public Value(T value)
	{
		this.value = value;
	}

	public override string ToString()
	{
		return this.value.ToString();
	}

	public override Type GetType()
	{
		return this.value.GetType();
	}
}
//==========================================================================================================================