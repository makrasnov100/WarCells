using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HideExtraSettings : MonoBehaviour
{
    public TMP_Dropdown curDropDown;
    public GameObject cellPadding;
    public GameObject cellMaxDist;
    public GameObject cellSymetry;
    public GameObject cellPaddingT;
    public GameObject cellMaxDistT;
    public GameObject cellSymetryT;


    public void UpdateVisibilityStatus()
    {
        if (curDropDown.options[curDropDown.value].text == "Hex")
        {
            cellPadding.SetActive(false);
            cellMaxDist.SetActive(false);
            cellSymetry.SetActive(false);
            cellPaddingT.SetActive(false);
            cellMaxDistT.SetActive(false);
            cellSymetryT.SetActive(false);
        }
        else
        {
            cellPadding.SetActive(true);
            cellMaxDist.SetActive(true);
            cellSymetry.SetActive(true);
            cellPaddingT.SetActive(true);
            cellMaxDistT.SetActive(true);
            cellSymetryT.SetActive(true);
        }
    }
}
