using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalReports.Shared.Common;

public static class StringPrelucration
{
    public static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return input.Substring(0, 1).ToUpper() + input.Substring(1).ToLower();
    }

    public static string GetRandomFileName(int minChars, int maxChars)
    {
        maxChars++;
        string goodChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        Random random = new Random();

        int charsNumber = random.Next(minChars, maxChars);

        char[] randomFileName = new char[charsNumber];
        for (int i = 0; i < charsNumber; i++)
        {
            randomFileName[i] = goodChars[random.Next(goodChars.Length)];
        }

        string output = new string(randomFileName);

        return output;


    }
}
