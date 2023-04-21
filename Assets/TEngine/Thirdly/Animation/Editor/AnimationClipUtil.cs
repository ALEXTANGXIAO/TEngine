/*------------------------------------------------------------------------------
Original Author: Pikachuxxxx
Adapted By: Brandon Lyman
This script is an adaptation of Pikachuxxxx's utiltiy to reverse an animation 
clip in Unity. Please find the original Github Gist here:
https://gist.github.com/8101da6d14a5afde80c7c180e3a43644.git
ABSOLUTELY ALL CREDIT FOR THIS SCRIPT goes to Pikachuxxxx. Thank you so much for
your original script!
Unfortunately, their method that utilizes 
"AnimationUtility.GetAllCurves()" is obsolete, according to the official
unity documentation:
https://docs.unity3d.com/ScriptReference/AnimationUtility.GetAllCurves.html 
The editor suggests using "AnimationUtility.GetCurveBindings()" in its stead,
and this script reveals how that can be accomplished as it is slightly
different from the original methodology. I also added in some logic to 
differentiate between the original clip and the new clip being created, as 
I experienced null reference exceptions after the original "ClearAllCurves()" 
call. Additionally, I placed the script's logic in a ScriptableWizard class to 
fit the needs for my project. For more information on ScriptableWizards, please
refer to this Unity Learn Tutorial:
https://learn.unity.com/tutorial/creating-basic-editor-tools#5cf6c8f2edbc2a160a8a0951
Hope this helps and please comment with any questions. Thanks!
------------------------------------------------------------------------------*/

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
//using static DG.DemiEditor.DeGUIKey;

public class SpiteAnimationClip : ScriptableWizard
{
    public string NewFileName = "";

    public float BeginTime = 0;

    public float EndTime = 1;

    [MenuItem("Tools/SplitAnimationClip...")]
    private static void SplitAnimationClipShow()
    {
        ScriptableWizard.DisplayWizard<SpiteAnimationClip>("SpiteAnimationClip...", "splite");
    }

    private void OnWizardCreate()
    {
        string directoryPath =
            Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
        string fileName =
            Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject));
        string fileExtension =
            Path.GetExtension(AssetDatabase.GetAssetPath(Selection.activeObject));
        fileName = fileName.Split('.')[0];

        string copiedFilePath = "";
        if (NewFileName != null && NewFileName != "")
        {
            copiedFilePath = directoryPath + Path.DirectorySeparatorChar + NewFileName + fileExtension;
        }
        else
        {
            copiedFilePath = directoryPath + Path.DirectorySeparatorChar + fileName + $"_split_{BeginTime}_{EndTime}" + fileExtension;
        }

        AnimationClip originalClip = GetSelectedClip();

        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Selection.activeObject), copiedFilePath);

        AnimationClip reversedClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(copiedFilePath, typeof(AnimationClip));

        if (originalClip == null)
        {
            return;
        }

        float clipLength = originalClip.length;
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(originalClip);
        Debug.Log(curveBindings.Length);
        reversedClip.ClearCurves();
        float timeOffset = -1;
        foreach (EditorCurveBinding binding in curveBindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, binding);
            var keys = new  List<Keyframe>();// curve.keys;
            int keyCount = curve.keys.Length;
            for (int i = 0; i < keyCount; i++)
            {
                Keyframe K = curve.keys[i];

                if (K.time >= BeginTime && K.time <= EndTime) 
                {
                    if (timeOffset < 0) 
                    {
                        timeOffset = K.time;
                    }
                    K.time = K.time - timeOffset;
                    keys.Add(K);
                    Debug.LogError("time " + clipLength + " index " + i + " time " + K.time);
                }
               
            }
            curve.keys = keys.ToArray() ;
            reversedClip.SetCurve(binding.path, binding.type, binding.propertyName, curve);
        }

        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(originalClip);

        var eventsList= new List<AnimationEvent>();
        if (events.Length > 0)
        {
            for (int i = 0; i < events.Length; i++)
            {
                var eventTemp = events[i];
                if (eventTemp.time <= BeginTime && eventTemp.time >= EndTime)
                {
                    eventTemp.time = eventTemp.time - timeOffset;
                    eventsList.Add(eventTemp);
                }
            }
            AnimationUtility.SetAnimationEvents(reversedClip, eventsList.ToArray());
        }

        Debug.Log("[[ReverseAnimationClip.cs]] Successfully reversed " +
        "animation clip " + fileName + ".");
    }

    private AnimationClip GetSelectedClip()
    {
        Object[] clips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets);
        if (clips.Length > 0)
        {
            return clips[0] as AnimationClip;
        }
        return null;
    }
}