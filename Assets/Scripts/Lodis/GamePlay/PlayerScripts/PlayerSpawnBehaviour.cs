﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Contexts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lodis
{
    /// <summary>
    /// the player spawn behaviour script is used to allow the player spawn objects.
    ///It highlights the selected area and checks to see if a panel is occupied or even exists before allowing the player to spawn
    ///a block on it.
    /// </summary>
    public class PlayerSpawnBehaviour : MonoBehaviour
    {
        //The block the player is going to place
        [SerializeField]
        private BlockVariable blockRef;
        //the blocks the player has to choose from
        [SerializeField]
        private List<GameObject> blocks;
        private int current_index;
        //The direction the player is inputting. Used to determine where the object will spawn
        [SerializeField]
        private Vector2Variable direction;
      
        //used to get access to the list of available panels
        [SerializeField]
        private PlayerMovementBehaviour player;
        //Used to store all panels that the player can spawn the object on
        public Dictionary<string, GameObject> panels_in_range;
        //Used to store the blocks current rotation
        private Quaternion block_rotation;
        [SerializeField]
        private int block_rotation_degrees;
        //The angle at which the block is being rotated
        [SerializeField]
        private float rotation_val;
        //The amount of materials a player has at any given time
        [SerializeField]
        private IntVariable materials;
        //How quickly the player can gain more materials
        [SerializeField]
        private float material_regen_rate;
        //The amount of time the has past since the last material regen
        private float material_regen_time;
        [SerializeField]
        private GameObject DeletionBlockObject;
        private Color SelectionColor;
        [SerializeField]
        private Event OnDeleteEnabled;
        [SerializeField]
        private Event OnDeleteDisabled;
        private float _time;
        [FormerlySerializedAs("_towerSpawnBuffer")] [SerializeField]
        private float _blockSpawnBuffer;
        [SerializeField] private List<string> _listOfTowerSelectionButtons;
        [SerializeField] public bool overdriveEnabled;
        [SerializeField] private int _materialsRegenVal;
        [SerializeField] private int _materialCap;
        private Vector3 BlockForward;
        private bool DeleteEnabled;
        [FormerlySerializedAs("_buildStateEnabled")] public bool buildStateEnabled;
        private PanelBehaviour _panel;
        private bool _panelSelectionInputDown;
        private bool _towerSelectionInputDown;
        [SerializeField] private ArrowBehaviour _arrow;
        // Use this for initialization
        void Start()
        {
            panels_in_range = new Dictionary<string, GameObject>();
            block_rotation = transform.rotation;
            BlockForward = transform.forward;
            blockRef.Block = blocks[0];
            current_index = 0;
            materials.Val = 60;
            material_regen_time = Time.time + material_regen_rate;
            buildStateEnabled = true;
        }
        /// <summary>
        /// Checks the amount of materials the player has 
        /// before allowing them to purchase something
        /// </summary>
        /// <param name="costOfItem"></param>
        /// <returns></returns>
        private bool CheckMaterial(int costOfItem)
        {
            if (overdriveEnabled)
            {
                return true;
            }
            if (materials.Val >= costOfItem)
            {
                return true;
            }
            return false;
        }

        private void BuyItem(int costOfItem)
        {
            if (overdriveEnabled)
            {
                return;
            }

            materials.Val -= costOfItem;
        }
        /// <summary>
        /// Returns the color of the current block 
        /// the player has selected to update the temp ui
        /// </summary>
        /// <returns></returns>
        public Color GetCurrentBlock()
        {
            return blockRef.Color;
        }
        //Adds materials to the players material pool
        public void AddMaterials(int Amount)
        {
            materials.Val += Amount;
            if (materials.Val > _materialCap)
            {
                materials.Val = _materialCap;
            }
        }
        public void EnableDeletion()
        {
            OnDeleteEnabled.Raise(gameObject);
            blockRef.Block = DeletionBlockObject;
            SelectionColor = Color.magenta;
           PlaceBlock();
            DeleteEnabled = true;
        }
        public void DisableDeletion()
        {
            OnDeleteDisabled.Raise(gameObject);
            blockRef.Block = blocks[current_index];
            DeleteEnabled = false;
        }

        public bool CheckBlockSpawnTimer()
        {
            if (Time.time < _time)
            {
                return false;
            }
            else
            {
                _time = Time.time + _blockSpawnBuffer;
                return true;
            }
        }
        //Finds and highlights all neighboring panels in cardinal directions 
        public void FindNeighbors()
        {
            //Disables the players movement so input can be used for block placement
            player.canMove = false;
            //Creates a new dictionary to store the blocks in range
            panels_in_range = new Dictionary<string, GameObject>();
            //Used to find the position the block can be placed
            Vector2 DisplacementX = new Vector2(1, 0);
            Vector2 DisplacementY = new Vector2(0, 1);
            //Loops through all panels to find those whose position is the
            //player current position combined with x or y displacement
            foreach (GameObject panel in player.Panels)
            {
                _panel = panel .GetComponent<PanelBehaviour>();
                var coordinate = _panel.Position;
                if ((player.Position + DisplacementX) == coordinate)
                {
                    if (_panel.BlockCapacityReached && buildStateEnabled && !DeleteEnabled)
                    {
                        _panel.Selected = false;
                        continue;
                    }
                    panels_in_range.Add("Forward", panel);
                    _panel.SelectionColor = SelectionColor;
                    _panel.Selected = true;
                }
                else if ((player.Position - DisplacementX) == coordinate)
                {
                    if (_panel.BlockCapacityReached && buildStateEnabled&& !DeleteEnabled)
                    {
                        continue;
                    }
                    panels_in_range.Add("Behind", panel);
                    _panel.SelectionColor = SelectionColor;
                    _panel.Selected = true;
                }
                else if ((player.Position + DisplacementY) == coordinate)
                {
                    if (_panel.BlockCapacityReached && buildStateEnabled&& !DeleteEnabled)
                    {
                        continue;
                    }
                    panels_in_range.Add("Above", panel);
                    _panel.SelectionColor = SelectionColor;
                    _panel.Selected = true;
                }
                else if ((player.Position - DisplacementY) == coordinate)
                {
                    if (_panel.BlockCapacityReached && buildStateEnabled&& !DeleteEnabled)
                    {
                        continue;
                    }
                    panels_in_range.Add("Below", panel);
                    _panel.SelectionColor = SelectionColor;
                    _panel.Selected = true;
                }
            }
        }
        //Unhighlights all selected panels
        public void UnHighlightPanels()
        {
            if(DeleteEnabled || player.panelStealActive)
            {
                return;
            }
            foreach (GameObject panel in panels_in_range.Values)
            {
                panel.GetComponent<PanelBehaviour>().Selected = false;
            }
            panels_in_range = new Dictionary<string, GameObject>();
            player.canMove = true;
        }

        public void PanelSelectionButtonDown()
        {
//            Debug.Log("stick down");
//            _panelSelectionInputDown = true;
//            buildStateEnabled = true;
//            PlaceBlock();
        }
        public void PanelSelectionButtonUp()
        {
           
        }

        public void CheckTowerButtonDown()
        {
            foreach (var button in _listOfTowerSelectionButtons)
            {
                if (Input.GetAxisRaw(button) == 1 || Input.GetAxisRaw(button) == -1)
                {
                    PlaceBlock();
                    _towerSelectionInputDown = true;
                    return;
                }
            }
            if (DeleteEnabled)
            {
                return;
            }
            player.canMove = true;
            UnHighlightPanels();
            _towerSelectionInputDown = false;
        }

        public void TowerButtonUp()
        {
            _towerSelectionInputDown = false;
        }
        public void PlaceBlock()
        {
           
            FindNeighbors();
            if(buildStateEnabled)
            { 
                if ( direction.Val.x== -1)
                {
                    PlaceBlockLeft();
                    buildStateEnabled = false;
                }
                else if (direction.Val.x== 1)
                {
                    PlaceBlockRight();
                    buildStateEnabled = false;
                }
                else if (direction.Val.y== -1)
                {
                    PlaceBlockBelow();
                    buildStateEnabled = false;
                }
                else if (direction.Val.y== 1)
                {
                    PlaceBlockUp();
                    buildStateEnabled = false;
                }
            }
            buildStateEnabled = InRange(direction.X,-.9f, .9f) && InRange(direction.Y,-.9f, .9f);
        }
        //Places the current block to the left of the player
        public void PlaceBlockLeft()
        {
            //The desired direction the block will be placed
            direction.Val = new Vector2(-1, 0);
            bool canPlace = panels_in_range.ContainsKey("Behind")&&CheckMaterial(blockRef.Cost);
            //Checks to see if the panel exists in the list and the players movement is frozen
            if (player.CheckPanels(direction.Val) == false ||  player.canMove)
            {
                UnHighlightPanels();
                return;
            }
            else if (canPlace)
            {
                BuyItem(blockRef.Cost);
                var position = new Vector3(panels_in_range["Behind"].transform.position.x, blockRef.Block.transform.position.y, panels_in_range["Behind"].transform.position.z);
                GameObject BlockCopy = Instantiate(blockRef.Block, position, block_rotation);
                BlockCopy.GetComponent<BlockBehaviour>().currentPanel = panels_in_range["Behind"];
                BlockCopy.GetComponentInChildren<BlockBehaviour>().owner = gameObject;
                panels_in_range["Behind"].GetComponent<PanelBehaviour>().Occupied = true;
                panels_in_range["Behind"].GetComponent<PanelBehaviour>().Selected = false;
                BlockCopy.GetComponent<BoxCollider>().isTrigger = true;
            }
            else
            {
                System.Console.WriteLine("Invalid Spawn Location");
                UnHighlightPanels();
                return;
            }
            UnHighlightPanels();
        }
        //Places the current block to the right of the player
        public void PlaceBlockRight()
        {
            //The desired direction the block will be placed
            direction.Val = new Vector2(1, 0);
            bool canPlace = panels_in_range.ContainsKey("Forward")&&CheckMaterial(blockRef.Cost);
            //Checks to see if the panel exists in the list and the players movement is frozen
            if (player.CheckPanels(direction.Val) == false ||player.canMove)
            {
                UnHighlightPanels();
                return;
            }
            else if (canPlace)
            {
                BuyItem(blockRef.Cost);
                var position = new Vector3(panels_in_range["Forward"].transform.position.x, blockRef.Block.transform.position.y, panels_in_range["Forward"].transform.position.z);
                GameObject BlockCopy = Instantiate(blockRef.Block, position, block_rotation);
                BlockCopy.GetComponent<BlockBehaviour>().currentPanel = panels_in_range["Forward"];
                BlockCopy.GetComponentInChildren<BlockBehaviour>().owner = gameObject;
                panels_in_range["Forward"].GetComponent<PanelBehaviour>().Occupied = true;
                panels_in_range["Forward"].GetComponent<PanelBehaviour>().Selected = false;
                BlockCopy.GetComponent<BoxCollider>().isTrigger = true;
            }
            else
            {
                System.Console.WriteLine("Invalid Spawn Location");
                UnHighlightPanels();
                return;
            }
            UnHighlightPanels();
        }
        //Places the current block above the player
        public void PlaceBlockUp()
        {
            //The desired direction the block will be placed
            direction.Val = new Vector2(0, 1);
            bool canPlace = panels_in_range.ContainsKey("Above")&&CheckMaterial(blockRef.Cost);
            //Checks to see if the panel exists in the list and the players movement is frozen
            if (player.CheckPanels(direction.Val) == false || player.canMove)
            {
                UnHighlightPanels();
                return;
            }
            else if (canPlace)
            {
                BuyItem(blockRef.Cost);
                var position = new Vector3(panels_in_range["Above"].transform.position.x, blockRef.Block.transform.position.y, panels_in_range["Above"].transform.position.z);
                GameObject BlockCopy = Instantiate(blockRef.Block, position, block_rotation);
                BlockCopy.GetComponent<BlockBehaviour>().currentPanel = panels_in_range["Above"];
                BlockCopy.GetComponentInChildren<BlockBehaviour>().owner = gameObject;
                panels_in_range["Above"].GetComponent<PanelBehaviour>().Occupied = true;
                panels_in_range["Above"].GetComponent<PanelBehaviour>().Selected = false;
                BlockCopy.GetComponent<BoxCollider>().isTrigger = true;
                
            }
            else
            {
                System.Console.WriteLine("Invalid Spawn Location");
                UnHighlightPanels();
                return;
            }
            UnHighlightPanels();
        }
        //Places the current block above the player
        public void PlaceBlockBelow()
        {
            //The desired direction the block will be placed
            direction.Val = new Vector2(0, -1);
            bool canPlace = panels_in_range.ContainsKey("Below")&&CheckMaterial(blockRef.Cost);
            //Checks to see if the panel exists in the list and the players movement is frozen
            if (player.CheckPanels(direction.Val) == false || player.canMove)
            {
                UnHighlightPanels();
                return;
            }
            else if (canPlace)
            {
                BuyItem(blockRef.Cost);
                var position = new Vector3(panels_in_range["Below"].transform.position.x, blockRef.Block.transform.position.y, panels_in_range["Below"].transform.position.z);
                GameObject BlockCopy = Instantiate(blockRef.Block, position, block_rotation);
                BlockCopy.GetComponent<BlockBehaviour>().currentPanel = panels_in_range["Below"];
                BlockCopy.GetComponentInChildren<BlockBehaviour>().owner = gameObject;
                panels_in_range["Below"].GetComponent<PanelBehaviour>().Occupied = true;
                panels_in_range["Below"].GetComponent<PanelBehaviour>().Selected = false;
                BlockCopy.GetComponent<BoxCollider>().isTrigger = true;
            }
            else
            {
                System.Console.WriteLine("Invalid Spawn Location");
                UnHighlightPanels();
                return;
            }
            UnHighlightPanels();
        }

        public void SelectBlock0()
        {
            player.canMove = false;
            _towerSelectionInputDown = true;
            SelectionColor = Color.red;
            blockRef.Block = blocks[0];
            current_index = 0;
            block_rotation = Quaternion.Euler(blockRef.Block.transform.rotation.eulerAngles.x, block_rotation_degrees,
                blocks[current_index].transform.rotation.z);
            
        }
        public void SelectBlock1()
        {
            player.canMove = false;
            _towerSelectionInputDown = true;
            SelectionColor = Color.green;
            blockRef.Block = blocks[1];
            current_index = 1;
            block_rotation = Quaternion.Euler(blockRef.Block.transform.rotation.eulerAngles.x, block_rotation_degrees,
                blocks[current_index].transform.rotation.z);
        }
        public void SelectBlock2()
        {
            player.canMove = false;
            _towerSelectionInputDown = true;
            SelectionColor = Color.yellow;
            blockRef.Block = blocks[2];
            current_index = 2;
            block_rotation = Quaternion.Euler(blockRef.Block.transform.rotation.eulerAngles.x, block_rotation_degrees,
                blocks[current_index].transform.rotation.z);
        }
        public void SelectBlock3()
        {
            player.canMove = false;
            _towerSelectionInputDown = true;
            SelectionColor = Color.white;
            blockRef.Block = blocks[3];
            current_index = 3;
            block_rotation = Quaternion.Euler(blockRef.Block.transform.rotation.eulerAngles.x, block_rotation_degrees,
                blocks[current_index].transform.rotation.z);
        }
        //Rotates the block so that it faces right 
        public void RotateBlockRight()
        {
            block_rotation_degrees = 0;
            _arrow.ShowArrowTemporarily(0);
        }
        //Rotates the block so that it faces left 
        public void RotateBlockLeft()
        {
            block_rotation_degrees = 180;
            _arrow.ShowArrowTemporarily(180);
        }
        //Rotates the block so that it faces left 
        public void RotateBlockUp()
        {
            block_rotation_degrees = -90;
            _arrow.ShowArrowTemporarily(-90);
        }
        //Rotates the block so that it faces left 
        public void RotateBlockDown()
        {
            block_rotation_degrees = 90;
            _arrow.ShowArrowTemporarily(90);
        }

        private void UpdateArrow()
        {
            _arrow.RotateArrow(block_rotation_degrees);
            
            _arrow.HideArrow();
        }

        public bool InRange(float val, float min, float max)
        {
            if (val >= min && val <= max)
            {
                return true;
            }

            return false;
        }
        // Update is called once per frame
        void Update()
        {
            if (Time.time >= material_regen_time && materials.Val < _materialCap)
            {
                AddMaterials(_materialsRegenVal);
                material_regen_time = Time.time + material_regen_rate;
            }
            UpdateArrow();
            CheckTowerButtonDown();
        }

    }
}