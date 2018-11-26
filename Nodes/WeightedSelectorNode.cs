using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FluentBehaviourTree
{
    /// <summary>
    /// 带权重的选择节点
    /// </summary>
    public class WeightedSelectorNode : IParentBehaviourTreeNode
    {
        /// <summary>
        /// 节点名
        /// </summary>
        private string name;

        /// <summary>
        /// 子节点权重
        /// </summary>
        private List<float> childrenWeight;

        /// <summary>
        /// 子节点列表
        /// </summary>
        private List<IBehaviourTreeNode> children = new List<IBehaviourTreeNode>(); //todo: optimization, bake this to an array.

        public WeightedSelectorNode(string name,List<float> weight)
        {
            this.name = name;
            childrenWeight = weight;
        }

        public BehaviourTreeStatus Tick(TimeData time)
        {
            //根据子节点权重与子节点列表取出随机取出一个子节点
            var randomChild = ChooseObj(children, childrenWeight);
            
            //执行子节点的Tick函数并且返回执行状态
            var childStatus = randomChild.Tick(time);
            return childStatus;
        }

        /// <summary>
        /// Add a child node to the selector.
        /// </summary>
        public void AddChild(IBehaviourTreeNode child)
        {
            children.Add(child);
        }

        //choose random obj by weight
        private T ChooseObj<T>(List<T> tar, List<float> weight)
        {
            if (tar.Count != weight.Count)
            {
                Debug.LogError("match error");
            }

            //get totle value
            float totle = 0;
            for (int i = 0; i < weight.Count; i++)
            {
                totle += weight[i];
            }

            //get random index
            int randomIndex = 0;
            float random = Random.Range(0, totle);
            float left = 0;
            float right = weight[0];
            for (int i = 0; i < weight.Count; i++)
            {
                if (random >= left && random < right)
                {
                    randomIndex = i;
                    break;
                }
                left = left + weight[i];
                right = right + weight[i + 1];
            }
            return tar[randomIndex];
        }

     

    }
}
