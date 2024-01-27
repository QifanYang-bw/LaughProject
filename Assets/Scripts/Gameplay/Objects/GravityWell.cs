using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

namespace Assets.Scripts {
    public enum GravityWellModifierStatus {
        None,
        Attract,
        Repel
    }
    public class GravityWell : MonoBehaviour {
        public float strength;
        public float attractRatioPerSecond;
        public float repelRatioPerSecond;

        private float initialStrength;

        [Header("Runtime Variables")]
        public GravityWellModifierStatus status;

        private void Awake() {
            initialStrength = strength;
        }

        private void Start() {
            GravityManager.instance.AddGravityWell(this);
            LevelManager.instance.OnLevelReset += Reset;
        }

        private void FixedUpdate() {
            float realDeltaTime = Time.fixedDeltaTime / ForceCalculator.standardTimeDelta;
            switch (status) {
                case GravityWellModifierStatus.Attract:
                    strength += realDeltaTime * attractRatioPerSecond;
                    break;
                case GravityWellModifierStatus.Repel:
                    strength -= realDeltaTime * repelRatioPerSecond;
                    break;
                default:
                    break;
            }
        }

        public void Reset() {
            strength = initialStrength;
        }
    }
}