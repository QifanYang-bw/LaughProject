using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> {

    public GlobalConfigModel ConfigModel;
    public int levelProgress = 0;
    public int historyMaximumLevelProgress = 0;
    public bool isEditorModeOn;

    public int TitleSceneIndex = 0;
    public int LevelSelectionSceneIndex = 1;
    public int CreditSceneIndex = 2;
    public int BeginLevelIndex = 3;
    public int EndLevelIndex = 11;

    public bool CanShowLevelSelection;

    public UserDataModel UserDataModel;

    private GameObject _globalLevelCanvas;
    
    public GameState state;

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
            if (Input.GetKeyUp(KeyCode.N)) {
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

        SceneUIManager.Instance.ShowLevelTransition(LoadLevel);
        state = GameState.Game;
        levelProgress = BeginLevelIndex;
        if (levelProgress >= historyMaximumLevelProgress) {
            historyMaximumLevelProgress = levelProgress;
        }
        CanShowLevelSelection = false;
    }

    //当前关卡通关，跳到下一关卡并切换plane的texture
    public void CompleteLevel(LevelScoreModel scoreData) {
        if (AssetHelper.instance.ShouldShowScoreBoard(levelScenes()[levelProgress]) && scoreData != null) {
            state = GameState.ScoreBoard;

            Debug.LogFormat("Level Passed, Score is ({1}) = {2}",
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
        Assert.IsFalse(levelProgress < BeginLevelIndex || levelProgress > EndLevelIndex, $"levelProgress {levelProgress} out of range");
        state = GameState.Game;
        levelProgress += 1;
        if (levelProgress >= historyMaximumLevelProgress) {
            historyMaximumLevelProgress = levelProgress;
        }
        if (levelProgress >= EndLevelIndex) {
            GoPrologue();
            return;
        }
        LoadLevel();
    }

    public void GoPrologue() {
        GoTitleScreen();
    }

    public void GoTitleScreen() {
        state = GameState.Title;
        levelProgress = TitleSceneIndex;
        Debug.LogFormat("GameManager load title screen {0} {1}", levelProgress, levelScenes()[levelProgress]);
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
        SceneChange(levelScenes()[levelProgress]);
        //Material backgroundMaterial = AssetHelper.instance.BackgroundMaterials[levelProgress];
        //if (backgroundMaterial != null) {
        //    Debug.LogFormat("SceneChange background {0}, material {1}", levelScenes()[levelProgress], backgroundMaterial);
        //    Plane.Instance?.UpdateImage(levelScenes()[levelProgress], backgroundMaterial);
        //}

        SceneUIManager.Instance.RefreshCanvas();
    }

    private void SceneChange(string sceneName) {
        int sceneIndex = levelScenes().IndexOf(sceneName);
        if (sceneIndex == -1) {
            Debug.LogFormat("Scene {0} not found", sceneName);
            return;
        }
        AudioManager.Instance.PlayMusic(AssetHelper.instance.levelMusic[sceneIndex]);
        SceneUIManager.Instance.ClearCanvas();
        Debug.LogFormat("GameManager load scene {0}", sceneName);
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
