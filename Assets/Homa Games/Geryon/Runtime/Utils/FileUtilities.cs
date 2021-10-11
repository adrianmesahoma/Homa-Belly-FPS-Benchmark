using System.IO;
using UnityEngine.Networking;

namespace HomaGames.Geryon
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
