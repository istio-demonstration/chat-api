using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static  class DateTimeOffsetExtensions
    {
        public static int CalculateAge(this DateTimeOffset dateTimeOffset)
        {
            var today = DateTimeOffset.Now;
            var age = today.Year - dateTimeOffset.Year;
            if (today.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}
