using System;
using System.Collections.Generic;
using System.Linq;
using Association;
using Shouldly;
using Xunit;

namespace Data_Test
{
    public class DataFields_Test
    {
        [Fact]
        public void FieldNameGeneration()
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
            
            var data = new DataFields(transactions);

            data.FieldNames.Count.ShouldBe(8);
        }
    }
}
