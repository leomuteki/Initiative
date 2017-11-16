/* name:            ResourceManager.cs
 * AUTHOR:          Emmilio Segovia
 * DESCRIPTION:     The Resource Manager is meant to keep references of all assets like item
 *                  scriptable objects and Sounds that will need to be accessed. That way,
 *                  everything can on be loaded once in the beginning of the game instead of
 *                  having wait times throughout the game.
 * REQUIREMENTS:    Singleton class must be defined with a static "Instance" variable.
 *                  To avoid typos, item naming convention should mimic book-titles; have each 
 *                  word separate and each word starts with a capital letter except for 
 *                  articles (of, the, a, in). Example: "Oil of a Golem".
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : Singleton<ResourceManager> {
    //the Dictionaries that keep each reference
    private Dictionary<string, AudioClip> AudioDict;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(Instance);
        AudioDict = new Dictionary<string, AudioClip>();
        LoadSounds();
    }
    
    /// <summary>
    /// Loads the songs from the Resources folder
    /// </summary>
    private void LoadSounds()
    {
        object[] loaded_items = Resources.LoadAll("Audio");
        foreach ( AudioClip i in loaded_items) {
            if (!AudioDict.ContainsKey(i.name)) {
                AudioDict.Add(i.name, i);
            }
        }
    }
        
    public AudioClip GetAudio(string name)
    {
        if (AudioDict.ContainsKey(name)) {
            return AudioDict[name];
        }
        else {
            Debug.LogError(name + " not found, it may be mispelled.");
            return null;
        }
    }
    
}
