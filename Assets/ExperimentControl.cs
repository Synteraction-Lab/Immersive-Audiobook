using NRKernal.Record;
using System;
using System.IO;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using NRKernal;
#if UNITY_ANDROID && !UNITY_EDITOR
    using GalleryDataProvider = NativeGalleryDataProvider;
#else
    using GalleryDataProvider = MockGalleryDataProvider;
#endif

public class MockGalleryDataProvider
{
    public void InsertImage(byte[] data, string displayName, string folderName)
    {
        string path = string.Format("{0}/NrealShots/{1}", Application.persistentDataPath, folderName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        File.WriteAllBytes(string.Format("{0}/{1}", path, displayName), data);
    }

    public AndroidJavaObject InsertVideo(string originFilePath, string displayName, string folderName)
    {
        return null;
    }
}

public class NativeGalleryDataProvider
{
    private static AndroidJavaClass m_NativeClass;
    public static AndroidJavaClass NativeClass
    {
        get
        {
            if (m_NativeClass == null)
                m_NativeClass = new AndroidJavaClass("ai.nreal.android.gallery.GalleryDataProvider");
            return m_NativeClass;
        }
    }

    public const string MAIN_ACTIVITY_CLASS = "com.unity3d.player.UnityPlayer";

    private static AndroidJavaObject m_CurrentActivity;
    public static AndroidJavaObject CurrentActivity
    {
        get
        {
            if (m_CurrentActivity == null)
            {
                using (AndroidJavaClass jc = new AndroidJavaClass(MAIN_ACTIVITY_CLASS))
                {
                    m_CurrentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                }
            }
            return m_CurrentActivity;
        }
    }

    private AndroidJavaObject m_NativeObject;

    public NativeGalleryDataProvider()
    {
        m_NativeObject = new AndroidJavaObject("ai.nreal.android.gallery.GalleryDataProvider",
                                               CurrentActivity);
    }

    public void InsertImage(byte[] data, string displayName, string folderName)
    {
        InsertImageData(data, displayName, folderName);
    }

    public AndroidJavaObject InsertImageData(byte[] data, string displayName, string folderName)
    {
        AndroidJavaObject inputStream = new AndroidJavaObject("java.io.ByteArrayInputStream", data);
        return m_NativeObject.Call<AndroidJavaObject>("insertImage", inputStream, displayName, folderName, "image/png");
    }

    public AndroidJavaObject InsertVideo(string originFilePath, string displayName, string folderName)
    {
        return m_NativeObject.Call<AndroidJavaObject>("insertVideo", originFilePath, displayName, folderName);
    }
}



public class ExperimentControl : MonoBehaviour
{
    [SerializeField] GameObject preExperimentPanel;
    [SerializeField] AudioSource audioSource;
    [SerializeField] GameObject gazeRectile;
    [SerializeField] SubtitleGenerator subtitleGenerator;
    [SerializeField] LayerMask cullingMask = -1;
    [SerializeField] bool recordSession = false;
    [SerializeField] bool useRemote = false;
    [SerializeField] GameObject remotePanel, floatingPanel;

    NRVideoCapture m_VideoCapture = null;
    public static float audioTimer;
    public static bool experimentActive = false;
    public static AudioClip currentAudioClip;

    public string VideoSavePath
    {
        get
        {
            string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
            string filename = string.Format("Immersive_Audio_{0}.mp4", timeStamp);
            return Path.Combine(Application.persistentDataPath, filename);
        }
    }


    GalleryDataProvider galleryDataTool;
    // Start is called before the first frame update
    void Start()
    {
        if (useRemote)
        {
            remotePanel.SetActive(true);
            floatingPanel.SetActive(false);
            if (gazeRectile)
                gazeRectile.SetActive(false);
        } else
        {
            remotePanel.SetActive(false);
            floatingPanel.SetActive(true);
            if (gazeRectile)
                gazeRectile.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!experimentActive)
            return;

        if (NRInput.GetButtonDown(ControllerButton.TRIGGER))
        {
            if (experimentActive)
            {
                RestartExperiment();
            }
        }

        if (experimentActive)
            audioTimer = audioSource.time;
    }


    void RestartExperiment()
    {
        if (recordSession)
            OnTriggerStartRecording(false);
        audioSource.Stop();
        subtitleGenerator.RestartExperiment();
        experimentActive = false;
        if (!useRemote)
        {
            preExperimentPanel.SetActive(true);
            transform.position = Camera.main.transform.position;
            transform.eulerAngles = Camera.main.transform.eulerAngles.y * transform.up;
            if (gazeRectile)
                gazeRectile.SetActive(true);
        }
        
    }

    public void StartAudioBook()
    {
        
        experimentActive = true;
        if (!useRemote)
        {
            preExperimentPanel.SetActive(false);
            if (gazeRectile)
                gazeRectile.SetActive(false);
        }
        subtitleGenerator.SetCurrentAnchor();
        audioSource.clip = currentAudioClip;
        audioSource.Play();
        if (recordSession)
            OnTriggerStartRecording(true);
    }

