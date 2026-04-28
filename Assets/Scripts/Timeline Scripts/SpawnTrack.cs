using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.8f, 0.2f, 0.2f)]
[TrackClipType(typeof(SpawnClip))]
// No [TrackBindingType] — no scene object needed
public class SpawnTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<SpawnBehaviour>.Create(graph, inputCount);
    }
}