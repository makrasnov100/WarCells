using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditText : MonoBehaviour
{
    public int characterLimit;

    public TMP_Text text;
    public Animator anim;
    bool isEditing = false;

    private void Update()
    {
        if (isEditing)
        {
            AddTextEdit();
        }
    }

    public void AddTextEdit()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b' && text.text.Length != 0)
            {
                text.text = text.text.Substring(0, text.text.Length - 1);
            }
            else
            {
                if (characterLimit > text.text.Length)
                {
                    text.text += c;
                }
            }
        }
    }

    public void SwitchEdit()
    {
        isEditing = !isEditing;
        anim.SetBool("isEditing", isEditing);
    }
}
