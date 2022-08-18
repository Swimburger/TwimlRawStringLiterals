using System.Security;
using Twilio.AspNet.Core.MinimalApi;
using Twilio.TwiML;
using Twilio.TwiML.Voice;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var rawStringLiteralGroup = app.MapGroup("/raw-string-literals");
{
    rawStringLiteralGroup.MapGet("/", () => Results.Text("""
        <?xml version="1.0" encoding="utf-8"?>
        <Response>
            <Say>Ahoy matey!</Say>
            <Gather action="/come-aboard" method="GET" input="speech">
                <Say>
                    Would you like to come aboard?
                </Say>
            </Gather>
            <Say>We didn't receive any input. Goodbye!</Say>
        </Response>
        """,
        contentType: "application/xml"
    ));

    rawStringLiteralGroup.MapGet("/come-aboard", (string speechResult) => Results.Text($"""
        <?xml version="1.0" encoding="utf-8"?>
        <Response>
            <Say>
                {(speechResult.Equals("yes", StringComparison.OrdinalIgnoreCase)
                    ? "Welcome aboard!"
                    : "Maybe next time.")}     
            </Say>
        </Response>
        """ ,
        contentType: "application/xml"
    ));

    rawStringLiteralGroup.MapGet("/what-does-the-fox-say", () => Results.Text("""
        <?xml version="1.0" encoding="utf-8"?>
        <Response>
            <Gather action="/answer" method="GET" input="speech">
                <Say>
                    What does the fox say?
                </Say>
            </Gather>
            <Say>Ring-ding-ding-ding-dingeringeding!</Say>
        </Response>
        """,
        contentType: "application/xml"
    ));

    rawStringLiteralGroup.MapGet("/answer", (string speechResult) => Results.Text($"""
        <?xml version="1.0" encoding="utf-8"?>
        <Response>
            <Say>You said: {SecurityElement.Escape(speechResult)}</Say>
        </Response>
        """ ,
        contentType: "application/xml"
    ));

}


var twimlSdk = app.MapGroup("/twiml-sdk");
{
    twimlSdk.MapGet("/", () =>
    {
        var response = new VoiceResponse();
        response.Say("Ahoy matey!");
        var gather = new Gather(
            input: new List<Gather.InputEnum> {Gather.InputEnum.Speech},
            action: new Uri("/come-aboard", UriKind.Absolute),
            method: Twilio.Http.HttpMethod.Get
        );
        gather.Append(new Say("Would you like to come aboard?"));
        response.Append(gather);
        response.Say("We didn't receive any input. Goodbye!");
        return Results.Extensions.TwiML(response);
    });

    twimlSdk.MapGet("/come-aboard", (string speechResult) =>
    {
        var response = new VoiceResponse()
            .Say(speechResult.Equals("yes", StringComparison.OrdinalIgnoreCase)
                ? "Welcome aboard!"
                : "Maybe next time.");
        return Results.Extensions.TwiML(response);
    });

    twimlSdk.MapGet("/what-does-the-fox-say", () =>
    {
        var response = new VoiceResponse();
        var gather = new Gather(
            input: new List<Gather.InputEnum> {Gather.InputEnum.Speech},
            action: new Uri("/what-does-the-fox-say/answer", UriKind.Absolute),
            method: Twilio.Http.HttpMethod.Get
        );
        gather.Append(new Say("What does the fox say?"));
        response.Append(gather);
        response.Say("Ring-ding-ding-ding-dingeringeding!");
        return Results.Extensions.TwiML(response);
    });

    twimlSdk.MapGet("/what-does-the-fox-say/answer", (string speechResult) =>
    {
        var response = new VoiceResponse()
            .Say($"You said: {SecurityElement.Escape(speechResult)}");
        return Results.Extensions.TwiML(response);
    });
}


app.Run();