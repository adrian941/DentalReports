using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalReports.Shared.Common;

public static class MegagenUtility
{
    public static string GetMediaTypeHeader(string fileExtension)
    {

        switch (fileExtension.ToLower())
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".png":
                return "image/png";
            case ".txt":
                return "text/plain";
            case ".pdf":
                return "application/pdf";
            case ".mp4":
                return "video/mp4";
            case ".stl":
                return "application/sla"; //TODO: Research about .stl for all browser types
            default:
                return "application/octet-stream";

        }

    }
}
