// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using DentalReports.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit.Text;
using MimeKit;

namespace DentalReports.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender , IConfiguration configuration)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            await SendEmailAsync(Input.Email, "Confirm your email",
                         $"Hello {user.FirstName},<br /><br />" +
                         $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<br /><br />" +
                         $"Kindly,<br />Megagen Bucharest Team");

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return Page();
        }


        private async Task<bool> SendEmailAsync(string ToEmail, string Subject, string ConfirmLink)
        {
            string HostEmail = _configuration.GetValue<string>("EmailSender:Host");
            string HostPassword = _configuration.GetValue<string>("EmailSender:Password");
            int SmtpPort = _configuration.GetValue<int>("EmailSender:Port");
            //TODO : Delete logs
            Console.WriteLine(HostEmail);
            Console.WriteLine(SmtpPort);
            try
            {

                using (MimeMessage mailService = new MimeMessage())
                {

                    mailService.From.Add(MailboxAddress.Parse(HostEmail));


                    mailService.To.Add(MailboxAddress.Parse(ToEmail));
                    mailService.Subject = "Megagen Email Confirmation";
                    mailService.Body = new TextPart(TextFormat.Html) { Text = ConfirmLink };


                    using var smtp = new MailKit.Net.Smtp.SmtpClient();
                    await smtp.ConnectAsync("smtp.gmail.com", SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(HostEmail, HostPassword);
                    await smtp.SendAsync(mailService);
                    await smtp.DisconnectAsync(true);



                    return true;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }



    }
}
