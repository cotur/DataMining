using System.Collections.Generic;
using System.Linq;

namespace Cotur.DataMining.Association
{
    public class AssociationRule
    {
        public CNode NodeAB { get; private set; }
        public CNode NodeA { get; private set; }
        public CNode NodeB { get; private set; }

        public float Confidence { get; private set; }
        public float Lift { get; private set; }
        public float Conviction { get; private set; }
        public float Leverage { get; private set; }
        public float Coverage { get; private set; }


        public AssociationRule(CNode nodeAb, CNode nodeA, CNode nodeB)
        {
            NodeAB = nodeAb;
            NodeA = nodeA;
            NodeB = nodeB;
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
            Confidence = (float)NodeAB.Support / (float)NodeA.Support;
        }
        
        private void CalculateLift()
        {
            //this.Lift = (float)NodeAB.Support / ((float)NodeA.Support * (float)NodeB.Support);
            Lift = Confidence / NodeB.Support;
        }
        
        private void CalculateConviction()
        {
            Conviction = (1 - NodeB.Support) / (1 - Confidence);
        }
        
        private void CalculateLeverage()
        {
            Leverage = (float)NodeAB.Support - (float)(NodeA.Support * NodeB.Support);
        }

        private void CalculateCoverage()
        {
            Coverage = NodeA.Support;
        }

        private string GetCalculationsAsString()
        {
            return
                $"Confidence: {Confidence}, Lift: {Lift}, Conviction: {Conviction}, Leverage: {Leverage}, Coverage: {Coverage}";
        }

        public string ToDetailedString(DataFields dataFields)
        {
            return NodeA.ToDetailedString(dataFields) + " => " + NodeB.ToDetailedString(dataFields) + " || " + GetCalculationsAsString();
        }

        public static List<AssociationRule> GetAllRules(List<List<CNode>> eachLevelOfCNodes)
        {
            var rules = new List<AssociationRule>();

            foreach (var cNodeLevel in eachLevelOfCNodes)
            {
                if (cNodeLevel.First().ElementIDs.Count <= 1)
                    continue;

                foreach (var node in cNodeLevel)
                {
                    rules.AddRange(GetRules(node, eachLevelOfCNodes));
                }
            }

            return rules;
        }

        private static List<AssociationRule> GetRules(CNode node, List<List<CNode>> eachLevelOfCNodes)
        {
            var rules = new List<AssociationRule>();
            var subsets = SubSetsOf<int>(node.ElementIDs).OrderBy(x => x.Count()).ToList();

            for (int i = 1; i < (subsets.Count / 2); i++)
            {
                var nodeA = new CNode(subsets[i].ToList());
                var nodeB = new CNode(subsets[subsets.Count - i - 1].ToList());

                var aFound = eachLevelOfCNodes[nodeA.ElementIDs.Count - 1].FirstOrDefault(x => x.ElementIDs.OrderBy(t => t).SequenceEqual(nodeA.ElementIDs.OrderBy(t => t)));
                var bFound = eachLevelOfCNodes[nodeB.ElementIDs.Count - 1].FirstOrDefault(x => x.ElementIDs.OrderBy(t => t).SequenceEqual(nodeB.ElementIDs.OrderBy(t => t)));

                if (aFound != null && bFound != null)
                {
                    rules.Add(new AssociationRule(node, aFound, bFound).Calculate());
                    rules.Add(new AssociationRule(node, bFound, aFound).Calculate());
                }
            }

            return rules;
        }

        public static List<AssociationRule> GetLastLevelRules(List<List<CNode>> eachLevelOfCNodes)
        {
            var rules = new List<AssociationRule>();


            var cNodeLevel = eachLevelOfCNodes.Last();

            if (cNodeLevel.First().ElementIDs.Count <= 1)
                return rules;

            foreach (var node in cNodeLevel)
            {
                rules.AddRange(GetRules(node, eachLevelOfCNodes));
            }

            return rules;
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