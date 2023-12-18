using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalReports.Shared.Common;

public class Dates
{
    public DateTime FromDate { get; set; } = DateTime.Now;
    public DateTime ToDate { get; set; } = DateTime.Now.AddYears(30);
}
