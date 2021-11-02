using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace HomaGames.HomaBelly
{
	public static class FileUtilities
	{
		public static string ReadAllText(string filePath)
		{
			if (filePath.Contains("://") || filePath.Contains(":///"))
			{
				UnityWebRequest www = UnityWebRequest.Get(filePath);
				www.SendWebRequest();
                // Wait until async operation has finished
				while(!www.isDone)
                {
					continue;
                }
				return www.downloadHandler.text;
			}
			else
			{
				return File.ReadAllText(filePath);
			}
		}
		
		/// <summary>
		/// Async method to load and deserialize a text file in an Resouces folder.
		/// </summary>
		/// <param name="filePath">A resources folder</param>
		/// <typeparam name="T">Type to deserialize the json</typeparam>
		/// <returns>The deserialized object or null of something goes wrong.</returns>
		public static async Task<T> LoadAndDeserializeJsonFromResources<T>(string filePath)
		{
			T result = default;
			
			var assetRequest = Resources.LoadAsync<TextAsset>(filePath);
			var completed = false;
			TextAsset textAsset = null;
			string json = null;
			var loadSucceeded = false;
			assetRequest.completed += delegate(AsyncOperation operation)
			{
				completed = true;
				if (assetRequest.asset != null)
				{
					textAsset = assetRequest.asset as TextAsset;
				}
				
				if (textAsset == null)
				{
					Debug.LogError($"[ERROR] No <b>TextAsset</b> found. Check the file exist and the type is TextAsset in path: {filePath} Asset:{assetRequest.asset}");
				}
				else
				{
					json = textAsset.text;
					Resources.UnloadAsset(assetRequest.asset);
					loadSucceeded = true;
				}
			};

			await Task.Run(async delegate
			{
				while (!completed)
				{
					// Wait one frame
					await Task.Delay(16);
				}
				
				if (!loadSucceeded)
				{
					return result;
				}
				
				if (string.IsNullOrWhiteSpace(json))
				{
					Debug.LogError($"[ERROR] The loaded file in path: {filePath} doesn't have a valid text");
					return default;
				}
				
				try
				{
					result = (T)Json.Deserialize(json);
				}
				catch (Exception deserializationException)
				{
					Debug.LogError($"[ERROR] Error deserializing the file: {filePath} Error: {deserializationException}");
				}

				if (result == null)
				{
					Debug.LogError($"[ERROR] Error deserializing the file: {filePath} Json: {json}");
				}

				return result;
			});

			return result;
		}

        public static void CreateIntermediateDirectoriesIfNecessary(string filePath)
        {
			string parentPath = Directory.GetParent(filePath).ToString();
			if (!string.IsNullOrEmpty(parentPath) && !Directory.Exists(parentPath))
			{
				Directory.CreateDirectory(parentPath);
			}
		}
	}
}
