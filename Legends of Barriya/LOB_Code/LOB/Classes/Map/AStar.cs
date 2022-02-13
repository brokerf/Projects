using System;
using System.Collections.Generic;
using System.Linq;
using LOB.Classes.ObjectManagement.Actions;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using Microsoft.Xna.Framework;

namespace LOB.Classes.Map
{
    public sealed class Node : IComparable<Node>
    {
        internal float mCostToReach;
        internal float mCostToGoal;
        internal Point mPosition;
        internal Node mPredecessor;
        private readonly float mWeight;

        internal Node(float costToReach, Point position, float weight = 0)
        {
            mCostToReach = costToReach;
            mPosition = position;
            mWeight = weight;
        }

        internal void CalculateCost(Point point)
        {
            mCostToGoal = (float)Math.Sqrt(Math.Pow(mPosition.X - point.X, 2) + Math.Pow(mPosition.Y - point.Y, 2));
        }

        internal float CostToReach()
        {
            return mCostToReach + mWeight;
        }

        public int CompareTo(Node other)
        {
            if (mCostToReach + mCostToGoal < other.mCostToReach + other.mCostToGoal)
            {
                return -1;
            }
            return mCostToReach + mCostToGoal > other.mCostToReach + other.mCostToGoal ? 1 : 0;
        }
    }

    internal sealed class AStar
    {
        public readonly float mSideCost = (float)Math.Sqrt(2)-1;
        public readonly Dictionary<Point, List<Point>> mNeighbors;
        private readonly Dictionary<Point, List<Node>> mFilteredNeighbors;
        private readonly List<Point> mUnmovableObjectsLocations;
        private readonly int mBarrierStart;
        private readonly int mBarrierEnd;
        private readonly int mMapHeight;
        public readonly Dictionary<Point, float> mCostFromZeroToPoint = new Dictionary<Point, float>();

