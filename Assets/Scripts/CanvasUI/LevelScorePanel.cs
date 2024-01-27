using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.CanvasUI {
    public class LevelScorePanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {
        private TextMeshProUGUI totalScoreText, goldText, bulletTimeText, remainingTimeText;

        private void Awake() {
            totalScoreText = transform.Find("TotalScore").GetComponentInChildren<TextMeshProUGUI>();
            goldText = transform.Find("GoldScore").GetComponentInChildren<TextMeshProUGUI>();
            bulletTimeText = transform.Find("BulletTimeScore").GetComponentInChildren<TextMeshProUGUI>();
            remainingTimeText = transform.Find("RemainingTimeScore").GetComponentInChildren<TextMeshProUGUI>();
        }

        public void UpdateScore(LevelScoreModel data) {
            remainingTimeText.text = ((int)data.RemainingTimeScore()).ToString();
            totalScoreText.text = ((int)data.TotalScore()).ToString();
        }

        public void OnPointerDown(PointerEventData eventData) {
            SwitchScene();
        }
        public void OnPointerUp(PointerEventData eventData) {
            SwitchScene();
        }
        public void OnPointerClick(PointerEventData eventData) {
            SwitchScene();
        }

        public void SwitchScene() {
            GameManager.Instance.FadeOutLevel();
        }
    }
}