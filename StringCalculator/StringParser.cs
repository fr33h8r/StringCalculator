using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace StringCalculator
{
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

    public class DelimiterExttractorTests
    {
        [Fact]
        public void which_delimiters_returns_method()
        {
            //act
            var result = new ExtendedExtractor().GetDelimiters("//[***][%%%][$$$]\n1***2%%%3$$$4");
            //assert
            result.Should().BeEquivalentTo(new object[] {"***", "%%%", "$$$"});
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
                                    ? (IDelimiterExtractor) new ExtendedExtractor()
                                    : new CustomExtractor());

            return Extractor.GetDelimiters(input);
        }

        public virtual List<int> GetNumbers(string input)
        {
            var delimiters = GetDelimiters(input);

            var index = Extractor is DefaultExtractor ? 0 : input.IndexOf('\n');

            return input
                .Substring(index)
                .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();
        }
    }

    public class StringParserTests
    {
        readonly StringParser parser = new StringParser();

        [Fact]
        public void should_take_string_return_delim()
        {
            //act
            var res = parser.GetNumbers("1,2");
            //assert
            res.Should().BeEquivalentTo(new object[] {1, 2});
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

        [Fact]
        public void should_take_default_string_return_delimiter()
        {
            //act
            var res = parser.GetDelimiters("1,2");
            //assert
            res.Should().BeEquivalentTo(new object[] {","});
        }

        [Fact]
        public void should_take_custom_string_return_delimiter()
        {
            //act
            var res = parser.GetDelimiters("//;\n1;2");
            //assert
            res.Should().BeEquivalentTo(new object[] { ";" });
        }

        [Fact]
        public void should_take_extended_string_return_delimiters()
        {
            //act
            var res = parser.GetDelimiters("//[***][%%%][$$$]\n1***2%%%3$$$4");
            //assert
            res.Should().BeEquivalentTo(new object[] { "***", "%%%", "$$$" });
        }
    }
}
