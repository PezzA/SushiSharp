﻿# Board Cutter!
A demo implementation of the [Sushi Go!](https://boardgamegeek.com/boardgame/133473/sushi-go) card game in .Net.

[![.NET](https://github.com/PezzA/SushiSharp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/PezzA/SushiSharp/actions/workflows/dotnet.yml)

Main tech stack.

- [Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-8.0)
  - Some SSR content
  - Mostly interactive server content.
- [SignalR](https://learn.microsoft.com/en-us/aspnet/signalr/overview/getting-started/introduction-to-signalr)
  - Setup and managment of messaging to and from the client.
- [Akka.net](https://getakka.net/)
  - Manages concurrency and application state.
 
Authz/Authn is provided by the standard microsoft identity provider and is backed by a local
sqllite db.



