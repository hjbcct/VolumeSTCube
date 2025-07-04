using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventAnchor
{
    public enum AnchorState
    {
        Checked,
        Unchecked,
        Inactive
    }
    public class Anchor : MonoBehaviour
    {
        public Vector3 position = new Vector3(0, 0, 0);
        public float height = 0;
        public float radius = 0;
        public float startTime = 0;
        public float endTime = 0;
        public AnchorState state = AnchorState.Unchecked;

        public bool _debugMode = false;

        //  x: Բ��Բ������x
        //  y: Բ��Բ������y
        //  height: Բ���߶�
        //  radius: Բ���뾶
        //  startTime: Բ����ʼʱ��
        //  endTime: Բ������ʱ��
        public void init(float x, float y, float height, float radius, float startTime, float endTime)
        {
            this.position.x = x;
            this.position.z = (startTime - endTime) / 2;
            this.position.y = y;
            this.startTime = startTime;
            this.endTime = endTime;
            this.height = height;
            this.radius = radius;
        }

        public void setState(AnchorState state)
        {
            this.state = state;
        }

        public void setScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        private void Update()
        {
            if (!_debugMode)
            {
                //  �ر�MeshRenderer
                MeshRenderer meshRendererOfAnchor = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
                meshRendererOfAnchor.enabled = false;
            }
            else
            {
                //  ����MeshRenderer
                MeshRenderer meshRendererOfAnchor = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
                meshRendererOfAnchor.enabled = true;
            }
        }
    }
}