        internal AStar(int mapWidth, int mapHeight)
        {
            mMapHeight = mapHeight;
            mPortals = new []{new Dictionary<Point, Point>(), new Dictionary<Point, Point>()};
            mGates = new []{ new Dictionary<Point, (Point, Point)>(), new Dictionary<Point, (Point, Point)>() };
            (mBarrierStart, mBarrierEnd) = GameMap.GetBarrierWidthAndPosition(mapWidth);
            mBarrierEnd++;
            var unmovableObjects = DataStorage.mGameObjects.Where(obj => !obj.Value.mActions.OfType<Movement>().Any()).ToList();
            mUnmovableObjectsLocations = (from location in (from kvp in unmovableObjects select kvp.Value) select location.mObjectPosition).ToList();
            mNeighbors = new Dictionary<Point, List<Point>>();
            mFilteredNeighbors = new Dictionary<Point, List<Node>>();
            for (var x = 0; x < mapWidth; x++)
            {
                for (var y = 1; y <= mapHeight; y++)
                {
                    // All points on map
                    mNeighbors[new Point(x, y)] = new List<Point> {
                        new Point(x - 1, y),
                        new Point(x + 1, y),
                        new Point(x, y - 1),
                        new Point(x, y + 1),
                        new Point(x - 1, y-1),
                        new Point(x + 1, y+1),
                        new Point(x+1, y - 1),
                        new Point(x-1, y + 1)
                    }.Where(point => point.X >= 0 && point.Y > 0 && point.X < mapWidth && point.Y <= mapHeight).ToList();
                    
                    // Only points and neighbors without unmoving objects on them

                    if (mUnmovableObjectsLocations.Contains(new Point(x, y)))
                    {
                        continue;
                    }
                    mFilteredNeighbors[new Point(x, y)] = new List<Node>();
                    foreach (var valueTuple in mNeighbors[new Point(x, y)].Where(point => !mUnmovableObjectsLocations.Contains(point)))
                    {
                        var extraCost = 0f;
                        if (x != valueTuple.X && y != valueTuple.Y)
                        {
                            extraCost = mSideCost;
                        }
                        mFilteredNeighbors[new Point(x, y)].Add(new Node(0, valueTuple, extraCost));
                    }
                }
            }

            for (var x = -mapWidth+1; x < mapWidth; x++)
            {
                for (var y = -mapHeight+1; y < mapHeight; y++)
                {
                    mCostFromZeroToPoint[new Point(x, y)] = (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
                }
            }
        }

        internal readonly Dictionary<Point, Point>[] mPortals;
        private readonly Dictionary<Point, (Point, Point)>[] mGates;

        internal string SavePortals()
        {
            var data = string.Empty;
            foreach (var (key, value) in mPortals[0])
            {
                var index = mFilteredNeighbors[key].FindIndex(point => point.mPosition == value);
                data += "<" + index  + "|" + key.X + "," + key.Y + "|";
                data += value.X + "," + value.Y + ">";
            }
            data += ":";
            foreach (var (key, value) in mPortals[1])
            {
                var index = mFilteredNeighbors[key].FindIndex(point => point.mPosition == value);
                data += "<" + index + "|" + key.X + "," + key.Y + "|";
                data += value.X + "," + value.Y + ">";
            }
            return data;
        }

        internal void LoadPortals(string data)
        {
            for (var i = 0; i < 2; i++)
            {
                foreach (var points in data.Split(":")[i].Replace(">", string.Empty).Split("<"))
                {
                    if (points == string.Empty)
                    {
                        continue;
                    }
                    var keyValue = points.Split("|");
                    int.TryParse(keyValue[0], out var index);
                    var key = keyValue[1].Split(",");
                    var value = keyValue[2].Split(",");
                    var entrance = new Point();
                    int.TryParse(key[0], out entrance.X);
                    int.TryParse(key[1], out entrance.Y);
                    var exit = new Point();
                    int.TryParse(value[0], out exit.X);
                    int.TryParse(value[1], out exit.Y);
                    mPortals[i][entrance] = exit;
                    var exitNode = new Node(0, exit);
                    mFilteredNeighbors[entrance].Insert(index, exitNode);
                }
            }
        }

        public (Node, Node) OpenPortal(bool openingIsLeft)
        {
            var start = GetRandomPosition(openingIsLeft ? mBarrierStart - 1 : mBarrierStart + mBarrierEnd + 1);
            var exit = GetRandomPosition(!openingIsLeft ? mBarrierStart - 1 : mBarrierStart + mBarrierEnd + 1);
            mPortals[openingIsLeft ? 0 : 1][start] = exit;
            var entranceNode = new Node(0, start);
            var exitNode = new Node(0, exit);
            mFilteredNeighbors[start].Add(exitNode);
            mFilteredNeighbors[exit].Add(entranceNode);
            return (entranceNode, exitNode);
        }

        public void ClosePortal(Node entrancePosition, Node exitPosition, bool isPlayer)
        {
            foreach (var (_, gameObject) in DataStorage.mGameObjects)
            {
                if (entrancePosition == default || !gameObject.mObjectEvents.TryGetValue(EventType.MoveEvent, out var events) || events.Count <= 0 || ((MoveEvent)events[0]).mPath.Count <= 0)
                {
                    continue;
                }

                var path = ((MoveEvent)events[^1]).mPath;
                var i = path.FindIndex(point => point == entrancePosition.mPosition);
                if (i <= -1 || i >= path.Count)
                {
                    continue;
                }

                if (( i == 0 || path[i - 1] != exitPosition.mPosition) && (i + 1 == path.Count || path[i + 1] != exitPosition.mPosition))
                {
                    continue;
                }

                gameObject.mObjectEvents[EventType.MoveEvent] = new List<IEvent>();
                DataStorage.mUnitAnimationOffset[gameObject.mObjectPosition] = Vector2.Zero;
            }
            if (entrancePosition==default || !mFilteredNeighbors.ContainsKey(entrancePosition.mPosition))
            {
                return;
            }
            mFilteredNeighbors[entrancePosition.mPosition].Remove(exitPosition);
            mFilteredNeighbors[exitPosition.mPosition].Remove(entrancePosition);
            mPortals[isPlayer ? 0 : 1].Remove(entrancePosition.mPosition);
        }

        public void OpenGate(int wallSprite, Point position, bool isPlayer)
        {
            var sideOpening = wallSprite == 0;
            var start = position + (!sideOpening ? new Point(1, 0) : new Point(0, 1));
            var exit = position - (!sideOpening ? new Point(1, 0) : new Point(0, 1));

            mGates[isPlayer ? 0 : 1][position] = (exit, start);
        }

        public void OpenPoint(Point position, bool isPlayer)
        {
            mGates[isPlayer ? 0 : 1].Remove(position);
            mUnmovableObjectsLocations.Remove(position);
            var newNode = new Node(0, position);
            mFilteredNeighbors[position] = new List<Node>();
            foreach (var neighborPosition in mNeighbors[position].Where(neighborPosition => mFilteredNeighbors.ContainsKey(neighborPosition)))
            {
                mFilteredNeighbors[neighborPosition].Add(newNode);
                mFilteredNeighbors[position].Add(new Node(0, neighborPosition));
            }
        }

        public void ClosePoint(Point position, bool isPlayer, bool overrideGate = false)
        {
            mUnmovableObjectsLocations.Add(position);
            if (mGates[isPlayer ? 0 : 1].ContainsKey(position) && !overrideGate)
            {
                return;
            }
            mGates[isPlayer ? 0 : 1].Remove(position);
            if (!mFilteredNeighbors.ContainsKey(position))
            {
                return;
            }
            var neighbors = mFilteredNeighbors[position];
            foreach (var neighbor in neighbors)
            {
                var node = mFilteredNeighbors[neighbor.mPosition].FirstOrDefault(oldNeighbor => oldNeighbor.mPosition == position);
                if (node != default)
                {
                    mFilteredNeighbors[neighbor.mPosition].Remove(node);
                }
            }
            mFilteredNeighbors.Remove(position);
        }

        private Point GetRandomPosition(int xValue)
        {
            var random = new Random();
            var point = new Point(xValue, random.Next(1, mMapHeight));
            while (mPortals[0].ContainsKey(point) || mPortals[0].ContainsValue(point) || mPortals[1].ContainsKey(point) || mPortals[1].ContainsValue(point))
            {
                point.Y--;
                if (point.Y < 1)
                {
                    point.Y = mMapHeight;
                }
            }

            return point;
        }

        private List<Point> FinderLoop(ICollection<Point> closedQueue, Node targetNode, Node addedTargetNode, Node startNode)
        {
            var openQueue = new List<Node> { startNode };
            while (true)
            {
                var currentNode = openQueue[^1];
                openQueue.RemoveAt(openQueue.Count-1);
                if (currentNode.mPosition.X == addedTargetNode.mPosition.X && currentNode.mPosition.Y == addedTargetNode.mPosition.Y)
                {
                    addedTargetNode.mPosition = targetNode.mPosition;
                }
                if (currentNode.mPosition.X == targetNode.mPosition.X && currentNode.mPosition.Y == targetNode.mPosition.Y)
                {
                    return GetPath(currentNode);
                }
                
                closedQueue.Add(currentNode.mPosition);

                SortInNewNodes(GetNextNodes(currentNode, openQueue, closedQueue, addedTargetNode), openQueue);

                if (openQueue.Count == 0)
                {
                    return new List<Point>();
                }
            }
        }

        private void SortInNewNodes(List<Node> newNodes, List<Node> openQueue)
        {
            if (newNodes.Count == 0)
            {
                return;
            }

            var i = openQueue.Count - 1;
            var j = newNodes.Count - 1;
            newNodes = newNodes.OrderByDescending(node => node.CostToReach() + node.mCostToGoal).ToList();
            var cost = newNodes[j].CostToReach() + newNodes[j].mCostToGoal;
            while (i > -1)
            {
                if (cost <= openQueue[i].CostToReach() + openQueue[i].mCostToGoal)
                {
                    openQueue.Insert(i, newNodes[j]);
                    j--;
                    if (j < 0)
                    {
                        break;
                    }
                    cost = newNodes[j].CostToReach() + newNodes[j].mCostToGoal;
                }
                else
                {
                    i--;
                }
            }

            if (j > -1)
            {
                openQueue.InsertRange(0, newNodes.GetRange(0, j + 1));
            }
        }

        private List<Node> GetNextNodes(Node currentNode, ICollection<Node> openQueue, ICollection<Point> closedQueue, Node addedTargetNode)
        {
            var cost = currentNode.CostToReach() + 1;
            var newNodes = new List<Node>();
            if (!mFilteredNeighbors.TryGetValue(currentNode.mPosition, out var points))
            {
                return newNodes;
            }
            foreach (var point in points.ToList())
            {
                if (point == null || closedQueue.Contains(point.mPosition))
                {
                    continue;
                }

                var posNode = openQueue.FirstOrDefault(node => node.mPosition == point.mPosition);
                if (posNode != null)
                {
                    if (posNode.mCostToReach <= cost)
                    {
                        continue;
                    }

                    posNode.mCostToReach = cost;
                    posNode.mPredecessor = currentNode;
                    continue;
                }
                
                point.mCostToGoal =
                    mCostFromZeroToPoint[point.mPosition - addedTargetNode.mPosition];
                point.mPredecessor = currentNode;
                newNodes.Add(point);
            }
            return newNodes;
        }

        private List<Point> GetPortalsOnSide(bool unitIsOnTheRight, bool isPlayer)
        {
            return mPortals[isPlayer ? 0 : 1].Select(entranceExit =>
                unitIsOnTheRight ? entranceExit.Value.X >= mBarrierStart ? entranceExit.Value : entranceExit.Key :
                entranceExit.Value.X < mBarrierStart ? entranceExit.Value : entranceExit.Key).ToList();
        }

        // Used so you don't have to recalculate the same data when sending multiple units at once
        private List<Point> FindPathMulti(Node startNode,
            Node targetNode,
            bool isPlayer,
            List<Point> closedQueue,
            List<Point> portalsOnSide, bool unitIsOnTheRight)
        {
            var addedTargetNode = new Node(0, targetNode.mPosition);
            var targetIsOnTheRight = targetNode.mPosition.X > mBarrierStart + 1;


            //closedQueue.AddRange(mGatePerTeamLocations[!isPlayer ? 0 : 1]);

            if (targetIsOnTheRight == unitIsOnTheRight)
            {
                return FinderLoop(closedQueue, targetNode, targetNode, startNode);
            }

            if (mPortals[isPlayer ? 0 : 1].Count <= 0)
            {
                return new List<Point>();
            }

            closedQueue.RemoveAll(point => mPortals[isPlayer ? 0 : 1].ContainsKey(point) || mPortals[isPlayer ? 0 : 1].ContainsValue(point));

            var firstPortal = portalsOnSide[0];
            addedTargetNode.mPosition = firstPortal;
            var min = firstPortal.Y;
            foreach (var portal in portalsOnSide)
            {
                var distance = Math.Abs(startNode.mPosition.Y - portal.Y);
                if (distance >= min)
                {
                    continue;
                }
                min = distance;
                addedTargetNode.mPosition = portal;
            }
            return FinderLoop(closedQueue, targetNode, addedTargetNode, startNode);
        }

        internal List<Point> FindPath(Node startNode, Node targetNode, IEnumerable<Point> objectPositions, bool isPlayer)
        {
            var closedQueue = objectPositions.Where(position => !mUnmovableObjectsLocations.Contains(position)).ToList();
            closedQueue.AddRange(mPortals[!isPlayer ? 0 : 1].Values);
            closedQueue.AddRange(mPortals[!isPlayer ? 0 : 1].Keys);

            var unitIsOnTheRight = startNode.mPosition.X > mBarrierStart + 1;
            var portalsOnSide = GetPortalsOnSide(unitIsOnTheRight, isPlayer);

            var m = new List<((Point, Point), (Node, Node))>();
            foreach (var points in mGates[isPlayer ? 0 : 1].Values)
            {
                if (!mFilteredNeighbors.ContainsKey(points.Item1) || !mFilteredNeighbors.ContainsKey(points.Item2))
                {
                    // A building blocks the gate
                    continue;
                }
                var nodes = (new Node(0, points.Item2), new Node(0, points.Item1));
                m.Add((points, nodes));
                mFilteredNeighbors[points.Item1].Add(nodes.Item1);
                mFilteredNeighbors[points.Item2].Add(nodes.Item2);
            }

            var path = FindPathMulti(startNode, targetNode, isPlayer, closedQueue, portalsOnSide, unitIsOnTheRight);

            foreach (var points in m)
            {
                mFilteredNeighbors[points.Item1.Item1].Remove(points.Item2.Item1);
                mFilteredNeighbors[points.Item1.Item2].Remove(points.Item2.Item2);
            }

            /*
             * 
            foreach (var points in m)
            {
                var works = mFilteredNeighbors[points.Item1.Item1].Remove(points.Item2.Item1);
                var works2 = mFilteredNeighbors[points.Item1.Item2].Remove(points.Item2.Item2);
            }
             */

            return path;
        }
        
        internal List<List<Point>> FindPath(List<int> objectIDs, Node targetNode, List<Point> objectPositions, bool isPlayer)
        {
            var startPoints = new List<Node>();
            var targetNodes = GetTargets(targetNode, objectIDs.Count, objectPositions);

            foreach (var objectId in objectIDs)
            {
                startPoints.Add(new Node(0, DataStorage.GetObject(objectId).mObjectPosition));
                objectPositions.Remove(startPoints[^1].mPosition);
            }

            targetNodes = targetNodes.OrderBy(node => node.mPosition.X).ToList();

            var closedQueue = objectPositions.Where(position => !mUnmovableObjectsLocations.Contains(position)).ToList();
            closedQueue.AddRange(mPortals[!isPlayer ? 0 : 1].Values);
            closedQueue.AddRange(mPortals[!isPlayer ? 0 : 1].Keys);
            
            var portals = new Dictionary<bool, List<Point>>();
            var paths = new List<List<Point>>();

            var m = new List<((Point, Point), (Node, Node))>();
            foreach (var points in mGates[isPlayer ? 0 : 1].Values)
            {
                if (!mFilteredNeighbors.ContainsKey(points.Item1) || !mFilteredNeighbors.ContainsKey(points.Item2))
                {
                    // Something blocks the gate
                    continue;
                }
                var nodes = (new Node(0, points.Item2), new Node(0, points.Item1));
                m.Add((points, nodes));
                mFilteredNeighbors[points.Item1].Add(nodes.Item1);
                mFilteredNeighbors[points.Item2].Add(nodes.Item2);
            }

            for (var i = 0; i < targetNodes.Count; i++)
            {
                var unitIsOnTheRight = startPoints[i].mPosition.X > mBarrierStart + 1;
                if (!portals.TryGetValue(unitIsOnTheRight, out var portalsOnSide))
                {
                    portalsOnSide = GetPortalsOnSide(unitIsOnTheRight, isPlayer);
                    portals[unitIsOnTheRight] = portalsOnSide;
                }

                paths.Add(FindPathMulti(startPoints[i], targetNodes[i], isPlayer, closedQueue.ToList(), portalsOnSide, unitIsOnTheRight));
            }

            foreach (var points in m.Where(points => mFilteredNeighbors.ContainsKey(points.Item1.Item1) && mFilteredNeighbors.ContainsKey(points.Item1.Item2)))
            {
                mFilteredNeighbors[points.Item1.Item1].Remove(points.Item2.Item1);
                mFilteredNeighbors[points.Item1.Item2].Remove(points.Item2.Item2);
            }

            return paths;
        }
        
        private IEnumerable<Point> GetTargetsInt(Node targetNode, int amountSelected, ICollection<Point> objectPositions)
        {
            var neighbors = new List<Point> { targetNode.mPosition };
            var foundEnough = false;
            var i = 0;

            while (!foundEnough)
            {
                if (neighbors.Count <= i)
                {
                    break;
                }
                var newNeighbors = mNeighbors[neighbors[i]].Where(neighbor => !neighbors.Contains(neighbor)).ToList();
                neighbors.AddRange(newNeighbors);
                if (neighbors.Count >= amountSelected)
                {
                    // The || excludes all spaces surrounded by objects / inaccessible
                    var testNeighbors = neighbors.Where(neighbor => !objectPositions.Contains(neighbor) && !mNeighbors[neighbor].TrueForAll(objectPositions.Contains)).ToList();
                    if (testNeighbors.Count >= amountSelected)
                    {
                        foundEnough = true;
                        neighbors = testNeighbors;
                    }
                }
                i++;
            }
            
            neighbors = neighbors.GetRange(0, Math.Min(amountSelected, neighbors.Count));
            return neighbors;
        }

        internal IEnumerable<Point> GetTargets(Point targetPosition, int amountSelected, ICollection<Point> objectPositions)
        {
            return GetTargetsInt(new Node(0, targetPosition), amountSelected, objectPositions);
        }

        private List<Node> GetTargets(Node targetNode, int amountSelected, ICollection<Point> objectPositions)
        {
            var targets = new List<Node>();
            var neighbors = GetTargetsInt(targetNode, amountSelected, objectPositions);
            foreach (var neighborNode in neighbors.Select(neighbor => new Node(0, neighbor)))
            {
                neighborNode.CalculateCost(targetNode.mPosition);
                targets.Add(neighborNode);
            }
            targets = targets.OrderBy(neighbor => neighbor.mCostToGoal).ToList()
                .GetRange(0, Math.Min(amountSelected, targets.Count));
            return targets;
        }

        private static List<Point> GetPath(Node currentNode)
        {
            var path = new List<Point>();
            while (currentNode.mPredecessor != null)
            {
                path.Insert(0, currentNode.mPosition);
                currentNode = currentNode.mPredecessor;
            }
            
            return path;
        }
    }
}