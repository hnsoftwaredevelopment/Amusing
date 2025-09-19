// Plan / Pseudocode (detailed):
// 1. Provide a lightweight test double of Syncfusion's RuleModel so tests compile without requiring the Syncfusion package.
//    - Namespace: Syncfusion.Blazor.QueryBuilder
//    - Class: RuleModel with properties: string? Field, List<RuleModel>? Rules
//
// 2. Create xUnit test class `QueryBuilderHelperTests` in namespace `Amusing.Tests.Helpers`.
//    - Add tests for public methods from `Amusing.Helpers.QueryBuilderHelper`:
//      a) CollectFields(null) -> returns empty list
//         - Call CollectFields((RuleModel)null) and assert empty list.
//      b) CollectFields(single root with Field set, no children) -> returns list containing that field
//         - Create RuleModel with Field = "RootField", call CollectFields, assert contains "RootField".
//      c) CollectFields(wrapper root with only Rules populated -> collects nested fields)
//         - Create wrapper root with Field null and Rules containing children with Field values, including duplicates differing by case.
//         - Call CollectFields and verify distinct, case-insensitive result.
//      d) CollectFields(IEnumerable<RuleModel>) -> aggregates from multiple roots and deduplicates
//         - Create two separate root nodes with overlapping fields, call CollectFields(IEnumerable<RuleModel>), assert aggregated unique set.
//      e) DetermineQueryFromRules(null, any) -> throws ArgumentNullException
//         - Call DetermineQueryFromRules(null, "persons") and assert throws ArgumentNullException.
//
// 3. Use minimal assertions to verify behavior: Assert.Empty, Assert.Single, Assert.Contains, Assert.Equal with unordered comparison by sorting lists before comparing when needed.
//
// 4. Keep tests independent, deterministic and not reliant on external static helpers (we only test behaviors that don't call external QuerySelector/QueryBuilderSqlGenerator except null-argument check).
//
// Now the actual test code follows.

using System;
using System.Collections.Generic;
using System.Linq;
using Amusing.Helpers;
using Xunit;

namespace Syncfusion.Blazor.QueryBuilder
{
    // Lightweight test double for RuleModel so tests compile without Syncfusion package.
    public class RuleModel
    {
        public string? Field { get; set; }
        public List<RuleModel>? Rules { get; set; }
    }
}

namespace Amusing.Tests.Helpers
{
    public class QueryBuilderHelperTests
    {
        [Fact]
        public void CollectFields_NullRoot_ReturnsEmptyList()
        {
            // Arrange
            Syncfusion.Blazor.QueryBuilder.RuleModel? root = null;

            // Act
            List<string> result = QueryBuilderHelper.CollectFields(root);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void CollectFields_RootWithField_ReturnsThatField()
        {
            // Arrange
            var root = new Syncfusion.Blazor.QueryBuilder.RuleModel
            {
                Field = "RootField",
                Rules = null
            };

            // Act
            List<string> result = QueryBuilderHelper.CollectFields(root);

            // Assert
            Assert.Single(result);
            Assert.Contains("RootField", result);
        }

        [Fact]
        public void CollectFields_WrapperRootWithRules_CollectsNestedFieldsDistinctCaseInsensitive()
        {
            // Arrange: wrapper root has no Field but has Rules
            var child1 = new Syncfusion.Blazor.QueryBuilder.RuleModel { Field = "Festival" };
            var child2 = new Syncfusion.Blazor.QueryBuilder.RuleModel { Field = "festival" }; // duplicate differing by case
            var child3 = new Syncfusion.Blazor.QueryBuilder.RuleModel { Field = "Role" };
            var wrapper = new Syncfusion.Blazor.QueryBuilder.RuleModel
            {
                Field = null,
                Rules = new List<Syncfusion.Blazor.QueryBuilder.RuleModel> { child1, child2, child3 }
            };

            // Act
            List<string> result = QueryBuilderHelper.CollectFields(wrapper);

            // Assert: distinct ignoring case -> Festival and Role
            Assert.Equal(2, result.Count);
            // Compare case-insensitively
            Assert.Contains(result, s => string.Equals(s, "Festival", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(result, s => string.Equals(s, "Role", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void CollectFields_FromEnumerable_AggregatesAndDeduplicates()
        {
            // Arrange
            var r1 = new Syncfusion.Blazor.QueryBuilder.RuleModel { Field = "A" };
            var r2 = new Syncfusion.Blazor.QueryBuilder.RuleModel
            {
                Field = null,
                Rules = new List<Syncfusion.Blazor.QueryBuilder.RuleModel>
                {
                    new Syncfusion.Blazor.QueryBuilder.RuleModel { Field = "B" },
                    new Syncfusion.Blazor.QueryBuilder.RuleModel { Field = "a" } // duplicate of "A" in different case
                }
            };

            var roots = new List<Syncfusion.Blazor.QueryBuilder.RuleModel> { r1, r2 };

            // Act
            List<string> result = QueryBuilderHelper.CollectFields(roots);

            // Assert: should contain A and B only once each (case-insensitive)
            Assert.Equal(2, result.Count);
            Assert.Contains(result, s => string.Equals(s, "A", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(result, s => string.Equals(s, "B", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void DetermineQueryFromRules_NullRules_ThrowsArgumentNullException()
        {
            // Arrange
            Syncfusion.Blazor.QueryBuilder.RuleModel? rules = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => QueryBuilderHelper.DetermineQueryFromRules(rules!, "persons"));
        }
    }
}