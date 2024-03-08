using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class MessageController : MonoBehaviour
{
    public TextController textControl;
    public AudioSource audioSource;
    private List<AudioClip> audioClips;
    string[] messages;
    private List<int> oldMessages;
    private int messagePosition = 0;



    public void Initialize(string scene)
    {
        audioClips = new List<AudioClip>();
        oldMessages = new List<int>();
        string file = "Text/textFile" + scene;
        string file2 = "Audio/Scene " + scene + "/Recording_";
        var textFile = Resources.Load<TextAsset>(file);
        messages = textFile.text.Split("\n");
        for (int i = 0; i < messages.Length; i++)
        {
            messages[i] = messages[i].Replace("----", "\n");
            if (messages[i].Length > 1)
            {
                audioClips.Add(Resources.Load<AudioClip>(file2 + (i + 1).ToString()));
            }
        }
        
    }
    

    public float Play(int r)
    {
        if (oldMessages.Count == 0 || oldMessages[oldMessages.Count - 1] != r - 1)
        {
            oldMessages.Add(r - 1);
            messagePosition = oldMessages.Count - 1;
        }
            audioSource.clip = audioClips[r - 1];
            audioSource.Play();
            textControl.NewText(messages[r - 1]);
            return audioSource.clip.length;
    }

    public bool ShowOlder(bool paused)
    {
        if (messagePosition == oldMessages.Count -1 && paused==false)
        {
            textControl.AddText("\n(paused)");
            return true;
        }

        if (messagePosition > 0)
        {
            messagePosition--;
            textControl.NewText(messages[oldMessages[messagePosition]]);
            textControl.AddText("\n(paused)");
        }
        return true;
    }

    public bool ShowNewer()
    {
        if (messagePosition < oldMessages.Count - 1)
        {
            messagePosition++;
            textControl.NewText(messages[oldMessages[messagePosition]]);
            textControl.AddText("\n(paused)");
            return true;
        }
        textControl.NewText(messages[oldMessages[messagePosition]]);
        return false;
    }

}
