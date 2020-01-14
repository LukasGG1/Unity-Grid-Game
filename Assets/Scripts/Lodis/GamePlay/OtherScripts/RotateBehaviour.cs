﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBehaviour : MonoBehaviour {
    Quaternion quaternion;
    [SerializeField]
    private Vector3 axis;
    [SerializeField]
    private float speed;
	// Use this for initialization
	void Start () {
		axis = axis * speed;
	}
	
	// Update is called once per frame
	void Update () {
        
        transform.Rotate(axis, Space.World);
	}
}
