﻿using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Player
{
    public enum PlayerState
    {
        Idle,
        Forward,
        Back,
        Run,
        Jump,
        Left,
        Right
    }

    interface IPlayer
    {
        //Provide injection interface 
        Action<PlayerState> OnStateChanged { get; set; }
    }

    /// <summary>
    /// assemble all player physically behaviours 
    /// </summary>
    public class PlayerController : MonoBehaviour, IPlayer
    {
        public float walkSpeed = 6f;
        public float runSpeed = 10f;

        public float jumpSpeed = 5f;
        public float gravity = 20f;
        public Crosshairs mCrosshairs;

        private CharacterController _characterController;
        private Vector3 _moveDirection = Vector3.zero;
        private Camera _viewCamera;
        private PlayerState _currentState = PlayerState.Idle;

        //provide a 
        public Action<PlayerState> OnStateChanged { get; set; }

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _viewCamera = Camera.main;
        }

        private void Update()
        {
            Movement();
            MouseInput();
        }

        // move and Jump
        void Movement()
        {
            float tempSpeed = walkSpeed;
            if (_characterController.isGrounded)
            {
                //move
                float h = Input.GetAxis("Horizontal");
                float v = Input.GetAxis("Vertical");
                _moveDirection = new Vector3(h, 0, v);
                //listening state by v
                if (!h.Equals(0) || !v.Equals(0))
                {
                    if (v > 0.01f)
                    {
                        if (!_currentState.Equals(PlayerState.Run) || !Input.GetKey(KeyCode.LeftShift))
                        {
                            ChangeCurrentState(PlayerState.Forward);
                        }
                    }
                    else if (v < -0.01f )
                    {
                        ChangeCurrentState(PlayerState.Back);
                    }
                    else if (h > 0.01f)
                    {
                        ChangeCurrentState(PlayerState.Right);
                    }
                    else if (h < -0.01f)
                    {
                        ChangeCurrentState(PlayerState.Left);
                    }
                }
                else
                {
                    ChangeCurrentState(PlayerState.Idle);
                }

                //Run
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (v > 0.1f)
                    {
                        //only player press the leftShift and v is not zero does the character entry Run state
                        ChangeCurrentState(PlayerState.Run);
                        tempSpeed = runSpeed;
                    }
                }

                //Jump
                if (Input.GetButton("Jump"))
                {
                    ChangeCurrentState(PlayerState.Jump);
                    _moveDirection.y = jumpSpeed;
                }

                //correct moveDirection
                Quaternion angleAxis = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up);
                _moveDirection = angleAxis * _moveDirection;
            }

            _moveDirection.y -= gravity * Time.deltaTime;
            _characterController.Move(_moveDirection * tempSpeed * Time.deltaTime);
        }

        void MouseInput()
        {
            Ray ray = _viewCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.up * 2);
            float rayDistance;
            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                LookAt(point);
                mCrosshairs.transform.position = point;
                mCrosshairs.DetectTargets(ray);
            }
        }

        private void LookAt(Vector3 lookPoint)
        {
            Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
            transform.LookAt(heightCorrectedPoint);
        }

        private void ChangeCurrentState(PlayerState state)
        {
            //change the current state and invoke this OnStateChanged Event
            if (!state.Equals(_currentState))
            {
                Debug.Log(_currentState);
                _currentState = state;
                OnStateChanged?.Invoke(state);
            }
        }
    }
}