using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasUI {
    public class LevelJumpButton : MonoBehaviour {
        private Button _button;
        public int levelIndex;

        private void Awake() {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(JumpToLevel);
        }

        private void JumpToLevel() {
            GameManager.Instance.JumptoLevel(levelIndex);
        }

    }
}
