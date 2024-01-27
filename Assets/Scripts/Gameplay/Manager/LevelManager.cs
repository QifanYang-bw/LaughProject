using Assets.Scripts.CanvasUI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.XR;

namespace Assets.Scripts {
    public class LevelManager : MonoBehaviour {
        public static LevelManager instance;

        public bool isPaused = false;
        public Animator FullGoldAnimator;
        public SpriteRenderer PatternSprite;

        [Header("Runtime Variables")]
        public Transform objectParent;
        public float curElapsedTime;
        public LevelScoreModel previousScoreModel;

        public event Action OnLevelReset;

        // The following properties should belong to effectManager
        public TwistEffect twistEffect;
        public BlurEffect blurEffect;

        public List<Wall> wallList;
        public List<NPC> Npcs;

        private void Awake() {
            instance = this;
        }

        private void Start() {
            Camera mainCamera = Camera.main;
            twistEffect = mainCamera.gameObject.AddComponent<TwistEffect>();
            blurEffect = mainCamera.gameObject.AddComponent<BlurEffect>();
            objectParent = GameObject.Find("Objects")?.transform;

            if (objectParent != null)
                Npcs.AddRange(objectParent.GetComponentsInChildren<NPC>());
            foreach (var npc in Npcs)
            {
                npc.OnMoodChangEvent += CheckAllNpcLaugh;
            }

            curElapsedTime = Time.time;
            if (GameManager.Instance.UserDataModel.levelScoreDict.ContainsKey(GameManager.Instance.currentLevelName())) {
                previousScoreModel = GameManager.Instance.UserDataModel.levelScoreDict[GameManager.Instance.currentLevelName()];
            } else {
                previousScoreModel = LevelScoreModel.EmptyScore();
            }
            if (FullGoldAnimator != null) {
                FullGoldAnimator.gameObject.SetActive(false);
            }
            ResetAnimator();
            OnLevelReset += ResetAnimator;

            if (null != SceneUIManager.Instance) {
                SceneUIManager.Instance.OnRetryLevel += ResetLevel;
                SceneUIManager.Instance.OnPauseLevel += PauseLevel;
                SceneUIManager.Instance.OnResumeLevel += ResumeLevel;
            }
            if (GameManager.Instance.isEditorModeOn) {
                // 用于调试全收集动画
                // totalGold = GetFullGoldCount() - 1;
            }
            PauseLevel();
        }

        private void Update() {
            if (GravityManager.instance == null) {
                return;
            }
            if (GameManager.Instance.state != GameState.Game || isPaused) {
                // Not in game (e.g. ScoreBoard) or paused, disable keyboard Interactions
                //Debug.LogFormat("[bullettime] {0} {1}", GameManager.Instance.state, isPaused);
                return;
            }
        }

        public void AddWall(Wall wall) {
            if (wall == null) {
                return;
            }
            wallList.Add(wall);
        }

        public void Pass() {
            PauseLevel();
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
            if (FullGoldAnimator != null) {
                FullGoldAnimator.speed = 0;
            }
        }

        public void ResumeLevel() {
            if (!isPaused) {
                return;
            }
            isPaused = false;
            Time.timeScale = 1f;
            if (FullGoldAnimator != null) {
                FullGoldAnimator.speed = 1;
            }
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

        public void ResetAnimator() {
            if (FullGoldAnimator != null) {
                FullGoldAnimator.gameObject.SetActive(false);
            }
            SceneUIManager.Instance.UpdatePanelMainStrokeData(previousScoreModel);
            if (PatternSprite != null) {
                PatternSprite?.gameObject?.SetActive(!false);
            }
        }

        public LevelScoreModel GetScore() {
            return new LevelScoreModel(
                Time.time - curElapsedTime
            );
        }

        public int GetFullGoldCount() {
            Debug.LogFormat("[LevelManager] objectParent GetFullGoldCount {0}", objectParent);
            Bounty[] bountyTrans = objectParent.GetComponentsInChildren<Bounty>();
            int fullGoldScore = 0;
            foreach (Bounty bounty in bountyTrans) {
                fullGoldScore += bounty.gold;
            }
            Debug.LogFormat("fullGoldScore {0}", fullGoldScore);
            return fullGoldScore;
        }

        public void OnPatternCollected() {
            Debug.LogFormat("OnPatternCollected");
            SceneUIManager.Instance.UpdatePanelMainStrokeData(GetScore());
        }

        private void CheckAllNpcLaugh()
        {
            if (!Npcs.TrueForAll(npc => npc.IsLaughing))
                return;
            Debug.Log("level pass!");
            // todo lwttai
        }
    }
}