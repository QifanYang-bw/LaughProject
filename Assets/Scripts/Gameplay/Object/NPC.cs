using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Gameplay.Model;
using UnityEngine;

public class NPC : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UpdateByMood();

        initialMoodValue = MoodValue;
        if (LevelManager.instance != null) {
            LevelManager.instance.OnLevelReset += ResetState;
            LevelManager.instance.OnSwitchMode += ResetState;
        }
    }

    public void ResetState() {
        MoodValue = initialMoodValue;
        UpdateByMood();
    }

    public void OnVoiceWaveHit(VoiceWave voiceWave)
    {
        Debug.Log("OnVoiceWaveHit");
        switch (voiceWave.SoundType)
        {
            case SoundTypes.Laugh:
                MoodValue -= 1;
                break;
            case SoundTypes.Noise:
                MoodValue += 1;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        UpdateByMood();
        OnMoodChangEvent?.Invoke();
    }

    //void OnTriggerEnter2D(Collider2D other)
    //{
    //    // todo test other and modify MoodValue
    //    if (other.gameObject.GetComponent<Player>()!=null)
    //    {
    //        Debug.Log("hit by player");
    //        MoodValue -= 1;
    //    }
    //
    //    UpdateByMood();
    //}

    private void UpdateByMood()
    {
        if (MoodValue > 1)
        {
            MoodBubble.GetComponent<Renderer>().enabled = true;
            MoodBubble.GetComponent<SpriteRenderer>().sprite = AngrySprites;
        }
        else if (MoodValue < 1)
        {
            MoodBubble.GetComponent<Renderer>().enabled = true;
            MoodBubble.GetComponent<SpriteRenderer>().sprite = HappySprites;
        }
        else
        {
            MoodBubble.GetComponent<Renderer>().enabled = false;
        }

        if (IsLaughing && !GetComponent<AudioSource>().isPlaying)
        {
            GetComponent<AudioSource>().Play();
        }
        else
        {
            GetComponent<AudioSource>().Stop();
        }
    }


    [SerializeField]
    private int initialMoodValue = 0;

    // mood value
    public int MoodValue = 0;
    public bool IsLaughing
    {
        get { return MoodValue <= 0; }
    }
    public event Action OnMoodChangEvent;
    public GameObject MoodBubble;
    public Sprite HappySprites;
    public Sprite AngrySprites;
}
