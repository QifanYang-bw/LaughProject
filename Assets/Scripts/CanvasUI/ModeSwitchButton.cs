using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasUI {
    public class ModeSwitchButton : MonoBehaviour {
        private Button _button;

        public Sprite trialImage, actionImage;

        private void Start() {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(SwitchMode);
        }

        private void SwitchMode() {
            LevelManager.instance.SwitchMode();
            if (LevelManager.instance.mode == LevelMode.LevelModeTrial) {
                GetComponent<Image>().sprite = trialImage;
            } else if (LevelManager.instance.mode == LevelMode.LevelModeAction) {
                GetComponent<Image>().sprite = actionImage;
            }

        }
    }
}
