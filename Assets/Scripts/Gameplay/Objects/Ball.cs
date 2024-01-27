using Assets.Scripts;
using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine.UI;

namespace Assets.Scripts {
    [Serializable]
    public class BallData {
        public Vector3 position;
        public Vector3 initialSpeed;
    }

    public class Ball : MonoBehaviour {
        public Vector3 initialSpeed;

        [Header("Runtime Variables")]
        public Vector3 speed;
        public Vector3 accel;
        public bool isTrajectoryOn;

        public const float oneSixth = 1 / 6f;
        float cubeRootTwo, w0, w1, c1, c2, c3, c4, d1, d2, d3;

        public delegate Vector2 acceletationDelegate(Vector2 pos);
        public delegate Vector4 rawUpdateDelegate(Vector4 state, float dt, Vector2 accel);

        private Vector4 rawUpdate(Vector4 state, float dt, Vector2 accel) {
            Vector2 dv = accel * dt;
            Vector2 dp = new Vector2(state.z, state.w) * dt;
            return new Vector4(dp.x, dp.y, dv.x, dv.y);
        }

        private Vector4 rkUpdate(rawUpdateDelegate fUpdate, Vector4 state, float dt, acceletationDelegate GetAccel) {
            Vector2 accel = GetAccel(state);
            Vector4 k1 = fUpdate(state, dt, accel);
            Vector4 k2 = fUpdate(state + dt * k1 * .5f, dt, accel);
            Vector4 k3 = fUpdate(state + dt * k2 * .5f, dt, accel);
            Vector4 k4 = fUpdate(state + dt * k3, dt, accel);
            return oneSixth * (k1 + k2 * 2 + k3 * 2 + k4);
        }

        private void Start() {
            cubeRootTwo = Mathf.Pow(2, 1 / 3.0f);
            w0 = -cubeRootTwo / (2 - cubeRootTwo);
            w1 = 1 / (2 - cubeRootTwo);
            c1 = w1 * 0.5f;
            c2 = (w0 + w1) * 0.5f;
            c3 = c2;
            c4 = c1;
            d1 = w1;
            d2 = w0;
            d3 = w1;

            speed = initialSpeed;

            BallData data = new BallData();
            data.position = transform.position;
            data.initialSpeed = initialSpeed;

            GravityManager.instance.ball = this;
            GravityManager.instance.ballData = data;
        }

        private void FixedUpdate() {
            if (LevelManager.instance.isPaused) {
                return;
            }

            if (isTrajectoryOn) {
                Vector3[] trajectory = PlotPredictionLine(GravityManager.instance.predictionLineStepCount);
                GetComponent<LineRenderer>().SetPositions(trajectory);
                GetComponent<LineRenderer>().positionCount = trajectory.Length;
            }
        }

        private void Update() {
            if (LevelManager.instance.isPaused) {
                return;
            }

            int steps = 500;
            float multiplier = 1 / (float)steps;

            for (int i = 0; i < steps; i++) {
                Vector2 pos = transform.position;

                float inGameSpeedRatio = GravityManager.instance.speedRatio;
                float simulatorDt = Time.deltaTime * inGameSpeedRatio * multiplier;

                Vector2 p1 = pos + c1 * (Vector2)speed * simulatorDt;
                Vector2 a1 = GravityManager.instance.GetAcceleration(p1);
                Vector2 v1 = (Vector2)speed + d1 * a1 * simulatorDt;

                Vector2 p2 = p1 + c2 * (Vector2)v1 * simulatorDt;
                Vector2 a2 = GravityManager.instance.GetAcceleration(p2);
                Vector2 v2 = (Vector2)v1 + d2 * a2 * simulatorDt;

                Vector2 p3 = p2 + c3 * (Vector2)v2 * simulatorDt;
                Vector2 a3 = GravityManager.instance.GetAcceleration(p3);
                Vector2 v3 = (Vector2)v2 + d3 * a3 * simulatorDt;

                transform.position = p3 + c4 * v3 * simulatorDt;
                speed = v3;
            }
            Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
            if (!(viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0)) {
                LevelManager.instance.Fail();
            }
        }

        public void SwitchTrajectoryState(bool isOn) {
            if (isTrajectoryOn == isOn) {
                return;
            }
            isTrajectoryOn = isOn;
            GetComponent<LineRenderer>().enabled = isOn;
        }

        private Vector2 GetSpeed(Vector2 accel, Vector2 speed, float speedRatio) {
            speed = (speed + accel * speedRatio) * (float)GravityManager.instance.damping;
            return speed;
        }
        private Vector2 GetPositionDelta(Vector2 speed, float speedRatio) {
            return speed * speedRatio;
        }

        private Vector3[] PlotPredictionLine(int steps) {
            Vector3[] plotline = new Vector3[steps];
            Vector2 localPos = transform.position;
            Vector2 localAccel;
            Vector2 localSpeed = speed;

            int newLength = 0;

            int segmentCoverage = GravityManager.instance.predictionLineSegmentCoverage;
            for (int i = 0; i < steps; i++) {
                for (int j = 0; j < segmentCoverage; j++) {
                    float multiplier = 1;
                    float inGameSpeedRatio = GravityManager.instance.speedRatio;
                    float simulatorDt = Time.deltaTime * multiplier * inGameSpeedRatio;

                    localAccel = GravityManager.instance.GetAcceleration(localPos);
                    Vector2 speed_half_step = localSpeed + localAccel * simulatorDt * 0.5f;
                    localPos = localPos + speed_half_step * simulatorDt;
                    Vector2 localAccelNext = GravityManager.instance.GetAcceleration(localPos);
                    localSpeed = speed_half_step + localAccelNext * simulatorDt * 0.5f;
                }
                plotline[i] = localPos;
                if (GravityManager.instance.IsPositionCollidedWithWell(localPos)) {
                    newLength = i + 1;
                    break;
                }
            }
            if (newLength == 0) {
                return plotline;
            } else {
                Vector3[] slicedPlotLine = new Vector3[newLength];
                for (int i = 0; i < newLength; i++) {
                    slicedPlotLine[i] = plotline[i];
                }
                return slicedPlotLine;
            }
        }
    }
}
