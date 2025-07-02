# CCFlockCLI

**CCFlockCLI** is a cross-platform .NET-powered command-line tool for managing APIs and interacting with cloud-connected services in a secure and modular way. It supports encrypted API key storage, JWT decoding, jokes, soccer data, email triggers, and weather lookups—all from your terminal.

---

## 🌟 Features

- 🔐 **Encrypted API Key Management**  
  Securely stores and encrypts your API keys with AES-256 and OS-level protections.

- 📧 **Google Mail Triggering**  
  Basic placeholder logic for initiating Gmail-related tasks.

- ⚽ **Soccer API Interaction**  
  Fetch teams, players, and leagues using flexible command-line options like:
  - `-a`, `--all`
  - `-r`, `--random`
  - `-numX`, `-asc`, `-desc`, etc.

- 🤣 **Joke Generator**  
  Pulls random jokes from an external source to keep you entertained.

- 🌦️ **Weather API Access**  
  Retrieves local or ranged weather data for your system or specified location.

- 🧪 **JWT Utilities**  
  Decode and inspect JSON Web Tokens from any source.

- 🐍 **Snake Game**  
  Play an ANSI-colored snake game directly in your terminal.

---

## 🔧 Installation

### 💻 macOS (via Homebrew)

```bash
brew tap shubh1229/ccflock
brew install ccflock
```

### 🪟 Windows

Build using the instructions below, or use a prebuilt executable from [Releases](https://github.com/Shubh1229/CCFlockCLI/releases).

---

## 🛠 Build Instructions

```bash
git clone https://github.com/Shubh1229/CCFlockCLI.git
cd CCFlockCLI

# For a self-contained executable
dotnet publish -c Release -r osx-arm64 --self-contained true -o publish

# On Windows, use:
# dotnet publish -c Release -r win-x64 --self-contained true -o publish
```

Executable will be located in the `publish/` directory as `ccflock`.

---

## 🔐 Security Model

All secrets are stored inside:

```
~/.ccflock/APIsecrets.json         # Encrypted API entries
~/.ccflock/APIsecretKeyAPIKEY.txt  # Encrypted key file for AES
```

- Uses AES with a 256-bit key generated per user.
- Adds optional Windows `File.Encrypt()` protection.
- Secrets are serialized using `System.Text.Json`.
- Keys are not stored in plain-text once encrypted.

---

## 🧪 Example Usage

```bash
ccflock
# Opens the main CLI menu. Supports navigation via input commands.
```

Example commands:
```bash
ccflock soccer -r -num5
ccflock joke
ccflock weather --city "New York"
ccflock jwt --decode "YOUR.JWT.TOKEN"
ccflock snake
```

---

## 📁 File Structure Overview

| File                    | Purpose                                |
|-------------------------|----------------------------------------|
| `Program.cs`            | Entry point and CLI dispatcher         |
| `SecretProtector.cs`    | AES encryption/decryption logic        |
| `KeyAPI.cs`             | Manages encrypted key creation         |
| `GoogleMailAPI.cs`      | Placeholder for Gmail API features     |
| `SoccerAPI.cs`          | Soccer data access + CLI parsing       |
| `RunSoccerAPI.cs`       | Handles soccer CLI interactions        |
| `WeatherAPI.cs`         | Handles weather lookups                |
| `JokeAPI.cs`            | Fetches jokes from an API              |
| `JWTokenDecoder.cs`     | Decode or generate JWTs                |
| `SnakeGame.cs`          | Snake game rendering and logic         |
| `SerializeJSON.cs`      | Pretty JSON formatting utility         |
| `JsonContext.cs`        | JSON source generator context (System.Text.Json) |

---

## 👤 Author

Arihant Singh  
GitHub: [@Shubh1229](https://github.com/Shubh1229)

---

## 📝 License

Licensed under the Apache 2.0 License. See `LICENSE` for details.
