using UnityEngine;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    public class MixerExample : MonoBehaviour
    {
        public AnimationClip[] clips;
        //public bool canInteruptTransition = true;
        public float totalWeight;

        private PlayableGraph _graph;
        private AnimMixer _AnimMixer;

        private void Start()
        {
            _graph = PlayableGraph.Create();
            var animUnit1 = new AnimPlayer(_graph, clips[0], 0.1f);
            var animUnit2 = new AnimPlayer(_graph, clips[1], 0.1f);
            var animUnit3 = new AnimPlayer(_graph, clips[2], 0.1f);
            _AnimMixer = new AnimMixer(_graph);
            _AnimMixer.AddInput(animUnit1);
            _AnimMixer.AddInput(animUnit2);
            _AnimMixer.AddInput(animUnit3);

            AnimHelper.SetOutput(_graph, GetComponent<Animator>(), _AnimMixer);
            AnimHelper.Start(_graph);
        }

        private void Update()
        {
            if(Input.GetKey(KeyCode.X))
            {
                _AnimMixer.TransitionTo(0);
            }
            else if(Input.GetKey(KeyCode.C))
            {
                _AnimMixer.TransitionTo(1);
            }
            else if (Input.GetKey(KeyCode.V))
            {
                _AnimMixer.TransitionTo(2);
            }

            totalWeight = 0f;
            for (int i = 0; i < clips.Length; i++)
            {
                totalWeight += _AnimMixer.GetWeight(i);
            }
            if (totalWeight > 1f) 
                Debug.Log($"权重超出1: {totalWeight}");
        }

        private void OnDisable()
        {
            _graph.Destroy();
        }
    }
}
