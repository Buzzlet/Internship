using System;

namespace SalaryUpdate.Services
{
    public static class Extensions
    {
        public static decimal RoundToDollars(this decimal d)
        {
            var cashParts = d.ToString("0.##").Split('.');
            var dollars = Convert.ToDecimal(cashParts[0]);
            if (cashParts.Length < 2)
                return dollars;

            var centsString = cashParts[1];
            var cents = Convert.ToDecimal(centsString);

            if (centsString.Length < 2)
            {
                cents = Convert.ToDecimal(centsString) * 10;
            }

            if (cents >= 50) // round up
            {
                dollars++;
            }
            return dollars;
        }
    }
}