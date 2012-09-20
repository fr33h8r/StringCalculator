using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using NSubstitute;
using Xunit;

namespace StringCalculator
{
    public class Calculator
    {
        private readonly StringParser stringParser;

        public Calculator(StringParser stringParser)
        {
            this.stringParser = stringParser;
        }

        public int Add(string numbers)
        {
            var nums = stringParser.GetNumbers(numbers);

            var negatives = nums.Where(n => n < 0).ToList();
            if (negatives.Any()) throw new NegativeNumberException("negatives not allowed: " + string.Join(", ", negatives));

            return nums
                .Where(n => n < 1000)
                .Sum();
        }

        public int Subtract(string numbers)
        {
            var nums = stringParser.GetNumbers(numbers);
            
            var negatives = nums.Where(n => n < 0).ToList();
            if (negatives.Any())
                throw new NegativeNumberException("negatives not allowed: " + string.Join(", ", negatives));
            var result = nums.Where(x => x < 1000).ToList();

            var neg = result.Skip(1).Sum();
            return result[0] - neg;
        }
    }

    public class NegativeNumberException : Exception
    {
        public NegativeNumberException(string message) : base(message) { }
    }

    public class CalculatorTests
    {
        private const string InputText = "some text";
        private readonly Calculator calc;
        private readonly StringParser parser;

        public CalculatorTests()
        {
            parser = Substitute.For<StringParser>();
            calc = new Calculator(parser);
        }

        [Fact]
        public void should_sum_all_numbers_on_add()
        {
            //arrange
            parser.GetNumbers(InputText).Returns(new List<int> { 1, 2, 3 });
            //act
            var result = calc.Add(InputText);
            //assert
            result.Should().Be(6);
        }

        [Fact]
        public void should_ignore_numbers_greater_than_1000_on_add()
        {
            //arrange
            parser.GetNumbers(InputText).Returns(new List<int> { 1, 2000, 3 });
            //act
            var result = calc.Add(InputText);
            //assert
            result.Should().Be(4);
        }

        [Fact]
        public void on_negatives_should_throw_exception_on()
        {
            //arrange
            parser.GetNumbers(InputText).Returns(new List<int>{-1, -2, 3});
            //act assert
            Action action = () => calc.Add(InputText);
            action.ShouldThrow<NegativeNumberException>().WithMessage("negatives not allowed: -1, -2");
        }

        [Fact]
        public void should_take_unknown_number_of_arguments_return_sum_on_add()
        {
            //arrange
            var rand = new Random();
            var numbers = Enumerable.Range(1, rand.Next(2, 10)).Select(i => rand.Next(0, 100)).ToList();
            parser.GetNumbers(InputText).Returns(numbers);
            //act
            var result = calc.Add(InputText);
            //assert
            result.Should().Be(numbers.Sum());
        }

        [Fact]
        public void should_subtract_all_numbers()
        {
            //arrange
            parser.GetNumbers(InputText).Returns(new List<int> { 5, 2, 3 });
            //act
            var result = calc.Subtract(InputText);
            //assert
            result.Should().Be(0);
        }

        [Fact]
        public void should_ignore_numbers_greater_than_1000_on_subtract()
        {
            //arrange
            parser.GetNumbers(InputText).Returns(new List<int> { 3, 2000, 1 });
            //act
            var result = calc.Subtract(InputText);
            //assert
            result.Should().Be(2);
        }

        [Fact]
        public void on_negatives_should_throw_exception_on_subtract()
        {
            //arrange
            parser.GetNumbers(InputText).Returns(new List<int> { -1, -2, 3 });
            //act assert
            Action action = () => calc.Subtract(InputText);
            action.ShouldThrow<NegativeNumberException>().WithMessage("negatives not allowed: -1, -2");
        }

        [Fact]
        public void should_take_unknown_number_of_arguments_return_sum_on_substract()
        {
            //arrange
            var rand = new Random();
            var numbers = Enumerable.Range(1, rand.Next(2, 10)).Select(i => rand.Next(0, 100)).ToList();
            parser.GetNumbers(InputText).Returns(numbers);
            //act
            var result = calc.Add(InputText);
            //assert
            result.Should().Be(numbers.Sum());
        }
    }
}
