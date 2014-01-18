using UnityEngine;
using System.Collections;

struct DebugUtils
{
	public static void assert(bool condition, string message)
	{
		if(!condition)
		{
			Debug.LogError(message);
			throw new System.Exception(message);
		}
	}

	public static void warning(string message)
	{
		Debug.LogWarning (message);
	}

	public static void log(string message)
	{
		Debug.Log (message);
	}
}
