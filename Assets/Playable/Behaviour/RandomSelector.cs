using UnityEngine;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    public class RandomSelector : AnimSelector
    {
        public RandomSelector(PlayableGraph graph) : base(graph)
        {
            
        }

        public override int Select()
        {
            if (ClipCount <= 1)
            {
                CurrentIndex = 0;
            }
            else
            {
                CurrentIndex = Random.Range(0, ClipCount);
            }
            
            return CurrentIndex;
        }
    } 
}
