using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NorskaLib.GoogleSheetsDatabase;

[CreateAssetMenu(fileName = "DataContainer", menuName = "Custom/DataContainer")]
public class DataContainer : DataContainerBase
{
    [PageName("TheOcean_detailed")]
    public List<AudiobookIllustration> the_martian_test;
}

[System.Serializable]
public class AudiobookIllustration
{
    public float timestamp;
    public float duration;
    public string filename;
    public int[] frequency;
}
