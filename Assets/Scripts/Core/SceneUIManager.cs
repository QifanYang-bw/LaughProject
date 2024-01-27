using System;
using System.Collections;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Assets.Scripts.CanvasUI;

namespace Assets.Scripts {
    public class SceneUIManager : Singleton<SceneUIManager>
    {
        public event Action OnPauseLevel;
        public event Action OnRetryLevel;
        public event Action OnResumeLevel;

        public Transform _pnlMain;
        public StrokePanel _pnlMainStroke;
        public Transform _pnlMenu;
        public Transform _btnLevelSelection;
        public CoverScreen _viewScreen;
        public Slider BulletTimeSlider;
        public LevelScorePanel _pnlScore;
        public ConfigPanel _pnlConfig;
        public TotalScorePanel _pnlTotalScore;
        public Button _btnPlay;

        public GameObject FadeMask;
        private bool isFading;

        public delegate void OnTransitionFinishedDelegate();

        public void ShowLevelTransition(OnTransitionFinishedDelegate onTransitionFinishedMethod) {
            if (isFading) {
                return;
            }
            isFading = true;
            GameObject maskObject = Instantiate(FadeMask, transform);
            FadeMask mask = maskObject.GetComponent<FadeMask>();
            mask.OnEnterMaskTransitionPoint += () => onTransitionFinishedMethod();
            mask.OnEnterMaskTransitionPoint += delegate () {
                isFading = false;
            };
            maskObject.SetActive(true);
        }

        public void SwitchBulletTimeEffect(bool isEnabled) {
            if (LevelManager.instance == null) {
                return;
            }
            GravityManager.instance.ball.SwitchTrajectoryState(isEnabled);
            LevelManager.instance.blurEffect.intensity = isEnabled ? 0.4f : 0f;
        }

        public void UpdatePanelMainStrokeData(LevelScoreModel model) {
            _pnlMainStroke.UpdateLevelData(GameManager.Instance.currentLevelName(), model);
        }

        public void RefreshCanvas() {
            _pnlMain.gameObject.SetActive(true);
            _pnlMainStroke.UpdateData(GameManager.Instance.UserDataModel);

            _viewScreen.gameObject.SetActive(true);
            _viewScreen.isOnClickContinueEnabled = true;
            _pnlScore.gameObject.SetActive(false);
            _pnlMenu.gameObject.SetActive(false);
            _btnPlay.gameObject.SetActive(true);
            _pnlConfig.gameObject.SetActive(false);
            _pnlTotalScore.gameObject.SetActive(false);

            BulletTimeSlider.gameObject.SetActive(false);
        }
        public void ClearCanvas() {
            _viewScreen.gameObject.SetActive(false);
            _pnlMain.gameObject.SetActive(false);
            _pnlScore.gameObject.SetActive(false);
            _pnlMenu.gameObject.SetActive(false);
            _btnPlay.gameObject.SetActive(false);
            _pnlConfig.gameObject.SetActive(false);
            _pnlTotalScore.gameObject.SetActive(false);
            BulletTimeSlider.gameObject.SetActive(false);
        }

        #region 暂停开始界面
        public void Play()
        {
            Resume();
        }

        public void Pause() {
            OnPauseLevel?.Invoke();
            _viewScreen.isOnClickContinueEnabled = false;
            _viewScreen.gameObject.SetActive(true);
            _btnPlay.gameObject.SetActive(false);
            _pnlMenu.gameObject.SetActive(true);
            _pnlMain.gameObject.SetActive(false);
            _pnlConfig.gameObject.SetActive(false);
            _pnlTotalScore.gameObject.SetActive(false);
            _btnLevelSelection.gameObject.SetActive(!GameManager.Instance.isLevelSelectionDisabled);
        }

        public void Retry() {
            OnRetryLevel?.Invoke();
        }

        public void Resume() {
            OnResumeLevel?.Invoke();
            _viewScreen.gameObject.SetActive(false);
            _btnPlay.gameObject.SetActive(false);
            _pnlMenu.gameObject.SetActive(false);
            _pnlMain.gameObject.SetActive(true);
            _pnlScore.gameObject.SetActive(false);
            _pnlConfig.gameObject.SetActive(false);
            _pnlTotalScore.gameObject.SetActive(false);
        }
        #endregion

        //结算界面
        public void ShowScoreView(LevelScoreModel data) {
            _viewScreen.gameObject.SetActive(true);
            _viewScreen.isOnClickContinueEnabled = true;
            _btnPlay.gameObject.SetActive(false);
            _pnlScore.gameObject.SetActive(true);
            _pnlMain.gameObject.SetActive(false);
            _pnlMenu.gameObject.SetActive(false);
            _pnlConfig.gameObject.SetActive(false);
            _pnlTotalScore.gameObject.SetActive(false);
            BulletTimeSlider.gameObject.SetActive(false);

            _pnlScore.UpdateScore(data);
        }

        // Config View
        public void ShowConfigView() {
            _viewScreen.gameObject.SetActive(true);
            _viewScreen.isOnClickContinueEnabled = true;
            _btnPlay.gameObject.SetActive(false);
            _pnlScore.gameObject.SetActive(false);
            _pnlMain.gameObject.SetActive(false);
            _pnlMenu.gameObject.SetActive(false);
            _pnlTotalScore.gameObject.SetActive(false);
            _pnlConfig.gameObject.SetActive(true);
            _pnlConfig.UpdateData();
            BulletTimeSlider.gameObject.SetActive(false);
        }

        // 总结算界面
        public void ShowTotalScoreView(UserDataModel data) {
            _viewScreen.gameObject.SetActive(true);
            _viewScreen.isOnClickContinueEnabled = true;
            _btnPlay.gameObject.SetActive(false);
            _pnlScore.gameObject.SetActive(false);
            _pnlMain.gameObject.SetActive(false);
            _pnlMenu.gameObject.SetActive(false);
            _pnlConfig.gameObject.SetActive(false);
            BulletTimeSlider.gameObject.SetActive(false);

            _pnlTotalScore.gameObject.SetActive(true);
            _pnlTotalScore.UpdateModel(data);
        }

        public void JumpToTitle() {
            GameManager.Instance.GoTitleScreen();
        }
        public void GoLevelSelection() {
            GameManager.Instance.GoToLevelSelection();
        }

        #region BulletTime
        /// <summary>
        /// Enter bullet time
        /// </summary>
        public void OnActivateBulletTime() {
            if (GameManager.Instance.ConfigModel.Difficulty != GlobalDifficultyType.Easy) {
                BulletTimeSlider.gameObject.SetActive(true);
            }
            Camera.main.GetComponent<BlurEffect>().intensity = 0.4f;
        }

        /// <summary>
        /// Exit bullet time
        /// </summary>
        public void OnDeactivateBulletTime() {
            BulletTimeSlider.gameObject.SetActive(false);
            Camera.main.GetComponent<BlurEffect>().intensity = 0f;
        }
        /// <summary>
        /// Edit remaining bullet time value
        /// </summary>
        /// param 进度条的进度值（0到1）name="value"></param>
        public void OnChangeSlider(float val)
        {
            BulletTimeSlider.value = val;
        }
        #endregion

    }
}