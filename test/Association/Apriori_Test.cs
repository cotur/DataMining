using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Cotur.DataMining.Association.Apriori;
using Xunit;
using Xunit.Abstractions;

namespace Cotur.DataMining.Association
{
    public class Apriori_Test
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public Apriori_Test(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Apriori_One()
        {
            var transaction1 = new List<int>() { 0, 1, 2, 3 };
            var transaction2 = new List<int>() { 0, 2, 3, 4, 5, 6 };
            var transaction3 = new List<int>() { 1, 4, 5, 7 };
            var transaction4 = new List<int>() { 0, 1, 4, 5 };
            var transaction5 = new List<int>() { 0, 1, 2, 5, 7 };
            var transaction6 = new List<int>() { 0, 1, 3, 4, 5 };
            var transaction7 = new List<int>() { 0, 1, 5, 7 };

            var transactions = new List<List<int>>()
            {
                transaction1,
                transaction2,
                transaction3,
                transaction4,
                transaction5,
                transaction6,
                transaction7
            };

            var maxCol = transactions.Max(x => x.Max());

            var dataFields = new DataFields(maxCol, transactions);

            var myApriori = new Apriori.Apriori(dataFields);
            var minimumSupport = 0.4f; // %40 Minimum Support

            myApriori.CalculateCNodes(minimumSupport);

            myApriori.Rules.ShouldNotBeNull();
            myApriori.Rules.Count.ShouldNotBe(0);
            myApriori.Rules.Count(x => x.Confidence >= .7f).ShouldBe(12);

            _testOutputHelper.WriteLine("Top rules ordered by Confidence (Up to 10)");
            foreach (var associationRule in myApriori.Rules.OrderByDescending(x => x.Confidence).Take(10))
            {
                _testOutputHelper.WriteLine(associationRule.ToDetailedString(dataFields));
            }
        }

        [Fact]
        public void Apriori_Market_Basket_One()
        {
            var dataFields = DataFields.ReadFromCsv(@"..\..\..\..\..\data\csv\small-market-basket.csv");

            var myApriori = new Apriori.Apriori(dataFields);

            myApriori.CalculateCNodes(.4f);

            myApriori.Rules.ShouldNotBeNull();
            myApriori.Rules.Count.ShouldNotBe(0);
            myApriori.Rules.Count(x => x.Confidence >= .7f).ShouldBe(12);

            _testOutputHelper.WriteLine("Top rules ordered by Confidence (Up to 10)");
            foreach (var associationRule in myApriori.Rules.OrderByDescending(x => x.Confidence).Take(10))
            {
                _testOutputHelper.WriteLine(associationRule.ToDetailedString(dataFields));
            }
        }

        [Fact]
        public void Apriori_Big()
        {
            var csvFilePath = @"..\..\..\..\..\data\csv\big-text-1.csv";

            var dataFields = DataFields.ReadFromCsv(csvFilePath);

            var myApriori = new Apriori.Apriori(dataFields);

            myApriori.CalculateCNodes(0.003f);
            myApriori.Rules.Count.ShouldBeGreaterThan(0);

            _testOutputHelper.WriteLine("Top rules ordered by Confidence (Up to 10)");
            foreach (var associationRule in myApriori.Rules.OrderByDescending(x => x.Confidence).Take(10))
            {
                _testOutputHelper.WriteLine(associationRule.ToDetailedString(dataFields));
            }
        }
    }
}
