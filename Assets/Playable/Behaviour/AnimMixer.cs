using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    /// <summary>
    /// 动画过渡混合
    /// </summary>
    public class AnimMixer : AdapterBase
    {
        /// <summary>
        /// 输入源的数量
        /// </summary>
        public int InputCount { get; private set; }
        /// <summary>
        /// 当前的播放的输入端口索引
        /// </summary>
        public int CurrentIndex => _currentIndex;
        private int _currentIndex;
        /// <summary>
        /// 是否正在动画过渡中
        /// </summary>
        public bool IsTransition => _isTransition;
        private bool _isTransition;
        /// <summary>
        /// 动画混合器，用以混合两个过渡的动画
        /// </summary>
        private AnimationMixerPlayable _animMixer;
        
        /// <summary>
        /// 完全过渡到目标动画所剩时间
        /// </summary>
        private float _timeToNext;
        
        private int _targetIndex;
        
        /// <summary>
        /// 过渡过程中当前动画的权重的变化速度
        /// </summary>
        private float _currentSpeed;
        /// <summary>
        /// 过渡过程中需要减少权重的端口索引列表
        /// </summary>
        private List<int> _declinedIndex; 
        /// <summary>
        /// 过渡过程中需要减少的权重的变化速度
        /// </summary>
        private float _declinedSpeed;
        /// <summary>
        /// 过渡过程中所有递减权重动画的权重和
        /// </summary>
        private float _declinedWeight;
        

        public AnimMixer(PlayableGraph graph) : base(graph)
        {
            _animMixer = AnimationMixerPlayable.Create(graph, 0, true);
            m_adapterPlayable.AddInput(_animMixer, 0, 1f);
            _declinedIndex = new List<int>();
            InputCount = 0;
            _targetIndex = -1;
        }

        public override void AddInput(Playable playable)
        {
            _animMixer.AddInput(playable, 0, 0f);
            InputCount++;
            
            // 第一个添加的输入源的权重为1
            if(InputCount == 1)
            {
                _animMixer.SetInputWeight(0, 1f);
                _currentIndex = 0;
            }
        }

        public override void Enable()
        {
            base.Enable();

            if(InputCount > 0)
            {
                AnimHelper.Enable(_animMixer, 0); // 启用端口0的输入源
            }
            _animMixer.SetTime(0f);
            _animMixer.Play();
            m_adapterPlayable.SetTime(0f);
            m_adapterPlayable.Play();
            _animMixer.SetInputWeight(0, 1f);
            _currentIndex = 0;
            _targetIndex = -1;
        }

        public override void Disable()
        {
            base.Disable();
            
            for (int i = 0; i < InputCount; i++)
            {
                _animMixer.SetInputWeight(i, 0f);
                AnimHelper.Disable(_animMixer.GetInput(i));
            }
            _animMixer.Pause();
            m_adapterPlayable.Pause();
        }

        /// <summary>
        /// 处理过渡过程中的混合动画权重
        /// </summary>
        /// <param name="playable"></param>
        /// <param name="info"></param>
        public override void OnPrepareFrame(Playable playable, FrameData info)
        {
            base.OnPrepareFrame(playable, info);

            if (!enable) return; 
            if (!_isTransition || _targetIndex < 0) return;

            if (_timeToNext > 0f)
            {
                // 在过渡时间中
                
                _timeToNext -= info.deltaTime;
                
                // 除了目标动画以外的所有输入动画的权重
                _declinedWeight = 0f;
                for (int i = 0; i < _declinedIndex.Count; i++)
                {
                    var w = ModifyWeight(_declinedIndex[i], -info.deltaTime * _declinedSpeed);
                    if(w <= 0f)
                    {
                        // 当前索引的动画权重为0，断开该索引的动画
                        AnimHelper.Disable(_animMixer, _declinedIndex[i]);
                        _declinedIndex.Remove(_declinedIndex[i]);
                    }
                    else
                    {
                        _declinedWeight += w;
                    }
                }
                _declinedWeight += ModifyWeight(_currentIndex, -info.deltaTime * _currentSpeed);
                
                // 设置过渡目标的权重
                SetWeight(_targetIndex, 1f - _declinedWeight);
                return;
            }
            // 过渡结束
            _isTransition = false;
            // 断开当前端口的动画
            AnimHelper.Disable(_animMixer, _currentIndex);
            // 更新当前端口号
            _currentIndex = _targetIndex;
            _targetIndex = -1;
        }

        /// <summary>
        /// 过渡到指定端口索引的动画
        /// 权重的计算思路：
        /// 1.当只有2个动画过渡的时候，目标动画权重线性减少，当前动画权重线性增加，两者权重和为1
        /// 2.当存在多个动画过渡的时候，例如过渡的过程中需要过渡到另一个潜在动画状态，需要考虑过渡的程度。将占据过半权重的动画视为当前动画，
        /// 将剩余的动画视为递减权重的动画，最后将潜在动画视为目标动画，所有动画的权重和为1
        /// </summary>
        /// <param name="potentialTargetIndex">潜在目标端口索引</param>
        public void TransitionTo(int potentialTargetIndex)
        {
            if(_isTransition && _targetIndex >= 0)
            {
                // 有目标动画正在过渡中，此时需要过渡到新的潜在目标动画
                
                // 潜在目标动画与目标动画相同，不处理
                if (potentialTargetIndex == _targetIndex) return;
                
                if(potentialTargetIndex == _currentIndex)
                {
                    // 潜在目标动画与当前动画相同，将当前动画设置为目标动画
                    _currentIndex = _targetIndex;
                }
                else if (GetWeight(_currentIndex) > GetWeight(_targetIndex))
                {
                    // 目标动画的权重尚未过半（过渡进度未过半），将目标动画端口添加到递减列表中，后续目标动画的权重不断减少
                    _declinedIndex.Add(_targetIndex);
                }
                else
                {
                    // 当前动画的权重小于一半（过渡进度已过半），将当前动画端口添加到递减列表中，后续目当前动画端口的权重不断减少
                    _declinedIndex.Add(_currentIndex);
                    // 将目标动画看作为当前动画
                    _currentIndex = _targetIndex;
                }
            }
            else
            {
                // 没有正在过渡的动画，潜在目标就是当前动画，不处理
                if (potentialTargetIndex == _currentIndex) return;
            }

            // 设置目标动画
            _targetIndex = potentialTargetIndex;
            // 目标动画不在视为递减
            _declinedIndex.Remove(_targetIndex);
            AnimHelper.Enable(_animMixer, _targetIndex);
            // 计算剩余过渡时间
            _timeToNext = GetTargetEnterTime(_targetIndex) * (1f - GetWeight(_targetIndex));
            // 计算递减列表中动画的权重的递减速度
            _declinedSpeed = 2f / _timeToNext;
            // 计算当前权重的递减速度
            _currentSpeed = GetWeight(_currentIndex) / _timeToNext;
            _isTransition = true;
        }

        /// <summary>
        /// 获取对应输入源的权重
        /// </summary>
        /// <param name="index">输入源端口索引</param>
        /// <returns></returns>
        public float GetWeight(int index)
        {
            if (index >= 0 && index < InputCount)
                return _animMixer.GetInputWeight(index);
            return 0f;
        }
        
        /// <summary>
        /// 设置对应输入源的权重
        /// </summary>
        /// <param name="index">输入源端口索引</param>
        /// <param name="weight">权重</param>
        public void SetWeight(int index, float weight)
        {
            if (index >= 0 && index < InputCount)
                _animMixer.SetInputWeight(index, weight);
        }
        
        /// <summary>
        /// 增量修改权重的值
        /// </summary>
        /// <param name="index">输入源端口索引</param>
        /// <param name="delta">权重的变化量</param>
        /// <returns>当前的权重</returns>
        private float ModifyWeight(int index, float delta)
        {
            if (index < 0 || index >= InputCount)
                return 0;
            
            float weight = Mathf.Clamp01(_animMixer.GetInputWeight(index) + delta);
            _animMixer.SetInputWeight(index, weight);
            return weight;
        }

        /// <summary>
        /// 获取目标动画的过渡时间
        /// </summary>
        /// <param name="index">输入源端口索引</param>
        /// <returns></returns>
        private float GetTargetEnterTime(int index)
        {
            return ((ScriptPlayable<AdapterPlayableBehaviour>)_animMixer.GetInput(index)).GetBehaviour().GetAnimEnterTime();
        }
        
    }
}
