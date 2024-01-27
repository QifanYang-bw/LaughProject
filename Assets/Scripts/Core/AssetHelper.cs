using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum MusicType {
    None = -1,
    TeachLevel = 0,
    MainLevel = 1,
    MainMenu = 2,
}
public enum SfxType {
    StrokeComplete = 0,
    CompleteLevel = 1,
    TouchGold = 2,
    BallRolling = 3,
    BulletTime = 4,
}
public enum GameState {
    Title = 0,
    Game = 1,
    ScoreBoard = 2,
    Prologue = 3,
}

namespace Assets.Scripts {
    public class AssetHelper : MonoBehaviour {
        public static AssetHelper instance;

        public GameObject LevelCanvas;
        public List<Material> BackgroundMaterials;
        public GameObject WavePrefab;

        public List<string> TutorialSceneNames = new List<string>
        {
            "Tutorial_0",
            "Tutorial_1",
            "Tutorial_2",
        };

        public List<string> GameLevelSceneNames = new List<string>
        {
            "Level_1",
            "Level_2",
            "Level_3",
            "Level_4",
            "Level_5",
            "Level_6",
            "Level_7",
        };

        //场景名称列表
        public List<string> levelScenes = new List<string> {
            "TitleScreenScene",
            "Tutorial_0",
            "Tutorial_1",
            "Tutorial_2",
            "Level_1",
            "Level_2",
            "Level_3",
            "Level_4",
            "Level_5",
            "Level_6",
            "Level_7",
            "Ending"
        };

        //BGM列表
        public List<MusicType> levelMusic = new List<MusicType> {
            MusicType.MainMenu,
            MusicType.TeachLevel,
            MusicType.TeachLevel,
            MusicType.TeachLevel,
            MusicType.TeachLevel,
            MusicType.TeachLevel,
            MusicType.TeachLevel,
            MusicType.MainLevel,
            MusicType.MainLevel,
            MusicType.MainLevel,
            MusicType.MainLevel,
            MusicType.MainMenu
        };

        private void Awake() {
            instance = this;
        }

        public bool ShouldShowScoreBoard(string sceneName) {
            int sceneIndex = levelScenes.IndexOf(sceneName);
            if (sceneIndex == -1) {
                Debug.LogFormat("Scene {0} not found", sceneName);
                return false;
            }
            return sceneIndex <= 3;
        }
    }
}