# BiteBot Quick Reference Card

## City Codes

When using commands that require a city parameter, use these codes:

| City | Code Options |
|------|--------------|
| Ramallah | `-r` or `R` |
| Nablus | `-n` or `N` |

## Command Quick Reference

| Command | Purpose | Example |
|---------|---------|---------|
| `/add` | Add new restaurant | `/add name:Pizza Hut city:-r url:https://example.com` |
| `/suggest` | Get random restaurant | `/suggest city:-r` |
| `/update` | Update restaurant info | `/update restaurant:Pizza Hut name:New Name` |
| `/delete` | Delete restaurant | `/delete restaurant:Pizza Hut` |
| `/order` | Create order thread | `/order restaurant:Pizza Hut` |
| `/summarize` | AI order summary | `/summarize` (must be in an order thread) |

## Tips

- 🔍 **Autocomplete**: Start typing restaurant names and the bot will suggest matches
- 🔗 **URLs**: URLs are optional but helpful for sharing menus
- ✏️ **Updating**: Leave parameters empty to keep current values
- 🗑️ **Removing URLs**: Use `url:remove` to delete a restaurant's URL
- 🧵 **Order Threads**: Use order threads to keep discussions organized
- 🤖 **AI Summaries**: The bot reads all messages in the thread and creates a summary

## Common Workflows

### Adding Your First Restaurant
1. Use `/add` with name, city code, and optional URL
2. Example: `/add name:Shawarma King city:-r url:https://example.com`

### Organizing a Group Order
1. Use `/order` and select a restaurant
2. Bot creates a dedicated thread
3. Team members post their orders in the thread
4. Use `/summarize` to get AI-generated summary
5. Share summary with restaurant

### Finding a Restaurant Suggestion
1. Use `/suggest` with your city code
2. Bot picks a random restaurant from that city
3. Perfect for when the team can't decide!

### Updating Restaurant Info
1. Use `/update` and select the restaurant from autocomplete
2. Provide only the fields you want to change
3. Example: `/update restaurant:Pizza Hut url:https://newurl.com`

---

💡 **Pro Tip**: Pin this reference in your Discord channel for easy access!

