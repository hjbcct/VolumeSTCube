using DateUtils;
using EventAnchor;
using MapController;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityVolumeRendering;

public class TClipper : MonoBehaviour
{
    private Map map;
    private UpperPlane upperPlane;
    private ControlPanel controlPanel;
    private float mapPositionY;
    private float upperPlanePositionY;
    private bool isActiveUpper = true;

    private VolumeControllerObject volumeControllerObj;
    private AnchorList anchorList;

    public bool isUpperDragging = false;
    public bool isMapDragging = false;
    public TextMeshPro mapText;
    public TextMeshPro upperClipedText;
    public TextMeshPro timeRangeText;
    public TextMeshProUGUI upperPlaneText;

    // Start is called before the first frame update
    void Start()
    {
        volumeControllerObj = GameObject.FindObjectOfType<VolumeControllerObject>();
        map = GameObject.FindObjectOfType<Map>();
        upperPlane = GameObject.FindObjectOfType<UpperPlane>();
        controlPanel = GameObject.FindObjectOfType<ControlPanel>();
        anchorList = GameObject.FindObjectOfType<AnchorList>();

        mapPositionY = map.transform.position.y;
        upperPlanePositionY = upperPlane.transform.position.y;

        updateTClipper();
    }


    public void resetAll()
    {
        setClipWindowByTime(volumeControllerObj.GetBottomY(), volumeControllerObj.GetTopY());

        updateTClipper();
    }

    public void SetMapDragging(bool isMapDrag)
    {
        isMapDragging = isMapDrag;
        mapText.gameObject.SetActive(isDragging());
        upperClipedText.gameObject.SetActive(isDragging());
        timeRangeText.gameObject.SetActive(isDragging());
    }

    public void SetUpperClipDragging(bool isUpperDrag)
    {
        isUpperDragging = isUpperDrag;
        mapText.gameObject.SetActive(isDragging());
        upperClipedText.gameObject.SetActive(isDragging());
        timeRangeText.gameObject.SetActive(isDragging());
    }

    public void updateTClipper()
    {
        //  ���ݵ�ͼ��UpperCliper��λ�ø���volumeController
        mapPositionY = map.transform.position.y;
        upperPlanePositionY = upperPlane.transform.position.y;

        float mapPositionYNormalized = (mapPositionY - volumeControllerObj.GetBottomY()) / volumeControllerObj.GetHeight();
        float upperPlanePositionYNormalized = (upperPlanePositionY - volumeControllerObj.GetBottomY()) / volumeControllerObj.GetHeight();

        ////  ����TimeRangeSlider
        controlPanel.updateTimeRangeSlide((mapPositionYNormalized + upperPlanePositionYNormalized) / 2);

        Vector2 clipWindow = new Vector2(mapPositionYNormalized, upperPlanePositionYNormalized); 
        volumeControllerObj.SetClipedHeight(clipWindow);

        //  ���ݵ�ͼ��UpperCliper��λ�ø���Textλ�ú�����
        mapText.transform.position = new Vector3(mapText.transform.position.x, mapPositionY - 1, mapText.transform.position.z);
        upperClipedText.transform.position = new Vector3(upperClipedText.transform.position.x, upperPlanePositionY - 1, upperClipedText.transform.position.z);
        //mapText.text = DateUtil.getDateStrByFloat(mapPositionYNormalized, DateUtil.DefaultStartDate, DateUtil.DefaultEndDate, ((int)(DateUtil.DefaultEndDate - DateUtil.DefaultStartDate).TotalDays) + 1);
        //upperClipedText.text = DateUtil.getDateStrByFloat(upperPlanePositionYNormalized, DateUtil.DefaultStartDate, DateUtil.DefaultEndDate.AddDays(2), ((int)(DateUtil.DefaultEndDate - DateUtil.DefaultStartDate).TotalDays));
        DateTime mapTextDate = DateUtil.getDateTimeByFloat(mapPositionYNormalized, DateUtil.DefaultStartDate, DateUtil.DefaultEndDate, ((int)(DateUtil.DefaultEndDate - DateUtil.DefaultStartDate).TotalDays) + 1);
        DateTime upperClipedTextDate = DateUtil.getDateTimeByFloat(upperPlanePositionYNormalized, DateUtil.DefaultStartDate, DateUtil.DefaultEndDate.AddDays(2), ((int)(DateUtil.DefaultEndDate - DateUtil.DefaultStartDate).TotalDays));
        mapText.text = DateUtil.getDateStrByDateTime(mapTextDate);
        upperClipedText.text = DateUtil.getDateStrByDateTime(upperClipedTextDate);
        timeRangeText.text = DateUtil.getDayRangeByDateTime(mapTextDate, upperClipedTextDate).ToString() + " Days";
        timeRangeText.rectTransform.transform.position = new Vector3(timeRangeText.rectTransform.transform.position.x, (mapPositionY + upperPlanePositionY) / 2 - 1, timeRangeText.rectTransform.transform.position.z);

        //  ��������ʼ�ճ��������
        timeRangeText.transform.forward = Camera.main.transform.forward;
        mapText.transform.forward = Camera.main.transform.forward;
        upperClipedText.transform.forward = Camera.main.transform.forward;

    }

    public void toggleUpperPlaneActive()
    {
        isActiveUpper = !isActiveUpper;
        if (!isActiveUpper)
        {
            upperPlaneText.text = "Show UpperPlane";
        }
        else
        {
            upperPlaneText.text = "Hide UpperPlane";
        }

        upperPlane.gameObject.SetActive(isActiveUpper);
    }

    public float GetMapPositionY()
    {
        return mapPositionY;
    }

    public float GetUpperPlanePositionY()
    {
        return upperPlanePositionY;
    }

    public bool isDragging()
    {
        return isMapDragging || isUpperDragging;
    }

    public void setClipWindowByTime(float startTime, float endTime)
    {
        map.transform.position = new Vector3(map.transform.position.x, startTime, map.transform.position.z);
        upperPlane.transform.position = new Vector3(upperPlane.transform.position.x, endTime, upperPlane.transform.position.z);

        updateTClipper();
        updateAnchorList();
    }

    //  ����Floatƽ��Window
    public void setClipWindowByFloat(float val)
    {
        float delta_height = volumeControllerObj.GetBottomY() + volumeControllerObj.GetHeight() * val;
        float map_delta_height = Mathf.Clamp(delta_height - (upperPlanePositionY - mapPositionY) / 2, volumeControllerObj.GetBottomY(), volumeControllerObj.GetTopY() - (upperPlanePositionY - mapPositionY));
        float upperPlane_delta_height = Mathf.Clamp(delta_height + (upperPlanePositionY - mapPositionY) / 2, volumeControllerObj.GetBottomY() + (upperPlanePositionY - mapPositionY), volumeControllerObj.GetTopY());
        map.transform.position = new Vector3(map.transform.position.x, map_delta_height, map.transform.position.z);
        upperPlane.transform.position = new Vector3(upperPlane.transform.position.x, upperPlane_delta_height, upperPlane.transform.position.z);

        updateTClipper();
        updateAnchorList();
    }

    public void updateAnchorList()
    {
        anchorList.SetActiveByTimeRange(mapPositionY, upperPlanePositionY);

    }
}
