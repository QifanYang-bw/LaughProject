using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasUI {
    public class FadeMask : MonoBehaviour {
        public Image ImageMask;
        private float Speed = 5f;
        public event Action OnEnterMaskTransitionPoint;
        public event Action OnMaskTransitionComplete;

        public int mode;

        private void Start () {
            StartCoroutine(BeDark());
        }

        public IEnumerator BeDark()//½¥°×
        {
            Debug.LogFormat("FadeMask BeDark Enter");
            while (1 - ImageMask.color.a > 0.01f) {
                ImageMask.color = Color.Lerp(ImageMask.color, new Color(1, 1, 1, 1), Speed * Time.unscaledDeltaTime);
                yield return null;
            }
            OnEnterMaskTransitionPoint?.Invoke();
            yield return null;
            while (ImageMask.color.a > 0.01f) {
                float deltaTime = Mathf.Min(Time.unscaledDeltaTime, 0.01f);
                ImageMask.color = Color.Lerp(ImageMask.color, new Color(1, 1, 1, 0), Speed * deltaTime);
                yield return null;
            }
            Debug.LogFormat("FadeMask BeDark Exit");
            OnMaskTransitionComplete?.Invoke();
            ImageMask.gameObject.SetActive(false);
            yield return null;
        }
    }
}