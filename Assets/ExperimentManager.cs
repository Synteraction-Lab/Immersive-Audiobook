using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ExperimentManager : MonoBehaviour
{
    public enum bookNames
    {
        the_ocean,
        the_martian,
        blood_work,
        dogs_of_riga
    }

    [System.Serializable]
    public class Meta
    {
        public bookNames bookName = bookNames.the_martian;
        public int frequency = 6;
        public float timingOffset = 0f;
        public float fadeTime = 2f;
        public bool maintainGap = false;
        public bool showTimeline = false;
    }
    
    [SerializeField] Meta meta;
    public class IllustrationTimestamp
    {
        public Sprite audiobookSprite;
        public float druation;
        public float startTimeInSeconds;
        public IllustrationTimestamp(float startTime, float duration, string name)
        {
            startTimeInSeconds = startTime;
            this.druation = duration;
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
    private const float imgReelOffset = 312.32f, imgReelMid = -259f, imgReelX = -911f, intermediateTransparency = 0.2f;

    private void Start()
    {
        currentIllusIndex = -1;
        nextIllusIndex = 0;
        foreach(Image img in illusContainers)
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
                if (clipTime > Mathf.Max((illustrationTimestamps[nextIllusIndex].startTimeInSeconds - meta.fadeTime/2f), 0f))
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
                    } else
                    {
                        StartCoroutine(DisplayIllustrationReel(currentReelContainerIndex));
                        currentReelContainerIndex++;
                        if(currentReelContainerIndex >= illusReel.Length)
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
        foreach (Image img in illusContainers)
        {
            img.gameObject.SetActive(false);
        }
        if (meta.showTimeline)
            InitializeIllustrationReel();
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
        switch(meta.bookName)
        {
            case bookNames.the_ocean:
                audiobookIllustrations = data.the_martian_test;
                bookName = "the_ocean";
                break;
            case bookNames.the_martian:
                audiobookIllustrations = data.the_martian_test;
                bookName = "the_martian";
                break;
            case bookNames.blood_work:
                audiobookIllustrations = data.the_martian_test;
                bookName = "blood_work";
                break;
            case bookNames.dogs_of_riga:
                audiobookIllustrations = data.the_martian_test;
                bookName = "dogs_of_riga";
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
            if(ai.frequency[meta.frequency-1] == 1)
                illustrationTimestamps.Add(new IllustrationTimestamp(ai.timestamp, ai.duration, ai.filename));
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
        

        if(nextIllusIndex < illustrationTimestamps.Count)
        {
            nextTimestamp = illustrationTimestamps[nextIllusIndex].startTimeInSeconds;
        } else
        {
            nextTimestamp = clip.length;
        }

        float timeDiff = nextTimestamp - illustrationTimestamps[currentIllusIndex].startTimeInSeconds;

        while (timer < (timeDiff - meta.fadeTime/2f))
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Fade out
        while (timer < (timeDiff + meta.fadeTime / 2f))
        {
            Color c = currentContainer.color;
            c = new Color(c.r, c.g, c.b, (timeDiff + meta.fadeTime / 2f - timer) / meta.fadeTime);
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
        for(int i = 0; i < illusReel.Length; i++)
        {
            illusReel[i].rectTransform.anchoredPosition = new Vector2(imgReelX, imgReelMid + (Mod((-i+1),5)-2) * imgReelOffset);
        }
        currentReelContainerIndex = 0;
        illusReel[0].sprite = illustrationTimestamps[0].audiobookSprite;
        illusReel[0].color = new Color(1f, 1f, 1f, intermediateTransparency);
        illusReel[1].sprite = illustrationTimestamps[1].audiobookSprite;
        illusReel[1].color = new Color(1f, 1f, 1f, 0f);
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
        float yEndMid = yStartMid + imgReelOffset;
        float yStartNext = nextContainer.rectTransform.anchoredPosition.y;
        float yEndNext = yStartNext + imgReelOffset;
        float yStartPre = preContainer.rectTransform.anchoredPosition.y;
        float yEndPre = yStartPre + imgReelOffset;
        float yStartPrePre = prePreContainer.rectTransform.anchoredPosition.y;
        float yEndPrePre = yStartPrePre + imgReelOffset;



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

        nextNextContainer.rectTransform.anchoredPosition = new Vector2(imgReelX, imgReelMid - 2 * imgReelOffset);

    }

    void RollUpContainers(Image imgContainer, float startPosY, float endPosY, float startTransparency, float endTransparency, float fraction)
    {
        if(imgContainer.sprite != null)
        {
            imgContainer.color = new Color(1f, 1f, 1f, (endTransparency - startTransparency) * fraction + startTransparency);
        } else
        {
            imgContainer.color = new Color(1f, 1f, 1f, 0f);
        }

        imgContainer.rectTransform.anchoredPosition = new Vector2(imgReelX, (endPosY - startPosY) * fraction + startPosY);

    }

    int Mod(int a, int n) => (a % n + n) % n;
}
