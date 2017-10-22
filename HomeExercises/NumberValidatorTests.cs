using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    public class 
        NumberValidatorTests
    {
        [Test]
        public void TestConstructor_WithNegativePrecision_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new NumberValidator(-1, 2, true));
        }

        [Test]
        public void TestConstructor_WithNegativeScale_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new NumberValidator(1, -2, true));
        }

        [Test]
        public void TestConstructor_WithScaleGreaterPrecision_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new NumberValidator(1, 2, true));
        }

        [Test]
        public void TestConstructor_WithZeroScale_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => new NumberValidator(1, 0, true));
        }

        [Test]
        public void IsValid_OnEmptyString_False()
        {
            new NumberValidator(17, 2, true).IsValidNumber("").Should().BeFalse();
        }

        [Test]
        public void IsValid_OnNull_False()
        {
            new NumberValidator(17, 2, true).IsValidNumber(null).Should().BeFalse();
        }

        [Test]
        public void IsValid_OnNonDigitString_False()
        {
            new NumberValidator(17, 2, true).IsValidNumber("a.sd").Should().BeFalse();
        }

        [Test]
        public void IsValid_OnGoodString_True()
        {
            new NumberValidator(17, 2, true).IsValidNumber("0.0").Should().BeTrue();
        }

        [Test]
        public void IsValid_OnGoodStringWithSign_True()
        {
            new NumberValidator(17, 2, true).IsValidNumber("+0.0").Should().BeTrue();
        }

        [Test]
        public void IsValid_OnGoodStringWithSpace_False()
        {
            new NumberValidator(17, 2, true).IsValidNumber(" 12.0").Should().BeFalse();
            new NumberValidator(17, 2, true).IsValidNumber("12.0 ").Should().BeFalse();
        }

        [Test]
        public void IsValid_EquipollenceCommaAndPoint_True()
        {
            new NumberValidator(17, 2, true).IsValidNumber("12.0").Should().BeTrue();
            new NumberValidator(17, 2, true).IsValidNumber("12,0").Should().BeTrue();
        }

        [TestCase(17,2,"0,0")]
        [TestCase(17,2,"0")]
        [TestCase(17, 2, "+0")]
        [TestCase(17,2,"0,00")]
        [TestCase(17,2,"00,00")]
        [TestCase(17,2,"-0,00")]
        [TestCase(17,2,"+0.00")]
        [TestCase(17,3,"0,000")]
        public void IsValid_DifferentCase_True(int prec, int scale, string value)
        {
            new NumberValidator(prec, scale, false).IsValidNumber(value).Should().BeTrue();
        }

        [Test]
        public void IsValid_FractionLengthGreaterScale_False()
        {
            new NumberValidator(17, 2, false).IsValidNumber("1.23456").Should().BeFalse();
        }
        [Test]
        public void IsValid_IntAndFracLengthGreaterPrecision_False()
        {
            new NumberValidator(6, 2, false).IsValidNumber("121.23456").Should().BeFalse();
        }

        [Test]
        public void IsValid_SignAndIntAndFracLengthGreaterPrecision_False()
        {
            new NumberValidator(3, 2, false).IsValidNumber("-1.23").Should().BeFalse();
        }

        [Test]
        public void IsValid_SignAndIntAndFracLengthEqualPrecision_True()
        {
            new NumberValidator(4, 2, false).IsValidNumber("-1.23").Should().BeTrue();
        }

        [Test]
        public void IsValid_OnlyPositiveFlagIsFalseAndValueIsNegative_False()
        {
            new NumberValidator(17, 2, true).IsValidNumber("-1.23").Should().BeFalse();
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