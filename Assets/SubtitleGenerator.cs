using NRKernal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SubtitleGenerator : MonoBehaviour
{
    public enum Script
    {
        Kidney,
        Muscle,
        Epidermis,
        Heart,
        WWS_Phrases,
        WWS_Full
    }

    [SerializeField] DataContainer subtitleConfigs;
    [SerializeField] GameObject subtitleObject;
    //[SerializeField] GameObject preExperimentPanel;
    //[SerializeField] AudioClip audioBook;
    //[SerializeField] AudioSource audioSource;
    //[SerializeField] Transform headTransform;
    [SerializeField] float raycastDistance = 5f;
    [SerializeField] float horizontalAngle = 50f;
    [SerializeField] float autoDestroyTime = 5f;
    //[SerializeField] float preExperimentWaitTime = 3f;
    [SerializeField] LayerMask layer;
    [SerializeField] float defaultDistance = 3f;
    public bool isHeadLocked = false;
    //[SerializeField] bool isFullSentence = false;
    [SerializeField] int textPlacement = 0;
    [SerializeField] Transform cameraCanvasAnchor, worldCanvasAnchor;
    [SerializeField] AudioClip[] audioClips;

    Transform currentCanvasAnchor;
    //[SerializeField] GameObject gazeRectile;
    int index = 0;
    //bool experimentActive = false;
    // Start is called before the first frame update

    Script currentScript;
    List<Phrase> currentPhrases;

    private void Start()
    {
        currentScript = Script.WWS_Full;
        currentPhrases = subtitleConfigs.WMS_FullText;
        ExperimentControl.currentAudioClip = audioClips[4];
    }

    public void RestartExperiment()
    {
        StartCoroutine(ChangeTrackingType(NRHMDPoseTracker.TrackingType.Tracking6Dof));
        index = 0;
    }

    public void SetCurrentAnchor()
    {
        currentCanvasAnchor = isHeadLocked ? worldCanvasAnchor : cameraCanvasAnchor;
        if (isHeadLocked)
        {
            StartCoroutine(ChangeTrackingType(NRHMDPoseTracker.TrackingType.Tracking0DofStable));
        }
        else
        {
            currentCanvasAnchor.transform.localPosition = Vector3.forward * defaultDistance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!ExperimentControl.experimentActive)
            return;
        
        float startTime = (float)currentPhrases[index].Timestamp_START / 1000f;
        if (ExperimentControl.audioTimer >= ((float)currentPhrases[index].Timestamp_START / 1000f))
        {
            GameObject subtitleObj = Instantiate(subtitleObject);
            float endTime = (float)currentPhrases[index].Timestamp_END / 1000f;
            float lifeSpan = Mathf.Min(endTime - startTime, autoDestroyTime);
            subtitleObj.GetComponent<WorldAnchoredSubtitle>().SetProperties(currentPhrases[index].Text, lifeSpan);
            PositionSubtitleObj(subtitleObj);
            index++;
        }
    }

    void PositionSubtitleObj(GameObject subtitleObj)
    {
        if (isHeadLocked)
        {
            subtitleObj.transform.parent = currentCanvasAnchor;
            subtitleObj.transform.localPosition = Vector3.zero;
            subtitleObj.transform.LookAt(Camera.main.transform.position);
        }
        else
        {
            RaycastHit? hitInfo = CheckFanCastNearestHit();

            if (hitInfo == null)
            {
                if (textPlacement == 0)
                {
                    subtitleObj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * defaultDistance;
                }
                else if (textPlacement == 1) 
                {
                    subtitleObj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * defaultDistance - Camera.main.transform.right * defaultDistance / 4f;
                } else if (textPlacement == 2)
                {
                    subtitleObj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * defaultDistance + Camera.main.transform.right * defaultDistance / 4f;
                }
                subtitleObj.transform.LookAt(Camera.main.transform.position);
            }
            else
            {
                subtitleObj.transform.position = ((RaycastHit)hitInfo).point;
                //subtitleObj.transform.forward = ((RaycastHit)hitInfo).normal;
                subtitleObj.transform.LookAt(Camera.main.transform.position);
            }
        }
    }
    RaycastHit? CheckFanCastNearestHit()
    {
        RaycastHit hitInfo;
        float shortestDistance = float.MaxValue;
        RaycastHit? shortestHit = null;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, raycastDistance, layer))
        {
            shortestDistance = hitInfo.distance;
            shortestHit = hitInfo;
        }
        if (Physics.Raycast(Camera.main.transform.position, Quaternion.Euler(0f, -horizontalAngle, 0f) * Camera.main.transform.forward, out hitInfo, raycastDistance, layer))
        {
            if(hitInfo.distance < shortestDistance)
            {
                shortestDistance = hitInfo.distance;
                shortestHit = hitInfo;
            }
        }
        if (Physics.Raycast(Camera.main.transform.position, Quaternion.Euler(0f, horizontalAngle, 0f) * Camera.main.transform.forward, out hitInfo, raycastDistance, layer))
        {
            if (hitInfo.distance < shortestDistance)
            {
                shortestDistance = hitInfo.distance;
                shortestHit = hitInfo;
            }
        }
        return shortestHit;
    }

    public void SetDefaultDistance(Dropdown dropDown)
    {
        defaultDistance = 1f + (float)dropDown.value *2f;
    }

    public void SetHeadLockActive(Toggle toggle)
    {
        isHeadLocked = toggle.isOn;
    }

    public void SetTextPlacement(Dropdown dropDown)
    {
        textPlacement = dropDown.value;
    }

    public void SetCurrentScript(Dropdown dropDown)
    {
        currentScript = (Script)dropDown.value;
        switch (currentScript)
        {
            case Script.Kidney:
                currentPhrases = subtitleConfigs.Kidney;
                ExperimentControl.currentAudioClip = audioClips[0];
                break;
            case Script.Muscle:
                currentPhrases = subtitleConfigs.Muscle;
                ExperimentControl.currentAudioClip = audioClips[1];
                break;
            case Script.Epidermis:
                currentPhrases = subtitleConfigs.Epidermis;
                ExperimentControl.currentAudioClip = audioClips[2];
                break;
            case Script.Heart:
                currentPhrases = subtitleConfigs.Heart;
                ExperimentControl.currentAudioClip = audioClips[3];
                break;
            case Script.WWS_Phrases:
                currentPhrases = subtitleConfigs.WMS_Phrases;
                ExperimentControl.currentAudioClip = audioClips[4];
                break;
            case Script.WWS_Full:
                currentPhrases = subtitleConfigs.WMS_FullText;
                ExperimentControl.currentAudioClip = audioClips[4];
                break;
            default:
                break;
        }
    }

    private IEnumerator ChangeTrackingType(NRHMDPoseTracker.TrackingType type)
    {
        WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        if (type == NRHMDPoseTracker.TrackingType.Tracking0Dof && NRSessionManager.Instance.NRHMDPoseTracker.TrackingMode != type)
        {
            while (!NRSessionManager.Instance.NRHMDPoseTracker.ChangeTo0Dof(null))
            {
                yield return waitForEndOfFrame;
            }
        }
        else if (type == NRHMDPoseTracker.TrackingType.Tracking0DofStable && NRSessionManager.Instance.NRHMDPoseTracker.TrackingMode != type)
        {
            while (!NRSessionManager.Instance.NRHMDPoseTracker.ChangeTo0DofStable(null))
            {
                yield return waitForEndOfFrame;
            }
            StartCoroutine(DelayedSetWorldCanvasAnchor());

        }
        else if (type == NRHMDPoseTracker.TrackingType.Tracking3Dof && NRSessionManager.Instance.NRHMDPoseTracker.TrackingMode != type)
        {
            while (!NRSessionManager.Instance.NRHMDPoseTracker.ChangeTo3Dof(null))
            {
                yield return waitForEndOfFrame;
            }
        }
        else if (type == NRHMDPoseTracker.TrackingType.Tracking6Dof && NRSessionManager.Instance.NRHMDPoseTracker.TrackingMode != type)
        {
            while (!NRSessionManager.Instance.NRHMDPoseTracker.ChangeTo6Dof(null))
            {
                yield return waitForEndOfFrame;
            }
        }

    }

    IEnumerator DelayedSetWorldCanvasAnchor()
    {
        yield return new WaitForSeconds(.8f);
        currentCanvasAnchor.transform.position = Camera.main.transform.position + Camera.main.transform.forward * defaultDistance;
    }
}
