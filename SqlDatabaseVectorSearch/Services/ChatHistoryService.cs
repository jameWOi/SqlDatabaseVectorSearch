using Microsoft.JSInterop;
using System.Text.Json;

public class ChatSession
{
    public Guid ConversationId { get; set; }
    public string? Title { get; set; }
    public DateTime Created { get; set; }
}

public class ChatHistoryService
{
    private const string SessionsKey = "chat-sessions";
    private readonly IJSRuntime js;

    public ChatHistoryService(IJSRuntime js)
    {
        this.js = js;
    }

    public async Task<List<ChatSession>> GetSessionsAsync()
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", SessionsKey);
        return string.IsNullOrWhiteSpace(json)
            ? new List<ChatSession>()
            : JsonSerializer.Deserialize<List<ChatSession>>(json) ?? new List<ChatSession>();
    }

    public async Task AddOrUpdateSessionAsync(ChatSession session)
    {
        var sessions = await GetSessionsAsync();
        var existing = sessions.FindIndex(s => s.ConversationId == session.ConversationId);
        if (existing >= 0)
        {
            sessions[existing] = session;
        }
        else
        {
            sessions.Insert(0, session); // newest first
        }

        await js.InvokeVoidAsync("localStorage.setItem", SessionsKey, JsonSerializer.Serialize(sessions));
    }

    public async Task RemoveSessionAsync(Guid conversationId)
    {
        var sessions = await GetSessionsAsync();
        sessions.RemoveAll(s => s.ConversationId == conversationId);
        await js.InvokeVoidAsync("localStorage.setItem", SessionsKey, JsonSerializer.Serialize(sessions));
    }
}
