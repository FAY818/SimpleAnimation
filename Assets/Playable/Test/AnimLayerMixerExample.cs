using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    public class AnimLayerMixerExample : MonoBehaviour
    {
        public List<LayerData> layerData;
        public List<AnimationClip> clips;
        private PlayableGraph _graph;
        private AnimLayerMixer _animLayerMixer;

        private void Start()
        {
            _graph = PlayableGraph.Create("AnimLayerMixerExample");
            _animLayerMixer = new AnimLayerMixer(_graph);

            if (layerData.Count != layerData.Count)
            {
                return;
            }
            
            for (int i = 0; i < layerData.Count; i++)
            {
                var layerData = this.layerData[i];
                layerData.adapter = new AnimPlayer(_graph, clips[i], 0);
                _animLayerMixer.AddInput(layerData);
            }
            
            AnimHelper.SetOutput(_graph, GetComponent<Animator>(), _animLayerMixer);
            AnimHelper.Start(_graph);
        }
        
        private void OnDisable()
        {
            _graph.Destroy();
        }
    }
}