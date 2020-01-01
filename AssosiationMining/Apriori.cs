using System.Collections.Generic;
using System.Linq;

namespace Cotur.DataMining.AssociationMining
{
    public class DataFields
    {
        public List<string> FieldNames { get; private set; }

        public List<bool[]> Rows { get; private set; }

        public DataFields(List<string> FieldNames, List<bool[]> Rows)
        {
            this.FieldNames = FieldNames;
            this.Rows = Rows;
        }

        public DataFields(List<bool[]> Rows)
        {
            this.Rows = Rows;
            FillNames();
        }

        public DataFields(int FieldCount, List<List<int>> Transactions, List<string> FieldNames = null)
        {
            this.Rows = new List<bool[]>();
            foreach (List<int> transaction in Transactions)
                this.Rows.Add(CreateRow(FieldCount, transaction));

            if (FieldNames == null)
                FillNames();
            else
                this.FieldNames = FieldNames;
        }

        public DataFields(int FieldCount, List<int[]> Transactions, List<string> FieldNames = null)
        {
            this.Rows = new List<bool[]>();
            foreach (int[] transaction in Transactions)
                this.Rows.Add(CreateRow(FieldCount, transaction));

            if (FieldNames == null)
                FillNames();
            else
                this.FieldNames = FieldNames;
        }

        private bool[] CreateRow(int size, List<int> idS)
        {
            bool[] row = new bool[size];
            foreach (int id in idS)
                row[id] = true;

            return row;
        }
        private bool[] CreateRow(int size, int[] idS)
        {
            bool[] row = new bool[size];
            foreach (int id in idS)
                row[id] = true;

            return row;
        }

        private void FillNames()
        {
            for (int i = 0; i < Rows.First().Length; i++)
            {
                FieldNames.Add("Column " + i);
            }
        }

        public string GetElementName(int ElementID)
        {
            return FieldNames[ElementID];
        }

        public List<string> GetElementsName(List<int> ElementIDs)
        {
            return FieldNames.Where((e, i) => ElementIDs.Contains(i)).ToList();
        }

        public bool[] GetOneRow(int RowID)
        {
            return Rows[RowID];
        }
    }
    public class Apriori
    {
        public DataFields Data;
        public List<CNode> CNodes = null;
        public List<List<CNode>> EachLevelOfNodes = new List<List<CNode>>();
        public List<AssociationRule> Rules;
        public Apriori(DataFields Data)
        {
            this.Data = Data;
        }
        public void CalculateCNodes(float minSupport)
        {
            this.CNodes = null;
            this.EachLevelOfNodes = new List<List<CNode>>();
            this.Rules = null;
            _CalculateCNodes(minSupport);
        }
        private void _CalculateCNodes(float minSupport, List<CNode> cNodes = null)
        {
            this.CNodes = cNodes ?? this.CNodes;

            if(this.CNodes == null)
            {
                FirstCNodes();
            }

            foreach (CNode node in this.CNodes)
            {
                node.CalculateSupport(Data.Rows);
            }

            CNodes.RemoveAll(node => node.Support < minSupport);

            EachLevelOfNodes.Add(CNodes);


            if (this.CNodes.Count > 1 && CalculateNextStep(minSupport))
                _CalculateCNodes(minSupport, this.CNodes);

            Rules = AssociationRule.GetLastLevelRules(EachLevelOfNodes);
        }
        private void FirstCNodes()
        {
            this.CNodes = new List<CNode>();
            for (int i = 0; i < Data.Rows.First().Length; i++)
            {
                this.CNodes.Add(new CNode(i));
            }
        }
        private bool CalculateNextStep(float minSupport)
        {
            List<CNode> tempList = new List<CNode>();

            if(CNodes.First().ElementIDs.Count == 1)
            {
                for (int i = 0; i < CNodes.Count; i++)
                {
                    for (int k = i + 1; k < CNodes.Count; k++)
                    {
                        CNode newNode = new CNode(CNodes[i].ElementIDs[0]);
                        newNode.AddElement(CNodes[k].ElementIDs[0]);
                        newNode.CalculateSupport(Data.Rows);
                        tempList.Add(newNode);
                    }
                }
            }
            else
            {
                foreach (var groups in CNodes.GroupByElements(CNodes.First().ElementIDs.Count - 1))
                {
                    int groupsCount = groups.Count();
                    for (int i = 0; i < groupsCount - 1; i++)
                    {
                        for (int k = i + 1; k < groupsCount; k++)
                        {
                            if (k == groupsCount)
                                break;

                            CNode newNode = groups.ElementAt(i).FullCopyMe();
                            newNode.AddElement(groups.ElementAt(k).ElementIDs.Last());
                            newNode.CalculateSupport(Data.Rows);

                            if (newNode.Support >= minSupport)
                                tempList.Add(newNode);
                        }
                    }
                }
            }

            if(tempList.Count >= 1)
            {
                //this.CNodes = CNode.ClearSame(tempList);// => this action deletes last cNode
                this.CNodes = tempList;
                return true;
            }
            return false;
        }
    }
    public class CNode
    {
        public List<int> ElementIDs;
        public float Support;

