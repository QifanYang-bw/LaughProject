using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //var ratio = GetPosDeltaRatioByInput();
        //transform.position += ratio * MoveSpeed * Time.deltaTime;
    }

    void FixedUpdate()
    {
        MoveByInput();
        FaceMousePos();
        TryHandleFireBtn();
        UpdateAimAreaVisibility();

        // add 45 degree for test icon
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, 45));
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
        // todo impl this
        Debug.Log("Fire wave");
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

    public float MoveSpeed = 30;
    public GameObject AimAreaObj;
}