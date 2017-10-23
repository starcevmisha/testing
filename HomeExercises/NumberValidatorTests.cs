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
//        [TestCase(-3, -2, TestName = "Negative Scale and Scale  greater than precision")]
        public void TestConstructor_ThrowArgumentException(int prec, int scale)
        {
            Assert.Throws<ArgumentException>(() => new NumberValidator(prec, scale, true));
        }

        [Test]
        public void TestConstructor_DoesNotThrowArgumentException_WithFlagAndScale()
        {
            Assert.DoesNotThrow(() => new NumberValidator(1, 0, true));
        }

        [Test]
        public void TestConstructor_DoesNotThrowArgumentException_WithScaleWithoutFlag()
        {
            Assert.DoesNotThrow(() => new NumberValidator(1, 0));
        }

        [Test]
        public void TestConstructor_DoesNotThrowArgumentException_WithFlagWithoutScale()
        {
            Assert.DoesNotThrow(() => new NumberValidator(1, onlyPositive: true));
        }


        [TestCase("", TestName = "EmptyString")]
        [TestCase(null, TestName = "Null")]
        [TestCase("a.sd", TestName = "NonDigitString")]
        public void IsValidNumber_ShouldBeFalse_OnBadCase(string value)
        {
            new NumberValidator(17, 2).IsValidNumber(value).Should().BeFalse();
        }


        [TestCase(" 12.0", TestName = "Space Before number")]
        [TestCase("12.0 ", TestName = "Space after number")]
        public void IsValidNumber_ShouldBeFalse_OnStringWithSpace(string value)
        {
            new NumberValidator(17, 2, true).IsValidNumber(value).Should().BeFalse();
        }


        [TestCase(17, 2, "0,0")]
        [TestCase(17, 2, "0")]
        [TestCase(17, 2, "+0")]
        [TestCase(17, 2, "0,00")]
        [TestCase(17, 2, "00,00")]
        [TestCase(17, 2, "-0,00")]
        [TestCase(17, 2, "+0.00")]
        [TestCase(17, 3, "0,000")]
        public void IsValidNumber_ShouldBeTrue_OnDifferentCase(int precisison, int scale, string value)
        {
            new NumberValidator(precisison, scale, false).IsValidNumber(value).Should().BeTrue();
        }

        [TestCase(17, 2, true, "12.34567", ExpectedResult = false,
            TestName = "IsValidNumber should be false when length of fraction part greater than scale")]
        [TestCase(6, 4, true, "121.2345", ExpectedResult = false,
            TestName = "IsValidNumber should be false when length of fraction and integer part greater than precision")]
        [TestCase(3, 2, false, "-1.23", ExpectedResult = false,
            TestName = "IsValidNumber should be false when length of all part greater than precision")]
        [TestCase(4, 2, false, "-1.23", ExpectedResult = true,
            TestName = "IsValidNumber should be true when length of all part is equal to precision")]
        [TestCase(17, 2, true, "-1.23", ExpectedResult = false,
            TestName = "IsValidNumber should be false when Negative number when only positive flag")]
        public bool ValidateNumber(int precision, int scale, bool onlyPositive, string value)
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