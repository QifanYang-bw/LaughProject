using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasUI {
    public class ConfigPanel : MonoBehaviour {
        [SerializeField]
        private Button EasyModeButton, NormalModeButton, HardModeButton;
        [SerializeField]
        private Slider MusicVolumeSlider, EffectVolumeSlider;

        public Dictionary<GlobalDifficultyType, Button> difficultySelectorDict;

        public void Awake() {
            difficultySelectorDict = new Dictionary<GlobalDifficultyType, Button> {
                { GlobalDifficultyType.Easy,  EasyModeButton },
                { GlobalDifficultyType.Normal,  NormalModeButton },
                { GlobalDifficultyType.Hard,  HardModeButton },
            };
        }

        public void UpdateData() {
            GlobalConfigModel model = GameManager.Instance.ConfigModel;
            ChangeDifficultyButtonState(model.Difficulty);
            MusicVolumeSlider.value = model.MusicVolume;
            EffectVolumeSlider.value = model.SfxVolume;

            EasyModeButton.onClick.AddListener(delegate { ChangeDifficulty(GlobalDifficultyType.Easy); });
            NormalModeButton.onClick.AddListener(delegate { ChangeDifficulty(GlobalDifficultyType.Normal); });
            HardModeButton.onClick.AddListener(delegate { ChangeDifficulty(GlobalDifficultyType.Hard); });

            MusicVolumeSlider.onValueChanged.AddListener(delegate { OnMusicVolumeChanged(); });
            EffectVolumeSlider.onValueChanged.AddListener(delegate { OnEffectVolumeChanged(); });
        } 

        private void OnMusicVolumeChanged() {
            AudioManager.Instance.SetMusicVolume(MusicVolumeSlider.value);
        }

        private void OnEffectVolumeChanged() {
            AudioManager.Instance.SetSfxVolume(EffectVolumeSlider.value);
        }

        private void ChangeDifficulty(GlobalDifficultyType mode) {
            GameManager.Instance.ConfigModel.Difficulty = mode;
            ChangeDifficultyButtonState(mode);
        }

        private void ChangeDifficultyButtonState(GlobalDifficultyType mode) {
            foreach (KeyValuePair<GlobalDifficultyType, Button> entry in difficultySelectorDict) {
                ColorBlock colorBlock = entry.Value.colors;
                if (entry.Key == mode) {
                    colorBlock.normalColor = new Color32(200, 178, 178, 255);
                } else {
                    colorBlock.normalColor = Color.white;
                }
                entry.Value.colors = colorBlock;
            }
        }
    }
}