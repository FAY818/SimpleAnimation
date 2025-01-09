using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 1.动画播放相关的方法
/// 2.动画状态相关的方法
/// 3.动画设置相关方法
/// </summary>
[RequireComponent(typeof(Animator))]
public partial class SimpleAnimation: MonoBehaviour
{
    /// <summary>
    /// 动画状态相关信息接口，这个和SimpleAnimationPlayable中的IState对应
    /// </summary>
    public interface State
    {
        bool enabled { get; set; }
        bool isValid { get; }
        float time { get; set; }
        float normalizedTime { get; set; }
        float speed { get; set; }
        string name { get; set; }
        float weight { get; set; }
        float length { get; }
        AnimationClip clip { get; }
        WrapMode wrapMode { get; set; }
    }
    public Animator animator
    {
        get
        {
            if (m_Animator == null)
            {
                m_Animator = GetComponent<Animator>();
            }
            return m_Animator;
        }
    }

    /// <summary>
    /// 动画更新是否是与物理系统的固定更新（FixedUpdate）同步。
    /// </summary>
    public bool animatePhysics
    {
        get { return m_AnimatePhysics; }
        set { m_AnimatePhysics = value; animator.updateMode = m_AnimatePhysics ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal; }
    }

    /// <summary>
    /// 动画的裁剪模式
    /// </summary>
    public AnimatorCullingMode cullingMode
    {
        get { return animator.cullingMode; }
        set { m_CullingMode = value;  animator.cullingMode = m_CullingMode; }
    }

    public bool isPlaying { get { return m_Playable.IsPlaying(); } }

    public bool playAutomatically
    {
        get { return m_PlayAutomatically; }
        set { m_PlayAutomatically = value; }
    }

    public AnimationClip clip
    {
        get { return m_Clip; }
        set
        {
            LegacyClipCheck(value);
            m_Clip = value;
        }  
    }

    /// <summary>
    /// 动画循环模式
    /// </summary>
    public WrapMode wrapMode
    {
        get { return m_WrapMode; }
        set { m_WrapMode = value; }
    }
    
    public void AddClip(AnimationClip clip, string newName)
    {
        LegacyClipCheck(clip);
        AddState(clip, newName);
    }

    public void Blend(string stateName, float targetWeight, float fadeLength)
    {
        m_Animator.enabled = true;
        Kick();
        m_Playable.Blend(stateName, targetWeight,  fadeLength);
    }

    public void CrossFade(string stateName, float fadeLength)
    {
        m_Animator.enabled = true;
        Kick();
        m_Playable.Crossfade(stateName, fadeLength);
    }

    public void CrossFadeQueued(string stateName, float fadeLength, QueueMode queueMode)
    {
        m_Animator.enabled = true;
        Kick();
        m_Playable.CrossfadeQueued(stateName, fadeLength, queueMode);
    }

    public int GetClipCount()
    {
        return m_Playable.GetClipCount();
    }

    public bool IsPlaying(string stateName)
    {
        return m_Playable.IsPlaying(stateName);
    }

    public void Stop()
    {
        m_Playable.StopAll();
    }

    public void Stop(string stateName)
    {
        m_Playable.Stop(stateName);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Sample()
    {
        m_Graph.Evaluate();
    }

    public bool Play()
    {
        m_Animator.enabled = true;
        Kick();
        if (m_Clip != null && m_PlayAutomatically)
        {
            m_Playable.Play(kDefaultStateName);
        }
        return false;
    }

    /// <summary>
    /// 添加动画片段后重建状态机
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="name"></param>
    public void AddState(AnimationClip clip, string name)
    {
        LegacyClipCheck(clip);
        Kick();
        if (m_Playable.AddClip(clip, name))
        {
            RebuildStates();
        }
    }

    public void RemoveState(string name)
    {
        if (m_Playable.RemoveClip(name))
        {
            RebuildStates();
        }
    }

    public bool Play(string stateName)
    {
        m_Animator.enabled = true;
        Kick();
        return m_Playable.Play(stateName);
    }

    public void PlayQueued(string stateName, QueueMode queueMode)
    {
        m_Animator.enabled = true;
        Kick();
        m_Playable.PlayQueued(stateName, queueMode);
    }

    public void RemoveClip(AnimationClip clip)
    {
        if (clip == null)
            throw new System.NullReferenceException("clip");

        if ( m_Playable.RemoveClip(clip) )
        {
            RebuildStates();
        }
       
    }

    public void Rewind()
    {
        Kick();
        m_Playable.Rewind();
    }

    public void Rewind(string stateName)
    {
        Kick();
        m_Playable.Rewind(stateName);
    }

    /// <summary>
    /// 获取当前状态，涉及不同的State类型转换
    /// </summary>
    /// <param name="stateName"></param>
    /// <returns></returns>
    public State GetState(string stateName)
    {
        SimpleAnimationPlayable.IState state = m_Playable.GetState(stateName);
        if (state == null)
            return null;

        return new StateImpl(state, this);
    }

    /// <summary>
    /// 获取状态集合
    /// </summary>
    /// <returns></returns>
    public IEnumerable<State> GetStates()
    {
        return new StateEnumerable(this);
    }

    /// <summary>
    /// 状态索引器
    /// </summary>
    /// <param name="name"></param>
    public State this[string name]
    {
        get { return GetState(name); }
    }

}
