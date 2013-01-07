# OpenTok API .NET
This is a .NET implementation of the OpenTok API. See complete documentation at http://www.tokbox.com/opentok/api/tools/documentation/api/server_side_libraries.html.

## Usage

### Configuration

Install from NuGet

```charp
PM > Install-Package OpenTokApi
```

```xml
<configuration>
 <appSettings>
    <add key="opentok.key" value="***API key***"/>
    <add key="opentok.secret" value="***API secret***"/>
    <add key="opentok.server" value="https://api.opentok.com"/>
    <add key="opentok.token.sentinel" value="T1=="/>
    <add key="opentok.sdk.version" value="opentokapi.net"/>
 </appSettings>
```

Explicitly set your OpenTok API key and secret.

### Generating Sessions

To generate an OpenTok session:

```csharp
var opentok = new OpenTok(); // will pull settings from config
string sessionId = opentok.CreateSession(Request.ServerVariables["REMOTE_ADDR"]);
```

To generate an OpenTok P2P session:

```csharp
var opentok = new OpenTok();
string sessionId = opentok.CreateSession(Request.ServerVariables["REMOTE_ADDR"], new { p2p_preference = "enabled" });
```

### Generating Tokens

To generate a session token:

```csharp
string token = opentok.GenerateToken(sessionId);
```

By default, the token has the "publisher" permission. To generate a token with a different set of permissions:

```csharp
string token = opentok.GenerateToken(sessionId, new { role = Roles.Moderator });
```

You can also pass in additional token options like "connection_data" and "expire_time":

```csharp
string token = opentok.GenerateToken(sessionId, new { connection_data = "id = 1", expire_time = DateTime.Now.AddDays(7) });
```
## Contributions

This library is ported from https://github.com/opentok/Opentok-.NET-SDK

## Note on Patches/Pull Requests

- Fork the project.
- Make your feature addition or bug fix.
- Add tests for it. This is important so I don't break it in a future version unintentionally.
- Send me a pull request. Bonus points for topic branches.