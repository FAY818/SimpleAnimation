using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    /// <summary>
    /// 动画过渡混合器
    /// </summary>
    public class AnimTransition : AdapterBase
    {
        public int inputCount { get; private set; }
        public int currentIndex => m_currentIndex;
        public bool isTransition => m_isTransition;

        private AnimationMixerPlayable m_animMixer; // 原生动画混合器

        #region 切换动画相关的变量
        private float m_timeToNext;
        private bool m_isTransition;
        private int m_targetIndex;
        private int m_currentIndex;
        private float m_currentSpeed;

        private List<int> m_declinedIndex; // 递减列表 
        private float m_declinedSpeed;
        private float m_declinedWeight;
        #endregion

        public AnimTransition(PlayableGraph graph):base(graph)
        {
            m_animMixer = AnimationMixerPlayable.Create(graph, 0, true);
            m_adapterPlayable.AddInput(m_animMixer, 0, 1f);

            m_declinedIndex = new List<int>();
            m_targetIndex = -1;
        }

        public override void AddInput(Playable playable)
        {
            base.AddInput(playable);
            m_animMixer.AddInput(playable, 0, 0f);
            inputCount++;
            if(inputCount == 1)
            {
                m_animMixer.SetInputWeight(0, 1f);
                m_currentIndex = 0;
            }
        }

        public override void Enable()
        {
            base.Enable();

            if(inputCount > 0)
            {
                AnimHelper.Enable(m_animMixer, 0);
            }
            m_animMixer.SetTime(0f);
            m_animMixer.Play();
            m_adapterPlayable.SetTime(0f);
            m_adapterPlayable.Play();

            m_animMixer.SetInputWeight(0, 1f);
            
            m_currentIndex = 0;
            m_targetIndex = -1;
        }

        public override void Disable()
        {
            base.Disable();
            for (int i = 0; i < inputCount; i++)
            {
                m_animMixer.SetInputWeight(i, 0f);
                AnimHelper.Disable(m_animMixer.GetInput(i));
            }
            m_animMixer.Pause();
            m_adapterPlayable.Pause();
        }

        /// <summary>
        /// 处理混合权重
        /// </summary>
        /// <param name="playable"></param>
        /// <param name="info"></param>
        public override void Execute(Playable playable, FrameData info)
        {
            base.Execute(playable, info);

            if (!enable) return;
            if (!m_isTransition || m_targetIndex < 0) return;

            
            if (m_timeToNext > 0f)
            {
                m_timeToNext -= info.deltaTime;

                m_declinedWeight = 0f;
                for (int i = 0; i < m_declinedIndex.Count; i++)
                {
                    var w = ModifyWeight(m_declinedIndex[i], -info.deltaTime * m_declinedSpeed);
                    if(w <= 0f)
                    {
                        // 当前索引的动画权重为0，不再起作用
                        AnimHelper.Disable(m_animMixer, m_declinedIndex[i]);
                        m_declinedIndex.Remove(m_declinedIndex[i]);
                    }
                    else
                    {
                        // 所有递减权重动画的权重和
                        m_declinedWeight += w;
                    }
                }
                m_declinedWeight += ModifyWeight(m_currentIndex, -info.deltaTime * m_currentSpeed);
                SetWeight(m_targetIndex, 1f - m_declinedWeight);
                return;
            }

            m_isTransition = false;
            
            AnimHelper.Disable(m_animMixer ,m_currentIndex);
            m_currentIndex = m_targetIndex;
            m_targetIndex = -1;
        }

        public void TransitionTo(int index)
        {
            if(m_isTransition && m_targetIndex >= 0)
            {
                if (index == m_targetIndex) return;
                if(index == m_currentIndex)
                {
                    m_currentIndex = m_targetIndex;
                }
                else if (GetWeight(m_currentIndex) > GetWeight(m_targetIndex))
                {
                    m_declinedIndex.Add(m_targetIndex);
                }
                else
                {
                    m_declinedIndex.Add(m_currentIndex);
                    m_currentIndex = m_targetIndex;
                }
            }
            else
            {
                if (index == m_currentIndex) return;
            }

            m_targetIndex = index;
            m_declinedIndex.Remove(m_targetIndex);
            AnimHelper.Enable(m_animMixer, m_targetIndex);
            m_timeToNext = GetTargetEnterTime(m_targetIndex) * (1f - GetWeight(m_targetIndex));
            m_declinedSpeed = 2f / m_timeToNext;
            m_currentSpeed = GetWeight(m_currentIndex) / m_timeToNext;
            m_isTransition = true;
        }

        public float GetWeight(int index)
        {
            if (index >= 0 && index < m_animMixer.GetInputCount())
                return m_animMixer.GetInputWeight(index);
            return 0f;
        }
        
        /// <summary>
        /// 设置对应索引的动画权重
        /// </summary>
        /// <param name="index"></param>
        /// <param name="weight"></param>
        public void SetWeight(int index, float weight)
        {
            if (index >= 0 && index < m_animMixer.GetInputCount())
                m_animMixer.SetInputWeight(index, weight);
        }

        private float GetTargetEnterTime(int index)
        {
            return ((ScriptPlayable<AdapterPlayableBehaviour>)m_animMixer.GetInput(index)).GetBehaviour().GetAnimEnterTime();
        }

        private float ModifyWeight(int index,float delta)
        {
            if (index < 0 || index >= inputCount)
                return 0;
            float weight = Mathf.Clamp01(m_animMixer.GetInputWeight(index) + delta);
            m_animMixer.SetInputWeight(index, weight);
            return weight;
        }
    }
}
