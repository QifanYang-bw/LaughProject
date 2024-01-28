using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Gameplay.Model;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public enum PlayerState {
    PlayerStateTrial = 0,
    PlayerStateActionPreparing = 1,
    PlayerStateActionFired = 2
}

public class Player : MonoBehaviour {

    public float MoveSpeed = 30;
    public float WaveRangeDegree = 30;
    public GameObject AimAreaObj;

    [Header("Runtime Variables")]
    private Vector3 initialPos;
    private Quaternion initialRot;
    public PlayerState state;
    public float CoolDown;

    // Start is called before the first frame update
    void Start()
    {
        initialPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        initialRot = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);

        if (LevelManager.instance != null ) {
            LevelManager.instance.OnLevelReset += ResetState;
            LevelManager.instance.OnSwitchMode += ResetState;
        }
    }

    // Update is called once per frame
    void Update()
    {
        FaceMousePos();
        TryHandleFireBtn();
        UpdateAimAreaVisibility();
    }

    void FixedUpdate()
    {
        MoveByInput();
    }

    public void ResetState() {
        transform.position = new Vector3(initialPos.x, initialPos.y, initialPos.z);
        transform.rotation = new Quaternion(initialRot.x, initialRot.y, initialRot.z, initialRot.w);
        ResetTrialCount();
    }

    public void ResetTrialCount() {
        if (LevelManager.instance == null) {
            return;
        }
        LevelMode mode = LevelManager.instance.mode;
        if (mode == LevelMode.LevelModeTrial) {
            state = PlayerState.PlayerStateTrial;
        } else if (mode == LevelMode.LevelModeAction) {
            state = PlayerState.PlayerStateActionPreparing;
        }
    }

    private void TryHandleFireBtn()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            FireLaughWave();
        }
    }

    private void FireLaughWave()
    {
        if (EventSystem.current.IsPointerOverGameObject()) {
            return;
        }
        if (state == PlayerState.PlayerStateActionFired) {
            return;
        }
        if (state == PlayerState.PlayerStateActionPreparing) {
            state = PlayerState.PlayerStateActionFired;
        }

        GameObject wave = Instantiate(AssetHelper.instance.WavePrefab, transform.parent) as GameObject;
        Debug.LogFormat("Generate Wave {0}", wave.gameObject.name);
        wave.transform.parent = transform.parent;
        wave.transform.position = transform.position;
        var voiceWave = wave.GetComponent<VoiceWave>();
        voiceWave.SoundType = SoundTypes.Laugh;
        var arc = voiceWave.Arc;
        arc.Center = transform.position;
        float degreeZ = transform.rotation.eulerAngles.z;
        degreeZ += 90;

        //// Hack
        //degreeZ = 10;
        var angle = Degree2Angle(degreeZ);
        arc.Angle = new ArcAngleModel(angle - Degree2Angle(WaveRangeDegree / 2), Degree2Angle(WaveRangeDegree));

        GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip);
    }

    private void UpdateAimAreaVisibility()
    {
        AimAreaObj.GetComponent<Renderer>().enabled = Input.GetKey(KeyCode.Mouse1);
    }

    private void FaceMousePos()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = new Vector2(
            mousePos.x - transform.position.x,
            mousePos.y - transform.position.y
        );
        transform.up = direction;
    }

    private void MoveByInput()
    {
        var ratio = GetPosDeltaRatioByInput();
        GetComponent<Rigidbody2D>().MovePosition(transform.position + ratio * MoveSpeed * Time.fixedDeltaTime);
        GetComponent<Animator>().SetBool("isMoving", ratio.x != 0 || ratio.y != 0);
    }

    private Vector3 GetPosDeltaRatioByInput()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        float x = 0, y = 0, xMargin = 0.04f, yMargin = 0.072f;
        if (Input.GetKey(KeyCode.W) && viewPos.y <= 1 - yMargin)
        {
            y += 1;
        }
        if (Input.GetKey(KeyCode.A) && viewPos.x >= xMargin)
        {
            x -= 1;

        }
        if (Input.GetKey(KeyCode.S) && viewPos.y >= yMargin)
        {
            y -= 1;
        }
        if (Input.GetKey(KeyCode.D) && viewPos.x <= 1 - xMargin)
        {
            x += 1;
        }

        if (y != 0)
            x *= math.sin(45);
        if (x != 0)
            y *= math.sin(45);
        return new Vector3(x, y, 0);
    }

    private static double Degree2Angle(double degree)
    {
        return degree / 180f * Math.PI;
    }
}