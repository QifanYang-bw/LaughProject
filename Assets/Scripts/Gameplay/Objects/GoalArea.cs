using System.Collections;
using UnityEngine;

namespace Assets.Scripts {
    public class GoalArea : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.gameObject.GetComponent<Ball>() == null) {
                Debug.LogWarningFormat("{0} collided with non-ball object {1}!",
                    gameObject.name, collision.gameObject.name);
                return;
            }
            Debug.LogFormat("{0} collided with ball object {1}, game passed",
                    gameObject.name, collision.gameObject.name);
            LevelManager.instance.Pass();
        }
    }
}