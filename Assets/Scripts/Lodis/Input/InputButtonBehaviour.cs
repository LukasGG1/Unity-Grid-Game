﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class InputButtonBehaviour : MonoBehaviour {
    [SerializeField]
    private List<InputVariable> inputs;
    [SerializeField]
    InputVariable newInput;
    public float inputBuffer;
    private bool canMove;
    private float timer;

    // Use this for initialization
    void Start()
    {
        canMove = true;
    }
    
    public void CheckButton()
    {
        foreach (var input in inputs)
        {
            if (Input.GetAxisRaw(input.Axis) == 1)
            {
                if(input.CheckTime())
                {
                    SendMessage(input.ButtonDownMessage,input.Arg);
                }
            }
            else if(Input.GetAxisRaw(input.Axis) == -1)
            {
                if (input.CheckTime())
                {
                    SendMessage(input.ButtonNegativeMessage);
                }
            }
            else if(input.ButtonUpMessage != "")
            {
                SendMessage(input.ButtonUpMessage);
            }
        }
    }
    public void AddInput(string Axis,string message1,string message2,string message3, object Arg)
    {
        newInput = InputVariable.CreateInstance(Axis, message1,message2,message3, Arg,inputBuffer);
        inputs.Add(newInput);
    }
    public void Clear()
    {
        inputs.Clear();
    }
	// Update is called once per frame
	void Update () {
        CheckButton();
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(InputButtonBehaviour))]
public class InputButtonEditor : Editor
{
    string button;
    string message1;
    string message2;
    string message3;
    [SerializeField]
    Object arg;
    
    public override void OnInspectorGUI()
    {
        InputButtonBehaviour myscript = (InputButtonBehaviour)target;
        DrawDefaultInspector();
        button = EditorGUILayout.TextField("Input Button Name",button);
        message1 = EditorGUILayout.TextField("Button Down Func",message1);
        message2 = EditorGUILayout.TextField("Button Up Func", message2);
        message3 = EditorGUILayout.TextField("Button Negative Func", message3);
        arg = EditorGUILayout.ObjectField("Argument",arg, typeof(object), true);
        if(GUILayout.Button("Add Input"))
        {
            myscript.AddInput(button,message1,message2,message3,arg);
        }
        if(GUILayout.Button("Clear"))
        {
            myscript.Clear();
        }
    }
}
#endif