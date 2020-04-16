using System.Collections.Generic;
using System.Linq;
using Data;

namespace Association
{
    public class CNode
    {
        public List<int> ElementIDs;
        public float Support;

        private CNode()
        {
            ElementIDs = new List<int>();
            Support = 0;
        }

        /// <summary>
        /// The Column Id of one element
        /// </summary>
        /// <param name="elementId">An integer that element's id</param>
        public CNode(int elementId)
        {
            ElementIDs = new List<int>(){ elementId };
        }

        /// <summary>
        /// The column ids of elements
        /// </summary>
        /// <param name="elementIDs">Integer IEnumerable that contains elements ids</param>
        public CNode(IEnumerable<int> elementIDs)
        {
            ElementIDs = elementIDs.ToList();
        }

        public CNode AddElement(int elementId)
        {
            ElementIDs.Add(elementId);
            return this;
        }

        public void CalculateSupport(List<bool[]> warehouse)
        {
            Support = 0;
            foreach (bool[] row in warehouse)
            {
                var confirmation = true;
                foreach (int id in ElementIDs)
                {
                    if (row[id] == false)
                    {
                        confirmation = false;
                        break;
                    }
                }

                if (confirmation)
                { 
                    Support++;
                }
            }

            Support = (float)Support / (float)warehouse.Count;
        }

        public CNode FullCopyMe()
        {
            var newNode = new CNode();
            foreach (var item in ElementIDs)
            {
                newNode.ElementIDs.Add(item);
            }
            return newNode;
        }

        public string ToString(DataFields data)
        {
            return string.Join(", ", data.GetElementsName(ElementIDs));
        }
    }

    static class CNodeExtensions
    {
        public static IEnumerable<IGrouping<IEnumerable<int>, CNode>> GroupByElements(this IEnumerable<CNode> nodes) =>
            nodes.GroupByElements(nodes.Min(node => node.ElementIDs.Count));

        public static IEnumerable<IGrouping<IEnumerable<int>, CNode>> GroupByElements(this IEnumerable<CNode> nodes, int count) =>
            nodes.GroupBy(node => node.ElementIDs.Take(count), new SequenceCompare());

        private class SequenceCompare : IEqualityComparer<IEnumerable<int>>
        {
            public bool Equals(IEnumerable<int> x, IEnumerable<int> y) => x.SequenceEqual(y);

            public int GetHashCode(IEnumerable<int> obj)
            {
                unchecked
                {
                    var hash = 17;
                    foreach (var i in obj)
                        hash = hash * 23 + i.GetHashCode();
                    return hash;
                }
            }
        }
    }
}