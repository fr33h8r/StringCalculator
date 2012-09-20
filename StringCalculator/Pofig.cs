using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace PrimeFactorsKata
{
    public class Calculator
    {
        private readonly IDataProvider provider;

        public Calculator(IDataProvider provider)
        {
            this.provider = provider;
        }

        public int Add()
        {
            var nums = provider.TakeNums();
            return nums.Select(int.Parse).Sum();
        }
    }

    public class CalcTests
    {
        [Fact]
        public void should_take_numbers_return_sum()
        {
            //arrange
            var dataProvider = Substitute.For<IDataProvider>();
            dataProvider.TakeNums().Returns(new List<string>{"1", "2", "5"});
            var calc = new Calculator(dataProvider);
            //act
            var res = calc.Add();
            //assert
            res.Should().Be(8);
        }
    }

    public interface IDataProvider
    {
        List<string> TakeNums();
    }

    public class TextDataProvider : IDataProvider
    {
        public List<string> TakeNums()
        {
            return new List<string>{"11", "23"};
        }
    }

    public class WebDataProvider : IDataProvider
    {
        public List<string> TakeNums()
        {
            var client = new WebClient();
            var nums =
                client.DownloadString(
                    "https://raw.github.com/gist/3654363/6332c098dd8c8d07540b797b73bd603c64ee0c44/gistfile1.txt").Split(';').
                    ToList();
            return nums;
        }
    }

    public class NumbersProvidersTests
    {
        [Fact]
        public void should_take_string_numbers()
        {
            //arrange
            var dataProv = new WebDataProvider();
            //act
            var res = dataProv.TakeNums();
            //
            res.Should().BeEquivalentTo(new List<string>{"10", "20", "30", "40"});
        }
    }

    
}