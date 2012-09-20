using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;

namespace StringCalculatorKata
{
    public class StringCalculatorKata
    {
        [Fact]
        public void should_return_0_for_empty_string()
        {
            // arrange
            var calculator = new StringCalculator();

            // act
            int add = calculator.Add("");

            // assert
            add.Should().Be(0);
        }

        [Fact]
        public void should_return_5_for_5()
        {
            // arrange
            var calculator = new StringCalculator();

            // act
            int add = calculator.Add("5");

            // assert
            add.Should().Be(5);
        }

        [Fact]
        public void should_return_14_for_5_and_9()
        {
            // arrange
            var calculator = new StringCalculator();

            // act
            int add = calculator.Add("5,9");

            // assert
            add.Should().Be(14);
        }

        [Fact]
        public void should_return_sum_for_unknown_amount_of_numbers()
        {
            // arrange 
            int[] numbersWithUnknownAmounts = GetRandomNumbers();
            string numbers = String.Join(",", numbersWithUnknownAmounts);
            var calculator = new StringCalculator();

            // act
            int add = calculator.Add(numbers);

            // assert
            add.Should().Be(numbersWithUnknownAmounts.Sum());
        }

        private static int[] GetRandomNumbers()
        {
            return Enumerable.Repeat(1, GetRandomNumber()).Select(_ => GetRandomNumber()).ToArray();
        }

        [Fact]
        public void random_should_be_random()
        {
            GetRandomNumbers().Should().NotEqual(GetRandomNumbers());
        }

        [Fact]
        public void should_support_new_line_as_delimiter()
        {
            // arrange 
            var calculator = new StringCalculator();

            // act
            int add = calculator.Add("3,4\n6\n7");

            // assert
            add.Should().Be(3 + 4 + 6 + 7);
        }

        [Fact]
        public void should_support_custom_delimiter()
        {
            // arrange
            var calculator = new StringCalculator();

            // act
            int result = calculator.Add("//*\n4*3*2");

            // assert
            result.Should().Be(4 + 3 + 2);
        }

        [Fact]
        public void should_throw_exception_for_negatives_and_message_should_list_all_negatives()
        {
            // arrange
            var calculator = new StringCalculator();

            // act
            Action act = () => calculator.Add("-1,2,-3");

            // assert
            act.ShouldThrow<InvalidOperationException>()
                .WithMessage("negatives are not allowed: " + "-1,-3");
        }

        [Fact]
        public void should_ignore_numbers_greater_than_1000()
        {
            // arrange 
            var calculator = new StringCalculator();

            // act
            int result = calculator.Add("2,1002");

            // assert
            result.Should().Be(2);
        }

        [Fact]
        public void should_support_delimiters_of_any_length()
        {
            // arrange
            var calculator = new StringCalculator();

            // act
            int result = calculator.Add("//[***]\n1***2***3");

            // assert
            result.Should().Be(1 + 2 + 3);
        }

        [Fact]
        public void should_support_multiple_delimiters()
        {
            // arrange
            var calculator = new StringCalculator();

            // act 
            int result = calculator.Add("//[==-][&&$]\n2==-3==-6&&$8&&$9");

            // assert
            result.Should().Be(2 + 3 + 6 + 8 + 9);
        }

        private static int GetRandomNumber()
        {
            return Math.Abs(Guid.NewGuid().GetHashCode())%1000;
        }
    }

    public class StringCalculator
    {
        private readonly List<int> negatives = new List<int>();
        private string currentDelimiter;
        private string[] delimiters = new[] {",", "\n"};
        private string numbers;

        public int Add(string numbers)
        {
            this.numbers = numbers;
            FindCustomDelimiters();

            int result = InternalAdd(this.numbers);
            if (negatives.Count > 0)
            {
                throw new InvalidOperationException("negatives are not allowed: " + String.Join(",", negatives));
            }
            return result;
        }

        private void FindCustomDelimiters()
        {
            if (numbers.StartsWith("//"))
            {
                delimiters = numbers[2] != '[' ? new[] {numbers[2].ToString()} : GetCustomDelimiters();
                numbers = numbers.SkipWhile(c => c != '\n').AsString();
            }
        }

        private string[] GetCustomDelimiters()
        {
            var t = new StringBuilder();
            return numbers
                .TakeWhile(c => c != '\n')
                .Aggregate(new List<string>(), (list, c) =>
                                                   {
                                                       if (c == '[')
                                                       {
                                                           t.Clear();
                                                       }
                                                       else if (c == ']')
                                                       {
                                                           list.Add(t.ToString());
                                                       }
                                                       else t.Append(c);
                                                       return list;
                                                   }).ToArray();
        }

        private int InternalAdd(string items)
        {
            if (items == "")
            {
                return 0;
            }
            int separatorIndex = IndexOfAny(items);
            if (separatorIndex == -1)
            {
                return Parse(items);
            }
            var head = items.Take(separatorIndex).AsString();
            var tail = items.Skip(separatorIndex + currentDelimiter.Length).AsString();
            return Parse(head) + InternalAdd(tail);
        }

        private int IndexOfAny(string items)
        {
            foreach (string delimiter in delimiters)
            {
                int indexOf = items.IndexOf(delimiter, StringComparison.Ordinal);
                if (indexOf > -1)
                {
                    currentDelimiter = delimiter;
                    return indexOf;
                }
            }
            return -1;
        }

        private int Parse(string items)
        {
            int result = items.ToInt();
            if (result < 0)
            {
                negatives.Add(result);
            }
            return result > 1000 ? 0 : result;
        }
    }

    public static class StringExtensions
    {
        public static int ToInt(this string s)
        {
            return Int32.Parse(s);
        }

        public static string AsString(this IEnumerable<char> chars)
        {
            return String.Join("", chars);
        }
    }
}