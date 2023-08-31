using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleSheetsForUnity;
public class ExperimentManager : MonoBehaviour
{
    public enum bookNames
    {
        blood_work,
        dogs_of_riga,
        the_martian,
        the_ocean,
        educated,
        the_wind,
        dogs_of_riga_full,
        the_ocean_full,
        blood_work_user,
        dogs_of_riga_user,
        the_martian_user,
        the_ocean_user,
        educated_user,
        the_wind_user,
    }

    [System.Serializable]
    public class Meta
    {
        public bookNames bookName = bookNames.the_martian;
        public int frequency = 6;
        public float timingOffset = 0f;
        public float fadeTime = 2f;
        public float scale = 1f;
        public bool maintainGap = false;
        public bool showTimeline = false;
    }

    [SerializeField] Meta meta;
    public class IllustrationTimestamp
    {
        public Sprite audiobookSprite;
        public float druation;
        public float startTimeInSeconds;
        public IllustrationTimestamp(float startTime, float duration, string name, bool isUser)
        {
            startTimeInSeconds = startTime;
            this.druation = duration;
            if (isUser)
                audiobookSprite = Resources.Load<Sprite>("Illustrations_User/" + name);
            else
                audiobookSprite = Resources.Load<Sprite>("Illustrations/" + name);
        }
    }

    List<IllustrationTimestamp> illustrationTimestamps = new List<IllustrationTimestamp>();
    AudioClip clip;
    [SerializeField] DataContainer data;
    [SerializeField] Image[] illusContainers; //for morphing between two
    [SerializeField] Image[] illusReel; //for rolling effect
    [SerializeField] Image illusBorder;
    [SerializeField] AudioSource audioSource;
    private bool experimentStarted;
    private float experiemntStartTime;
    private float experimentTimer;
    private float clipTime;
    private int currentIllusIndex, nextIllusIndex;
    private int currentReelContainerIndex;
    private const float imgDefaultDim = 500f, imgReelMid = -192f, imgReelX = -833f, intermediateTransparency = 0.2f;

