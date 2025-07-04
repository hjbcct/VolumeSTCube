using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityVolumeRendering;
namespace MapController
{
    public class UpperPlane : MonoBehaviour
    {
        public bool dragable = true;
        public float movementSpeed = 1;
        UnityEngine.RaycastHit hit;
        public LayerMask dragableLayer;

        private VolumeControllerObject volumeControllerObj;
        private TClipper tClipper;
        
        // Start is called before the first frame update
        void Start()
        {
            volumeControllerObj = FindObjectOfType<VolumeControllerObject>();
            tClipper = FindObjectOfType<TClipper>();
        }

        // Update is called once per frame
        void Update()
        {
            if(tClipper.isMapDragging)
            {
                return;
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (dragable)
            {
                //  ������̧����û�е����UI�ϣ���û�����϶���ͼ����ֹͣ�϶�
                if (Input.GetMouseButtonUp(0) && tClipper.isUpperDragging)
                {
                    //tClipper.isUpperDragging = false;
                    tClipper.SetUpperClipDragging(false);
                    tClipper.updateAnchorList();
                }
                //  �����������˿��϶��������ϣ���û�е����UI�ϣ���û�����϶���ͼ����ʼ�϶�
                if (Physics.Raycast(ray, out hit, 1000, dragableLayer.value) && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !tClipper.isMapDragging)
                {
                        //tClipper.isUpperDragging = true;
                        tClipper.SetUpperClipDragging(true);
                }
                Vector3 movementDir = Vector3.zero;
                //  ��������϶�
                if (tClipper.isUpperDragging)
                {
                    movementDir.y += Input.GetAxis("Mouse Y") * movementSpeed;
                    if (transform.position.y + movementDir.y <= volumeControllerObj.GetTopY() && transform.position.y + movementDir.y >= tClipper.GetMapPositionY() + 0.18f)
                    {
                        transform.Translate(movementDir);
                        tClipper.updateTClipper();
                    }
                }
            }
        }

        void setHeight(float height)
        {
            Vector3 position = transform.position;
            position.y = height;
            transform.position = position;
            tClipper.updateTClipper();
            return;
        }


    }
}

