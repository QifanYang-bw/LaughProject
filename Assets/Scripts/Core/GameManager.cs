using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Assets.Scripts.SceneUIManager;

public class GameManager : Singleton<GameManager> {

    public GlobalConfigModel ConfigModel;
    public int levelProgress = 0;
    public int historyMaximumLevelProgress = 0;
    public bool isEditorModeOn;
    public bool enableWellColliderDetection;

    public int MainLevelSceneIndex = 4;
    public int TotalLevel = 11;
    public int LevelSelectionSceneIndex = 12;
    public int CreditSceneIndex = 13;
    public float NormalDifficultyBulletTimeCostPerSec = 0.5f;
    public float HardDifficultyBulletTimeCostPerSec = 1f;
    public UserDataModel UserDataModel;

    private GameObject _globalLevelCanvas;
    
    public GameState state;
    public bool isLevelSelectionDisabled = true;

    public new void Awake() {
        base.Awake();
        UserDataModel = new UserDataModel();
        ConfigModel = new GlobalConfigModel();
    }

    private void Start() {
        state = GameState.Title;
        GameObject mainGameObject = GameObject.Find("Game");
        if (mainGameObject != null) {
            LevelManager levelManager = mainGameObject.GetComponent<LevelManager>();
            if (levelManager != null) {
                state = GameState.Game;
                SceneUIManager.Instance.RefreshCanvas();
            }
        }
        if (state != GameState.Game) {
            SceneUIManager.Instance.ClearCanvas();
        }
        AudioManager.Instance.PlayMusic(AssetHelper.instance.levelMusic[levelProgress]);
    }

    public void Update() {
        if (isEditorModeOn) {
            if (Input.GetKeyUp(KeyCode.A)) {
                GoNextLevel();
            }
        }
    }

    public void StartGame() {
        state = GameState.Game;

        AssetHelper.instance.LevelCanvas = GameObject.Find("CanvasLevel");
        if (AssetHelper.instance.LevelCanvas == null) {
            return;
        }
        GameObject _globalLevelCanvas = Instantiate(AssetHelper.instance.LevelCanvas);
        DontDestroyOnLoad(_globalLevelCanvas);

        levelProgress = 1;
        SceneUIManager.Instance.ShowLevelTransition(LoadLevel);
    }

    //当前关卡通关，跳到下一关卡并切换plane的texture
    public void CompleteLevel(LevelScoreModel scoreData) {
        if (!AssetHelper.instance.ShouldShowScoreBoard(levelScenes()[levelProgress]) && scoreData != null) {
            state = GameState.ScoreBoard;

            Debug.LogFormat("Level Passed, Score is ( {1}) = {2}",
                scoreData.RemainingTimeScore(),
                scoreData.TotalScore()
            );
            AudioManager.Instance.PlaySFX(SfxType.CompleteLevel);
            UserDataModel.levelScoreDict[AssetHelper.instance.levelScenes[levelProgress]] = scoreData;
            SceneUIManager.Instance.ShowScoreView(scoreData);
        } else {
            Debug.LogFormat("Level Passed without score");
            GameManager.Instance.FadeOutLevel();
        }
    }

    //当前关卡通关，跳到下一关卡并切换plane的texture
    public void FadeOutLevel() {
        SceneUIManager.Instance.ShowLevelTransition(GoNextLevel);
    }

    public void GoNextLevel() {
        state = GameState.Game;
        levelProgress += 1;
        if (levelProgress >= historyMaximumLevelProgress) {
            historyMaximumLevelProgress = levelProgress;
        }
        if (levelProgress >= TotalLevel) {
            GoPrologue();
            return;
        }
        LoadLevel();
    }

    public void GoPrologue() {
        state = GameState.Prologue;
        levelProgress = TotalLevel;
        SceneChange(levelScenes()[levelProgress]);

        SceneUIManager.Instance.ClearCanvas();
        Debug.LogFormat("GameManager load ending scene", levelScenes()[levelProgress]);
    }

    public void GoTitleScreen() {
        state = GameState.Title;
        levelProgress = 0;
        SceneChange(levelScenes()[levelProgress]);

        SceneUIManager.Instance.ClearCanvas();
        Debug.LogFormat("GameManager load title screen", levelScenes()[levelProgress]);
    }

    public void GoToLevelSelection() {
        state = GameState.Title;
        levelProgress = LevelSelectionSceneIndex;
        SceneChange(levelScenes()[levelProgress]);
    }

    public void GoToCredits() {
        state = GameState.Title;
        levelProgress = CreditSceneIndex;
        SceneChange(levelScenes()[levelProgress]);
    }

    public void JumptoLevel(int newLevelProgress) {
        state = GameState.Game;
        levelProgress = newLevelProgress;
        LoadLevel();
    }

    public void LoadLevel() {
        if (levelProgress >= MainLevelSceneIndex) {
            isLevelSelectionDisabled = false;
        }
        SceneChange(levelScenes()[levelProgress]);
        Material backgroundMaterial = AssetHelper.instance.BackgroundMaterials[levelProgress];
        if (backgroundMaterial != null) {
            Debug.LogFormat("SceneChange background {0}, material {1}", levelScenes()[levelProgress], backgroundMaterial);
            Plane.Instance?.UpdateImage(levelScenes()[levelProgress], backgroundMaterial);
        }

        SceneUIManager.Instance.RefreshCanvas();
        Debug.LogFormat("GameManager load scene {0}", levelScenes()[levelProgress]);
    }

    private void SceneChange(string sceneName) {
        int sceneIndex = levelScenes().IndexOf(sceneName);
        if (sceneIndex == -1) {
            Debug.LogFormat("Scene {0} not found", sceneName);
            return;
        }
        AudioManager.Instance.PlayMusic(AssetHelper.instance.levelMusic[sceneIndex]);
        SceneUIManager.Instance.ClearCanvas();
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    //场景名称列表
    private List<string> levelScenes() {
        return AssetHelper.instance.levelScenes;
    }

    public string currentLevelName() {
        return AssetHelper.instance.levelScenes[levelProgress];
    }

}
