﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using TagsCloudVisualization.Core;

namespace TagsCloudVisualization.Tests
{
    [TestFixture]
    class CircularCloudLayouterTests
    {
        private readonly Point center = new Point(500, 500);
        private CircularCloudLayouter circularCloudLayouter;

        [SetUp]
        public void SetUp()
        {
            circularCloudLayouter = new CircularCloudLayouter(center);
        }

        [TearDown]
        public void TearDown()
        {
            if (!Directory.Exists($@"C:\\TagCloudTests"))
                Directory.CreateDirectory($@"C:\\TagCloudTests");
            if (TestContext.CurrentContext.Result.Outcome != ResultState.Failure) return;
            var tagCloudImageCreator = new TagCloudImageCreator(circularCloudLayouter);
            var testName = TestContext.CurrentContext.Test.FullName;
            var path = $@"C:\\TagCloudTests\{testName}.jpg";
            tagCloudImageCreator.Save(path);
            Console.WriteLine($"Tag cloud visualization saved to file {path}");
        }

        [TestCase(0, 0, Description = "The constructor can take zero by any coordinate",
            TestName = "WhenZeroPoint")]
        [TestCase(1, 1, Description = "The constructor can accept positive numbers by any coordinate",
            TestName = "WhenPositivePoint")]
        [TestCase(-1, -1, Description = "The constructor can accept negative numbers by any coordinate",
            TestName = "WhenNegativePoint")]
        [TestCase(-1, 0, Description = "The constructor can take the coordinates of different characters",
            TestName = "WhenCoordinatesDifferentSigns")]
        public void Constructor_DoesNotThrow(int centerX, int centerY)
        {
            Action action = () => new CircularCloudLayouter(new Point(centerX, centerY));
            action.Should().NotThrow();
        }

        [TestCase(1, Description = "It is expected that one rectangle added to an empty layout will not intersect",
            TestName = "Added1Rectangle")]
        [TestCase(50, Description = "None of the fifty added rectangles are expected to intersect",
            TestName = "Added50Rectangle")]
        public void PutNextRectangle_AddDisjointRectangles(int countRectangles)
        {
            AddSameRectangles(countRectangles, 50);

            IntersectionOfAnyTwo(circularCloudLayouter.Rectangles.ToArray()).Should().HaveCount(0);
        }

        [TestCase(1, Description = "Expect only one rectangle in the layout, after we added one",
            TestName = "Added1Rectangle")]
        [TestCase(50, Description = "Expect only fifty rectangle in the layout, after we added one",
            TestName = "Added50Rectangle")]
        public void PutNextRectangle_NumberRectanglesShouldBeAsAdded(int countRectangles)
        {
            AddSameRectangles(countRectangles, 50);

            circularCloudLayouter.Rectangles.Should().HaveCount(countRectangles);
        }

        [TestCase(1, Description = "it is expected that after adding the rectangle the maximum values along the X and Y axes will be relatively close to the center",
            TestName = "Added1Rectangle")]
        [TestCase(50,  Description = "It is expected that after adding fifty rectangles the maximum values along the X and Y axes will be relatively close to the center",
            TestName = "Added50Rectangle")]
        public void PutNextRectangle_RectanglesShouldBeTightlyCentered(int countRectangles)
        {
            AddSameRectangles(countRectangles, 1);
            var centerOffsetX = circularCloudLayouter.GetMaxOffsetFromCenterAlongAxis(Axis.X);
            var centerOffsetY = circularCloudLayouter.GetMaxOffsetFromCenterAlongAxis(Axis.Y);

            centerOffsetX.Should().BeLessThan(10);
            centerOffsetY.Should().BeLessThan(10);
        }

        private IEnumerable<(Rectangle, Rectangle)> IntersectionOfAnyTwo(Rectangle[] source)
        {
            for (var i = 0; i < source.Length; i++)
            for (var j = i + 1; j < source.Length; j++)
                if (source[i].IntersectsWith(source[j]))
                    yield return (source[i], source[j]);
        }

        private void AddSameRectangles(int countRectangles, int size)
        {
            for (var i = 0; i < countRectangles; i++)
                circularCloudLayouter.PutNextRectangle(new Size(size, size));
        }
    }
}