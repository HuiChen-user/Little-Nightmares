using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("角色状态参数")]
    [Tooltip("角色移动速度")]
    public float moveSpeed = 5f;
    [Tooltip("角色潜伏速度")]
    public float crouchSpeed = 1f;
    [Tooltip("角色跳跃高度")] public float jumpheight = 1.5f;
    [Tooltip("y方向攀爬速度")]
    public float climbSpeed = 2f;

    [Tooltip("Z方向攀爬速度")] public float climbSpeedz = 1.5f;

    [Tooltip("场景重力")] 
    public float gravity = -9.18f;

    [Tooltip("转向速度")] public float turnSpeed = 10f;
    

    [Header("相机引用")] public Transform CameraTransform;

    private CharacterController _controller;
    private Vector3 velocity;
    private bool isCrouching = false;//检测是否潜行
    private bool isGrounding;//检测是否位于地面上
    private bool isClimbing = false;
    private bool isGrabing;
    
    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    
    void Update()
    {
        isGrabing=  Input.GetKey(KeyCode.LeftShift);
        if (inLadderZone&&isGrabing)
        {
            isClimbing = true;
            HandleClimb();
            return;
        }
        if (isClimbing)
        {
            isClimbing = false;
        }
        
        isGrounding = _controller.isGrounded;
        if (isGrounding&&velocity.y<0)
        {
            velocity.y = -2f;
        }
        Move();
         
        //跳跃功能实现
        if (Input.GetButtonDown("Jump") && isGrounding)
        {
            velocity.y = Mathf.Sqrt(jumpheight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        _controller.Move(velocity * Time.deltaTime);

       
    }

    void Move()
    {
        float horinzontal = Input.GetAxis("Horizontal");
        float depth = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(horinzontal,0,depth);
        RotateTowardsMovement(move);

        if (move.magnitude>=0.1f)
        {
            float speed = isCrouching ? crouchSpeed : moveSpeed;
            _controller.Move(move * speed * Time.deltaTime);
        }
        //蹲伏控制
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
        }
    }

    void RotateTowardsMovement(Vector3 move)
    {
        //
        if (move.sqrMagnitude>0.01f)
        {
            //只考虑XZ方向
            Vector3 flatMove = new Vector3(move.x, 0, move.z);
            //计算目标角度
            Quaternion targetRotation = Quaternion.LookRotation(flatMove);
            targetRotation *= Quaternion.Euler(0, -90, 0);
            //平滑旋转（避免瞬时转动）
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }
    }

    private bool inLadderZone = false;
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Ladder"))
        {
            //isClimbing = true;
            inLadderZone = true;
            Debug.Log("true");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            inLadderZone = false;
            isClimbing = false;
        }
    }

    void HandleClimb()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = -Input.GetAxisRaw("Horizontal");
        Vector3 climb = new Vector3(0, vertical * climbSpeed, horizontal*climbSpeedz);
        _controller.Move(climb * Time.deltaTime);
        velocity.y = 0;
    }
}
