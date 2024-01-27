using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasUI {
    public class ProloguePlayer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {
        private VideoPlayer _player;

        private void Awake() {
            _player = GetComponent<VideoPlayer>();
            _player.loopPointReached += EndReached;
        }

        public void OnPointerDown(PointerEventData eventData) {
            SkipVideo();
        }
        public void OnPointerUp(PointerEventData eventData) {
            SkipVideo();
        }
        public void OnPointerClick(PointerEventData eventData) {
            SkipVideo();
        }
        private void EndReached(UnityEngine.Video.VideoPlayer videoPlayer) {
            SkipVideo();
        }

        public void SkipVideo() {
            SceneUIManager.Instance.ShowTotalScoreView(GameManager.Instance.UserDataModel);
        }
    }
}
