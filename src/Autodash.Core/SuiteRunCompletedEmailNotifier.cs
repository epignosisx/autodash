using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Autodash.Core
{
    public class SuiteRunCompletedEmailNotifier : ISuiteRunCompletedNotifier
    {
        private readonly Func<IMongoDatabase> _dbFactory;
        private readonly ILoggerWrapper _logger;
        private readonly Uri _websiteRoot;
        private readonly string _emailTemplatePath;
        private readonly ConcurrentQueue<SuiteRun> _notificationQueue = new ConcurrentQueue<SuiteRun>();

        private int _isInitialized;
        private IDisposable _subscription;

        public SuiteRunCompletedEmailNotifier(Func<IMongoDatabase> dbFactory, ILoggerProvider loggerProvider, Uri websiteRoot, string emailTemplatePath)
        {
            _dbFactory = dbFactory;
            _logger = loggerProvider.GetLogger(GetType().Name);
            _websiteRoot = websiteRoot;
            _emailTemplatePath = emailTemplatePath;
        }

        public void Notify(SuiteRun suiteRun)
        {
            if (suiteRun == null) 
                throw new ArgumentNullException("suiteRun");

            _logger.Info("Queueing notification request of suite run: {0} - {1}", suiteRun.Id, suiteRun.TestSuiteSnapshot.Name);

            _notificationQueue.Enqueue(suiteRun);

            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) == 0)
            {
                _subscription = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMinutes(1))
                    .SelectMany(n => SendNotifications())
                    .Subscribe();
            }
        }

        private async Task<Unit> SendNotifications()
        {
            var db = _dbFactory();
            var emailConfig = await db.GetCollection<EmailConfiguration>("EmailConfiguration").FindAsync(new BsonDocument()).ToFirstOrDefaultAsync();
            if (emailConfig == null || string.IsNullOrEmpty(emailConfig.SmtpServer))
                return await Task.FromResult(Unit.Default);

            var projects = await db.GetCollection<Project>("Project").FindAsync(new BsonDocument()).ToListAsync();

            string emailTemplate = File.ReadAllText(_emailTemplatePath);
            StringBuilder sb = new StringBuilder();
            
            try
            {
                using (SmtpClient smtp = new SmtpClient(emailConfig.SmtpServer, emailConfig.Port))
                {
                    SuiteRun suiteRun;
                    while (_notificationQueue.TryDequeue(out suiteRun))
                    {
                        var project = projects.FirstOrDefault(n => n.Id == suiteRun.TestSuiteSnapshot.ProjectId);
                        if (project == null)
                            continue;

                        sb.Clear();
                        sb.Append(emailTemplate);

                        SendEmail(emailConfig, project, suiteRun, smtp, sb);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not send suite run completed email");
            }

            return Unit.Default;
        }

        private void SendEmail(EmailConfiguration emailConfig, Project project, SuiteRun suiteRun, SmtpClient smtp, StringBuilder emailTemplate)
        {
            try
            {
                var msg = new MailMessage();
                msg.From = new MailAddress(emailConfig.FromEmail);
                foreach (var memberEmail in project.MemberEmails)
                    msg.To.Add(memberEmail);

                string summary = string.Format("Autodash: {0}. Passed {1}, Failed: {2}",
                    suiteRun.TestSuiteSnapshot.Name, suiteRun.Result.PassedTotal, suiteRun.Result.FailedTotal
                    );
                
                emailTemplate.Replace("{{{{Summary}}}}", summary);
                emailTemplate.Replace("{{{{PassedTotal}}}}", suiteRun.Result.PassedTotal.ToString(CultureInfo.InvariantCulture));
                emailTemplate.Replace("{{{{FailedTotal}}}}", suiteRun.Result.FailedTotal.ToString(CultureInfo.InvariantCulture));

                UriBuilder uriBuilder = new UriBuilder(_websiteRoot);
                uriBuilder.Path = "/runs/" + Uri.EscapeDataString(suiteRun.Id) + "/report";                
                emailTemplate.Replace("{{{{ReportUrl}}}}", uriBuilder.ToString());

                uriBuilder.Path = "/runs" + Uri.EscapeDataString(suiteRun.Id) + "/report.html";
                emailTemplate.Replace("{{{{DownloadUrl}}}}", uriBuilder.ToString());

                msg.Subject = summary;
                msg.Body = emailTemplate.ToString();

                smtp.Send(msg);

                _logger.Info("Suite run completion email sent. " + summary);
            }
            catch (Exception ex)
            {
                string recipients = string.Join(", ", project.MemberEmails ?? Enumerable.Empty<string>());
                _logger.Error(ex, "Could not send suite run completed email to: " + recipients);
            }
        }
    }
}