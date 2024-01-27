using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioManager;

public class AudioManager : Singleton<AudioManager> {
    public List<AudioClip> MusicSource;
    public List<AudioClip> SfxSource;

    public AudioSource _audioSourceMusic;
    public AudioSource _audioSourceSfx;
    private AudioClip _clip;

    private MusicType currentMusic = MusicType.None;

    public void PlayMusic(MusicType mt, bool loop = true) {
        if (mt == currentMusic) {
            return;
        }
        if (mt == MusicType.None) {
            StopMusic();
            return;
        }
        currentMusic = mt;
        Debug.LogFormat("Play bgm {0}", mt);
        _clip = MusicSource[(int)mt];
        _audioSourceMusic.clip = _clip;
        _audioSourceMusic.loop = loop;
        _audioSourceMusic.volume = GameManager.Instance.ConfigModel.MusicVolume;
        _audioSourceMusic.Play();
    }
    public void PlaySFX(SfxType st, bool loop = false) {
        _clip = SfxSource[(int)st];
        _audioSourceSfx.clip = _clip;
        _audioSourceSfx.volume = GameManager.Instance.ConfigModel.SfxVolume * 0.75f;
        if (!loop) {
            _audioSourceSfx.PlayOneShot(_clip);
        }
        _audioSourceSfx.loop = loop;
        _audioSourceSfx.Play();
    }

    public void StopMusic() {
        _audioSourceMusic.Stop();
        _audioSourceSfx.Stop();
    }
    public void PauseMusic() {
        _audioSourceMusic.Pause();
    }
    public void ResumeMusic() {
        _audioSourceMusic.Play();
    }

    public void SetMusicVolume(float volume) {
        Debug.LogFormat("AudioManager SetMusicVolume {0}", volume);
        GameManager.Instance.ConfigModel.MusicVolume = volume;
        _audioSourceMusic.volume = volume;
    }
    public void SetSfxVolume(float volume) {
        Debug.LogFormat("AudioManager SetSfxVolume {0}", volume);
        GameManager.Instance.ConfigModel.SfxVolume = volume;
        _audioSourceSfx.volume = volume;
    }
}
