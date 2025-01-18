using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    public class AnimBlendTreeExample : MonoBehaviour
    {
        public Vector2 pointer;
        public BlendClip2D[] clips;
        //public ComputeShader shader;

        private BlendTree2D _blend;
        private PlayableGraph _graph;

        private void Start()
        {
            _graph = PlayableGraph.Create();

            _blend = new BlendTree2D(_graph, clips);

            AnimHelper.SetOutput(_graph, GetComponent<Animator>(), _blend);
            AnimHelper.Start(_graph);
        }

        private void Update()
        {
            _blend.SetPointer(pointer);
        }

        private void OnDisable()
        {
            _graph.Destroy();
        }
    }
}