# BiteBot 🍕🤖

A Discord bot for managing restaurant recommendations and organizing group food orders. Built with .NET 9, Discord.Net, PostgreSQL, and Ollama AI integration.

## Features

- 🍽️ **Restaurant Management** - Add, update, delete, and search restaurants by city
- 🎲 **Random Suggestions** - Get random restaurant suggestions for group decisions
- 📝 **Order Organization** - Create dedicated order threads for coordinating group orders
- 🤖 **AI-Powered Summaries** - Automatically summarize orders using Ollama LLM
- 📊 **Audit Logging** - Track all restaurant changes with full audit history
- 🔍 **Autocomplete** - Smart autocomplete for restaurant selection

## Commands

All commands are slash commands in Discord. Type `/` in Discord to see available commands.

📋 **Quick Reference**: See [QUICK_REFERENCE.md](QUICK_REFERENCE.md) for a printable command cheat sheet.

### `/add` - Add a New Restaurant

Add a new restaurant to the database.

**Parameters:**
- `name` (required) - Restaurant name
- `city` (required) - City code: `-r` or `R` for Ramallah, `-n` or `N` for Nablus
- `url` (optional) - Restaurant website or menu URL

**Example:**
```
/add name:Pizza Hut city:-r url:https://pizzahut.ps
```

---

### `/suggest` - Get a Random Restaurant Suggestion

Get a random restaurant recommendation from a specific city.

**Parameters:**
- `city` (required) - City code: `-r` or `R` for Ramallah, `-n` or `N` for Nablus

**Example:**
```
/suggest city:-r
```

---

### `/update` - Update an Existing Restaurant

Update information for an existing restaurant.

**Parameters:**
- `restaurant` (required) - Select restaurant from autocomplete
- `name` (optional) - New restaurant name (leave empty to keep current)
- `city` (optional) - New city: `-r`/`R` for Ramallah, `-n`/`N` for Nablus (leave empty to keep current)
- `url` (optional) - New URL (leave empty to keep current, use `remove` to delete)

**Example:**
```
/update restaurant:Pizza Hut name:Pizza Hut Express
/update restaurant:Pizza Hut url:remove
```

---

### `/delete` - Delete a Restaurant

Remove a restaurant from the database.

**Parameters:**
- `restaurant` (required) - Select restaurant from autocomplete

**Example:**
```
/delete restaurant:Pizza Hut
```

---

### `/order` - Create an Order Thread

Create a dedicated thread channel for organizing a group food order.

**Parameters:**
- `restaurant` (required) - Select restaurant from autocomplete

**Example:**
```
/order restaurant:Pizza Hut
```

**What it does:**
- Creates a new thread in the current channel
- Thread name includes restaurant name and date
- Posts restaurant details and URL
- Mentions the order initiator

---

### `/summarize` - Summarize Orders with AI

Use AI to summarize all orders in an order thread. Must be used inside an order thread.

**Parameters:** None

**Example:**
```
/summarize
```

**What it does:**
- Collects all messages from the thread
- Sends them to Ollama AI for analysis
- Generates a formatted summary of all orders
- Posts the summary in the thread

---

## Setup

### Prerequisites

