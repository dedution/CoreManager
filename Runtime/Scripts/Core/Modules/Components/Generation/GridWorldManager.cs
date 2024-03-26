using System.Collections;
using System.Collections.Generic;
using core.gameplay;
using UnityEngine;
using UnityEngine.PlayerLoop;

/// <summary>
/// Port of forest grid system from evil below
/// This legacy code will have more cleanups but its algorithm will remain the same as legacy
/// </summary>
namespace Generation.DynamicGrid
{
    public class GridWorldManager
    {
        private List<GridWorldPiece> GridWorldChunks = new List<GridWorldPiece>();
        private int GridChunk_CurrentIndex = 4;
        private int GridChunk_Size = 25;
        private Camera m_GameCamera;
        private bool m_wasInit = false;
        private bool m_isEnabled = true;

        public GridWorldManager(int _chunkSize, Camera _mainCamera)
        {
            // Sets the necessary parameters for the grid manager to function properly
            GridChunk_Size = _chunkSize;
            m_GameCamera = _mainCamera;
        }

        public void onUpdate()
        {
            if (m_wasInit && m_isEnabled)
                GridWorld_Update();
        }

        public void GridWorld_AddChunk(GridWorldPiece _chunk)
        {
            // Sets a new chunk id and adds it to the list
            _chunk.ChunkID = GridWorldChunks.Count;
            GridWorldChunks.Add(_chunk);

            // Init
            if (!m_wasInit)
                GridWorld_Init();
        }

        private void GridWorld_Init()
        {
            if (!m_isEnabled || !m_GameCamera || GridWorldChunks.Count < 9)
                return;

            m_wasInit = true;
        }

        /// <summary>
        /// Update will iterate the world chunks and try to find the closest central chunk
        /// After a new central chunk is found, other chunks will be readjusted acording to the algorithm
        /// </summary>
        private void GridWorld_Update()
        {
            if (!m_GameCamera || m_GameCamera.transform.position.y < -15f)
                return;

            float distance = 1000;
            int targetIndex = 0;

            foreach (GridWorldPiece chunk in GridWorldChunks)
            {
                Vector3 chunkPosition = chunk.transform.position;
                chunkPosition.x += GridChunk_Size / 2f;
                chunkPosition.z += GridChunk_Size / 2f;

                float _chunkDistance = Vector3.Distance(chunkPosition, m_GameCamera.transform.position);

                if (_chunkDistance < distance)
                {
                    targetIndex = chunk.ChunkID;
                    distance = _chunkDistance;
                }
            }

            if (targetIndex != GridChunk_CurrentIndex)
            {
                GridChunk_CurrentIndex = targetIndex;
                GridWorld_ReadjustChunks();
            }
        }

        private void GridWorld_ReadjustChunks()
        {
            Vector3 chunkPosition = GridWorldChunks[GridChunk_CurrentIndex].transform.position;

            /**
             * This is crappy but it works?
             * If two doorways are available on the target node, 5 chunks will be needed to be moved
             * If only one doorway is available on the target node, 3 chunks will be needed to be moved
             **/

            List<Vector3> SpotsToFill = new List<Vector3>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (!(i == 0 && j == 0))
                    {
                        Vector3 newChunkPosition = chunkPosition + new Vector3(j * GridChunk_Size, 0, i * GridChunk_Size);

                        if (!GridWorld_CheckPositionForChunk(newChunkPosition))
                            SpotsToFill.Add(newChunkPosition);
                    }
                }
            }

