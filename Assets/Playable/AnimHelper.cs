﻿using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    /// <summary>
    /// 动画静态工具类
    /// </summary>
    public static class AnimHelper
    {
        #region Playables相关
        
        /// <summary>
        /// 连接Root并设置输出节点
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="animator"></param>
        /// <param name="behaviour"></param>
        public static void SetOutput(PlayableGraph graph, Animator animator, AnimBehaviour behaviour)
        {
            var root = new Root(graph);
            root.AddInput(behaviour);
            AnimationPlayableOutput.Create(graph, "Animation", animator).SetSourcePlayable(root.GetAdapterPlayable());
        }

        public static void Start(PlayableGraph graph)
        {
            GetAdapter(graph.GetOutputByType<AnimationPlayableOutput>(0).GetSourcePlayable()).Enable(); // 激活root节点 
            graph.Play();
        }
        public static void Start(PlayableGraph graph, AnimBehaviour root)
        {
            root.Enable();
            graph.Play();
        }

        /// <summary>
        /// 激活
        /// </summary>
        /// <param name="playable"></param>
        public static void Enable(Playable playable)
        {
            GetAdapter(playable)?.Enable();
        }
        public static void Enable(AnimationMixerPlayable mixer,int index)
        {
            Enable(mixer.GetInput(index));
        }
        public static void Disable(Playable playable)
        {
            GetAdapter(playable)?.Disable();
        }
        public static void Disable(AnimationMixerPlayable mixer, int index)
        {
            Disable(mixer.GetInput(index));
        }

        /// <summary>
        /// 获取playable对象上的AnimAdapter(行为类)
        /// Playable -> PlayableHandle -> IPlayableBehaviour
        /// </summary>
        /// <param name="playable"></param>
        /// <returns></returns>
        public static AnimAdapter GetAdapter(Playable playable)
        {
            // 检测 playable 对象的类型是否是AnimAdapter
            if (typeof(AnimAdapter).IsAssignableFrom(playable.GetPlayableType()))
            {
                return ((ScriptPlayable<AnimAdapter>)playable).GetBehaviour();
            }
            return null;
        }

        #endregion

        public static ComputeShader GetComputer(string name)
        {
            return Object.Instantiate(Resources.Load<ComputeShader>("Compute/" + name));
        }

        #region 其他动画处理

        public static AnimationCurve GetAnimCurve(AnimationClip clip,string curveName)
        {
            EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(clip);
            foreach (var curve in curves)
            {
                if(curve.propertyName == curveName)
                {
                    return AnimationUtility.GetEditorCurve(clip, curve);
                }
            }
            return null;
        }

        #endregion
    }
}
