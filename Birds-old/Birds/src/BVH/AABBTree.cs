using Birds.src.bounding_areas;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Birds.src.BVH
{
    public class AABBTree
    {
        public Vector2 Position { get { return root.Position; } }
        public float Radius { get { return root.Radius; } }
        public AABBNode root;
        private Stack<AABBNode> freeNodes = new();
        private HashSet<IEntity> worldEntities = new();
        public List<(IEntity, IEntity)> CollissionPairs = new();

        public AABBTree()
        {
        }

        public void Add(IEntity we)
        {
            AABBNode leaf = AllocateLeafNode(we);
            if (root == null)
            {
                root = leaf;
                return;
            }
            // Stage 1: find the best sibling for the new leaf
            AABBNode bestSibling = FindBestSibling(leaf);

            // Stage 2: create a new parent

            AABBNode oldParent = bestSibling.Parent;
            AABBNode newParent = AllocateNode();
            newParent.Parent = oldParent;
            //newParent.BoundingCircle = Union(box, tree.nodes[sibling].box); done in stage 3
            if (oldParent != null)
            {
                if (oldParent.children[0] == bestSibling)
                    oldParent.children[0] = newParent;
                else
                    oldParent.children[1] = newParent;
            }
            else
            {
                root = newParent;
            }
            newParent.Add(bestSibling);
            newParent.Add(leaf);
            // Stage 3: walk back up the tree refitting AABBs
            AABBNode parent = newParent;
            do
            {
                parent.RefitBoundingBox();
                parent = parent.Parent;
            } while (parent != null);
        }

        public void UpdateTree(List<IEntity> newEntities){
            worldEntities.Clear();
            root = CreateTreeTopDown_Median(null, newEntities);
        }
        //for root: parent = null, newEntities is worldEntities
        public AABBNode CreateTreeTopDown_Median(AABBNode parent, List<IEntity> newEntities)
        {
            //step 0: HANDLE EDGE-CASES
            if (parent == null)
                UnravelTree();

            if (newEntities.Count == 0)
                return null;

            if (newEntities.Count == 1)
            {
                return AllocateLeafNode(newEntities[0]);
            }
            //TODO: if node==root, remove current tree if it exists and add worldEntities to newEntities


            //step 1: DECIDE WHAT AXIS TO SPLIT
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            foreach (IEntity we in newEntities)
            {
                if (we.Position.X > maxX)
                    maxX = we.Position.X;
                else if (we.Position.X < minX)
                    minX = we.Position.X;
                if (we.Position.Y > maxY)
                    maxY = we.Position.Y;
                else if (we.Position.Y < minY)
                    minY = we.Position.Y;

            }
            bool splitOnX = (maxX - minX) > (maxY - minY);

            //step 2: SPLIT ON CHOSEN AXIS
            if (splitOnX)
                newEntities.Sort((we1, we2) => we1.Position.X.CompareTo(we2.Position.X));
            else
                newEntities.Sort((we1, we2) => we1.Position.Y.CompareTo(we2.Position.Y));
            AABBNode node = AllocateNode();
            node.Add(CreateTreeTopDown_Median(node, newEntities.GetRange(0, newEntities.Count / 2)));
            node.Add(CreateTreeTopDown_Median(node, newEntities.GetRange(newEntities.Count / 2, newEntities.Count / 2 + newEntities.Count % 2)));
            node.RefitBoundingBox();
            return node;
        }
        // newEntities order will be affected 
        public AABBNode CreateTreeTopDown_SAH(AABBNode parent, List<IEntity> newEntities)
        {
            //step 0: HANDLE EDGE-CASES
            if (parent == null)
                UnravelTree();

            if (newEntities.Count == 0)
                return null;

            if (newEntities.Count == 1)
            {
                return AllocateLeafNode(newEntities[0]);
            }
            //TODO: if node==root, remove current tree if it exists and add worldEntities to newEntities
            //step 0: SETUP
            float minCost = float.MaxValue;
            int minCostSplitIndex = 0;
            int minCostAxis = 0;
            //step 1: DECIDE WHAT SPLIT TO USE
            //ADD: sort along x-axis
            for (int axis = 0; axis < 2; axis++)//x=0, y=1
            {
                if(axis == 0)
                    newEntities.Sort((a, b) => a.Position.X.CompareTo(b.Position.X));
                else
                    newEntities.Sort((a, b) => a.Position.Y.CompareTo(b.Position.Y));
                for (int i = 0; i < newEntities.Count - 1; i++)
                {
                    IEntity[] entities = newEntities.ToArray();
                    AxisAlignedBoundingBox AABB1 = AxisAlignedBoundingBox.CombinedAABB(entities[0..i]);
                    float cost1 = AABB1.Area;
                    AxisAlignedBoundingBox AABB2 = AxisAlignedBoundingBox.CombinedAABB(entities[(i + 1)..(entities.Length - 1)]);
                    float cost2 = AABB2.Area;
                    float total = cost1 + cost2;
                    if (total < minCost)
                    {
                        minCostSplitIndex = i;
                        minCost = total;
                        minCostAxis = axis;
                    }
                    BoundingAreaFactory.AABBs.Append(AABB1);
                    BoundingAreaFactory.AABBs.Append(AABB2);
                }
            }

            //step 2: SPLIT ON CHOSEN AXIS
            if (minCostAxis == 0)
                newEntities.Sort((we1, we2) => we1.Position.X.CompareTo(we2.Position.X));
            AABBNode node = AllocateNode();
            node.Add(CreateTreeTopDown_Median(node, newEntities.GetRange(0, minCostSplitIndex+1)));
            node.Add(CreateTreeTopDown_Median(node, newEntities.GetRange(minCostSplitIndex+1, newEntities.Count-minCostSplitIndex-1)));
            node.RefitBoundingBox();
            return node;
        }

        private AABBNode AllocateLeafNode(IEntity we)
        {
            AABBNode leafNode = AllocateNode();
            leafNode.Entity = we;
            worldEntities.Add(we);
            return leafNode;
        }

        private AABBNode AllocateNode()
        {
            if (freeNodes.Count == 0)
                return new AABBNode();
            else
            {
                AABBNode freeNode = freeNodes.Pop();
                freeNode.Reset();
                return freeNode;
            }
        }

        public AABBNode FindBestSibling(AABBNode leafNew)
        {
            AABBNode bestSibling = root;
            AxisAlignedBoundingBox combinedBest = root.AABB.CombinedAABB(leafNew.AABB);
            float bestCost = combinedBest.Area;
            BoundingAreaFactory.AABBs.Append(combinedBest);
            PriorityQueue<AABBNode, float> queue = new();
            queue.Enqueue(root, 0);

            while (queue.Count > 0)
            {
                queue.TryDequeue(
                        out AABBNode currentNode,
                        out float inheritedCost
                );

                if (inheritedCost >= bestCost)
                    return bestSibling;
                AxisAlignedBoundingBox combined = currentNode.AABB.CombinedAABB(leafNew.AABB);
                float combinedArea = combined.Area;
                BoundingAreaFactory.AABBs.Append(combined);
                float currentCost = combinedArea + inheritedCost;
                if (currentCost < bestCost)
                {
                    bestSibling = currentNode;
                    bestCost = currentCost;
                }
                inheritedCost += combinedArea - currentNode.AABB.Area;
                float cLow = leafNew.AABB.Area + inheritedCost;
                if (cLow < bestCost)
                {
                    if (currentNode.children[0] != null)
                        queue.Enqueue(currentNode.children[0], inheritedCost);
                    if (currentNode.children[1] != null)
                        queue.Enqueue(currentNode.children[1], inheritedCost);
                }
            }
            return bestSibling;
        }

        public void Draw(SpriteBatch sb)
        {
            root.Draw(sb);
        }

        public void ResolveInternalCollissions()
        {
            HashSet<IEntity> entities = new();
            foreach ((IEntity, IEntity) pair in CollissionPairs)
            {
                entities.Add(pair.Item1);
                entities.Add(pair.Item2);
            }
            foreach (IEntity e in entities)
                if(e is WorldEntity we)
                    we.GenerateAxes();
            foreach ((IEntity, IEntity) pair in CollissionPairs)
            {
                if (pair.Item1.CollidesWith(pair.Item2))
                {
                    pair.Item1.Collide(pair.Item2);
                    pair.Item2.Collide(pair.Item1);
                }
            }
            CollissionPairs.Clear();
        }

        private void RebuildTree()
        {
            UnravelTree();
            foreach (WorldEntity we in worldEntities)
                Add(we);
        }
        private void UnravelTree()
        {
            if (root != null)
            {
                Stack<AABBNode> nodesToRemove = new();
                nodesToRemove.Push(root);
                while (nodesToRemove.Count > 0)
                {
                    AABBNode currentNode = nodesToRemove.Pop();
                    foreach (AABBNode child in currentNode.children)
                    {
                        if (child != null)
                        {
                            if (child.Entity == null)
                                nodesToRemove.Push(child);
                        }
                    }
                    freeNodes.Push(currentNode);
                }
                root = null;
            }
        }

        public void Update(GameTime gameTime)
        {
            root.Update(gameTime);
            UpdateTree(worldEntities.ToList());
            GetInternalCollissions();
            ResolveInternalCollissions();
        }
        public void GetInternalCollissions(){
            //if(root != null)
                root.GetInternalCollissions(CollissionPairs);
        }
    }

}