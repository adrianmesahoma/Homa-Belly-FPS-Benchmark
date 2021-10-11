using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace HomaGames.GDPR
{
    public abstract class View<T> : View where T : View<T>
    {
        public static T Instance => GdprGameObject.GetComponent<T>();

        private void OnDestroy()
        {
            ActiveViews.Remove(this);
        }
    }

    public abstract class View : MonoBehaviour
    {
        public static event System.Action OnAllViewDismissed;
        public static event System.Action OnFirstViewShown;
        private static GameObject gdprPrefab;
        private static GameObject gdprPrefabInstance; 
        public static GameObject GdprGameObject
        {
            get
            {
                if(gdprPrefab==null)
                    gdprPrefab = (GameObject) Resources.Load(Constants.PREFAB_RESOURCE_NAME, typeof(GameObject));
                if(gdprPrefab==null)
                    Debug.LogError($"[Homa Games GDPR] Could not show GDPR. Prefab {Constants.PREFAB_RESOURCE_NAME} is missing");
                if (gdprPrefabInstance == null)
                {
                    OnFirstViewShown?.Invoke();
                    gdprPrefabInstance = Object.Instantiate(gdprPrefab);
                }
                return gdprPrefabInstance;
            }
        }
        public static readonly HashSet<View> ActiveViews = new HashSet<View>();

        public Transform root;

        protected virtual Transform backgroundClickBlocker { get; }

        protected View ParentView;

        public bool IsVisible
        {
            get => root && root.gameObject.activeSelf;

            set
            {
                if (root)
                {
                    root.gameObject.SetActive(value);
                    // Reset UI buttons states
                    if(value)
                        EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }

        public void Enter(View parentView = null)
        {
            if(backgroundClickBlocker)
                backgroundClickBlocker.gameObject.SetActive(true);
            ParentView = parentView;
            ActiveViews.Add(this);
            IsVisible = true;
            OnEnter();
        }

        public void Dismiss()
        {
            // Upon dismiss, first thing we always deactivate background click blocker.
            // Next view will be responsible of enabling it if desired
            if (backgroundClickBlocker)
                backgroundClickBlocker.gameObject.SetActive(false);

            ActiveViews.Remove(this);
            IsVisible = false;
            OnDismiss();
            if (ActiveViews.Count == 0)
            {
                OnAllViewDismissed?.Invoke();
                Destroy(gdprPrefabInstance);
            }
        }

        protected abstract void OnEnter();
        protected abstract void OnDismiss();
    }
}