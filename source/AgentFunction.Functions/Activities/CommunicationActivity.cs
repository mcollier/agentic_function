using AgentFunction.Functions.Agents;
using AgentFunction.Functions.Models;

using Azure.Communication.Email;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AgentFunction.Functions.Activities;

public class CommunicationActivity(CommsAgent commsAgent, EmailClient emailClient)
{
    [Function(nameof(RunComms))]
    public async Task<CommsResult> RunComms(
        [ActivityTrigger] ClaimAnalysisReport report,
        FunctionContext context)
    {
        ILogger logger = context.GetLogger(nameof(RunComms));

        return await commsAgent.ProcessAsync(report);
    }

    [Function(nameof(Send))]
    public async Task Send(
        [ActivityTrigger] CommsResult comms,
        FunctionContext context)
    {
        ILogger logger = context.GetLogger(nameof(Send));

        string senderEmailAddress = Environment.GetEnvironmentVariable("SENDER_EMAIL_ADDRESS")
            ?? throw new InvalidOperationException("Sender email address is not set.");

        var emailMessage = new EmailMessage(
            senderAddress: senderEmailAddress,
            content: new Azure.Communication.Email.EmailContent(subject: comms.Email.Subject ?? "No Subject")
            {
                PlainText = "Plain text version of the email.",
                Html = comms.Email.Body ?? "<p>No content</p>"
            },
            recipients: new EmailRecipients(
            [
                new EmailAddress(comms.Email.RecipientEmailAddress,
                                 comms.Email.RecipientName ?? "Valued Customer")
            ])
        );

        var emailSendOperation = await emailClient.SendAsync(
            wait: Azure.WaitUntil.Completed,
            message: emailMessage
        );

        await Task.CompletedTask;
    }
}