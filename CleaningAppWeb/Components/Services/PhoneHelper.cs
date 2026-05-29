using System.Text.RegularExpressions;

namespace CleaningAppWeb.Components.Services
{
    public static partial class PhoneHelper
    {
        public const string FormattedPhoneMask = @"\+7 \([0-9]{3}\) [0-9]{3}-[0-9]{2}-[0-9]{2}";


        [GeneratedRegex(FormattedPhoneMask)]
        private static partial Regex FormattedPhoneRegex();

        public static string NormalizeRawDigits(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var digits = new string(input.Where(char.IsDigit).ToArray());
            if (digits.Length == 0)
                return string.Empty;

            if (digits[0] == '8')
                digits = '7' + digits[1..];
            else if (digits[0] != '7')
                digits = '7' + digits;

            if (digits.Length > 11)
                digits = digits[..11];

            return digits;
        }

        public static string FormatPhoneFromRaw(string digits)
        {
            var nd = NormalizeRawDigits(digits);
            if (string.IsNullOrEmpty(nd))
                return string.Empty;

            var result = "+7";
            if (nd.Length > 1) result += " (" + nd[1..Math.Min(4, nd.Length)];
            if (nd.Length >= 5) result += ") " + nd[4..Math.Min(7, nd.Length)];
            if (nd.Length >= 8) result += "-" + nd[7..Math.Min(9, nd.Length)];
            if (nd.Length >= 10) result += "-" + nd[9..Math.Min(11, nd.Length)];

            return result;
        }

        public static bool MatchPhone(string number) => FormattedPhoneRegex().IsMatch(number);
    }
}
