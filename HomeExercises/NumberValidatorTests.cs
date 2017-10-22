using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    public class NumberValidatorTests
    {
        //CR(epeshk): в этом классе ещё остались похожие методы. Нужны ли они?
        
        [TestCase(-1,2,TestName = "Negative Precision")]
        [TestCase(1,-2,TestName = "Negative Scale")] 
        [TestCase(1,2,TestName = "Scale greater than precision")] 
        [TestCase(1,1,TestName = "Scale equal to precision")]      
        //CR(epeshk): есть ещё случай
        public void TestConstructor_ThrowArgumentException(int prec, int scale)
        {
            Assert.Throws<ArgumentException>(() => new NumberValidator(prec, scale, true));
        }
        
        //CR(epeshk): куда пропал тест с DoesNotThrow?

        [TestCase(17,2, "", TestName = "EmptyString")]
        [TestCase(17,2, null, TestName = "Null")]
        [TestCase(17,2, "a.sd", TestName = "NonDigitString")]
        //CR(epeshk): 17, 2 обычно используется, когда значения precision и scale нам безразличны. Давай сведём к минимуму явное использование констант 17, 2
        //CR(epeshk): IsValid или IsValidNumber?
        public void IsValid_ShouldBeFalse_OnBadCase(int precisison, int scale, string value)
        {
            new NumberValidator(precisison, scale, false).IsValidNumber(value).Should().BeFalse();
        }

        [TestCase (" 12.0",TestName = "Space Before number")]
        [TestCase ("12.0 ",TestName = "Space after number")]
        public void IsValidNumber_ShouldBeFalse_OnStringWithSpace(string value)
        {
            new NumberValidator(17, 2, true).IsValidNumber(value).Should().BeFalse();
        }

        [TestCase ("12.0",TestName = "Dot")]
        [TestCase ("12.0",TestName = "Comma")]
        public void IsValidNumber_ShouldBeTrue_WithCommaAndWithPoint(string value)
        {
            new NumberValidator(17, 2, true).IsValidNumber(value).Should().BeTrue();
        }

        [TestCase(17,2,"0,0")]
        [TestCase(17,2,"0")]
        [TestCase(17,2, "+0")]
        [TestCase(17,2,"0,00")]
        [TestCase(17,2,"00,00")]
        [TestCase(17,2,"-0,00")]
        [TestCase(17,2,"+0.00")]
        [TestCase(17,3,"0,000")]
        public void IsValid_ShouldBeTrue_OnDifferentCase(int precision, int scale, string value)
        {
            new NumberValidator(precision, scale, false).IsValidNumber(value).Should().BeTrue();
        }

        [TestCase(17, 2,true,"12.34567", ExpectedResult = false, TestName = "Length of fraction part greater than scale")]
        [TestCase(6, 4,true, "121.2345",ExpectedResult = false, TestName = "Length of fraction and integer part greater than precision")]
        //CR(epeshk): sign, fraction and integer part -> all parts
        //CR(epeshk): в TestName лучше упоминать ожидаемый результат
        [TestCase(3, 2,false, "-1.23",ExpectedResult = false, TestName = "Length of sign, fraction and integer part greater than precision")]
        [TestCase(4, 2,false, "-1.23",ExpectedResult = true, TestName = "Length of sign, fraction and integer part is equal to precision")]
        [TestCase(17, 2,true, "-1.23",ExpectedResult = false, TestName = "Negative number when only positive flag")]
        public bool ValidateNumber(int precision, int scale, bool onlyPositive, string value)
        {
            return new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value);
        }

        //CR(epeshk): обрати внимание, что конструктор можно вызывать по разному. Можно добавить ещё тестов на него
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