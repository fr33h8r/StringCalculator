using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using System.Linq;
using NSubstitute;
using Xunit;

namespace calc
{
    public class Calculator
    {
        public string Numbers { get; set; }

        public int Add(string numbers)
        {
            var result = new StringParser().GetNumbers(numbers);

            var negativeNums = result.Where(n => n < 0).ToList();
            if (negativeNums.Any()) throw new NegativeNumberException("negatives not allowed: " + string.Join(", ", negativeNums));

            return result.Sum();
        }

        public int Subtract(string numbers)
        {
            const int result = 0;
            return result;
        }
    }

    public class NegativeNumberException : Exception
    {
        public NegativeNumberException(string message) : base(message) { }
    }

    //Creator
    public interface IDelimiterExtractor
    {
        string[] GetDelimiters(string input);
    }

    //ConcreteCreatorA
    public class DefaultExtractor : IDelimiterExtractor
    {
        public string[] GetDelimiters(string input)
        {
            return new List<string>(new[]
                                        {
                                            input.Contains(',') ? "," : null,
                                            input.Contains('\n') ? "\n" : null
                                        })
                .Where(a => a != null)
                .ToArray();
        }
    }

    //ConcreteCreatorB
    public class CustomExtractor : IDelimiterExtractor
    {
        public string[] GetDelimiters(string input)
        {
            return new[] { input[2].ToString() };
        }
    }

    //ConcreteCreatorC
    public class ExtendedExtractor : IDelimiterExtractor
    {
        public string[] GetDelimiters(string input)
        {
            var strForSearch = input.Substring(0, input.IndexOf('\n'));
            var list = new List<string>();
            var result = "";
            var match = Regex.Match(strForSearch, @"[^\w\./\[,\n]");
            while (match.Success)
            {
                result += match.Value;
                match = match.NextMatch();
            }
            list.AddRange(result.Split(new[] { "]" }, StringSplitOptions.RemoveEmptyEntries));
            return list.ToArray();
        }
    }

    public class StringParser
    {
        protected IDelimiterExtractor Extractor { get; set; }

        public string[] GetDelimiters(string input)
        {
            Extractor = !input.StartsWith("//")
                                ? new DefaultExtractor()
                                : (input.Contains('[')
                                    ? (IDelimiterExtractor)new ExtendedExtractor()
                                    : new CustomExtractor());

            return Extractor.GetDelimiters(input);
        }

        public List<int> GetNumbers(string input)
        {
            var delimiters = GetDelimiters(input);

            var index = Extractor is DefaultExtractor ? 0 : input.IndexOf('\n');
            return input
                .Substring(index)
                .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .Where(a => a < 1000)
                .ToList();
        }
    }

    public class StringParserTests
    {
        readonly StringParser parser = new StringParser();

        [Fact]
        public void test_substitute()
        {
            //arrange
            const string input = "//;\n1;2";
            var delim = Substitute.For<IDelimiterExtractor>();
            delim.GetDelimiters(input).Returns(new[] { ";" });
            //act
            var res = new Calculator().Add(input);
            //assert
            res.Should().Be(3);
        }

        [Fact]
        public void should_take_string_return_delim()
        {
            //act
            var res = parser.GetNumbers("1,2");
            //assert
            res.Should().BeEquivalentTo(new object[] { 1, 2 });
        }

        [Fact]
        public void should_take_custom_delimiter_return_this()
        {
            //act
            var res = parser.GetNumbers("//;\n1;2");
            //assert
            Console.Out.WriteLine(string.Join(" ", res));
            res.Should().BeEquivalentTo(new object[] { 1, 2 });
        }

        [Fact]
        public void should_take_custom_delimiters_return_them()
        {
            //act
            var res = parser.GetNumbers("//[***][%%%][$$$]\n1***2%%%3$$$4");
            //assert
            Console.Out.WriteLine(string.Join(" ", res));
            res.Should().BeEquivalentTo(new object[] { 1, 2, 3, 4 });
        }
    }

}
