using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugText : MonoBehaviour
{
    public static DebugText Instance;
    private TextMeshProUGUI textContainer;
    [SerializeField] float fadeTime;
    float fadeTimer = 0f;
    [SerializeField] Color textColor = Color.red;
    [SerializeField] bool hide = false;
    // Start is called before the first frame update
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        textContainer = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if(fadeTimer > 0f)
        {
            fadeTimer -= Time.deltaTime;
        }
        textContainer.color = new Color(textColor.r, textColor.g, textColor.b, fadeTimer / fadeTime);
    }

    public void SetText(string text)
    {
        if (!hide)
        {
            textContainer.text = text;
            fadeTimer = fadeTime;
        }
        Debug.Log(text);
    }
}
