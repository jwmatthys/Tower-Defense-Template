using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class SpawnClip : PlayableAsset, ITimelineClipAsset
{
    public SpawnBehaviour template = new SpawnBehaviour();

    // Tells Timeline this clip doesn't need to blend
    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<SpawnBehaviour>.Create(graph, template);
        return playable;
    }
}