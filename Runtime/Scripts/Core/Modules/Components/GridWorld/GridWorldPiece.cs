using System.Collections;
using System.Collections.Generic;
using core.gameplay;
using core.modules;
using UnityEngine;

namespace Generation.DynamicGrid
{
    public class GridWorldPiece : baseGameActor
    {
        private int m_chunkID = 0;

        public int ChunkID
        {
            get
            {
                return m_chunkID;
            }
            set
            {
                m_chunkID = value;
            }
        }

        public bool AutoRegisterPiece = true;

        protected override void onStart()
        {
            // Register to manager, only after save data is loaded
            Vector3 _newposition = SaveSystem_GetData<Vector3>("position", transform.position);
            transform.position = _newposition;
            
            if(AutoRegisterPiece && GenerationManager.GetGridWorldManager() != null)
                RegisterPieceOnGridWorld();
        }

        public void RegisterPieceOnGridWorld()
        {
            GenerationManager.GetGridWorldManager().GridWorld_AddChunk(this);
        }

        public void UpdateChunkPosition(Vector3 _newPosition)
        {
            transform.position = _newPosition;
            SaveSystem_SetData("position", _newPosition);
        }
    }
}