using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.RunnerWorld
{
    public class RunnerWorldNode : MonoBehaviour
    {
        public void ClearNode()
        {

        }

        public void RandomizeNode()
        {

        }

        public float GetPosition()
        {
            return transform.localPosition.z;
        }

        public void SetPosition(float _newPos)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, _newPos);
        }

        public void SetPosition(Vector3 _newPos)
        {
            transform.localPosition = _newPos;
        }
    }
}