using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class AnimationLayerTest : MonoBehaviour
{
    public AnimationClip clipA;
    public AnimationClip clipB;
    public float clipAWeight = 1.0f;
    public float clipBWeight = 1.0f;
    
    public AvatarMask bodyAvatarMask;
    
    private PlayableGraph playableGraph;
    
    // Start is called before the first frame update
    void Start()
    {
        playableGraph= PlayableGraph.Create("MaskGraph");
        
        var animationOutputPlayable = AnimationPlayableOutput.Create(playableGraph, "AnimationOutput", GetComponent<Animator>());
        var layerMixerPlayable = AnimationLayerMixerPlayable.Create(playableGraph, 2);
        animationOutputPlayable.SetSourcePlayable(layerMixerPlayable);
        
        var clipPlayableA = AnimationClipPlayable.Create(playableGraph, clipA);
        var clipPlayableB = AnimationClipPlayable.Create(playableGraph, clipB);
        playableGraph.Connect(clipPlayableA, 0, layerMixerPlayable, 0);
        playableGraph.Connect(clipPlayableB, 0, layerMixerPlayable, 1);
        
        
        layerMixerPlayable.SetLayerMaskFromAvatarMask(1, bodyAvatarMask);
        layerMixerPlayable.SetInputWeight(0, clipAWeight);
        layerMixerPlayable.SetInputWeight(1, clipBWeight);
        
        playableGraph.Play();
    }
    
    void OnDisable()
    {
        //销毁该图创建的所有可播放项和PlayableOutput。
        playableGraph.Destroy();
    }
}
