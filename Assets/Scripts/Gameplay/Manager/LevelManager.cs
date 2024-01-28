using Assets.Scripts.CanvasUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
    public enum LevelMode {
        LevelModeTrial = 0,
        LevelModeAction = 1
    }

    public class LevelManager : MonoBehaviour {
        public static LevelManager instance;
        public bool isPaused = false;
        public LevelMode mode;

        [Header("Runtime Variables")]
        public Transform objectParent;
        public float curElapsedTime;
        public LevelScoreModel previousScoreModel;

        public event Action OnLevelReset;
        public event Action OnSwitchMode;

        public List<Wall> WallList;
        public List<NPC> NpcList;
        public List<Microphone> MicrophoneList;
        public List<Switch> SwitchList;

        private void Awake() {
            instance = this;
        }

        private void Start() {
            Camera mainCamera = Camera.main;
            objectParent = GameObject.Find("Objects")?.transform;
            if (objectParent == null) {
                Debug.Break();
                return;
            }

            NpcList.AddRange(objectParent.GetComponentsInChildren<NPC>());
            foreach (var npc in NpcList) {
                npc.OnMoodChangEvent += CheckAllNpcLaugh;
            }
            MicrophoneList.AddRange(objectParent.GetComponentsInChildren<Microphone>());
            SwitchList.AddRange(objectParent.GetComponentsInChildren<Switch>());

            Debug.LogFormat("LevelManager Microphones: {0}, NPCs: {1}, Switches: {2}", MicrophoneList.Count, NpcList.Count, SwitchList.Count);

            curElapsedTime = Time.time;
            if (GameManager.Instance.UserDataModel.levelScoreDict.ContainsKey(GameManager.Instance.currentLevelName())) {
                previousScoreModel = GameManager.Instance.UserDataModel.levelScoreDict[GameManager.Instance.currentLevelName()];
            } else {
                previousScoreModel = LevelScoreModel.EmptyScore();
            }

            if (null != SceneUIManager.Instance) {
                SceneUIManager.Instance.OnRetryLevel += ResetLevel;
                SceneUIManager.Instance.OnPauseLevel += PauseLevel;
                SceneUIManager.Instance.OnResumeLevel += ResumeLevel;
            }

            
            //PauseLevel();
        }

        private void Update() {
            //if (GameManager.Instance.state != GameState.Game || isPaused) {
            // Not in game (e.g. ScoreBoard) or paused, disable keyboard Interactions
            //Debug.LogFormat("[bullettime] {0} {1}", GameManager.Instance.state, isPaused);
            //    return;
            //}
        }

        public void AddWall(Wall wall) {
            if (wall == null) {
                return;
            }
            WallList.Add(wall);
        }

        public void Pass() {
            // PauseLevel();
            LevelScoreModel scoreData = GetScore();
            GameManager.Instance.CompleteLevel(scoreData);
        }

        public void Fail() {
            ResetLevel();
        }

        public void PauseLevel() {
            if (isPaused) {
                return;
            }
            isPaused = true;
            Time.timeScale = 0f;
        }

        public void ResumeLevel() {
            if (!isPaused) {
                return;
            }
            isPaused = false;
            Time.timeScale = 1f;
        }

        public void ResetLevel() {
            /* GravityManager.instance.ball.transform.parent = null;
            Destroy(GravityManager.instance.ball.gameObject);

            GameObject ball = (GameObject)Instantiate(AssetHelper.instance.Ball, objectParent);
            ball.transform.position = GravityManager.instance.ballData.position;
            ball.GetComponent<Ball>().initialSpeed = GravityManager.instance.ballData.initialSpeed;
            GravityManager.instance.ball = ball.GetComponent<Ball>(); */

            OnLevelReset();
            if (GameManager.Instance.isEditorModeOn) {
                // 用于调试全收集动画
                //totalGold = GetFullGoldCount() - 1;
            }
            curElapsedTime = Time.time;
        }

        public LevelScoreModel GetScore() {
            return new LevelScoreModel(
                Time.time - curElapsedTime
            );
        }

        private void CheckAllNpcLaugh()
        {
            if (mode == LevelMode.LevelModeTrial) {
                return;
            }
            if (!NpcList.TrueForAll(npc => npc.IsLaughing))
                return;
            Debug.LogFormat("Level {0} Pass!", GameManager.Instance.currentLevelName());
            Pass();
        }

        public void SwitchMode() {
            if (mode == LevelMode.LevelModeTrial) {
                mode = LevelMode.LevelModeAction;
            } else {
                mode = LevelMode.LevelModeTrial;
            }
            OnSwitchMode();
        }
    }
}