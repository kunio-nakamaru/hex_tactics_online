using System;
using System.Collections.Generic;
using Xunit;
using HexTacticsOnline.Lib;

namespace HexTacticsOnline.Lib.Test
{
    public class HexVector2Test
    {
        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            var vector = new HexVector2(1, 2);
            Assert.Equal(1, vector.X);
            Assert.Equal(2, vector.Y);
        }

        [Fact]
        public void OperatorPlus_ShouldAddVectorsCorrectly()
        {
            var vector1 = new HexVector2(1, 2);
            var vector2 = new HexVector2(3, 4);
            var result = vector1 + vector2;
            Assert.Equal(new HexVector2(4, 6), result);
        }

        [Fact]
        public void Equals_ShouldReturnTrueForEqualVectors()
        {
            var vector1 = new HexVector2(1, 2);
            var vector2 = new HexVector2(1, 2);
            Assert.True(vector1.Equals(vector2));
        }

        [Fact]
        public void Equals_ShouldReturnFalseForDifferentVectors()
        {
            var vector1 = new HexVector2(1, 2);
            var vector2 = new HexVector2(3, 4);
            Assert.False(vector1.Equals(vector2));
        }

        [Fact]
        public void GetRandomAround_ShouldReturnValidVector()
        {
            var vector = new HexVector2(1, 2);
            var exception = new List<HexVector2>();
            var result = vector.GetRandomAround(ref exception);
            Assert.NotEqual(HexVector2.Invalid, result.Vector2Int);
        }

        [Fact]
        public void SetUnAllocated_ShouldSetToInvalid()
        {
            var vector = new HexVector2(1, 2);
            vector.SetUnAllocated();
            Assert.Equal(HexVector2.Invalid, vector);
        }

        [Fact]
        public void OperatorEquals_ShouldReturnTrueForEqualVectors()
        {
            var vector1 = new HexVector2(1, 2);
            var vector2 = new HexVector2(1, 2);
            Assert.True(vector1 == vector2);
        }

        [Fact]
        public void OperatorNotEquals_ShouldReturnTrueForDifferentVectors()
        {
            var vector1 = new HexVector2(1, 2);
            var vector2 = new HexVector2(3, 4);
            Assert.True(vector1 != vector2);
        }

        [Fact]
        public void IsAllocated_ShouldReturnTrueForAllocatedVector()
        {
            var vector = new HexVector2(1, 2);
            Assert.True(vector.IsAllocated);
        }

        [Fact]
        public void IsAllocated_ShouldReturnFalseForUnallocatedVector()
        {
            var vector = HexVector2.Invalid;
            Assert.False(vector.IsAllocated);
        }

        [Fact]
        public void ToString_ShouldReturnCorrectFormat()
        {
            var vector = new HexVector2(1, 2);
            Assert.Equal("1:2", vector.ToString());
        }

        [Fact]
        public void GetPositionForDirection_ShouldReturnCorrectPosition()
        {
            var vector = new HexVector2(1, 2);
            var result = vector.GetPositionForDirection(0);
            Assert.Equal(new HexVector2(1, 3), result);
        }

        [Fact]
        public void GetClosestPositionForDestination_ShouldReturnClosestPosition()
        {
            var vector = new HexVector2(1, 2);
            var target = new HexVector2(3, 4);
            var possibilities = new List<HexVector2> { new HexVector2(2, 3), new HexVector2(3, 3) };
            var result = vector.GetClosestPositionForDestination(target, possibilities);
            Assert.Equal(new HexVector2(2, 3), result);
        }

        [Fact]
        public void HexDistance_ShouldReturnCorrectDistance()
        {
            var vector1 = new HexVector2(1, 2);
            var vector2 = new HexVector2(3, 4);
            var result = vector1.HexDistance(vector2);
            Assert.Equal(3, result);
        }

    }
}