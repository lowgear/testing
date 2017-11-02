using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	public class NumberValidatorTests
	{
		[TestCase(3, 2, true, "1.23", TestName = "ForPositiveIfOnlyPositive")]
		[TestCase(3, 2, true, "0.0", TestName = "ForZeroIfOnlyPositive")]
		[TestCase(4, 2, false, "-1.23", TestName = "IfPushingLimitsWithSign")]
		[TestCase(3, 2, false, "1.23", TestName = "IfPointExceedsPrecisionButDigitsDont")]
		[TestCase(3, 2, false, "1,23", TestName = "IfCommaInsteadOfPoint")]
		public void IsValidNumber_ShouldBeTrue(int precision, int scale, bool onlyPositive, string value)
		{
			new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value).Should().BeTrue();
		}


		[TestCase(3, 2, false, ".00", TestName = "IfPointIsNotLeadByDigits")]
		[TestCase(2, 1, false, ",0", TestName = "IfCommaIsNotLeadByDigits")]
		[TestCase(3, 2, false, "-1.", TestName = "IfPointIsNotFollowedByDigits")]
		[TestCase(2, 1, false, "0,", TestName = "IfCommaIsNotFollowedByDigits")]
		[TestCase(20, 19, false, "-", TestName = "ForBareSign")]
		[TestCase(20, 19, false, "+Sample.Text", TestName = "ForNotNumbers")]
		[TestCase(3, 2, true, "00.00", TestName = "IfPrecisionExceedsLimitWithLeadingZeroes")]
		[TestCase(3, 2, true, "+1.23", TestName = "IfPrecisionExceedsLimitWithPlusSign")]
		[TestCase(3, 2, false, "-1.23", TestName = "IfPrecisionExceedsLimitWithMinusSign")]
		[TestCase(3, 2, true, "41.23", TestName = "ForPositiveIfPrecisionExceedsLimit")]
		[TestCase(3, 2, true, "1.234", TestName = "IfScaleExceedsLimit")]
		[TestCase(3, 2, true, "-1.23", TestName = "ForNegativeIfOnlyPositive")]
		public void IsValidNumber_ShouldBeFalse(int precision, int scale, bool onlyPositive, string value)
		{
			new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value).Should().BeFalse();
		}


		[TestCase(1, 0, true)]
		[TestCase(3, 1, true)]
		public void NumberValidator_WhileConstructingFromNonNegativePrecisonGreaterThanNonNegativeScale_DoesntThrowException(int precision, int scale, bool onlyPositive)
		{
			ConstructingNumberValidator(precision, scale, onlyPositive).ShouldNotThrow();
		}


		[TestCase(-1, 2, true, TestName = "WhileConstructingFromNegativePrecison")]
		[TestCase(7, 7, true, TestName = "WhileConstructingFromPrecisonEqualToScale")]
		[TestCase(3, 4, true, TestName = "WhileConstructingFromPrecisonLessThanScale")]
		[TestCase(1, -2, true, TestName = "WhileConstructingFromNegativePrecisonAndNonNegativeScale")]
		public void NumberValidator_ShouldThrowArgumentException(int precision, int scale, bool onlyPositive)
		{
			ConstructingNumberValidator(precision, scale, onlyPositive).ShouldThrow<ArgumentException>();
		}


		private static Action ConstructingNumberValidator(int precision, int scale, bool onlyPositive)
		{
			return (() => new NumberValidator(precision, scale, onlyPositive));
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
				throw new ArgumentException("scale must be a non-negative number less than precision");
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