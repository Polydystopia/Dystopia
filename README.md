You have questions or want to contribute? -> [Join our discord](discord.gg/rtwgWTzxWy)

🚧🚧🚧 Under heavy construction 🚧🚧🚧

# Dystopia
This is a C#–based backend server emulator for The Battle of Polytopia.
Our goal is to deliver a complete, 100 % reimplementation of every official feature.
Looking ahead, we also plan to introduce unofficial enhancements—such as mod support or a web-based UI.

# How to join

### Supported OSes
Legend: 🟢 (Fully supported), 🟠 (Urls with exactly 28 characters supported, untested) 🟡 (Urls with exactly 28 characters supported), 🚧 (under progress), 🔴 (Not supported)
| OS/Platform      | Status |
|------------------|--------|
| Windows          | 🟢     |
| Linux            | 🟡     |
| MacOS            | 🟠     |
| Android          | 🚧     |
| iOS              | 🔴     |
| Nintendo Switch  | 🔴     |
| Tesla            | 🔴     |

## Windows:
1. Download BepInEx from [this link](https://builds.bepinex.dev/projects/bepinex_be/738/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.738%2Baf0cba7.zip) or download the latest BepInEx, Il2cpp, Windows, x64 from [BepInBuilds](https://builds.bepinex.dev/projects/bepinex_be) and extract it into your game folder. (usually at `C:\Program Files (x86)\Steam\steamapps\common\The Battle of Polytopia\`)
2. Start the game once. This will open a console, and it can take around 5 minutes on the first launch.
3. Download the latest release of [DystopiaBepInEx](https://github.com/Polydystopia/DystopiaBepInEx/releases/tag/v0.1) and extract it anywhere.
4. Place the dll inside of BepInEx\Plugins in your game folder.
5. Start the game again.
6. There will be a file `polydistopia_server_config.json` in the game folder. Edit this to have the server address you want.

## Linux
1. Find the game folder. This can be at:
    - `~/.steam/steam/steamapps/common/The Battle of Polytopia/`, if you installed the official deb
    - Somewhere under `/snap/` if you installed the snap
    - Under `~/.var/run/` if you installed the flatpak
    - Anywhere else, use `find / -type d -path "*/steamapps/common" 2>/dev/null` to find it.
2. Patch `global-metadata.dat`, remeber to replace `<PLACE ADRESS HERE>` with your chosen server adress. Note: this server address must be **exactly** 28 characters. Run this in a terminal at the game folder:
```sh
server_adress="<PLACE ADRESS HERE>"
if [ ${#server_adress -eq 28 ]; then
    sed -i "s#https://polytopia-prod.net/#$server_adress" ./Polytopia_Data/il2cpp_data/Metadata/global-metadata.dat && echo "Patched successfully!" || echo "Failed to patch, does the file exist and is it not already patched?"
else
    echo "Server url must be EXACTLY 28 characters
fi
```
3. Run the game. You will not notice anything. For verification Dystopia is installed, look at the news.

## MacOS
Similar to Linux. instructions TODO

# How to run

## Build
You need to put game files into the root/lib/ folder.
Ensure platform‐specific files are placed in the correct Windows or Linux subfolder.

The structure is the following:

```
root/
└── lib/
    ├── BepInEx/
    │   └── core/
    │       ├── windows/ <-- put BepInEx files here (Gameroot/BepInEx/core)
    │       └── linux/ <-- put BepInEx files here (Gameroot/BepInEx/core)
    │
    ├── Data/
    │   ├── AvatarData/ <-- put dumped avatar data here
    │   └── GameLogicData/ <-- put dumped game logic here (1.txt .. n.txt)
    │
    ├── Managed/
    │   └── interop/ <-- put managed dlls here (Gameroot/Polytopia_Data/Managed)
    │
    └── native/
        ├── windows/ 
        │   ├── GameAssembly.dll <-- put native dll here (Gameroot)
        │   ├── baselib.dll <-- put native dll here (Gameroot)
        │   ├── Data/
        │   │   └── Metadata/
        │   │       └── global-metadata.dat <-- put native one here (Gameroot/Data/Metadata/)
        │   └── interop/ <-- put native Il2CPPInterop files here (Gameroot/BepInEx/interop)
        │
        └── linux/
            ├── GameAssembly.so <-- put native so here (Gameroot)
            ├── Data/
            │   └── Metadata/
            │       └── global-metadata.dat <-- put native one here (Gameroot/Data/Metadata/)
            └── interop/ <-- put native Il2CPPInterop files here (Gameroot/BepInEx/interop)

```

## After build

### Magic
Put all files of DystopiaMagicOutputBin/ in DystopiaOutputBin/Native/Magic/.

### Steam
To fetch a Steam user’s username during your login flow, you must supply a valid Steam Web API key.

---

#### 1. Obtain Your Key

1. Go to the Steam Web API portal and sign in with your Steam account:  
   https://steamcommunity.com/dev
2. Register your domain and request an API key.
3. Copy the issued key.

---

#### 2. Configure Your .NET App

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
| 113 (beta) | 🔴 | 
| 105-112 | 🟡🌉 | 
| 104    | 🟢    |
| 51-103 | 🟡    |
| <51    | 🔴    |

## Login
Legend:
🟢 (Supported), 🔴 (unsupported)

| Type | Status |
|--------|---|
| Steam | 🟢 | 
| Android | 🔴 |
| IOS | 🔴 |
| Switch | 🔴 |
| Tesla | 🔴 |

## Server operating system
Legend:
🟢 (Supported), 🔴 (unsupported)

| Type | Status |
|--------|---|
| Windows | 🟢 | 
| Linux | 🟢 |
| MacOS | 🔴 |

# Contact
[discord.gg/rtwgWTzxWy](discord.gg/rtwgWTzxWy)

[polydystopia@juli.gg](mailto:polydystopia@juli.gg)
