using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

/// <summary>
/// 播放游戏对象上的单个动画剪辑
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayAnimationSample : MonoBehaviour
{
    public AnimationClip clip;
    private PlayableGraph playableGraph;

    void Start()
    {
        // 创建一个空的 PlayableGraph
        playableGraph = PlayableGraph.Create();
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        
        // 创建输出节点，指定输出源
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>()); 
        
        // AnimationClipPlayable必须包裹动画剪辑，使其与Playables API兼容
        var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip); 
        
        // 将可播放项连接到输出
        playableOutput.SetSourcePlayable(clipPlayable);
        
        // 播放该图
        playableGraph.Play();
    }

    void OnDisable()
    {
        //销毁该图创建的所有可播放项和PlayableOutput。
        playableGraph.Destroy();
    }
}