        private CNode()
        {
            this.ElementIDs = new List<int>();
            this.Support = 0;
        }

        /// <summary>
        /// The Column Id of one element
        /// </summary>
        /// <param name="ElementID">An integer that element's id</param>
        public CNode(int ElementID)
        {
            this.ElementIDs = new List<int>();
            ElementIDs.Add(ElementID);
        }

        /// <summary>
        /// The column ids of elements
        /// </summary>
        /// <param name="ElementIDs">Integer list that contains elements ids</param>
        public CNode(List<int> ElementIDs)
        {
            this.ElementIDs = ElementIDs;
        }

        /// <summary>
        /// The column ids of elements
        /// </summary>
        /// <param name="ElementIDs">Integer array that contains elements ids</param>
        public CNode(int[] ElementIDs)
        {
            this.ElementIDs = ElementIDs.ToList();
        }

        public CNode AddElement(int ElementID)
        {
            ElementIDs.Add(ElementID);
            return this;
        }

        public void CalculateSupport(List<bool[]> warehouse)
        {
            Support = 0;
            foreach (bool[] row in warehouse)
            {
                bool confirmation = true;
                foreach (int id in ElementIDs)
                {
                    if(row[id] == false)
                    {
                        confirmation = false;
                        break;
                    }
                }

                if (confirmation)
                    Support++;
            }

            Support = (float)Support / (float)warehouse.Count;
        }

        public CNode FullCopyMe()
        {
            CNode newMe = new CNode();
            foreach (int item in ElementIDs)
            {
                newMe.ElementIDs.Add(item);
            }
            return newMe;
        }


        public static List<CNode> ClearSame(List<CNode> lst)
        {
            List<int> sameElems = new List<int>();
            int id = 0;
            foreach (CNode item in lst)
            {
                id = lst.IndexOf(item);
                if (sameElems.IndexOf(id) != -1)
                    continue;

                for (int i = id + 1; i < lst.Count; i++)
                {
                    if (item == lst[i])
                    {
                        if (sameElems.IndexOf(i) != -1)
                            sameElems.Add(i);
                    }
                }
            }

            foreach (int x in sameElems)
                lst.RemoveAt(x);
            

            return lst;
        }

