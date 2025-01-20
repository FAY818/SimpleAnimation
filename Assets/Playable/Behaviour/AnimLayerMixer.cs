using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    [Serializable]
    public struct LayerData
    {
        public float weight;
        public AvatarMask avatarmask;
        public AdapterBase adapter;
        
        public LayerData(float weight, AdapterBase playable, AvatarMask avatarmask = null)
        {
            this.weight = weight;
            this.adapter = playable;
            this.avatarmask = avatarmask;
        }
    }

    /// <summary>
    /// 分层混合
    /// </summary>
    public class AnimLayerMixer : AdapterBase
    {
        public int InputCount { get; protected set; }
        private AnimationLayerMixerPlayable _layerMixer;
        private List<LayerData> _LayerDatas;

        public AnimLayerMixer(PlayableGraph graph) : base(graph)
        {
            _layerMixer = AnimationLayerMixerPlayable.Create(graph);
            m_adapterPlayable.AddInput(_layerMixer, 0, 1f);
            InputCount = 0;
            _LayerDatas = new List<LayerData>();
        }
        
        public void SetLayerWeight(int index, float weight)
        {
            if (index < 0 || index >= InputCount) return;
            
            var layerData = _LayerDatas[index];
            layerData.weight = weight;
            _layerMixer.SetInputWeight(index, weight);
        }

        public void SetLayerAvatarMask(int index, AvatarMask mask)
        {
            if (index < 0 || index >= InputCount) return;

            var layerData = _LayerDatas[index];
            layerData.avatarmask = mask;
            _layerMixer.SetLayerMaskFromAvatarMask((uint)index, mask);
        }

        // public override void AddInput(Playable playable)
        // {
        //     base.AddInput(playable);
        //     InputCount++;
        //     _layerMixer.SetInputCount(InputCount);
        //     _layerMixer.ConnectInput(InputCount, playable, 0, 0);
        // }
        
        // public void AddInput(AnimationClip clip, float enterTime = 0f)
        // {
        //     AddInput(new AnimPlayer(m_adapterPlayable.GetGraph(), clip, enterTime));
        // }
        
        public void AddInput(LayerData layerData)
        {
            _layerMixer.SetInputCount(InputCount + 1);
            _layerMixer.ConnectInput(InputCount, layerData.adapter.GetAdapterPlayable(), 0, 0);
            if (layerData.avatarmask != null)
            {
                _layerMixer.SetLayerMaskFromAvatarMask((uint)InputCount, layerData.avatarmask);
            }
            _layerMixer.SetInputWeight(InputCount, layerData.weight);
            _LayerDatas.Add(layerData);
            InputCount++;
        }
        
        public override void Enable()
        {
            base.Enable();

            if (InputCount <= 0) return;
            
            for (int i = 0; i < InputCount; i++)
            {
                var layerData = _LayerDatas[i];
                _layerMixer.SetInputWeight(i, layerData.weight);
                if (layerData.avatarmask != null)
                {
                    _layerMixer.SetLayerMaskFromAvatarMask((uint)i, layerData.avatarmask);
                }
                AnimHelper.Enable(_layerMixer, i);
            }
            
            _layerMixer.SetTime(0f);
            _layerMixer.Play();
            m_adapterPlayable.SetTime(0f);
            m_adapterPlayable.Play();
        }

        public override void Disable()
        {
            base.Disable();
            
            if (InputCount <= 0) return;

            for (int i = 0; i < InputCount; i++)
            {
                _layerMixer.SetInputWeight(i, 0f);
                AnimHelper.Disable(_layerMixer, i);
            }
            
            _layerMixer.Pause();
            m_adapterPlayable.Pause();
        }
    }

}