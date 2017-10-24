using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    public class NumberValidatorTests
    {
        [TestCase(-1, 2, TestName = "Negative Preci sion")]
        [TestCase(1, -2, TestName = "Negative Scale")]
        [TestCase(1, 2, TestName = "Scale greater than precision")]
        [TestCase(4, 4, TestName = "Scale equal to precision")]
        public void TestConstructor_ThrowArgumentException(int prec, int scale)
        {
            Assert.Throws<ArgumentException>(() => new NumberValidator(prec, scale, true));
        }

        [TestCase(1, 0, TestName = "With Zero Scale Without Flag")]
        [TestCase(5, 5, TestName = "With Equal Scale And Precisoin")]
        public void TestConstructor_DoesNotThrow(int precision, int scale)
        {
            Assert.DoesNotThrow(() => new NumberValidator(precision, scale));
        }


        [TestCase("12345", ExpectedResult = true)]
        [TestCase("-12345", ExpectedResult = false)]
        [TestCase("12.234", ExpectedResult = false)]
        public bool NumberValidator_TestWithDefaultArgs(string value)
        {
            return new NumberValidator(5).IsValidNumber(value);
        }


        [TestCase("", TestName = "EmptyString")]
        [TestCase(null, TestName = "Null")]
        [TestCase("a.sd", TestName = "NonDigitString")]
        [TestCase(" 12.0", TestName = "Space Before number")]
        [TestCase("12.0 ", TestName = "Space after number")]
        public void IsValidNumber_ShouldBeFalse_On(string value)
        {
            new NumberValidator(17, 2).IsValidNumber(value).Should().BeFalse();
        }


        [TestCase("0,0")]
        [TestCase("0")]
        [TestCase("+0")]
        [TestCase("0,00")]
        [TestCase("00,00")]
        [TestCase("-0,00")]
        [TestCase("+0.00")]
        [TestCase("0,000", 3)]
        public void IsValidNumber_ShouldBeTrue_One(string value, int scale = 2)
        {
            new NumberValidator(17, scale).IsValidNumber(value).Should().BeTrue();
        }

        [TestCase(17, 2, true, "12.34567", ExpectedResult = false,
            TestName = "false when length of fraction part greater than scale")]
        [TestCase(6, 4, true, "121.2345", ExpectedResult = false,
            TestName = "false when length of fraction and integer part greater than precision")]
        [TestCase(3, 2, false, "-1.23", ExpectedResult = false,
            TestName = "false when length of all part greater than precision")]
        [TestCase(4, 2, false, "-1.23", ExpectedResult = true,
            TestName = "true when length of all part is equal to precision")]
        [TestCase(17, 2, true, "-1.23", ExpectedResult = false,
            TestName = "false on negative number when only positive valid")]
        public bool IsValidNumber_ShouldBe(int precision, int scale, bool onlyPositive, string value)
        {
            return new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value);
        }
    }

    public class NumberValidator
    {
        private readonly Regex numberRegex;
        private readonly bool onlyPositive;
        private readonly int precision;
        private readonly int scale;

        public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
        {
            this.precision = precision;
            this.scale = scale;
            this.onlyPositive = onlyPositive;
            if (precision <= 0)
                throw new ArgumentException("precision must be a positive number");
            if (scale < 0 || scale >= precision)
                throw new ArgumentException("scale must be a non-negative number less or equal than precision");
            numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
        }

        public bool IsValidNumber(string value)
        {
            // Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
            // описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
            // Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
            // целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
            // Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

            if (string.IsNullOrEmpty(value))
                return false;

            var match = numberRegex.Match(value);
            if (!match.Success)
                return false;

            // Знак и целая часть
            var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
            // Дробная часть
            var fracPart = match.Groups[4].Value.Length;

            if (intPart + fracPart > precision || fracPart > scale)
                return false;

            if (onlyPositive && match.Groups[1].Value == "-")
                return false;
            return true;
        }
    }
}