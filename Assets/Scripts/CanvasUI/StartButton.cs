using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasUI {
    public class StartButton : MonoBehaviour {
        private Button _button;

        private void Start() {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(StartGame);
        }

        private void StartGame() {
            if (GameManager.Instance.CanShowLevelSelection) {
                GameManager.Instance.GoToLevelSelection();
            } else {
                GameManager.Instance.StartGame();
            }
        }
    }
}
