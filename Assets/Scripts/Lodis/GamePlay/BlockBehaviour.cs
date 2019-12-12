﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lodis
{
    public class BlockBehaviour : MonoBehaviour
    {
        //The current panel thye block is on
        [FormerlySerializedAs("CurrentPanel")] public GameObject currentPanel;
        //The player that owns the block
        [FormerlySerializedAs("Owner")] public GameObject owner;
        //the cost of materials to build this block
        public int cost;
        //The gun script attached to the bullet emitter
        GunBehaviour _gun;
        //he helath behaviour script attached to this block
        HealthBehaviour _armor;
        //The material block behaviour script attached to this block
        MaterialBlockBehaviour _materialMine;
        //The script of the panel this block is currently on
        PanelBehaviour _panel;

        [SerializeField] private Material _sleepingMateral;
        [SerializeField]
        private Material _defaultMaterial;

        public int BlockWeightVal;
        private bool _awake;
        private Material _currentMaterial;
        //If true, the player may upgrade this block, otherwise they must wait until it is
        public bool canUpgrade;
        public bool sleeping;
        [FormerlySerializedAs("OnUpgrade")] [SerializeField] private Event onUpgrade;
        [FormerlySerializedAs("OnBlockSpawn")] [SerializeField] private Event onBlockSpawn;
        // Use this for initialization
        void Start()
        {
            //initialzes local variables to be gameobjects components
            _gun = GetComponentInChildren<GunBehaviour>();
            _armor = GetComponent<HealthBehaviour>();
            _materialMine = GetComponent<MaterialBlockBehaviour>();
            _panel = currentPanel.GetComponent<PanelBehaviour>();
            _panel.blockCounter += BlockWeightVal;
            _currentMaterial = GetComponent<Renderer>().material;
            GetComponent<BlockBehaviour>().enabled = true;
            canUpgrade = false;
            _awake = true;
            if (_gun != null)
            {
                _gun.owner = owner.name;
            }
        }

        private void Awake()
        {
            //raises the event signaling the block has been spawned
            onBlockSpawn.Raise();
            _awake = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            //If the player cannot upgrade yet do nothing 
            if (!canUpgrade)
            {
                return;
            }
            //otherwise check the name of the block and upgrade
            if (other.name == "Attack Block(Clone)")
            {
                UpgradeAttack();
                onUpgrade.Raise();
            }
            else if (other.name == "Defense Block(Clone)")
            {
                UpgradeDefense();
                onUpgrade.Raise();
            }
            else if (other.name == "Material Block(Clone)")
            {
                UpgradeMaterial();
                onUpgrade.Raise();
            }
            //Destroys the block placed on top after the upgrade to free up space
            BlockBehaviour block;
            block = other.GetComponent<BlockBehaviour>();
            var destroyblock = other.GetComponent<DeletionBlockBehaviour>();
            if (block != null && destroyblock == null)
            {
                block.DestroyBlock();
                currentPanel.GetComponent<PanelBehaviour>().Occupied = true;
            }

        }
        //stes the can upgrade boolean to true
        public void enableUpgrades()
        {
            canUpgrade = true;
        }
        //Destroys this block instantly
        public void DestroyBlock()
        {
            _panel.Occupied = false;
            GameObject tempGameObject = gameObject;
            Destroy(tempGameObject);
        }
        public void MakeBlockSleep(Vector3 parentPosition)
        {
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<Renderer>().material = _sleepingMateral;
            MonoBehaviour[] components = GetComponents<MonoBehaviour>();
            sleeping = true;
            transform.localScale-= new Vector3(.5f,.5f,.5f);
            transform.position = parentPosition+Vector3.up;
            if (name == "Attack Block(Clone)")
            {
                GetComponentInChildren<GunBehaviour>().enabled = false;
            }
            
            foreach (MonoBehaviour component in components)
            {
                if (component is BlockBehaviour)
                {
                    continue;
                }
                component.enabled = false;
            }

            _awake = false;
        }

        public void WakeBlock()
        {
            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<Rigidbody>().isKinematic = false;
            MonoBehaviour[] components = GetComponents<MonoBehaviour>();
            sleeping = false;
            transform.localScale+= new Vector3(.5f,.5f,.5f);
            if (name == "Attack Block(Clone)")
            {
                GetComponentInChildren<GunBehaviour>().enabled = true;
            }
            
            foreach (MonoBehaviour component in components)
            {
                if (component is BlockBehaviour)
                {
                    continue;
                }
                component.enabled = true;
            }

            _awake = true;
        }
        
        //destroys this block after a specified time
        public void DestroyBlock(float time)
        {
            _panel.Occupied = false;
            _panel.blockCounter = 0;
            GameObject TempGameObject = gameObject;
            Destroy(TempGameObject,time);
        }
        //increases attack power and bullet count
        public void UpgradeAttack()
        {
            _gun.enabled = true;
            _gun.damageVal += 1;
            _gun.bulletCount += 5;
            gameObject.GetComponent<MeshRenderer>().material.color = new Color(1, .5f, 1);
        }
        //Increases health value 
        public void UpgradeDefense()
        {
            if (_armor != null)
            {
                _armor.enabled = true;
                _armor.Health.Val += 20;
                gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
        }
        //increases the materials gained 
        public void UpgradeMaterial()
        {
            if (_materialMine != null)
            {
                _materialMine.enabled = true;
                _materialMine.MaterialAmount += 2;
                gameObject.GetComponent<MeshRenderer>().material.color = new Color(1, .2f, 0f);
            }
        }


        private void Update()
        {
            
        }
    }
}