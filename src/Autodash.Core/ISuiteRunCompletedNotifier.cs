using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using NLog;

namespace Autodash.Core
{
    public interface ISuiteRunCompletedNotifier
    {
        void Notify(SuiteRun suiteRun);
    }

    public class SuiteRunCompletedEmailNotifier : ISuiteRunCompletedNotifier
    {
        private readonly IMongoDatabase _db;
        private readonly ILoggerWrapper _logger;
        private readonly ConcurrentQueue<SuiteRun> _notificationQueue = new ConcurrentQueue<SuiteRun>();

        private int _isInitialized;
        private IDisposable _subscription;

        public SuiteRunCompletedEmailNotifier(IMongoDatabase db, ILoggerProvider loggerProvider)
        {
            _db = db;
            _logger = loggerProvider.GetLogger(GetType().Name);
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
            var emailConfig = await _db.GetCollection<EmailConfiguration>("EmailConfiguration").FindAsync(new BsonDocument()).ToFirstOrDefaultAsync();
            if (emailConfig == null || string.IsNullOrEmpty(emailConfig.SmtpServer))
                return await Task.FromResult(Unit.Default);

            var projects = await _db.GetCollection<Project>("Project").FindAsync(new BsonDocument()).ToListAsync();

            try
            {
                SuiteRun suiteRun;

                using (SmtpClient smtp = new SmtpClient(emailConfig.SmtpServer, emailConfig.Port))
                {
                    while (_notificationQueue.TryDequeue(out suiteRun))
                    {
                        var project = projects.FirstOrDefault(n => n.Id == suiteRun.TestSuiteSnapshot.ProjectId);
                        if (project == null)
                            continue;

                        SendEmail(emailConfig, project, suiteRun, smtp);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not send suite run completed email");
            }
        }

        private void SendEmail(EmailConfiguration emailConfig, Project project, SuiteRun suiteRun, SmtpClient smtp)
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
                
                msg.Subject = summary;

                string body = "";
                body = body.Replace("{{{{Summary}}}}", summary)
                           .Replace("{{{{PassedTotal}}}}", suiteRun.Result.PassedTotal.ToString(CultureInfo.InvariantCulture))
                           .Replace("{{{{FailedTotal}}}}", suiteRun.Result.FailedTotal.ToString(CultureInfo.InvariantCulture))
                           .Replace("{{{{ReportUrl}}}}", "http://localhost:8080/runs/" + suiteRun.Id + "/report");

                msg.Body = body;

                smtp.Send(msg);
            }
            catch (Exception ex)
            {
                string recipients = string.Join(", ", project.MemberEmails ?? Enumerable.Empty<string>());
                _logger.Error(ex, "Could not send suite run completed email to: " + recipients);
            }
        }
    }

    public class EmailConfiguration
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string FromEmail { get; set; }

        public string SmtpServer { get; set; }
        public int Port { get; set; }

        public EmailConfiguration()
        {
            Port = 25;
            FromEmail = "noreply@autodash.org";
        }
    }
}
