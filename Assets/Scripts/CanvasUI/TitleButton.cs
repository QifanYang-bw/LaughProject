using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasUI {
    public class TitleButton : MonoBehaviour {
        private Button _button;

        private void Start() {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(delegate { GameManager.Instance.GoTitleScreen(); });
        }
    }
}
