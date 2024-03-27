using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.RunnerWorld
{
    public class RunnerWorldController
    {
        private GameObject m_NodePrefab;
        public List<RunnerWorldNode> m_RunnerNodes = new List<RunnerWorldNode>();

        [Range(5, 15)]
        private int m_NodeNumber = 15;

        [Range(1f, 10f)]
        public float m_MoveSpeed = 8f;

        [Range(0f, 1f)]
        private float m_Acceleration = 1f;

        [Range(5, 15)]
        private int m_NodeSize = 10;

        private bool m_enabled = false;

        public RunnerWorldController(GameObject nodePrefab, int nodeSize = 10, int nodeNumber = 15)
        {
            Runner_Init(nodePrefab,nodeSize, nodeNumber);
        }

        public void Runner_Init(GameObject nodePrefab, int nodeSize = 10, int nodeNumber = 15)
        {
            m_enabled = true;
            m_NodeNumber = nodeNumber;
            m_NodePrefab = nodePrefab;
            m_NodeSize = nodeSize;

            Runner_PopulateNodes();
        }

        public void onUpdate()
        {
            if(m_enabled)
                Runner_UpdateNodes();
        }

        private RunnerWorldNode Runner_GetPreviousNode(int id)
        {
            return m_RunnerNodes[Runner_GetPreviousNodeID(id)];
        }

        private int Runner_GetPreviousNodeID(int id)
        {
            if (id == 0)
                return m_RunnerNodes.Count - 1;
            else
                return id - 1;
        }

        private void Runner_PopulateNodes()
        {
            for (int i = 0; i < m_NodeNumber; i++)
            {
                GameObject temp = Object.Instantiate(m_NodePrefab);
                RunnerWorldNode node = temp.GetComponent<RunnerWorldNode>();

                temp.transform.localEulerAngles = new Vector3(0, 0, 0);
                temp.transform.localPosition = new Vector3(0, 0, m_NodeSize * i);
                temp.name = "RunnerNode-" + (i + 1);

                m_RunnerNodes.Add(node);
            }
        }

        private void Runner_UpdateNodes()
        {
            for (int i = 0; i < m_RunnerNodes.Count; i++)
            {
                RunnerWorldNode _previous = Runner_GetPreviousNode(i);
                bool wasTeleported = false;

                // If reached the end, teleport to the back of the nodes
                if (m_RunnerNodes[i].GetPosition() < -m_NodeSize)
                {
                    m_RunnerNodes[i].SetPosition(_previous.GetPosition() + m_NodeSize);
                    wasTeleported = true;
                }

                // First node constantly move backwards, others skip movement during the frame they teleport
                if (i == 0 || !wasTeleported)
                    m_RunnerNodes[i].transform.localPosition -= Vector3.forward * (m_MoveSpeed * m_Acceleration) * Time.deltaTime;
            }
        }
    
        public bool Runner_GetState()
        {
            return m_enabled;
        }

        public void Runner_SetState(bool _state)
        {
            m_enabled = _state;
        }

        public void Runner_Reset()
        {
            m_enabled = false;

            // Destroy nodes
            for (int i = 0; i < m_RunnerNodes.Count; i++)
            {
                Object.Destroy(m_RunnerNodes[i].gameObject);
            }

            m_RunnerNodes.Clear();
        }
    }
}