        public static bool operator ==(CNode x, CNode y)
        {
            if (x.ElementIDs.Count != y.ElementIDs.Count)
                return false;

            bool same = true;
            for (int i = 0; i < x.ElementIDs.Count; i++)
            {
                if (x.ElementIDs[i] != y.ElementIDs[i])
                {
                    same = false;
                    break;
                }
            }
            return same;
        }
        public static bool operator !=(CNode x, CNode y)
        {
            return !(x == y);
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
    public class AssociationRule
    {
        public CNode NodeAB;
        public CNode NodeA;
        public CNode NodeB;

        public float Confidence;
        public float Lift;
        public float Conviction;
        public float Leverage;
        public float Coverage;

        
        public AssociationRule(CNode NodeAB, CNode NodeA, CNode NodeB)
        {
            this.NodeAB = NodeAB;
            this.NodeA = NodeA;
            this.NodeB = NodeB;
        }

        public AssociationRule Calculate()
        {
            CalculateConfidence();
            CalculateLift();
            CalculateConviction();
            CalculateLeverage();
            CalculateCoverage();
            return this;
        }
        private void CalculateConfidence()
        {
            this.Confidence = (float)NodeAB.Support / (float)NodeA.Support;
        }
        private void CalculateLift()
        {
            //this.Lift = (float)NodeAB.Support / ((float)NodeA.Support * (float)NodeB.Support);
            this.Lift = Confidence / NodeB.Support;
        }
        private void CalculateConviction()
        {
            this.Conviction = (1 - NodeB.Support) / (1 - Confidence);
        }
        private void CalculateLeverage()
        {
            this.Leverage = (float)NodeAB.Support - (float)(NodeA.Support * NodeB.Support);
        }
        private void CalculateCoverage()
        {
            this.Coverage = NodeA.Support;
        }


        private string GetCalc()
        {
            return string.Format("Confidence: {0}, Lift: {1}, Conviction: {2}, Leverage: {3}, Coverage: {4}",
                Confidence, Lift, Conviction, Leverage, Coverage);
        }
        public string ToString(DataFields data)
        {
            return NodeA.ToString(data) + " => " + NodeB.ToString(data) + " || " + GetCalc();
        }

        public static List<AssociationRule> GetAllRules(List<List<CNode>> EachLevelOfCNodes)
        {
            List<AssociationRule> Rules = new List<AssociationRule>();

            foreach (List<CNode> CNodeLevel in EachLevelOfCNodes)
            {
                if (CNodeLevel.First().ElementIDs.Count <= 1)
                    continue;

                foreach (CNode Node in CNodeLevel)
                {
                    Rules.AddRange(GetRules(Node, EachLevelOfCNodes));
                }
            }

            return Rules;
        }
        
        private static List<AssociationRule> GetRules(CNode Node, List<List<CNode>> EachLevelOfCNodes)
        {
            List<AssociationRule> Rules = new List<AssociationRule>();
            List<IEnumerable<int>> subsets = SubSetsOf<int>(Node.ElementIDs).ToList();
            
            CNode nodeA;
            CNode nodeB;
            bool aFound;
            bool bFound;
            
            for (int i = 1; i < (subsets.Count / 2); i++)
            {
                

                nodeA = new CNode(subsets[i].ToList());
                nodeB = new CNode(subsets[subsets.Count - i - 1].ToList());

                aFound = false;
                bFound = false;

                if (nodeA.ElementIDs.Count > 0 && nodeB.ElementIDs.Count > 0)
                {
                    foreach (CNode node in EachLevelOfCNodes[nodeA.ElementIDs.Count - 1])
                    {
                        if (!aFound && node == nodeA)
                        {
                            nodeA = node;
                            aFound = true;

                            if(bFound)
                                break;
                        }

                        if (!bFound && node == nodeB)
                        {
                            nodeB = node;
                            bFound = true;
                            if (aFound)
                                break;
                        }
                    }

                    if (aFound && bFound)
                    {
                        Rules.Add(new AssociationRule(Node, nodeA, nodeB).Calculate());
                        Rules.Add(new AssociationRule(Node, nodeB, nodeA).Calculate());
                    }
                }
            }

            //Rules.RemoveAll(x => x.Lift < (Rules.Max(a => a.Lift) * .9f));
            //return Rules.Take(5).ToList();

            return Rules;
        }

        public static List<AssociationRule> GetLastLevelRules(List<List<CNode>> EachLevelOfCNodes)
        {
            List<AssociationRule> Rules = new List<AssociationRule>();


            List<CNode> CNodeLevel = EachLevelOfCNodes.Last();
            
            if (CNodeLevel.First().ElementIDs.Count <= 1)
                return Rules;

            foreach (CNode Node in CNodeLevel)
            {
                Rules.AddRange(GetRules(Node, EachLevelOfCNodes));
            }

            return Rules;
        }
        private static IEnumerable<IEnumerable<T>> SubSetsOf<T>(IEnumerable<T> source)
        {
            if (!source.Any())
                return Enumerable.Repeat(Enumerable.Empty<T>(), 1);

            var element = source.Take(1);

            var haveNots = SubSetsOf(source.Skip(1));
            var haves = haveNots.Select(set => element.Concat(set));

            return haves.Concat(haveNots);
        }
    }
}
