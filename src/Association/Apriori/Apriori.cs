using System;
using System.Collections.Generic;
using System.Linq;

namespace Cotur.DataMining.Association
{
    public class Apriori
    {
        public DataFields Data { get; private set; }
        public List<CNode> CNodes { get; private set; } = null;
        public List<List<CNode>> EachLevelOfNodes { get; private set; } = new List<List<CNode>>();
        public List<AssociationRule> Rules { get; private set; }

        public Apriori(DataFields data)
        {
            if (data == null)
            {
                throw new Exception("Apriori object can not be created with null DataFields");
            }
            Data = data;
        }

        public void CalculateCNodes(float minSupport)
        {
            if (minSupport <= 0)
            {
                throw new Exception("Minimum support should be bigged than 0");
            }

            CNodes = null;
            EachLevelOfNodes = new List<List<CNode>>();
            Rules = null;
            _CalculateCNodes(minSupport);
        }

        private void _CalculateCNodes(float minSupport, List<CNode> cNodes = null)
        {
            CNodes = cNodes ?? CNodes;

            if (CNodes == null)
            {
                FirstCNodes();
            }

            foreach (var node in CNodes)
            {
                node.CalculateSupport(Data.Rows);
            }

            CNodes.RemoveAll(node => node.Support < minSupport);
            EachLevelOfNodes.Add(CNodes);
            
            if (CNodes.Count > 1 && CalculateNextStep(minSupport))
            {
                _CalculateCNodes(minSupport, this.CNodes);
            }

            Rules = AssociationRule.GetLastLevelRules(EachLevelOfNodes);
        }

        private void FirstCNodes()
        {
            CNodes = new List<CNode>();
            for (int i = 0; i < Data.Rows.First().Length; i++)
            {
                CNodes.Add(new CNode(i));
            }
        }

        private bool CalculateNextStep(float minSupport)
        {
            var tempList = new List<CNode>();

            if (CNodes.First().ElementIDs.Count == 1)
            {
                for (var i = 0; i < CNodes.Count; i++)
                {
                    for (var k = i + 1; k < CNodes.Count; k++)
                    {
                        var newNode = new CNode(CNodes[i].ElementIDs[0]);
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
                    var groupsCount = groups.Count();
                    for (var i = 0; i < groupsCount - 1; i++)
                    {
                        for (var k = i + 1; k < groupsCount; k++)
                        {
                            if (k == groupsCount)
                                break;

                            var newNode = groups.ElementAt(i).FullCopyMe();
                            newNode.AddElement(groups.ElementAt(k).ElementIDs.Last());
                            newNode.CalculateSupport(Data.Rows);

                            if (newNode.Support >= minSupport)
                                tempList.Add(newNode);
                        }
                    }
                }
            }

            if (tempList.Count >= 1)
            {
                //this.CNodes = CNode.ClearSame(tempList);// => this action deletes last cNode
                this.CNodes = tempList;
                return true;
            }
            return false;
        }
    }
}
