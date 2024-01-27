using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasUI {
    public class RetryButton : MonoBehaviour {
        private Button _button;

        private void Start() {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Retry);
        }

        private void Retry() {
            LevelManager.instance.Fail();
        }
    }
}