- Docker and Docker Compose
- Discord Bot Token (from [Discord Developer Portal](https://discord.com/developers/applications))
- Discord Guild (Server) ID

### Quick Start

1. Clone the repository
2. Copy `.env.example` to `.env` and fill in your Discord credentials
3. Run `docker-compose up -d`
4. Invite the bot to your server
5. Start using commands!

### Detailed Installation

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd BiteBot
   ```

2. **Create a `.env` file** in the same directory as `docker-compose.yml`:

   You can copy the example file:
   ```bash
   cp .env.example .env
   ```

   Then edit `.env` with your values:

   ```env
   # Discord Bot Configuration
   DiscordToken=YOUR_DISCORD_BOT_TOKEN_HERE
   GuildID=YOUR_DISCORD_GUILD_ID_HERE

   # Postgres Database Configuration
   POSTGRES_USER=bitebot-admin
   POSTGRES_PASSWORD=your-secure-password-here
   POSTGRES_DB=bitebot_db
   POSTGRES_PORT=5432

   # Database Connection String (for local development)
   DefaultConnection=Host=127.0.0.1;Port=5432;Database=bitebot_db;Username=bitebot-admin;Password=your-secure-password-here

   # Ollama AI Configuration
   OllamaUrl=http://localhost:11434
   OllamaModel=llama3.1
   OLLAMA_MODEL=llama3.1
   ```

   **Important:** Replace the following values:
   - `YOUR_DISCORD_BOT_TOKEN_HERE` - Your Discord bot token
   - `YOUR_DISCORD_GUILD_ID_HERE` - Your Discord server ID
   - `your-secure-password-here` - A secure password for PostgreSQL

3. **Get Discord Bot Token:**
   - Go to [Discord Developer Portal](https://discord.com/developers/applications)
   - Create a new application (or select existing)
   - Go to "Bot" section
   - Click "Reset Token" to get your bot token
   - Enable "Server Members Intent" and "Message Content Intent"
   - Copy the token to your `.env` file

4. **Get Guild ID:**
   - In Discord, enable Developer Mode (User Settings → Advanced → Developer Mode)
   - Right-click your server and select "Copy Server ID"
   - Paste the ID into your `.env` file

5. **Invite Bot to Your Server:**
   - In Discord Developer Portal, go to OAuth2 → URL Generator
   - Select scopes: `bot`, `applications.commands`
   - Select bot permissions: `Send Messages`, `Create Public Threads`, `Send Messages in Threads`, `Read Message History`
   - Copy the generated URL and open it in your browser
   - Select your server and authorize

6. **Start the services:**
   ```bash
   docker-compose up -d
   ```

   This will start:
   - PostgreSQL database
   - Ollama AI service (pulls llama3.1 model)
   - BiteBot Discord bot

7. **Verify the bot is online:**
   - Check your Discord server - the bot should appear online
   - Type `/` in any channel to see available commands

### Configuration Options

#### Changing the AI Model

To use a different Ollama model (e.g., `llama3`, `mistral`, `codellama`):

1. Update `OLLAMA_MODEL` in `.env`:
   ```env
   OLLAMA_MODEL=mistral
   ```

2. Restart the services:
   ```bash
   docker-compose down
   docker-compose up -d
   ```

#### Local Development (without Docker)

For local development with user secrets:

1. Install .NET 9 SDK
2. Install PostgreSQL locally
3. Install Ollama locally and pull a model
4. Set user secrets:
   ```bash
   cd BiteBot/BiteBot
   dotnet user-secrets set "DiscordToken" "YOUR_TOKEN"
   dotnet user-secrets set "GuildID" "YOUR_GUILD_ID"
   dotnet user-secrets set "DefaultConnection" "YOUR_CONNECTION_STRING"
   dotnet user-secrets set "OllamaUrl" "http://localhost:11434"
   dotnet user-secrets set "OllamaModel" "llama3.1"
   ```
5. Run the bot:
   ```bash
   dotnet run
   ```

### Stopping the Bot

```bash
docker-compose down
```

To also remove the database volume:
```bash
docker-compose down -v
```

---

## Architecture

### Technology Stack

- **Language:** C# (.NET 9)
- **Discord Library:** Discord.Net
- **Database:** PostgreSQL with Entity Framework Core
- **AI:** Ollama (Local LLM)
- **Containerization:** Docker & Docker Compose

### Project Structure

```
BiteBot/
├── Commands/              # Discord slash command handlers
├── Constants/             # Application constants
├── Data/                  # Entity Framework DbContext and migrations
├── Helpers/               # Utility classes (validation, parsing)
├── Interactions/          # Discord autocomplete handlers
├── Models/                # Data models and entities
├── Repositories/          # Data access layer
└── Services/              # Business logic services
```

### Key Services

- **Bot Service** - Discord client management and event handling
- **Restaurant Service** - Restaurant CRUD operations
- **Audit Service** - Tracks all changes to restaurants
- **AI Service** - Integration with Ollama for order summaries

---

## Database Schema

### Restaurants Table
- `Id` - Unique identifier (GUID)
- `Name` - Restaurant name
- `City` - Enum (Ramallah, Nablus)
- `Url` - Restaurant website/menu URL

### RestaurantAuditLogs Table
- `Id` - Unique identifier (GUID)
- `RestaurantId` - Reference to restaurant
- `Action` - Create, Update, or Delete
- `Timestamp` - When the action occurred
- `Username` - Discord username who performed the action
- `UserId` - Discord user ID
- `ChangeDetails` - JSON of what changed
- `ChangeDescription` - Human-readable description

---

## Troubleshooting

For detailed Docker commands and operations, see [DOCKER_COMMANDS.md](DOCKER_COMMANDS.md).

### Bot Not Responding
- Check bot is online in Discord
- Verify `DiscordToken` is correct in `.env`
- Check logs: `docker-compose logs bitebot`

### Database Connection Errors
- Verify PostgreSQL is running: `docker-compose ps`
- Check database credentials in `.env`
- Check logs: `docker-compose logs postgres`

### AI Summarize Not Working
- Verify Ollama is running: `docker-compose ps`
- Check if model is downloaded: `docker-compose logs ollama-setup`
- Try a smaller model if running out of memory

### Commands Not Showing
- Wait a few minutes after bot starts (Discord caches commands)
- Verify bot has `applications.commands` scope
- Check `GuildID` is correct in `.env`

---


## Support

For issues or questions, please open an issue on the repository.