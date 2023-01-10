# Discord Binding Sample

A Dapr Pluggable Component sample that allows applications to bind to Discord and send and receive messages.

## Prerequisites

### Create a Discord Application and Bot

The bindings in this sample are exposed within Discord as an "application bot". This README will not replicate the comprehensive Discord documentation for creation of a bot available [here](https://discord.com/developers/docs/intro).

### Create a Discord Test Server

It is recommended that developers test application bots on a test (i.e. personal) Discord server; specific instructions are [here](https://support.discord.com/hc/en-us/articles/204849977-How-do-I-create-a-server-).

### Install the Application Bot on the Test Server

You can use the Discord Developer Portal's OAuth2::URL Generator to generate a URL that installs the application bot on the test server.  More instructions are [here](),https://discord.com/developers/docs/getting-started#installing-your-app. 

> NOTE: This sample requires the bot to have the `bot` scope and `Read Messages/View Channels` and `Send Messages` bot permissions on any associated server.

## Configuration

An application that binds to the component should have a Pluggable Component configuration file that looks like the following:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: discord-binding
spec:
  type: bindings.discord-binding
  version: v1
  metadata:
  - name: token
    value: "<discord bot token>"
```

> NOTE: To prevent tokens from being mistakenly committed to a repro, it is best to use Dapr secrets components to store the token and then simply reference it from the binding component configuration.

## Application

The application can send messages to Discord, using HTTP or the Dapr Client SDK, by invoking the `SendMessage` operation of the output binding with JSON data:

```json
{
    "channelId": "<channel ID>",
    "content": "<text to send>"
}
```

An example HTTP request would look like:

`POST http://localhost:<dapr port>/v1.0/bindings/discord-binding`

```json
{
    "data": {         
        "channelId": "1234567890123456789",
        "content": "Hello world from Dapr!"
    },
    "operation": "SendMessage"
}
```

>NOTE: The channel ID can be found by [enabling developer mode](https://discord.com/developers/docs/game-sdk/store#application-test-mode) in Discord, right-clicking the channel name, and selecting `Copy ID` from the context menu.

The application can read messages sent to Discord by exposing a `POST` route that matches the binding name (i.e. `discord-binding`), and accepts a JSON body of the form:

```json
{
    "channelId": "<channel ID>",
    "content": "<text to send>"
}
```
