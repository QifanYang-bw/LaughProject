using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasUI {
    public class StrokeCompleteAnimator : MonoBehaviour {
        public event Action OnStrokeEnd;

        private void OnStrokeComplete() {
            Debug.LogFormat("OnStrokeComplete");
            OnStrokeEnd();
            gameObject.SetActive(false);
        }
    }
}
