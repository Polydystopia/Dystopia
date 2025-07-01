You have questions or want to contribute? -> [Join our discord](discord.gg/rtwgWTzxWy)

🚧🚧🚧 Under heavy construction 🚧🚧🚧

# Dystopia
This is a C#–based backend server emulator for The Battle of Polytopia.
Our goal is to deliver a complete, 100 % complete reimplementation of every official feature.
Looking ahead, we also plan to introduce unofficial enhancements—such as mod support or a web-based UI.

# How to build
1. Gain access to private repo with libs or recreate libs directory
2. `git submodule update`
3. run

## Steam
To fetch a Steam user’s username during your login flow, you must supply a valid Steam Web API key.

---

### 1. Obtain Your Key

1. Go to the Steam Web API portal and sign in with your Steam account:  
   https://steamcommunity.com/dev
2. Register your domain and request an API key.
3. Copy the issued key.

---

### 2. Configure Your .NET App

Add your API key to the ASP.NET application configuration.

#### Example using  `appsettings.json`

```json
{
  "Steam": {
    "ApiKey": "YOUR_STEAM_API_KEY"
  }
}
```

# How to contribute
We welcome contributions! Just choose an open issue and submit a pull request.
If you’d like to suggest a new feature, please open a discussion or get in touch with us.

# Progress
Under heavy construction

## Feature support
Legend:
🟢 (Supported), 🟡 (partially supported), 🔴 (unsupported yet)

| Feature           | Status |
|-------------------|--------|
| Lobby             | 🟢     |
| Ingame            | 🟢     |
| Matchmaking       | 🟢     |
| Friends           | 🟢     |
| Private Matches   | 🟢     |
| Own Profile       | 🟡     |
| Replays           | 🔴     |
| Leaderboards      | 🔴     |
| Tournaments       | 🔴     |
| Weekly challenges | 🔴     |

### Tribes
Legend:
🟢 (Supported), 🟡 (won´t add)

| Tribe   | Status |
|---------|--------|
| Normal  | 🟢     |
| Special | 🟡     |

### Skins
Legend:
🟡 (won´t add)

| Tribe   | Status |
|---------|--------|
| All     | 🟡     |

## Game version support
Legend:
🟢 (Supported), 🟡 (Supported, untested), 🔴 (unsupported), 🌉 (IL2CPP interop)

| Version | Status |
|--------|-------|
| 105-112 | 🟡🌉  | 
| 104    | 🟢    |
| 51-103 | 🟡    |
| <51    | 🔴    |

# Contact
[discord.gg/rtwgWTzxWy](discord.gg/rtwgWTzxWy)

[polydystopia@juli.gg](mailto:polydystopia@juli.gg)
