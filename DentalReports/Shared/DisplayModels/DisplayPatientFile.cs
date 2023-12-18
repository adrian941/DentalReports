using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalReports.Shared.DisplayModels;

public class DisplayPatientFile
{
    public int Id { get; set; } = -1;
    public string Name { get; set; } = string.Empty;
    public long SizeBytes { get; set; } = 0;
}
