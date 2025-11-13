using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Aspose.Email;

using HtmlAgilityPack;

namespace MalformedHtmlFix
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool isSuccess = SetLicenses();
            Debug.WriteLine($"Aspose License set successfully: {isSuccess}");
            if (isSuccess) { RunMain(args); }
            else Debug.WriteLine($"Aspose License did not load.");
        }

        private static void RunMain(string[] args)
        {
            string strMalformedHtml = @"
<!DOCTYPE html>
<html>
<head>
    <title>Robust Parsing Test</title>
</head>
<body>
    <h1>Parsing Robustness Demo</h1>
    <!-- 1. Unclosed <a> tag and nested tags -->
    <div id='content'>
        <a href='/link1'>Link One - No closing tag
        <p>Nested paragraph inside the broken link, proving HAP fixes the structure.
            <span>A nested span.</span>
        </p>
    </div>

    <!-- 2. Malformed table structure -->
    <table>
        <tr>
            <td>Cell A
        <tr>
            <td>Cell B</td> <!-- This cell is closed, but the second <tr> is missing -->
            <td>Cell C
    </table>

    <!-- 3. Self-closing tags -->
    <p>
        This is a line break: <br>
        And a horizontal rule: <hr/>
        An image tag: <img src='placeholder.png'>
    </p>
</body>
</html>
";
            Console.WriteLine("--- Starting HtmlAgilityPack Robustness Test ---");
            string htmlFixedHtml = HtmlHelper.SanitizeHtmlDocument(strMalformedHtml);
            Console.WriteLine("--- Fixed HTML ---");
            Console.WriteLine(htmlFixedHtml);
            Console.WriteLine("--- Test Complete ---");

            Console.WriteLine(htmlFixedHtml);

            MailMessage message = new MailMessage();
            message.From = "sender@example.com";
            message.To.Add("recipient@example.com");
            message.Subject = "Email with Auto-Fixed Nested HTML";
            message.HtmlBody = htmlFixedHtml;

            message.Save("FixedNestedEmail.msg", SaveOptions.DefaultMsgUnicode);

            Console.WriteLine("Email created successfully with all nested HTML tags fixed.");
            Console.ReadLine();
        }

        private static bool SetLicenses()
        {
            var LicenseEmail = new Aspose.Email.License();
            //var LicensePdf = new Aspose.Pdf.License();
            //var LicenseWords = new Aspose.Words.License();
            //var LicenseCells = new Aspose.Cells.License();
            //var LicenseSlides = new Aspose.Slides.License();
            //var LicenseImaging = new Aspose.Imaging.License();

            try
            {
                // Default to use Embedded Resource file instead of physical file
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "Aspose.Total.lic";
                var LicensePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourceName));
                if (!string.IsNullOrWhiteSpace(LicensePath))
                {
                    LicenseEmail.SetLicense(LicensePath);
                    //LicensePdf.SetLicense(LicensePath);
                    //LicenseWords.SetLicense(LicensePath);
                    //LicenseCells.SetLicense(LicensePath);
                    //LicenseSlides.SetLicense(LicensePath);
                    //LicenseImaging.SetLicense(LicensePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, ex);
                Debug.WriteLine("Error initialising Aspose License", ex);
                return false;
            }
            return true;
        }
    }
}