﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Lodis
{
    //deletion blocks are invisible blocks used to delete others. They are only selected while
    //the player is in the deletion state.
    public class DeletionBlockBehaviour : MonoBehaviour
    {
        private PlayerSpawnBehaviour _player;
        [SerializeField] private Event _onDelete;
        private BlockBehaviour _deletionBlock;
        //particles to be played when a block is deleted
        [SerializeField] private ParticleSystem ps;
        private void Start()
        {
            _deletionBlock = GetComponent<BlockBehaviour>();
            _player = _deletionBlock.owner.GetComponent<PlayerSpawnBehaviour>();
            _deletionBlock.DestroyBlock(.5f);
        }
        //plays the particles when a block is deleted for a spcified duration
        public void PlayParticleSystems(float duration)
        {
            var tempPs = Instantiate(ps,transform.position,transform.rotation);
            tempPs.Play();
            tempPs.playbackSpeed = 2.0f;
            Destroy(tempPs, duration);
        }
        //Refunds the player half of the energy used to buildthe block
        private void GetRefund(BlockBehaviour block)
        {
            _player.AddMaterials(block.cost /2);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            var block = other.GetComponent<BlockBehaviour>();
            if (block != null)
            {
                PlayParticleSystems(1.5f);
                GetRefund(block);
                block.DestroyBlock(1.0f);
                _onDelete.Raise();
            }
        }
    }
}
