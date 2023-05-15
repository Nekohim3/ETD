using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public  float       MoveSpeed;
    public  Rigidbody2D Rb;
    private Vector2     _movement;
    private bool        _run;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MovementInput();
    }

    private void FixedUpdate()
    {
        Rb.velocity = _movement * MoveSpeed * (_run ? 5 : 1);
    }

    void MovementInput()
    {
        var mx = Input.GetAxisRaw("Horizontal");
        var my = Input.GetAxisRaw("Vertical");

        _movement = new Vector2(mx, my).normalized;
        _run      = Input.GetKey(KeyCode.LeftShift);
    }
}
