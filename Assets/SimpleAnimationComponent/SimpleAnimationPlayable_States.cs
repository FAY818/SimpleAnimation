using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System;

/// <summary>
/// SimpleAnimationPlayable 中与State相关的部分
/// </summary>
public partial class SimpleAnimationPlayable : PlayableBehaviour
{
    private int m_StatesVersion = 0; //todo ？
    private void InvalidateStates() { m_StatesVersion++; }
    
    /// <summary>
    /// 状态机集合，可以遍历
    /// </summary>
    private class StateEnumerable: IEnumerable<IState>
    {
        private SimpleAnimationPlayable m_Owner;
        public StateEnumerable(SimpleAnimationPlayable owner)
        {
            m_Owner = owner;
        }

        public IEnumerator<IState> GetEnumerator()
        {
            return new StateEnumerator(m_Owner);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new StateEnumerator(m_Owner);
        }

        class StateEnumerator : IEnumerator<IState>
        {
            private int m_Index = -1;
            private int m_Version;
            private SimpleAnimationPlayable m_Owner;
            public StateEnumerator(SimpleAnimationPlayable owner)
            {
                m_Owner = owner;
                m_Version = m_Owner.m_StatesVersion;
                Reset();
            }

            private bool IsValid() { return m_Owner != null && m_Version == m_Owner.m_StatesVersion; }

            IState GetCurrentHandle(int index)
            {
                if (!IsValid())
                    throw new InvalidOperationException("The collection has been modified, this Enumerator is invalid");

                if (index < 0 || index >= m_Owner.m_States.Count)
                    throw new InvalidOperationException("Enumerator is invalid");

                StateInfo state = m_Owner.m_States[index];
                if (state == null)
                    throw new InvalidOperationException("Enumerator is invalid");

                return new StateHandle(m_Owner, state.index, state.playable);
            }

            object IEnumerator.Current { get { return GetCurrentHandle(m_Index); } }

            IState IEnumerator<IState>.Current { get { return GetCurrentHandle(m_Index); } }

            public void Dispose() { }

            public bool MoveNext()
            {
                if (!IsValid())
                    throw new InvalidOperationException("The collection has been modified, this Enumerator is invalid");

                do
                { m_Index++; } while (m_Index < m_Owner.m_States.Count && m_Owner.m_States[m_Index] == null);

                return m_Index < m_Owner.m_States.Count;
            }

            public void Reset()
            {
                if (!IsValid())
                    throw new InvalidOperationException("The collection has been modified, this Enumerator is invalid");
                m_Index = -1;
            }
        }
    }
    
    /// <summary>
    /// 动画状态机属性定义接口
    /// </summary>
    public interface IState
    {
        bool IsValid();

        bool enabled { get; set; }

        float time { get; set; }

        float normalizedTime { get; set; }

        float speed { get; set; }

        string name { get; set; }

        float weight { get; set; }

        float length { get; }

        AnimationClip clip { get; }

        WrapMode wrapMode { get; }
    }

    /// <summary>
    /// 动画状态机接口实现类，封装具体某个动画状态的相关属性
    /// </summary>
    public class StateHandle : IState
    {
        public int index { get { return m_Index; } }
        private SimpleAnimationPlayable m_Parent;
        private int m_Index;
        private Playable m_Target;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="s">？</param>
        /// <param name="index">状态索引</param>
        /// <param name="target">目标可播放对像</param>
        public StateHandle(SimpleAnimationPlayable s, int index, Playable target)
        {
            m_Parent = s;
            m_Index = index;
            m_Target = target;
        }

        public bool IsValid()
        {
            return m_Parent.ValidateInput(m_Index, m_Target);
        }

