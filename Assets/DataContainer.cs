using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NorskaLib.GoogleSheetsDatabase;

[CreateAssetMenu(fileName = "DataContainer", menuName = "Custom/DataContainer")]
public class DataContainer : DataContainerBase
{
    [PageName("Blood Work")]
    public List<AudiobookIllustration> blook_work;
    [PageName("Dogs of Riga")]
    public List<AudiobookIllustration> dogs_of_riga;
    [PageName("The Martian")]
    public List<AudiobookIllustration> the_martian;
    [PageName("The Ocean")]
    public List<AudiobookIllustration> the_ocean;
    [PageName("Educated")]
    public List<AudiobookIllustration> educated;
    [PageName("The Wind")]
    public List<AudiobookIllustration> the_wind;
}

[System.Serializable]
public class AudiobookIllustration
{
    public float timestamp;
    public float duration;
    public string filename;
    public int[] frequency;
}
