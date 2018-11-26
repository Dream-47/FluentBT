using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBehaviourTree
{
    //在这个类里面，利用stack类在虚拟内存中构建了一个虚拟行为树
    //依次进入Stack集合，进入前验明集合顶部的身份，确立与顶部节点的关系，子节点全部添加完删除Stack顶部节点，最终编制成虚拟行为树

    /// <summary>
    /// Fluent API for building a behaviour tree.
    /// </summary>
    public class BehaviourTreeBuilder
    {
        /// <summary>
        /// Last node created.
        /// </summary>
        private IBehaviourTreeNode curNode = null;

        /// <summary>
        /// Stack node nodes that we are build via the fluent API.
        /// </summary>
        private Stack<IParentBehaviourTreeNode> parentNodeStack = new Stack<IParentBehaviourTreeNode>();

        /// <summary>
        /// Create an action node.
        /// </summary>
        public BehaviourTreeBuilder Do(string name, Func<TimeData, BehaviourTreeStatus> fn)
        {
            if (parentNodeStack.Count <= 0)
            {
                throw new ApplicationException("Can't create an unnested ActionNode, it must be a leaf node.");
            }

            var actionNode = new ActionNode(name, fn);
            parentNodeStack.Peek().AddChild(actionNode);
            return this;
        }

        /// <summary>
        /// Like an action node... but the function can return true/false and is mapped to success/failure.
        /// </summary>
        public BehaviourTreeBuilder Condition(string name, Func<TimeData, bool> fn)
        {
            //其实在这里是新建了aciton 不过节点的委托返回只有bool false
            return Do(name, t => fn(t) ? BehaviourTreeStatus.Success : BehaviourTreeStatus.Failure);
        }

        /// <summary>
        /// Create an inverter node that inverts the success/failure of its children.
        /// </summary>
        public BehaviourTreeBuilder Inverter(string name)
        {
            var inverterNode = new InverterNode(name);

            if (parentNodeStack.Count > 0)
            {
                parentNodeStack.Peek().AddChild(inverterNode);
            }

            parentNodeStack.Push(inverterNode);
            return this;
        }

        /// <summary>
        /// Create a sequence node.
        /// </summary>
        public BehaviourTreeBuilder Sequence(string name)
        {
            var sequenceNode = new SequenceNode(name);

            if (parentNodeStack.Count > 0)
            {
                parentNodeStack.Peek().AddChild(sequenceNode);
            }

            parentNodeStack.Push(sequenceNode);
            return this;
        }

        /// <summary>
        /// Create a parallel node.
        /// </summary>
        public BehaviourTreeBuilder Parallel(string name, int numRequiredToFail, int numRequiredToSucceed)
        {
            var parallelNode = new ParallelNode(name, numRequiredToFail, numRequiredToSucceed);

            if (parentNodeStack.Count > 0)
            {
                parentNodeStack.Peek().AddChild(parallelNode);
            }

            parentNodeStack.Push(parallelNode);
            return this;
        }

        /// <summary>
        /// Create a selector node.
        /// </summary>
        public BehaviourTreeBuilder Selector(string name)
        {
            //新建一个节点
            var selectorNode = new SelectorNode(name);

            //找到当前堆栈集合的顶部，将当前节点添加到上一个节点的列表中
            if (parentNodeStack.Count > 0)
            {
                parentNodeStack.Peek().AddChild(selectorNode);
            }

            //入堆
            parentNodeStack.Push(selectorNode);
            return this;         
        }

        /// <summary>
        /// 创建一个权重选择节点
        /// </summary>
        public BehaviourTreeBuilder WeightedSelector(string name,List<float> chidrenWeight)
        {
            //新建一个权重节点
            var weightedSelectorNode = new WeightedSelectorNode(name,chidrenWeight);

            //找到当前堆栈集合的顶部，将当前节点添加到上一个节点的列表中
            if (parentNodeStack.Count > 0)
            {
                parentNodeStack.Peek().AddChild(weightedSelectorNode);
            }

            //权重节点加入集合
            parentNodeStack.Push(weightedSelectorNode);
            return this;
        }

        /// <summary>
        /// Splice a sub tree into the parent tree.
        /// </summary>
        public BehaviourTreeBuilder Splice(IBehaviourTreeNode subTree)
        {
            if (subTree == null)
            {
                throw new ArgumentNullException("subTree");
            }

            if (parentNodeStack.Count <= 0)
            {
                throw new ApplicationException("Can't splice an unnested sub-tree, there must be a parent-tree.");
            }

            parentNodeStack.Peek().AddChild(subTree);
            return this;
        }

        /// <summary>
        /// Build the actual tree.
        /// </summary>
        public IBehaviourTreeNode Build()
        {
            if (curNode == null)
            {
                throw new ApplicationException("Can't create a behaviour tree with zero nodes");
            }
            return curNode;
        }

        /// <summary>
        /// Ends a sequence of children.
        /// </summary>
        public BehaviourTreeBuilder End()
        {
            curNode = parentNodeStack.Pop();
            return this;
        }
    }
}
