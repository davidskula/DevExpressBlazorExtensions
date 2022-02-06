using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpressBlazorExtensions.Pages
{
    public interface ITreeViewCheckableContext
    {
        void RegisterNodeView(ITreeNodeView nodeView);
        void UnregisterNodeView(ITreeNodeView nodeView);
        void ToggleNodeState(string id);
    }
    
    public class TreeViewCheckableContext<TData> : ITreeViewCheckableContext
    {
        private readonly Dictionary<string, TreeNode> nodeDic = new Dictionary<string, TreeNode>();

        public void FillFromHierarchyData(IEnumerable<TData> items, Func<TData, string> idSelector, Func<TData, IEnumerable<TData>> childrenSelector)
        {
            nodeDic.Clear();
            FillFromHierarchyData(null, items, idSelector, childrenSelector);
        }
        
        private void FillFromHierarchyData(TreeNode parent, IEnumerable<TData> items, Func<TData, string> idSelector, Func<TData, IEnumerable<TData>> childrenSelector)
        {
            foreach (var item in items)
            {
                var id = idSelector(item);
                var treeNode = new TreeNode(id, item);
                treeNode.Parent = parent;
                FillFromHierarchyData(treeNode, childrenSelector(item), idSelector, childrenSelector);
                parent?.Children.Add(treeNode);
                nodeDic.Add(id, treeNode);
            }
        }

        public void FillFromFlatData(IEnumerable<TData> items, Func<TData, string> idSelector, Func<TData, string> parentIdSelector)
        {
            nodeDic.Clear();

            var unassignedChildren = new List<(string parentId, TreeNode treeNode)>();
            foreach (var item in items)
            {
                var id = idSelector(item);
                var parentId = parentIdSelector(item);
                var treeNode = new TreeNode(id, item);
                if (parentId != null)
                {
                    if (nodeDic.ContainsKey(parentId))
                        nodeDic[parentId].Children.Add(treeNode);
                    else 
                        unassignedChildren.Add((parentId, treeNode));
                }
                nodeDic.Add(id, treeNode);
            }

            foreach (var unassignedChild in unassignedChildren)
            {
                if (nodeDic.ContainsKey(unassignedChild.parentId))
                    nodeDic[unassignedChild.parentId].Children.Add(unassignedChild.treeNode);
                else
                    throw new Exception($"Parent id '{unassignedChild.parentId}' not found");
            }
        }

        public IEnumerable<TData> GetSelected()
        {
            return nodeDic.Values.Where(x => x.State == true).Select(x => x.Data);
        }

        public void RegisterNodeView(ITreeNodeView nodeView)
        {
            var node = nodeDic[nodeView.Id];
            node.NodeView = nodeView;
        }

        public void UnregisterNodeView(ITreeNodeView nodeView)
        {
            var node = nodeDic[nodeView.Id];
            node.NodeView = null;
        }

        public void ToggleNodeState(string id)
        {
            var node = nodeDic[id];
            if (node.State == true)
                node.State = false;
            else
                node.State = true;
            SetChildrenStateBasedOnParent(node);
            SetParentStateBasedOnChildren(node);
        }

        private void SetChildrenStateBasedOnParent(TreeNode node)
        {
            if (node.State == null)
                return;
            foreach (var child in node.Children)
            {
                child.State = node.State;
                SetChildrenStateBasedOnParent(child);
            }
        }
        
        private void SetParentStateBasedOnChildren(TreeNode node)
        {
            var parent = node.Parent;
            if (parent == null)
                return;
            
            var childrenTrue = parent.Children.Count(x => x.State == true);
            var childrenFalse = parent.Children.Count(x => x.State == false);
            var childrenAll = parent.Children.Count;
            
            bool? childrenState = null;
            if (childrenAll == childrenTrue)
                childrenState = true;
            else if (childrenAll == childrenFalse)
                childrenState = false;
            
            if (childrenState == parent.State)
                return;
            parent.State = childrenState;
            SetParentStateBasedOnChildren(node);
        }
        
        class TreeNode
        {
            public TreeNode(string id, TData data)
            {
                Id = id;
                Data = data;
            }
        
            public string Id { get; }
            public TreeNode Parent { get; set; }
            public IList<TreeNode> Children { get; } = new List<TreeNode>();
            public TData Data { get; }

            private bool? state = false;
            public bool? State
            {
                get => state;
                set
                {
                    if (state == value) return;
                    state = value;
                    NodeView?.SetState(state);
                }
            }

            private ITreeNodeView nodeView;
            public ITreeNodeView NodeView
            {
                get => nodeView;
                set
                {
                    nodeView = value;
                    nodeView?.SetState(State);
                }
            }
        }
    }

    public interface ITreeNodeView : IDisposable
    {
        string Id { get; }
        void SetState(bool? state);
    }
}