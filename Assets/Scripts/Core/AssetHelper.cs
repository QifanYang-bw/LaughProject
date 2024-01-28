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
        public List<Material> WaveMaterials;
        public List<Sprite> SpeakerSprites;
        public List<Sprite> WallSprites;
        public GameObject WavePrefab;

        public List<string> TutorialSceneNames;

        public List<string> GameLevelSceneNames;

        //场景名称列表
        public List<string> levelScenes;

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
            return false;
        }
    }
}