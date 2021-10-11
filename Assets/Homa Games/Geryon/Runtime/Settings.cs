using UnityEngine;

namespace HomaGames.Geryon {

	public sealed class Settings : ScriptableObject {

#pragma warning disable 0649
		[SerializeField] private string androidAppID;
		[SerializeField] private string iOSAppID;
		[SerializeField] private bool debugEnabled;
#pragma warning restore 0649

		public void SetAndroidID(string androidAppID)
        {
			this.androidAppID = androidAppID;
        }

        public void SetiOSID(string iOSAppID)
        {
			this.iOSAppID = iOSAppID;
        }

        public void SetDebugEnabled(bool enabled)
        {
			this.debugEnabled = enabled;
        }

		public string GetAndroidID () {

			return androidAppID;

		}

		public string GetIOSID () {

			return iOSAppID;

		}

        public bool IsDebugEnabled()
        {
			return debugEnabled;
        }
	}
}