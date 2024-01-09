using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalReports.Shared.Common;

public static class MegagenDates
{

    public static string GetMegagenShortDate(DateTime date)
    {
        string htmlSpaceCode = "&nbsp;";
        string dayString = date.Day < 10 ? $"{htmlSpaceCode} {date.Day}" : date.Day.ToString();
        return $"{dayString} {GetShortMonth(date.Month)} {date.Year}";
    }
    public static string GetMegagenLongDate(DateTime date)
    {
        string htmlSpaceCode = "&nbsp;";
        
    
        


        return $"{date.Day.ToString()}-{GetShortMonth(date.Month)}-{date.Year}";
    }


    public static string GetShortMonth(int monthNumber)
    {
        return GetMonth(monthNumber).Substring(0, 3);
    }

    public static string GetMonth(int monthNumber)
    {
        switch (monthNumber)
        {
            case 1:
                return "January";
            case 2:
                return "February";
            case 3:
                return "March";
            case 4:
                return "April";
            case 5:
                return "May";
            case 6:
                return "June";
            case 7:
                return "July";
            case 8:
                return "August";
            case 9:
                return "September";
            case 10:
                return "October";
            case 11:
                return "November";
            default:
                return "December";
        }

    }
}
