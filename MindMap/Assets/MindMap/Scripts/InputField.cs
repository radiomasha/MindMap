using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputField : MonoBehaviour
{
    [Header("Value from Input field")] [SerializeField]
    private string inputField;

    [SerializeField] private GameObject reactionGroup;
    [SerializeField] private TMP_Text textField;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GrabFromInputField(string input)
    {
        inputField = input;
        DisplayReactionToInput();
    }

    private void DisplayReactionToInput()
    {
        textField.text = "Welcome"+inputField;
        reactionGroup.SetActive(true);
    }
}
