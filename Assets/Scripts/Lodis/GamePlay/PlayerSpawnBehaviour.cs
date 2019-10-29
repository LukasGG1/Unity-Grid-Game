﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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
        private bool DeleteEnabled;
        // Use this for initialization
        void Start()
        {
            panels_in_range = new Dictionary<string, GameObject>();
            block_rotation = transform.rotation;
            blockRef.Block = blocks[0];
            current_index = 0;
            materials.Val = 100;
            material_regen_time = Time.time + material_regen_rate;
        }
        /// <summary>
        /// Checks the amount of materials the player has 
        /// before allowing them to purchase something
        /// </summary>
        /// <param name="costOfItem"></param>
        /// <returns></returns>
        private bool CheckMaterial(int costOfItem)
        {
            if (materials.Val >= costOfItem)
            {
                materials.Val -= costOfItem;
                return true;
            }
            return false;
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
        }
        public void EnableDeletion()
        {
            OnDeleteEnabled.Raise(gameObject);
            blockRef.Block = DeletionBlockObject;
            SelectionColor = Color.red;
            FindNeighbors();
            DeleteEnabled = true;
        }
        public void DisableDeletion()
        {
            OnDeleteDisabled.Raise(gameObject);
            blockRef.Block = blocks[current_index];
            SelectionColor = Color.green;
            DeleteEnabled = false;
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
                var coordinate = panel.GetComponent<PanelBehaviour>().Position;
                if ((player.Position + DisplacementX) == coordinate)
                {
                    panels_in_range.Add("Forward", panel);
                    panel.GetComponent<PanelBehaviour>().SelectionColor = SelectionColor;
                    panel.GetComponent<PanelBehaviour>().Selected = true;
                }
                else if ((player.Position - DisplacementX) == coordinate)
                {
                    panels_in_range.Add("Behind", panel);
                    panel.GetComponent<PanelBehaviour>().SelectionColor = SelectionColor;
                    panel.GetComponent<PanelBehaviour>().Selected = true;
                }
                else if ((player.Position + DisplacementY) == coordinate)
                {
                    panels_in_range.Add("Above", panel);
                    panel.GetComponent<PanelBehaviour>().SelectionColor = SelectionColor;
                    panel.GetComponent<PanelBehaviour>().Selected = true;
                }
                else if ((player.Position - DisplacementY) == coordinate)
                {
                    panels_in_range.Add("Below", panel);
                    panel.GetComponent<PanelBehaviour>().SelectionColor = SelectionColor;
                    panel.GetComponent<PanelBehaviour>().Selected = true;
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
        //Sets the current block to the next block in the list
        public void NextBlock()
        {
            current_index++;
            if (current_index > blocks.Count - 1)
            {
                current_index = 0;
            }
            blockRef.Block = blocks[current_index];
        }
        //Sets the current block to the next block in the list
        public void PreviousBlock()
        {
            current_index--;
            if (current_index < 0)
            {
                current_index = blocks.Count - 1;
            }
            blockRef.Block = blocks[current_index];
        }
        //Places the current block to the left of the player
        public void PlaceBlockLeft()
        {
            Debug.Log("Tried to place left");
            //The desired direction the block will be placed
            direction.Val = new Vector2(-1, 0);
            //Checks to see if the panel exists in the list and the players movement is frozen
            if (player.CheckPanels(direction.Val) == false || player.canMove)
            {
                UnHighlightPanels();
                return;
            }
            else if (CheckMaterial(blockRef.Cost))
            {
                var position = new Vector3(panels_in_range["Behind"].transform.position.x, transform.position.y, panels_in_range["Behind"].transform.position.z);
                GameObject BlockCopy = Instantiate(blockRef.Block, position, block_rotation);
                BlockCopy.GetComponent<BlockBehaviour>().CurrentPanel = panels_in_range["Behind"];
                BlockCopy.GetComponentInChildren<BlockBehaviour>().Owner = gameObject;
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
            //Checks to see if the panel exists in the list and the players movement is frozen
            if (player.CheckPanels(direction.Val) == false ||  player.canMove)
            {
                UnHighlightPanels();
                return;
            }
            else if (CheckMaterial(blockRef.Cost))
            {
                var position = new Vector3(panels_in_range["Forward"].transform.position.x, transform.position.y, panels_in_range["Forward"].transform.position.z);
                GameObject BlockCopy = Instantiate(blockRef.Block, position, block_rotation);
                BlockCopy.GetComponent<BlockBehaviour>().CurrentPanel = panels_in_range["Forward"];
                BlockCopy.GetComponentInChildren<BlockBehaviour>().Owner = gameObject;
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
            //Checks to see if the panel exists in the list and the players movement is frozen
            if (player.CheckPanels(direction.Val) == false || player.canMove)
            {
                UnHighlightPanels();
                return;
            }
            else if (CheckMaterial(blockRef.Cost))
            {

                var position = new Vector3(panels_in_range["Above"].transform.position.x, transform.position.y, panels_in_range["Above"].transform.position.z);
                GameObject BlockCopy = Instantiate(blockRef.Block, position, block_rotation);
                BlockCopy.GetComponent<BlockBehaviour>().CurrentPanel = panels_in_range["Above"];
                BlockCopy.GetComponentInChildren<BlockBehaviour>().Owner = gameObject;
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
            //Checks to see if the panel exists in the list and the players movement is frozen
            if (player.CheckPanels(direction.Val) == false || player.canMove)
            {
                UnHighlightPanels();
                return;
            }
            else if (CheckMaterial(blockRef.Cost))
            {
                var position = new Vector3(panels_in_range["Below"].transform.position.x, transform.position.y, panels_in_range["Below"].transform.position.z);
                GameObject BlockCopy = Instantiate(blockRef.Block, position, block_rotation);
                BlockCopy.GetComponent<BlockBehaviour>().CurrentPanel = panels_in_range["Below"];
                BlockCopy.GetComponentInChildren<BlockBehaviour>().Owner = gameObject;
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
        //Rotates the block -90 degrees
        public void RotateBlock()
        {
            rotation_val -= 90;
            block_rotation = Quaternion.Euler(0, rotation_val, 0);
        }
        
        // Update is called once per frame
        void Update()
        {
            if (Time.time >= material_regen_time)
            {
                AddMaterials(15);
                material_regen_time = Time.time + material_regen_rate;
            }
        }

    }
}