using UnityEngine;
using System.Collections;

/// <summary>
/// Callback Coroutine Method
/// </summary>
/// <param name="o"></param>
/// <returns></returns>
public delegate IEnumerator CallbackCoroutineMethod(params object[] o);

/// <summary>
/// Callback void Method
/// </summary>
/// <param name="o"></param>
public delegate void CallbackVoidMethod(params object[] o);


/// <summary>
/// Callback void Method
/// </summary>
/// <param name="o"></param>
public delegate int CallbackIntMethod(params object[] o);


/// <summary>
/// Callback void Method
/// </summary>
/// <param name="o"></param>
public delegate bool CallbackBoolMethod(params object[] o);

/// <summary>
/// 
/// </summary>
public delegate void CallbackResourceLoad(Object o, params object[] param);