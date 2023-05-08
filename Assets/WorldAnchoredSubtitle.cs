using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class WorldAnchoredSubtitle : MonoBehaviour
{
    [SerializeField] TextMeshPro textContainer;
    float lifeTime = 5f;
    float timeAfterSpawn = 0f;
    // Start is called before the first frame update
    void OnEnable()
    {
        timeAfterSpawn = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(lifeTime > 0f)
        {
            if(timeAfterSpawn < lifeTime)
            {
                timeAfterSpawn += Time.deltaTime;
            } else
            {
                Destroy(gameObject);
            }
        }
    }

    public void SetProperties(string text, float lifeTime)
    {
        textContainer.text = text;
        this.lifeTime = lifeTime;
    }


    
}
