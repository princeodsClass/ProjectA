using System;
using System.Collections.Generic;

/// <summary>
/// data value
/// </summary>
public class DataValue
{
	static T get<T>(DataValue v)
	{
		return ((DataValue<T>)v).value;
	}

	public static implicit operator int(DataValue v)
	{
		if (v is DataValue<int>) return get<int>(v);
		if (v is DataValue<long>) return (int)get<long>(v);
		if (v is DataValue<float>) return (int)get<float>(v);
		return 0;
	}

	public static implicit operator long(DataValue v)
	{
		if (v is DataValue<long>) return get<long>(v);
		if (v is DataValue<int>) return (long)get<int>(v);
		if (v is DataValue<float>) return (long)get<float>(v);
		return 0;
	}

	public static implicit operator float(DataValue v)
	{
		if (v is DataValue<float>) return get<float>(v);
		if (v is DataValue<long>) return (float)get<long>(v);
		if (v is DataValue<int>) return (float)get<int>(v);
		return 0f;
	}

    public static implicit operator uint(DataValue v)
    {
		if (v is DataValue<uint>) return get<uint>(v);
		if (v is DataValue<float>) return (uint)get<float>(v);
        if (v is DataValue<long>) return (uint)get<long>(v);
        if (v is DataValue<int>) return (uint)get<int>(v);
		
		return 0;
    }

    public static implicit operator string(DataValue v)
	{
		return v.ToString();
	}

	public static implicit operator string[](DataValue v)
	{
		return ComUtil.DivideString(v);
	}

	public static implicit operator int[](DataValue v)
	{
		string[] arr = ComUtil.DivideString(v);
		return System.Array.ConvertAll<string, int>(arr, Value.TryParseInt32);
	}

	public static implicit operator long[](DataValue v)
	{
		string[] arr = ComUtil.DivideString(v);
		return Array.ConvertAll<string, long>(arr, Value.TryParseInt64);
	}

	public static implicit operator float[](DataValue v)
	{
		string[] arr = ComUtil.DivideString(v);
		return System.Array.ConvertAll<string, float>(arr, Value.TryParseFloat);
	}

	public static implicit operator DataValue(int v)
	{
		return new DataValue<int>(v);
	}

	public static implicit operator DataValue(long v)
	{
		return new DataValue<long>(v);
	}

	public static implicit operator DataValue(float v)
	{
		return new DataValue<float>(v);
	}

	public static implicit operator DataValue(string v)
	{
		return new DataValue<string>(v);
	}

    public static implicit operator DataValue(uint v)
    {
        return new DataValue<uint>(v);
    }
}


/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class DataValue<T> : DataValue
{
    public T value;

	public DataValue()
    {
        this.value = default(T);
    }

    public DataValue(T value)
    {
        this.value = value;
    }

    public override string ToString()
    {
        return this.value.ToString();
    }
}