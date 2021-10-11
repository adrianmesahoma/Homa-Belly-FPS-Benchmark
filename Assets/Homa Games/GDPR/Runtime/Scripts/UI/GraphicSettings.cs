using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.GDPR
{
    public abstract class GraphicSettings<T> : MonoBehaviour where T : Component
    {
        private T _component;

        private void Awake()
        {
            _component = GetComponent<T>();
        }

        void Start()
        {
            UpdateGraphics();
        }

        private void UpdateGraphics()
        {
            Settings settings = (Settings) Resources.Load(Constants.SETTINGS_RESOURCE_NAME, typeof(Settings));
            if (settings != null)
            {
                UpdateGraphics(settings, _component);
            }
        }

        protected abstract void UpdateGraphics(Settings settings, T component);

#if UNITY_EDITOR

        private void OnValidate()
        {
            _component = GetComponent<T>();
            UpdateGraphics();
        }
#endif
    }
}