        public bool enabled
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_States[m_Index].enabled;
            }

            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                if (value)
                    m_Parent.m_States.EnableState(m_Index);
                else
                    m_Parent.m_States.DisableState(m_Index);

            }
        }

        public float time
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_States.GetStateTime(m_Index);
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                m_Parent.m_States.SetStateTime(m_Index, value);
            }
        }

        public float normalizedTime
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");

                float length = m_Parent.m_States.GetClipLength(m_Index);
                if (length == 0f)
                    length = 1f;

                return m_Parent.m_States.GetStateTime(m_Index) / length;
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");

                float length = m_Parent.m_States.GetClipLength(m_Index);
                if (length == 0f)
                    length = 1f;

                m_Parent.m_States.SetStateTime(m_Index, value *= length);
            }
        }

        public float speed
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_States.GetStateSpeed(m_Index);
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                m_Parent.m_States.SetStateSpeed(m_Index, value);
            }
        }

        public string name
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_States.GetStateName(m_Index);
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                if (value == null)
                    throw new System.ArgumentNullException("A null string is not a valid name");
                m_Parent.m_States.SetStateName(m_Index, value);
            }
        }

        public float weight
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_States[m_Index].weight;
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                if (value < 0)
                    throw new System.ArgumentException("Weights cannot be negative");

                m_Parent.m_States.SetInputWeight(m_Index, value);
            }
        }

        public float length
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_States.GetStateLength(m_Index);
            }
        }

        public AnimationClip clip
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_States.GetStateClip(m_Index);
            }
        }

        public WrapMode wrapMode
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_States.GetStateWrapMode(m_Index);
            }
        }
    }

    /// <summary>
    /// 动画状态机相关的信息封装类
    /// </summary>
    private class StateInfo
    {
        private float m_Time; // 动画播放到的时间
        private bool m_TimeIsUpToDate; //是否是最新时间
        
        public void InvalidateTime()
        {
            m_TimeIsUpToDate = false;
        }
        
        public void Initialize(string name, AnimationClip clip, WrapMode wrapMode)
        {
            m_StateName = name;
            m_Clip = clip;
            m_WrapMode = wrapMode;
        }

        public float GetTime()
        {
            if (m_TimeIsUpToDate)
                return m_Time;

            m_Time = (float)m_Playable.GetTime();
            m_TimeIsUpToDate = true;
            return m_Time;
        }

        public void SetTime(float newTime)
        {
            m_Time = newTime;
            m_Playable.ResetTime(m_Time);
            m_Playable.SetDone(m_Time >= m_Playable.GetDuration());
        }

        public void Enable()
        {
            if (m_Enabled)
                return;

            m_EnabledDirty = true;
            m_Enabled = true;
        }

        public void Disable()
        {
            if (m_Enabled == false)
                return;

            m_EnabledDirty = true;
            m_Enabled = false;
        }

        public void Pause()
        {
            m_Playable.SetPlayState(PlayState.Paused);
        }

        public void Play()
        {
            m_Playable.SetPlayState(PlayState.Playing);
        }

        public void Stop()
        {
            m_FadeSpeed = 0f;
            ForceWeight(0.0f);
            Disable();
            SetTime(0.0f);
            m_Playable.SetDone(false);
            if (isClone)
            {
                m_ReadyForCleanup = true;
            }
        }

        /// <summary>
        /// 强制刷新动画权重
        /// </summary>
        /// <param name="weight"></param>
        public void ForceWeight(float weight)
        {
           m_TargetWeight = weight;
           m_Fading = false;
           m_FadeSpeed = 0f;
           SetWeight(weight);
        }

        public void SetWeight(float weight)
        {
            m_Weight = weight;
            m_WeightDirty = true;
        }

        /// <summary>
        /// 设置淡出的权重和速度
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="speed"></param>
        public void FadeTo(float weight, float speed)
        {
            m_Fading = Mathf.Abs(speed) > 0f;
            m_FadeSpeed = speed;
            m_TargetWeight = weight;
        }

        public void DestroyPlayable()
        {
            if (m_Playable.IsValid())
            {
                m_Playable.GetGraph().DestroySubgraph(m_Playable);
            }
        }

        /// <summary>
        /// 标记为克隆状态机
        /// </summary>
        /// <param name="handle">克隆源</param>
        public void SetAsCloneOf(StateHandle handle)
        {
            m_ParentState = handle;
            m_IsClone = true;
        }

        public bool enabled
        {
            get { return m_Enabled; }
        }
        private bool m_Enabled;

        public int index
        {
            get { return m_Index; }
            set
            {
                Debug.Assert(m_Index == 0, "Should never reassign Index");
                m_Index = value;
            }
        }
        private int m_Index;

        public string stateName
        {
            get { return m_StateName; }
            set { m_StateName = value; }
        }
        private string m_StateName;

        /// <summary>
        /// 
        /// </summary>
        public bool fading
        {
            get { return m_Fading; }
        }
        private bool m_Fading;

        /// <summary>
        /// 融合的目标权重
        /// </summary>
        public float targetWeight
        {
            get { return m_TargetWeight; }
        }
        private float m_TargetWeight;

        /// <summary>
        /// 当前动画权重
        /// </summary>
        public float weight
        {
            get { return m_Weight; }
        }
        float m_Weight;

        public float fadeSpeed
        {
            get { return m_FadeSpeed; }
        }
        float m_FadeSpeed;

        public float speed
        {
            get { return (float)m_Playable.GetSpeed(); }
            set { m_Playable.SetSpeed(value); }
        }

        public float playableDuration
        {
            get { return (float)m_Playable.GetDuration(); }
        }

        public AnimationClip clip
        {
            get { return m_Clip; }
        }
        private AnimationClip m_Clip;

        public void SetPlayable(Playable playable)
        {
            m_Playable = playable;
        }

        public bool isDone { get { return m_Playable.IsDone(); } }

        public Playable playable
        {
            get { return m_Playable; }
        }
        private Playable m_Playable;

        public WrapMode wrapMode
        {
            get { return m_WrapMode; }
        }
        private WrapMode m_WrapMode;

        //Clone information
        public bool isClone
        {
            get { return m_IsClone; }
        }
        private bool m_IsClone;

        public bool isReadyForCleanup
        {
            get { return m_ReadyForCleanup; }
        }
        private bool m_ReadyForCleanup;

        /// <summary>
        /// 克隆父，当前状态机的信息来源于父状态机，区别于节点和继承中的父子关系
        /// </summary>
        public StateHandle parentState
        {
            get { return m_ParentState; }
        }
        public StateHandle m_ParentState;

        // 标记Enabled是否被修改，无论是开启/关闭
        public bool enabledDirty
        {
            get { return m_EnabledDirty; }
        }
        private bool m_EnabledDirty;

        /// <summary>
        /// 标记权重是否被修改
        /// </summary>
        public bool weightDirty
        {
            get { return m_WeightDirty; }
        }
        private bool m_WeightDirty;

        public void ResetDirtyFlags()
        { 
            m_EnabledDirty = false;
            m_WeightDirty = false;
        }
    }

    /// <summary>
    /// 将StateInfo中的信息转换到StateHandle对象中
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private StateHandle StateInfoToHandle(StateInfo info)
    {
        return new StateHandle(this, info.index, info.playable);
    }

    /// <summary>
    /// StateInfo的统一管理类，通过索引可以访问每个状态的信息
    /// </summary>
    private class StateManagement
    {
        private List<StateInfo> m_States;

        public int Count
        {
            get { return m_Count; }
        }
        private int m_Count;

        public StateInfo this[int i]
        {
            get
            {
                return m_States[i];
            }
        }

        public StateManagement()
        {
            m_States = new List<StateInfo>();
        }
        
        public StateInfo InsertState()
        {
            StateInfo state = new StateInfo();

            int firstAvailable = m_States.FindIndex(s => s == null); // 首个空闲位置
            if (firstAvailable == -1)
            {
                firstAvailable = m_States.Count;
                m_States.Add(state);
            }
            else
            {
                m_States.Insert(firstAvailable, state);
            }

            state.index = firstAvailable;
            m_Count++;
            return state;
        }
        
        public bool AnyStatePlaying()
        {
            return m_States.FindIndex(s => s != null && s.enabled) != -1;
        }

        public void RemoveState(int index)
        {
            StateInfo removed = m_States[index];
            m_States[index] = null;
            removed.DestroyPlayable();
            m_Count = m_States.Count;
        }

        /// <summary>
        /// 移除 AnimationClip 及其对应状态机
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public bool RemoveClip(AnimationClip clip)
        {
            bool removed = false;
            for (int i = 0; i < m_States.Count; i++)
            {
                StateInfo state = m_States[i];
                if (state != null && state.clip == clip)
                {
                    RemoveState(i);
                    removed = true;
                }
            }
            return removed;
        }

        public StateInfo FindState(string name)
        {
            int index = m_States.FindIndex(s => s != null && s.stateName == name);
            if (index == -1)
                return null;

            return m_States[index];
        }

        public void EnableState(int index)
        {
            StateInfo state = m_States[index];
            state.Enable();
        }

        public void DisableState(int index)
        {
            StateInfo state = m_States[index];
            state.Disable();
        }

        public void SetInputWeight(int index, float weight)
        {
            StateInfo state = m_States[index];
            state.SetWeight(weight);
        }

        public void SetStateTime(int index, float time)
        {
            StateInfo state = m_States[index];
            state.SetTime(time);
        }

        public float GetStateTime(int index)
        {
            StateInfo state = m_States[index];
            return state.GetTime();
        }

        /// <summary>
        /// 是否是从某个状态克隆的
        /// </summary>
        /// <param name="potentialCloneIndex">潜在的克隆状态机索引</param>
        /// <param name="originalIndex">克隆源状态机索引/param>
        /// <returns></returns>
        public bool IsCloneOf(int potentialCloneIndex, int originalIndex)
        {
            StateInfo potentialClone = m_States[potentialCloneIndex];
            return potentialClone.isClone && potentialClone.parentState.index == originalIndex;
        }

        public float GetStateSpeed(int index)
        {
            return m_States[index].speed;
        }
        public void SetStateSpeed(int index, float value)
        {
            m_States[index].speed = value;
        }

        public float GetInputWeight(int index)
        {
            return m_States[index].weight;
        }

        public float GetStateLength(int index)
        {
            AnimationClip clip = m_States[index].clip;
            if (clip == null)
                return 0f;
            float speed = m_States[index].speed;
            if (speed == 0f)
                return Mathf.Infinity;

            return clip.length / speed;
        }

        public float GetClipLength(int index)
        {
            AnimationClip clip = m_States[index].clip;
            if (clip == null)
                return 0f;

            return clip.length;
        }

        public float GetStatePlayableDuration(int index)
        {
            return m_States[index].playableDuration;
        }

        public AnimationClip GetStateClip(int index)
        {
            return m_States[index].clip;
        }

        public WrapMode GetStateWrapMode(int index)
        {
            return m_States[index].wrapMode;
        }

        public string GetStateName(int index)
        {
            return m_States[index].stateName;
        }

        public void SetStateName(int index, string name)
        {
            m_States[index].stateName = name;
        }

        public void StopState(int index, bool cleanup)
        {
            if (cleanup)
            {
                RemoveState(index);
            }
            else
            {
                m_States[index].Stop();
            }
        }
    }

    /// <summary>
    /// 可成队的状态机
    /// </summary>
    private struct QueuedState
    {
        public QueuedState(StateHandle s, float t)
        {
            state = s;
            fadeTime = t;
        }

        public StateHandle state;
        public float fadeTime;
    }

}
