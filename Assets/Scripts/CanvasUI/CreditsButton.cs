using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasUI {
    public class CreditsButton : MonoBehaviour {
        private Button _button;

        private void Start() {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(GoToCredits);
        }

        private void GoToCredits() {
            GameManager.Instance.GoToCredits();
        }
    }
}
