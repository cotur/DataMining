using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Association;
using Shouldly;
using Xunit;

namespace Association
{
    public class Apriori_Test
    {
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

            var data = new DataFields(maxCol, transactions);

            var myApriori = new Apriori(data);
            var minimumSupport = 0.4f; // %40 Minimum Support

            myApriori.CalculateCNodes(minimumSupport);

            myApriori.Rules.ShouldNotBeNull();
            myApriori.Rules.Count.ShouldNotBe(0);
            myApriori.Rules.Count(x => x.Confidence >= .7f).ShouldBe(12);
        }

        [Fact]
        public void Apriori_Market_Basket_One()
        {
            var dataFields = DataFields.ReadFromFile(@"..\..\..\..\..\data\csv\small-market-basket.csv");

            var myApriori = new Apriori(dataFields);

            myApriori.CalculateCNodes(.4f);

            myApriori.Rules.ShouldNotBeNull();
            myApriori.Rules.Count.ShouldNotBe(0);
            myApriori.Rules.Count(x => x.Confidence >= .7f).ShouldBe(12);
        }

        [Fact]
        public void Apriori_Big()
        {
            var csvFilePath = @"..\..\..\..\..\data\csv\big-text-1.csv";

            var dataFields = DataFields.ReadFromFile(csvFilePath);

            var apriori = new Apriori(dataFields);

            apriori.CalculateCNodes(0.003f);
            apriori.Rules.Count.ShouldBeGreaterThan(0);
        }
    }
}