            //Exclude center nodes
            if (SpotsToFill.Count > 0)
            {
                if (SpotsToFill.Count == 3)
                {
                    Vector3 targetChunk = chunkPosition;
                    Vector3 freeside = GridWorld_GetFreeChunkPlaces(targetChunk)[0];

                    bool targetIsX = false;

                    if (Mathf.Abs(freeside.x) - Mathf.Abs(targetChunk.x) == 0f)
                    {
                        if (freeside.z > targetChunk.z)
                            targetChunk.z -= GridChunk_Size * 2;
                        else
                            targetChunk.z += GridChunk_Size * 2;

                        targetIsX = true;
                    }
                    else
                    {
                        if (freeside.x > targetChunk.x)
                            targetChunk.x -= GridChunk_Size * 2;
                        else
                            targetChunk.x += GridChunk_Size * 2;

                        targetIsX = false;
                    }

                    //Reassign
                    for (int i = 0; i <= 2; i++)
                    {
                        if (!targetIsX)
                        {
                            GridWorldPiece _chunk = GridWorld_GetChunkAtPosition(new Vector3(targetChunk.x, 0, SpotsToFill[i].z));

                            _chunk.UpdateChunkPosition(SpotsToFill[i]);
                        }
                        else
                        {
                            GridWorldPiece _chunk = GridWorld_GetChunkAtPosition(new Vector3(SpotsToFill[i].x, 0, targetChunk.z));

                            _chunk.UpdateChunkPosition(SpotsToFill[i]);
                        }
                    }
                }
                else if (SpotsToFill.Count == 5)
                {
                    List<Vector3> Chunks = GridWorld_GetInUseChunkPlaces(chunkPosition);
                    Dictionary<Vector3, Vector3> RepoList = new Dictionary<Vector3, Vector3>();

                    Vector3 DiagChunk_Origin = Vector3.zero;
                    Vector3 DiagChunk_Target = Vector3.zero;

                    foreach (Vector3 usedChunk in Chunks)
                    {
                        Vector3 targetChunk = usedChunk;
                        Vector3 freeside = GridWorld_GetFreeChunkPlaces(targetChunk)[0];

                        bool targetIsX = false;
                        bool targetIsNegative = true;

                        if (Mathf.Abs(freeside.x) - Mathf.Abs(targetChunk.x) == 0f)
                        {
                            if (freeside.z > targetChunk.z)
                                targetChunk.z -= GridChunk_Size * 2;
                            else
                            {
                                targetChunk.z += GridChunk_Size * 2;
                                targetIsNegative = false;
                            }

                            targetIsX = true;
                        }
                        else
                        {
                            if (freeside.x > targetChunk.x)
                                targetChunk.x -= GridChunk_Size * 2;
                            else
                            {
                                targetChunk.x += GridChunk_Size * 2;
                                targetIsNegative = false;
                            }

                            targetIsX = false;
                        }

                        //Reassign
                        for (int i = -1; i <= 1; i++)
                        {
                            if (!targetIsX)
                            {
                                Vector3 targetPos = new Vector3(targetChunk.x, 0, targetChunk.z + GridChunk_Size * i);
                                Vector3 originPos = targetPos;

                                if (targetIsNegative)
                                    targetPos += new Vector3(GridChunk_Size * 3, 0, 0);
                                else
                                    targetPos -= new Vector3(GridChunk_Size * 3, 0, 0);

                                if (!RepoList.ContainsKey(originPos))
                                    RepoList.Add(originPos, targetPos);
                                else
                                {
                                    RepoList.Remove(originPos);

                                    DiagChunk_Origin = originPos;
                                    DiagChunk_Target = targetChunk;
                                }
                            }
                            else
                            {
                                Vector3 targetPos = new Vector3(targetChunk.x + GridChunk_Size * i, 0, targetChunk.z);
                                Vector3 originPos = targetPos;

                                if (targetIsNegative)
                                    targetPos += new Vector3(0, 0, GridChunk_Size * 3);
                                else
                                    targetPos -= new Vector3(0, 0, GridChunk_Size * 3);

                                if (!RepoList.ContainsKey(originPos))
                                    RepoList.Add(originPos, targetPos);
                                else
                                {
                                    RepoList.Remove(originPos);

                                    DiagChunk_Origin = originPos;
                                    DiagChunk_Target = targetChunk;
                                }
                            }

                        }
                    }

                    //Teleport chunks to correct position
                    foreach (Vector3 originPos in RepoList.Keys)
                    {
                        GridWorldPiece _chunker = GridWorld_GetChunkAtPosition(originPos);

                        _chunker.UpdateChunkPosition(RepoList[originPos]);

                        SpotsToFill.Remove(RepoList[originPos]);
                    }

                    GridWorldPiece _chunk = GridWorld_GetChunkAtPosition(DiagChunk_Origin);

                    _chunk.UpdateChunkPosition(SpotsToFill[0]);
                }
            }
        }

        private bool GridWorld_CheckPositionForChunk(Vector3 Position)
        {
            foreach (GridWorldPiece t in GridWorldChunks)
            {
                Vector3 chunkPosition = t.transform.position;

                if (chunkPosition == Position)
                    return true;
            }

            return false;
        }

        private GridWorldPiece GridWorld_GetChunkAtPosition(Vector3 Position)
        {
            GridWorldPiece chunk = null;

            foreach (GridWorldPiece t in GridWorldChunks)
            {
                if (t.transform.position == Position)
                {
                    chunk = t;
                    break;
                }
            }

            return chunk;
        }

        private List<Vector3> GridWorld_GetInUseChunkPlaces(Vector3 Position)
        {
            List<Vector3> Chunks = new List<Vector3>();

            GridWorldPiece Chunk = null;
            Vector3 testPos = new Vector3(0, 0, 0);

            for (int i = -1; i <= 1; i++)
            {
                testPos = Position + new Vector3(i * GridChunk_Size, 0, 0);
                Chunk = GridWorld_GetChunkAtPosition(testPos);

                if (Chunk != null && testPos != Position)
                {
                    Chunks.Add(testPos);
                }
            }

            for (int j = -1; j <= 1; j++)
            {
                testPos = Position + new Vector3(0, 0, j * GridChunk_Size);
                Chunk = GridWorld_GetChunkAtPosition(testPos);

                if (Chunk != null && testPos != Position)
                    Chunks.Add(testPos);
            }

            return Chunks;
        }

        private List<Vector3> GridWorld_GetFreeChunkPlaces(Vector3 Position)
        {
            List<Vector3> Chunks = new List<Vector3>();

            GridWorldPiece Chunk = null;

            Vector3 testPos = Position + new Vector3(0, 0, 0);
            Chunk = GridWorld_GetChunkAtPosition(testPos);

            for (int i = -1; i <= 1; i++)
            {
                testPos = Position + new Vector3(i * GridChunk_Size, 0, 0);
                Chunk = GridWorld_GetChunkAtPosition(testPos);

                //If position is free, save it
                if (Chunk == null)
                    Chunks.Add(testPos);
            }

            for (int j = -1; j <= 1; j++)
            {
                testPos = Position + new Vector3(0, 0, j * GridChunk_Size);
                Chunk = GridWorld_GetChunkAtPosition(testPos);

                //If position is free, save it
                if (Chunk == null)
                    Chunks.Add(testPos);
            }

            return Chunks;
        }

        public void GridWorldManager_Reset(int _chunkSize = 25, Camera _mainCamera = null)
        {
            m_wasInit = false;
            GridWorldChunks.Clear();
            GridChunk_Size = _chunkSize;
            GridChunk_CurrentIndex = 4;
            GridWorldManager_SetCamera(_mainCamera);
        }

        public void GridWorldManager_SetCamera(Camera _mainCamera)
        {
            m_GameCamera = _mainCamera;
            GridWorld_Init();
        }

        public void GridWorldManager_SetEnabled(bool _isEnabled)
        {
            m_isEnabled = _isEnabled;
        }
    }
}