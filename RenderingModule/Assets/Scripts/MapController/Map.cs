using AxisController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityVolumeRendering;
namespace MapController
{
    public class Map : MonoBehaviour
    {
        public bool dragable = true;
        public float movementSpeed = 1;
        UnityEngine.RaycastHit hit;
        public LayerMask mapHighlightClickableLayer;

        private TClipper tClipper;
        private VolumeControllerObject volumeControllerObj;

        private void Start()
        {
            volumeControllerObj = FindObjectOfType<VolumeControllerObject>();
            tClipper = FindObjectOfType<TClipper>();
        }

        // Update is called once per frame
        void Update()
        {
            if (tClipper.isUpperDragging)
            {
                return;
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (dragable)
            {
                //  ������̧����û�е����UI�ϣ���û�����϶���ͼ����ֹͣ�϶�
                if (Input.GetMouseButtonUp(0) && tClipper.isMapDragging)
                {
                    //tClipper.isMapDragging = false;
                    tClipper.SetMapDragging(false);
                    tClipper.updateAnchorList();
                }
                //  �����������˿��϶��������ϣ���û�е����UI�ϣ���û�����϶�ƽ�棬��ʼ�϶���ͼ
                if (Physics.Raycast(ray, out hit, 1000, mapHighlightClickableLayer.value) && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !tClipper.isUpperDragging)
                {
                    //tClipper.isMapDragging = true;
                    tClipper.SetMapDragging(true);
                }
                Vector3 movementDir = Vector3.zero;
                //  ��������϶�
                if (tClipper.isMapDragging)
                {
                    movementDir.y += Input.GetAxis("Mouse Y") * movementSpeed;
                    if (transform.position.y + movementDir.y >= volumeControllerObj.GetBottomY() && transform.position.y + movementDir.y <= tClipper.GetUpperPlanePositionY() - 0.18f)
                    {
                        transform.Translate(movementDir);
                        tClipper.updateTClipper();
                    }
                }
            }
        }
        public void setHeight(float height)
        {
            Vector3 position = transform.position;
            position.y = height;
            transform.position = position;
            tClipper.updateTClipper();
            return;
        }
    }
}

