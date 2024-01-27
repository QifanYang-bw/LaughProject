using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasUI {
    public class ExitButton : MonoBehaviour {
        private Button _button;

        private void Start() {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(ExitGame);
        }

        private void ExitGame() {
            GameManager.Instance.ExitGame();
        }
    }
}
