#if HE_SYSCORE

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UX.API
{
    public static class Json
	{
		[Serializable]
		private class JSONList<T>
        {
			public List<T> result;
        }

		/// <summary>
		/// A utility method for wrapping JSON arrays in a result tag as Unity's JsonUtility expects
		/// </summary>
		/// <param name="JSON">the JSON array to wrap</param>
		/// <returns></returns>
		public static string ArrayWrapper(string JSON)
		{
			if (JSON.StartsWith("["))
				return "{\"result\":" + JSON + "}";
			else
				return JSON;
		}

		/// <summary>
		/// Takes a typical JSON array string, wraps it in a object structure and returns the members as a List
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="jsonString"></param>
		/// <returns></returns>
		public static List<T> FetchArray<T>(string jsonString)
        {
			var output = JsonUtility.FromJson<JSONList<T>>(ArrayWrapper(jsonString));
			return output.result;
        }
	}
}

#endif