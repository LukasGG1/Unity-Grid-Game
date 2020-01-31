﻿using System.Collections;
using System.Collections.Generic;
using Lodis.GamePlay.GridScripts;
using UnityEngine;
using VariableScripts;

namespace Lodis
{
	public class BinaryTreeBehaviour : MonoBehaviour
    {
    
    	public BinaryTree Decisions;
    	// Use this for initialization
    	void Start ()
    	{
    			
    	}
    
    	
        public void TraverseTree()
        {
	        Decisions.currentNode = Decisions.nodes[0];
	        for (int i = 0; i < Decisions.nodes.Count;)
	        {
		        if (Decisions.currentNode.actionName != "")
		        {
			        SendMessage(Decisions.currentNode.actionName);
		        }
		        if (Decisions.currentNode.HasChildren())
		        {
			        if (Decisions.currentNode.ConditionMet)
			        {
				        Decisions.currentNode = Decisions.currentNode.ChildRight;
				        i++;
			        }
			        else 
			        {
				        Decisions.currentNode = Decisions.currentNode.ChildLeft;
				        i++;
			        }
		        }
		        else
		        {
			        break;
		        }
	        }
        }
    	// Update is called once per frame
    	void Update () {
    		
    		TraverseTree();
    	}
    }

}

