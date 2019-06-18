﻿using System;
using System.Linq;
using Deveroom.VisualStudio.Diagonostics;
using Deveroom.VisualStudio.Editor.Services.Parser;
using Deveroom.VisualStudio.Monitoring;
using FluentAssertions;
using Moq;
using Xunit;

namespace Deveroom.VisualStudio.Tests.Editor.Services
{
    public class DeveroomGherkinParserTests
    {
        [Fact]
        public void Should_provide_parse_result_when_unexpected_end_of_file()
        {
            var sut = new DeveroomGherkinParser(new SpecFlowGherkinDialectProvider("en-US"),
                new Mock<IMonitoringService>().Object);

            var result = sut.ParseAndCollectErrors(@"
Feature: Addition
@tag
", 
                new DeveroomNullLogger(), out var gherkinDocument, out var errors);
            gherkinDocument.Should().NotBeNull();
            result.Should().BeFalse();
        }

        [Fact]
        public void Should_tolerate_backslash_at_end_of_line_in_DataTable()
        {
            var sut = new DeveroomGherkinParser(new SpecFlowGherkinDialectProvider("en-US"),
                new Mock<IMonitoringService>().Object);

            var result = sut.ParseAndCollectErrors(@"
Feature: Addition
Scenario: Add two numbers
	When I press
		| foo |
		| bar \
",
                new DeveroomNullLogger(), out var gherkinDocument, out var errors);
            gherkinDocument.Should().NotBeNull();
            result.Should().BeFalse();
        }

        [Fact]
        public void Should_tolerate_unfinished_DataTable()
        {
            var sut = new DeveroomGherkinParser(new SpecFlowGherkinDialectProvider("en-US"),
                new Mock<IMonitoringService>().Object);

            var result = sut.ParseAndCollectErrors(@"
Feature: Addition
Scenario: Add two numbers
	When I press
		| foo |
		| bar
",
                new DeveroomNullLogger(), out var gherkinDocument, out var errors);
            gherkinDocument.Should().NotBeNull();
            result.Should().BeFalse();
        }
    }
}
