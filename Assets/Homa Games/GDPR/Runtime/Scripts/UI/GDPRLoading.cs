using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.GDPR
{
    [RequireComponent(typeof(Image), typeof(CanvasGroup))]
    public class GDPRLoading : MonoBehaviour
    {
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        void Update()
        {
            rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, rectTransform.rotation * Quaternion.Euler(0f,0f,120f), Time.deltaTime);
            if (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Mathf.Lerp(0f, 0.25f, Time.deltaTime * 10f);
            }
        }
    }
}