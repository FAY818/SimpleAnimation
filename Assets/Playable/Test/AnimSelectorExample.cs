using UnityEngine;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    public class AnimSelectorExample : MonoBehaviour
    {
        public AnimationClip[] clips;
        public int index;
        public float remainTime;

        private PlayableGraph _graph;
        private AnimSelector _selector;

        private void Start()
        {
            _graph = PlayableGraph.Create();

            _selector = new AnimSelector(_graph);
            foreach (var clip in clips)
            {
                _selector.AddInput(clip, 0.2f);
            }

            AnimHelper.SetOutput(_graph, GetComponent<Animator>(), _selector);
            _selector.Select();
            AnimHelper.Start(_graph);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if(_selector.enable)
                {
                    _selector.Disable();
                }
                else
                {
                    _selector.Select(Select());
                    _selector.Enable();
                }
            }

            remainTime = _selector.remainTime;
        }

        private int Select()
        {
            index = ++index % clips.Length;
            return index;
        }

        private void OnDisable()
        { 
            _graph.Destroy();
        } 
    }
}
