using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.CanvasUI {
    public class LevelSelectionPanel : MonoBehaviour {
        public LevelJumpButton[] _buttons;

        private void Awake() {
            _buttons = GetComponentsInChildren<LevelJumpButton>();
        }

        private void Start() {
            foreach (LevelJumpButton button in _buttons) {
                button.gameObject.SetActive(button.levelIndex <= GameManager.Instance.historyMaximumLevelProgress);
            }
        }
    }
}