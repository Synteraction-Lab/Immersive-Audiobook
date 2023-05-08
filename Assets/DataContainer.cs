using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NorskaLib.GoogleSheetsDatabase;

[CreateAssetMenu(fileName = "DataContainer", menuName = "Custom/DataContainer")]
public class DataContainer : DataContainerBase
{
    [PageName("FullText")]
    public List<Subtitle> Subtitles;
    
    
    [PageName("Phrases")]
    public List<Phrase> Phrases;
}

[System.Serializable]
public class Subtitle
{
    public string ID;
    public int Timestamp_START;
    public int Timestamp_END;
    public string Text;
}
[System.Serializable]
public class Phrase
{
    public string ID;
    public int Timestamp_START;
    public int Timestamp_END;
    public string Text;
}