    private void Start()
    {
        currentIllusIndex = -1;
        nextIllusIndex = 0;
        foreach (Image img in illusContainers)
        {
            img.gameObject.SetActive(false);
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {

            StartExperiment();
        }

        if (experimentStarted)
        {
            //experimentTimer = Time.realtimeSinceStartup - experiemntStartTime;
            clipTime = audioSource.time;
            if (nextIllusIndex < illustrationTimestamps.Count)
            {
                if (clipTime > Mathf.Max((illustrationTimestamps[nextIllusIndex].startTimeInSeconds - meta.fadeTime / 2f + meta.timingOffset), 0f))
                {
                    currentIllusIndex = nextIllusIndex;
                    nextIllusIndex++;
                    if (!meta.showTimeline)
                    {
                        int containerIndex = 0;
                        while (containerIndex < illusContainers.Length && illusContainers[containerIndex].gameObject.activeInHierarchy)
                        {
                            containerIndex++;
                        }
                        if (containerIndex <= illusContainers.Length)
                            StartCoroutine(DisplayCrossFadeIllustration(containerIndex));
                        else
                            DebugText.Instance.SetText("All image container occupied. Please debug!");
                    }
                    else
                    {
                        StartCoroutine(DisplayIllustrationReel(currentReelContainerIndex));
                        currentReelContainerIndex++;
                        if (currentReelContainerIndex >= illusReel.Length)
                        {
                            currentReelContainerIndex = 0;
                        }
                    }
                }
            }
        }
    }

    public void StartExperiment()
    {
        experimentStarted = true;
        InitializeContent();
        if (illustrationTimestamps.Count <= 0)
        {
            DebugText.Instance.SetText("No illustration assigned");
            return;
        }
        if (!clip)
        {
            DebugText.Instance.SetText("No clip assigned");
            return;
        }
        audioSource.Play();
        DebugText.Instance.SetText("Experiment Started!");
        currentIllusIndex = -1;
        nextIllusIndex = 0;
        illusBorder.rectTransform.localScale = Vector3.one;
        foreach (Image img in illusContainers)
        {
            img.gameObject.SetActive(false);
            img.rectTransform.localScale = Vector3.one * meta.scale;

        }
        if (meta.showTimeline)
        {
            foreach (Image img in illusReel)
            {
                img.gameObject.SetActive(true);
            }
            InitializeIllustrationReel();
        }
        else
        {
            foreach (Image img in illusReel)
            {
                img.gameObject.SetActive(false);
            }
            illusBorder.rectTransform.localScale = Vector3.one * meta.scale;
        }
        experiemntStartTime = Time.realtimeSinceStartup;
        InputController.touchEnabled = true;
    }

    public void PauseExperiment()
    {

    }

    public void StopExperiment()
    {
        InputController.touchEnabled = false;
        audioSource.Stop();
        StopAllCoroutines();
        DebugText.Instance.SetText("Experiment Stopped!");
    }

    void InitializeContent()
    {
        illustrationTimestamps.Clear();
        List<AudiobookIllustration> audiobookIllustrations;
        string bookName = "undefined";
        switch (meta.bookName)
        {
            case bookNames.the_ocean:
                audiobookIllustrations = data.the_ocean;
                bookName = "the_ocean";
                break;
            case bookNames.the_martian:
                audiobookIllustrations = data.the_martian;
                bookName = "the_martian";
                break;
            case bookNames.blood_work:
                audiobookIllustrations = data.blook_work;
                bookName = "blood_work";
                break;
            case bookNames.dogs_of_riga:
                audiobookIllustrations = data.dogs_of_riga;
                bookName = "dogs_of_riga";
                break;
            case bookNames.educated:
                audiobookIllustrations = data.educated;
                bookName = "educated";
                break;
            case bookNames.the_wind:
                audiobookIllustrations = data.the_wind;
                bookName = "the_wind";
                break;
            case bookNames.dogs_of_riga_full:
                audiobookIllustrations = data.dogs_of_riga_full;
                bookName = "dogs_of_riga_full";
                break;
            case bookNames.the_ocean_full:
                audiobookIllustrations = data.the_ocean_full;
                bookName = "the_ocean_full";
                break;
            case bookNames.blood_work_user:
                audiobookIllustrations = data.blood_word_user;
                bookName = "blood_work";
                break;
            case bookNames.dogs_of_riga_user:
                audiobookIllustrations = data.dogs_of_riga_user;
                bookName = "dogs_or_riga";
                break;
            case bookNames.the_martian_user:
                audiobookIllustrations = data.the_martian_user;
                bookName = "the_martian";
                break;
            case bookNames.the_ocean_user:
                audiobookIllustrations = data.the_ocean_user;
                bookName = "the_ocean";
                break;
            case bookNames.educated_user:
                audiobookIllustrations = data.educated_user;
                bookName = "educated";
                break;
            case bookNames.the_wind_user:
                audiobookIllustrations = data.the_wind_user;
                bookName = "the_wind";
                break;
            default:
                audiobookIllustrations = null;
                bookName = "undefined";
                break;
        }

        if (audiobookIllustrations == null || bookName == "undefined")
        {
            DebugText.Instance.SetText("Something wrong with the data");
            return;
        }

        foreach (AudiobookIllustration ai in audiobookIllustrations)
        {
            if (ai.frequency[meta.frequency - 1] == 1)
            {
                if (bookName.Contains("user"))
                    illustrationTimestamps.Add(new IllustrationTimestamp(ai.timestamp, ai.duration, ai.filename, true));
                else
                    illustrationTimestamps.Add(new IllustrationTimestamp(ai.timestamp, ai.duration, ai.filename, false));
            }
        }


        clip = Resources.Load<AudioClip>("AudioClips/" + bookName);
        audioSource.clip = clip;
    }

    IEnumerator DisplayCrossFadeIllustration(int containerIndex)
    {
        Image currentContainer = illusContainers[containerIndex];
        float timer = 0f;

        currentContainer.sprite = illustrationTimestamps[currentIllusIndex].audiobookSprite;


        Color color = currentContainer.color;
        color = new Color(color.r, color.g, color.b, 0f);
        currentContainer.color = color;
        //imageBorderRenderer.material.color = color;

        currentContainer.gameObject.SetActive(true);
        //imageBorderRenderer.gameObject.SetActive(true);
        //Fade in
        while (timer < meta.fadeTime)
        {
            Color c = currentContainer.color;
            c = new Color(c.r, c.g, c.b, timer / meta.fadeTime);
            currentContainer.color = c;
            //imageBorderRenderer.material.color = c;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        color = currentContainer.color;
        color = new Color(color.r, color.g, color.b, 1f);
        currentContainer.color = color;
        //imageBorderRenderer.material.color = color;

        float nextTimestamp = 0f;


        if (nextIllusIndex < illustrationTimestamps.Count)
        {
            nextTimestamp = illustrationTimestamps[nextIllusIndex].startTimeInSeconds;
        }
        else
        {
            nextTimestamp = clip.length;
        }

        float timeDiff = nextTimestamp - illustrationTimestamps[currentIllusIndex].startTimeInSeconds;

        while (timer < (timeDiff - meta.fadeTime))
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Fade out
        while (timer < timeDiff)
        {
            Color c = currentContainer.color;
            c = new Color(c.r, c.g, c.b, (timeDiff - timer) / meta.fadeTime);
            currentContainer.color = c;
            //mageBorderRenderer.material.color = c;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        currentContainer.gameObject.SetActive(false);
        //imageBorderRenderer.gameObject.SetActive(false);
        //currentImageIndex = -1;
    }
    private void InitializeIllustrationReel()
    {
        for (int i = 0; i < illusReel.Length; i++)
        {
            illusReel[i].rectTransform.anchoredPosition = new Vector2(imgReelX, imgReelMid + (Mod((-i + 1), 5) - 2) * imgDefaultDim);
        }
        currentReelContainerIndex = 0;
        illusReel[0].sprite = illustrationTimestamps[0].audiobookSprite;
        illusReel[0].color = new Color(1f, 1f, 1f, intermediateTransparency);
        illusReel[1].sprite = illustrationTimestamps[1].audiobookSprite;
        illusReel[1].color = new Color(1f, 1f, 1f, 0f);
        illusReel[2].sprite = null;
        illusReel[2].color = new Color(1f, 1f, 1f, 0f);
        illusReel[3].sprite = null;
        illusReel[3].color = new Color(1f, 1f, 1f, 0f);
        illusReel[4].sprite = null;
        illusReel[4].color = new Color(1f, 1f, 1f, 0f);
    }
    IEnumerator DisplayIllustrationReel(int midContainerIndex)
    {
        Image prePreContainer = illusReel[Mod((midContainerIndex - 2), 5)];
        Image preContainer = illusReel[Mod((midContainerIndex - 1), 5)];
        Image midContainer = illusReel[midContainerIndex];
        Image nextContainer = illusReel[Mod((midContainerIndex + 1), 5)];
        Image nextNextContainer = illusReel[Mod((midContainerIndex + 2), 5)];
        float timer = 0f;

        if (nextIllusIndex + 1 < illustrationTimestamps.Count)
            nextNextContainer.sprite = illustrationTimestamps[nextIllusIndex + 1].audiobookSprite;
        else
            nextNextContainer.sprite = null;

        float yStartMid = midContainer.rectTransform.anchoredPosition.y;
        float yEndMid = yStartMid + imgDefaultDim;
        float yStartNext = nextContainer.rectTransform.anchoredPosition.y;
        float yEndNext = yStartNext + imgDefaultDim;
        float yStartPre = preContainer.rectTransform.anchoredPosition.y;
        float yEndPre = yStartPre + imgDefaultDim;
        float yStartPrePre = prePreContainer.rectTransform.anchoredPosition.y;
        float yEndPrePre = yStartPrePre + imgDefaultDim;



        while (timer < meta.fadeTime)
        {
            RollUpContainers(midContainer, yStartMid, yEndMid, intermediateTransparency, 1f, timer / meta.fadeTime);
            RollUpContainers(nextContainer, yStartNext, yEndNext, 0f, intermediateTransparency, timer / meta.fadeTime);
            RollUpContainers(preContainer, yStartPre, yEndPre, 1f, intermediateTransparency, timer / meta.fadeTime);
            RollUpContainers(prePreContainer, yStartPrePre, yEndPrePre, intermediateTransparency, 0f, timer / meta.fadeTime);
            //imageBorderRenderer.material.color = c;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        nextNextContainer.rectTransform.anchoredPosition = new Vector2(imgReelX, imgReelMid - 2 * imgDefaultDim);

    }

    void RollUpContainers(Image imgContainer, float startPosY, float endPosY, float startTransparency, float endTransparency, float fraction)
    {
        if (imgContainer.sprite != null)
        {
            imgContainer.color = new Color(1f, 1f, 1f, (endTransparency - startTransparency) * fraction + startTransparency);
        }
        else
        {
            imgContainer.color = new Color(1f, 1f, 1f, 0f);
        }

        imgContainer.rectTransform.anchoredPosition = new Vector2(imgReelX, (endPosY - startPosY) * fraction + startPosY);

    }

    public void ChangeFrequencyLevel(Slider slider)
    {
        meta.frequency = (int)slider.value * 2;
        ValueChanger.onValueChange.Invoke();
    }

    public void ChangeTiming(Slider slider)
    {
        meta.timingOffset = (int)slider.value;
        ValueChanger.onValueChange.Invoke();
    }

    public void ChangeScale(Slider slider)
    {
        meta.scale = Mathf.Pow(1.3f, slider.value);
        ValueChanger.onValueChange.Invoke();
    }

    public void ChangeAudiobook(int index)
    {
        meta.bookName = (bookNames)index;
    }

    public void SwitchToTimeline(Toggle toggle)
    {
        meta.showTimeline = toggle.isOn;
    }

    int Mod(int a, int n) => (a % n + n) % n;

    public Meta GetMeta()
    {
        return meta;
    }

    //public void HandleDriveResponse(Drive.DataContainer dataContainer)
    //{
    //    if (dataContainer.QueryType == Drive.QueryType.getTable)
    //    {
    //        string rawJSon = dataContainer.payload;
    //
    //        // Check if the type is correct.
    //        if (string.Compare(dataContainer.objType, _tableName) == 0)
    //        {
    //            // Parse from json to the desired object type.
    //            PlayerInfo[] players = JsonHelper.ArrayFromJson<PlayerInfo>(rawJSon);
    //
    //            string logMsg = "<color=yellow>" + players.Length.ToString() + " objects retrieved from the cloud and parsed:</color>";
    //            for (int i = 0; i < players.Length; i++)
    //            {
    //                logMsg += "\n" +
    //                    "<color=blue>Name: " + players[i].name + "</color>\n" +
    //                    "Level: " + players[i].level + "\n" +
    //                    "Health: " + players[i].health + "\n" +
    //                    "Role: " + players[i].role + "\n";
    //            }
    //            Debug.Log(logMsg);
    //        }
    //    }
    //}
    //
    //private void OnEnable()
    //{
    //    // Suscribe for catching cloud responses.
    //    Drive.responseCallback += HandleDriveResponse;
    //}
    //
    //private void OnDisable()
    //{
    //    // Remove listeners.
    //    Drive.responseCallback -= HandleDriveResponse;
    //}
}
