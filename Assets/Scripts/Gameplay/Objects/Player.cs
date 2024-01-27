using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        _wavePrefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/VoiceWave.prefab", typeof(GameObject));
    }

    // Update is called once per frame
    void Update()
    {
        FaceMousePos();
        TryHandleFireBtn();
        UpdateAimAreaVisibility();
        // add 45 degree for test icon
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, 45));
    }

    void FixedUpdate()
    {
        MoveByInput();
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
        Debug.Log("Fire wave");
        GameObject wave = Instantiate(_wavePrefab, transform.parent) as GameObject;
        wave.transform.parent = transform.parent;
        wave.transform.position = transform.position;
        var arc = wave.GetComponent<VoiceWave>().Arc;
        arc.Center = transform.position;
        float degreeZ = transform.rotation.eulerAngles.z;
        degreeZ += 90;
        var angle = Degree2Angle(degreeZ);
        arc.Angle = new ArcAngleModel(angle - Degree2Angle(WaveRangeDegree / 2), Degree2Angle(WaveRangeDegree));
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
    }

    private Vector3 GetPosDeltaRatioByInput()
    {
        float x = 0, y = 0;
        if (Input.GetKey(KeyCode.W))
        {
            y += 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            x -= 1;

        }
        if (Input.GetKey(KeyCode.S))
        {
            y -= 1;
        }
        if (Input.GetKey(KeyCode.D))
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

    public float MoveSpeed = 30;
    public float WaveRangeDegree = 30;
    public GameObject AimAreaObj;
    private Object _wavePrefab;
}