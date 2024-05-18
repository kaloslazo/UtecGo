using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetUiText : MonoBehaviour {
    [SerializeField]
    private TMP_Text textField;
    [SerializeField]
    private string fixedText;

    public void OnSliderValueChanged(float numericValue) {
        textField.text = $"{fixedText}: {numericValue}";
    }
}