    void CreateVideoCapture(Action callback)
    {
        NRVideoCapture.CreateAsync(false, delegate (NRVideoCapture videoCapture)
        {
            NRDebugger.Info("Created VideoCapture Instance!");
            if (videoCapture != null)
            {
                m_VideoCapture = videoCapture;
                callback?.Invoke();
            }
            else
            {
                NRDebugger.Error("Failed to create VideoCapture Instance!");
            }
        });
    }


    void OnTriggerStartRecording(bool activate)
    {
        if (activate)
        {
            if (m_VideoCapture == null)
            {
                CreateVideoCapture(() =>
                {
                    StartVideoCapture();
                });
            }
            else if (!m_VideoCapture.IsRecording)
            {
                this.StartVideoCapture();
            }
        } else
        {
            if(m_VideoCapture != null && m_VideoCapture.IsRecording)
            {
                this.StopVideoCapture();
            }
        }
    }

    void StartVideoCapture()
    {
        if (m_VideoCapture == null || m_VideoCapture.IsRecording)
        {
            NRDebugger.Warning("Can not start video capture!");
            return;
        }

        CameraParameters cameraParameters = new CameraParameters();
        Resolution cameraResolution = NRVideoCapture.SupportedResolutions.ElementAt(1);
        cameraParameters.hologramOpacity = 0.0f;
        cameraParameters.frameRate = cameraResolution.refreshRate;
        cameraParameters.cameraResolutionWidth = cameraResolution.width;
        cameraParameters.cameraResolutionHeight = cameraResolution.height;
        cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
        // Set the blend mode.
        cameraParameters.blendMode = BlendMode.Blend;
        // Set audio state, audio record needs the permission of "android.permission.RECORD_AUDIO",
        // Add it to your "AndroidManifest.xml" file in "Assets/Plugin".
        cameraParameters.audioState = NRVideoCapture.AudioState.ApplicationAndMicAudio;

        m_VideoCapture.StartVideoModeAsync(cameraParameters, OnStartedVideoCaptureMode, true);
    }

    void OnStartedVideoCaptureMode(NRVideoCapture.VideoCaptureResult result)
    {
        if (!result.success)
        {
            NRDebugger.Info("Started Video Capture Mode faild!");
            return;
        }

        NRDebugger.Info("Started Video Capture Mode!");
        float volumeMic = NativeConstants.RECORD_VOLUME_MIC;
        float volumeApp = NativeConstants.RECORD_VOLUME_APP;
        m_VideoCapture.StartRecordingAsync(VideoSavePath, OnStartedRecordingVideo, volumeMic, volumeApp);
    }

    void OnStartedRecordingVideo(NRVideoCapture.VideoCaptureResult result)
    {
        if (!result.success)
        {
            NRDebugger.Info("Started Recording Video Faild!");
            return;
        }

        NRDebugger.Info("Started Recording Video!");
        m_VideoCapture.GetContext().GetBehaviour().SetCameraMask(cullingMask.value);

    }

    public void StopVideoCapture()
    {
        if (m_VideoCapture == null || !m_VideoCapture.IsRecording)
        {
            NRDebugger.Warning("Can not stop video capture!");
            return;
        }

        NRDebugger.Info("Stop Video Capture!");
        m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
    }

    void OnStoppedRecordingVideo(NRVideoCapture.VideoCaptureResult result)
    {
        if (!result.success)
        {
            NRDebugger.Info("Stopped Recording Video Faild!");
            return;
        }

        NRDebugger.Info("Stopped Recording Video!");
        m_VideoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
    }


    void OnStoppedVideoCaptureMode(NRVideoCapture.VideoCaptureResult result)
    {
        NRDebugger.Info("Stopped Video Capture Mode!");

        var encoder = m_VideoCapture.GetContext().GetEncoder() as VideoEncoder;
        string path = encoder.EncodeConfig.outPutPath;
        string filename = string.Format("Nreal_Shot_Video_{0}.mp4", NRTools.GetTimeStamp().ToString());

        StartCoroutine(DelayInsertVideoToGallery(path, filename, "Record"));

        // Release video capture resource.
        m_VideoCapture.Dispose();
        m_VideoCapture = null;
    }

    IEnumerator DelayInsertVideoToGallery(string originFilePath, string displayName, string folderName)
    {
        yield return new WaitForSeconds(0.1f);
        InsertVideoToGallery(originFilePath, displayName, folderName);
    }

    public void InsertVideoToGallery(string originFilePath, string displayName, string folderName)
    {
        NRDebugger.Info("InsertVideoToGallery: {0}, {1} => {2}", displayName, originFilePath, folderName);
        if (galleryDataTool == null)
        {
            galleryDataTool = new GalleryDataProvider();
        }

        galleryDataTool.InsertVideo(originFilePath, displayName, folderName);
    }

    public void SetRecordSessionActive(Toggle toggle)
    {
        recordSession = toggle.isOn;
    }
    void OnDestroy()
    {
        // Release video capture resource.
        m_VideoCapture?.Dispose();
        m_VideoCapture = null;
    }
}
