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
    [PageName("Dogs of Riga Full")]
    public List<AudiobookIllustration> dogs_of_riga_full;
    [PageName("The Ocean Full")]
    public List<AudiobookIllustration> the_ocean_full;
    [PageName("Blood Work User")]
    public List<AudiobookIllustration> blood_word_user;
    [PageName("Dogs of Riga User")]
    public List<AudiobookIllustration> dogs_of_riga_user;
    [PageName("The Martian User")]
    public List<AudiobookIllustration> the_martian_user;
    [PageName("The Ocean User")]
    public List<AudiobookIllustration> the_ocean_user;
    [PageName("Educated User")]
    public List<AudiobookIllustration> educated_user;
    [PageName("The Wind User")]
    public List<AudiobookIllustration> the_wind_user;
}

[System.Serializable]
public class AudiobookIllustration
{
    public float timestamp;
    public float duration;
    public string filename;
    public int[] frequency;
}
