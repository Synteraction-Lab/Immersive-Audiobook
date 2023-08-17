using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NorskaLib.GoogleSheetsDatabase;

[CreateAssetMenu(fileName = "DataContainer", menuName = "Custom/DataContainer")]
public class DataContainer : DataContainerBase
{
    [PageName("1_Kidney")]
    public List<Phrase> Kidney;
    
    [PageName("2_Muscle")]
    public List<Phrase> Muscle;

    [PageName("3_Epidermis")]
    public List<Phrase> Epidermis;

    [PageName("4_Heart")]
    public List<Phrase> Heart;

    [PageName("WhyWeSleep_Phrases")]
    public List<Phrase> WMS_Phrases;

    [PageName("WhyWeSleep_FullText")]
    public List<Phrase> WMS_FullText;
}

[System.Serializable]
public class Phrase
{
    public string ID;
    public int Timestamp_START;
    public int Timestamp_END;
    public string Text;
}
