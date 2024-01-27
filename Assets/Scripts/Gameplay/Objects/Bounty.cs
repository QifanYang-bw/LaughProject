using System.Collections;
using UnityEngine;

namespace Assets.Scripts {
    public class Bounty : MonoBehaviour {
        public int gold;
        public bool hasTriggered = false;

        private void Start() {
            LevelManager.instance.OnLevelReset += Reset;
        }

        private void OnDestroy() {
            LevelManager.instance.OnLevelReset -= Reset;
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.gameObject.GetComponent<Ball>() == null) {
                //  Debug.LogWarningFormat("{0} collided with non-ball object {1}!",
                //      gameObject.name, collision.gameObject.name);
                return;
            }
            if (hasTriggered) {
                return;
            }
            AudioManager.Instance.PlaySFX(SfxType.TouchGold);
            hasTriggered = true;
            Color color = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, .1f);
        }

        public void Reset() {
            hasTriggered = false;
            Color color = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 1f);
        }
    }
}