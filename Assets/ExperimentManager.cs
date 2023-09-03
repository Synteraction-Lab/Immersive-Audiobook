using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleSheetsForUnity;
using System.IO;
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

    [System.Serializable]
    public class AudiobookIllustration
    {
        public float timestamp;
        public float duration;
        public string filename;
        public string frequency;
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
            {
                audiobookSprite = Resources.Load<Sprite>("Illustrations_User/" + name);
                if (audiobookSprite == null)
                    audiobookSprite = Resources.Load<Sprite>("Illustrations/" + name);
                //audiobookSprite = Resources.Load<Sprite>(Application.persistentDataPath + "/Workshop/" + name + ".png");
                //audiobookSprite = ExperimentManager.LoadNewSprite(Application.persistentDataPath + "/Workshop/" + name + ".png");
                //if(audiobookSprite == null)
                //   audiobookSprite = ExperimentManager.LoadNewSprite(Application.persistentDataPath + "/Workshop/" + name + ".jpg");
                //audiobookSprite = ExperimentManager.LoadNewSprite(Application.persistentDataPath + "/Workshop/" + name + ".png");
                //audiobookSprite = Resources.Load<Sprite>(Application.persistentDataPath + "/Placeholder/" + name);
            }
            else
                audiobookSprite = Resources.Load<Sprite>("Illustrations/" + name);
        }
    }

    List<IllustrationTimestamp> illustrationTimestamps = new List<IllustrationTimestamp>();
    AudioClip clip;
    //[SerializeField] DataContainer data;
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
    private string _tableName;
    private Dictionary<string, AudiobookIllustration[]> illustrationLookUpTable = new Dictionary<string, AudiobookIllustration[]>();

    [SerializeField] GameObject loadingPanel;
    private bool loading;
    string saveFileName = "datasheet.json";

    private void Start()
    {
        illustrationLookUpTable.Clear();
        string saveFile = Application.persistentDataPath + "/" + saveFileName;
        if (File.Exists(saveFile))
        {
            string loadFile = File.ReadAllText(saveFile);
            var tables = JsonHelper.ArrayFromJson<Drive.DataContainer>(loadFile);
            for (int i = 0; i < tables.Length; i++)
            {
                Debug.Log("Loading Data from " + tables[i].objType + "\n" + tables[i].payload);
                AudiobookIllustration[] audiobookIllustrations = JsonHelper.ArrayFromJson<AudiobookIllustration>(tables[i].payload);
                illustrationLookUpTable.Add(tables[i].objType, audiobookIllustrations);
            }
        }
        currentIllusIndex = -1;
        nextIllusIndex = 0;
        foreach (Image img in illusContainers)
        {
            img.gameObject.SetActive(false);
        }

    }

    string GetTableNameWithEnum(bookNames bookName)
    {
        switch (bookName)
        {
            case bookNames.the_ocean:
                return "The Ocean";
            case bookNames.the_martian:
                return "The Martian";
            case bookNames.blood_work:
                return "Blood Work";
            case bookNames.dogs_of_riga:
                return "Dogs of Riga";
            case bookNames.educated:
                return "Educated";
            case bookNames.the_wind:
                return "The Wind";
            case bookNames.dogs_of_riga_full:
                return "Dogs of Riga Full";
            case bookNames.the_ocean_full:
                return "The Ocean Full";
            case bookNames.blood_work_user:
                return "Blood Work User";
            case bookNames.dogs_of_riga_user:
                return "Dogs of Riga User";
            case bookNames.the_martian_user:
                return "The Martian User";
            case bookNames.the_ocean_user:
                return "The Ocean User";
            case bookNames.educated_user:
                return "Educated User";
            case bookNames.the_wind_user:
                return "The Wind User";
            default:
                return "Undefined";
        }
    }

    public void RefreshData()
    {
        illustrationLookUpTable.Clear();
        loading = true;
        loadingPanel.SetActive(true);
        Drive.GetAllTables(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            StopExperiment();
        }

        if (loading)
        {
            if(illustrationLookUpTable.Count != 0)
            {
                loading = false;
                loadingPanel.SetActive(false);
            }
            
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
        if(meta.bookName == bookNames.blood_work_user)
        {
            illusBorder.gameObject.SetActive(false);
        } else
        {
            illusBorder.gameObject.SetActive(true);
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
        AudiobookIllustration[] audiobookIllustrations;
        string bookName = "undefined";
        if(!illustrationLookUpTable.TryGetValue(GetTableNameWithEnum(meta.bookName), out audiobookIllustrations))
        {
            DebugText.Instance.SetText("Book Name Look Up Failed!");
            return;
        }
        switch (meta.bookName)
        {
            case bookNames.the_ocean:
                bookName = "the_ocean";
                break;
            case bookNames.the_martian:
                bookName = "the_martian";
                break;
            case bookNames.blood_work:
                bookName = "blood_work";
                break;
            case bookNames.dogs_of_riga:
                bookName = "dogs_of_riga";
                break;
            case bookNames.educated:
                bookName = "educated";
                break;
            case bookNames.the_wind:
                bookName = "the_wind";
                break;
            case bookNames.dogs_of_riga_full:
                bookName = "dogs_of_riga_full";
                break;
            case bookNames.the_ocean_full:
                bookName = "the_ocean_full";
                break;
            case bookNames.blood_work_user:
                bookName = "blood_work";
                break;
            case bookNames.dogs_of_riga_user:
                bookName = "dogs_of_riga";
                break;
            case bookNames.the_martian_user:
                bookName = "the_martian";
                break;
            case bookNames.the_ocean_user:
                bookName = "the_ocean";
                break;
            case bookNames.educated_user:
                bookName = "educated";
                break;
            case bookNames.the_wind_user:
                bookName = "the_wind";
                break;
            default:
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
            string[] frequencies = ai.frequency.Split(',');
            if (int.Parse(frequencies[meta.frequency - 1]) == 1)
            {
                if (GetTableNameWithEnum(meta.bookName).Contains("User"))
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

    public void HandleDriveResponse(Drive.DataContainer dataContainer)
    {
        if (dataContainer.QueryType == Drive.QueryType.getAllTables)
        {
            string rawJSon = dataContainer.payload;

            File.WriteAllText(Application.persistentDataPath + "/" + saveFileName, rawJSon);

            Drive.DataContainer[] tables = JsonHelper.ArrayFromJson<Drive.DataContainer>(rawJSon);

            for (int i = 0; i < tables.Length; i++)
            {
                Debug.Log("Loading Data from " + tables[i].objType + "\n" + tables[i].payload);
                AudiobookIllustration[] audiobookIllustrations = JsonHelper.ArrayFromJson<AudiobookIllustration>(tables[i].payload);
                illustrationLookUpTable.Add(tables[i].objType, audiobookIllustrations);
            }

            
        }
    }
    
    private void OnEnable()
    {
        // Suscribe for catching cloud responses.
        Drive.responseCallback += HandleDriveResponse;
    }
    
    private void OnDisable()
    {
        // Remove listeners.
        Drive.responseCallback -= HandleDriveResponse;
    }

    public static Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

    public static Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Sprite NewSprite;
        Texture2D SpriteTexture = LoadTexture(FilePath);
        NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);

        return NewSprite;
    }
}
