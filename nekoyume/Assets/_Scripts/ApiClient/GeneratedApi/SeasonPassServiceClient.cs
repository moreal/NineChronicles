//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Do not modify the contents of this file directly.
//     Changes might be overwritten the next time the code is generated.
//     Source URL: https://9s6qapxbef.execute-api.us-east-2.amazonaws.com/internal/openapi.json
// </auto-generated>
//------------------------------------------------------------------------------
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Text;
using System.Linq;

public class SeasonPassServiceClient
{
    private string Url;

    public SeasonPassServiceClient(string url)
    {
        Url = url;
    }

    public void Dispose()
    {
    }

    [JsonConverter(typeof(ActionTypeTypeConverter))]
    public enum ActionType
    {
        hack_and_slash,
        hack_and_slash_sweep,
        battle_arena,
        raid,
        event_dungeon,
        wanted,
        explore_adventure_boss,
        sweep_adventure_boss,
    }

    public class ActionTypeTypeConverter : JsonConverter<ActionType>
    {
        public static readonly Dictionary<string, string> InvalidEnumMapping = new Dictionary<string, string>
        {
        };
        public override ActionType Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Number => (ActionType)reader.GetInt32(),
                JsonTokenType.String => Enum.Parse<ActionType>(InvalidEnumMapping.TryGetValue(reader.GetString(), out var validName) ? validName : reader.GetString()),
                _ => throw new JsonException(
                    $"Expected token type to be {string.Join(" or ", new[] { JsonTokenType.Number, JsonTokenType.String })} but got {reader.TokenType}")
            };
        }
        public override void Write(
            Utf8JsonWriter writer,
            ActionType value,
            JsonSerializerOptions options)
        {
            var enumString = value.ToString();
            if (InvalidEnumMapping.ContainsValue(enumString))
            {
                enumString = InvalidEnumMapping.First(kvp => kvp.Value == enumString).Key;
            }
            writer.WriteStringValue(enumString);
        }
    }

    public class ClaimRequestSchema
    {
        [JsonPropertyName("planet_id")]
        public PlanetID? PlanetId { get; set; }
        [JsonPropertyName("agent_addr")]
        public string AgentAddr { get; set; }
        [JsonPropertyName("avatar_addr")]
        public string AvatarAddr { get; set; }
        [JsonPropertyName("pass_type")]
        public PassType PassType { get; set; }
        [JsonPropertyName("season_index")]
        public int SeasonIndex { get; set; }
        [JsonPropertyName("force")]
        public bool Force { get; set; }
        [JsonPropertyName("prev")]
        public bool Prev { get; set; }
    }

    public class ClaimResultSchema
    {
        [JsonPropertyName("user")]
        public UserSeasonPassSchema User { get; set; }
        [JsonPropertyName("reward_list")]
        public List<ClaimSchema> RewardList { get; set; }
    }

    public class ClaimSchema
    {
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; }
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
        [JsonPropertyName("decimal_places")]
        public int DecimalPlaces { get; set; }
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public class CurrencyInfoSchema
    {
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }

    public class ExpInfoSchema
    {
        [JsonPropertyName("action_type")]
        public ActionType ActionType { get; set; }
        [JsonPropertyName("exp")]
        public int Exp { get; set; }
    }

    public class ExpRequestSchema
    {
        [JsonPropertyName("planet_id")]
        public PlanetID? PlanetId { get; set; }
        [JsonPropertyName("avatar_addr")]
        public string AvatarAddr { get; set; }
        [JsonPropertyName("pass_type")]
        public PassType PassType { get; set; }
        [JsonPropertyName("season_index")]
        public int SeasonIndex { get; set; }
        [JsonPropertyName("exp")]
        public int Exp { get; set; }
    }

    public class HTTPValidationError
    {
        [JsonPropertyName("detail")]
        public List<ValidationError> Detail { get; set; }
    }

    public class ItemInfoSchema
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }

    public class LevelInfoSchema
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }
        [JsonPropertyName("exp")]
        public int Exp { get; set; }
    }

    [JsonConverter(typeof(PassTypeTypeConverter))]
    public enum PassType
    {
        CouragePass,
        AdventureBossPass,
        WorldClearPass,
    }

    public class PassTypeTypeConverter : JsonConverter<PassType>
    {
        public static readonly Dictionary<string, string> InvalidEnumMapping = new Dictionary<string, string>
        {
        };
        public override PassType Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Number => (PassType)reader.GetInt32(),
                JsonTokenType.String => Enum.Parse<PassType>(InvalidEnumMapping.TryGetValue(reader.GetString(), out var validName) ? validName : reader.GetString()),
                _ => throw new JsonException(
                    $"Expected token type to be {string.Join(" or ", new[] { JsonTokenType.Number, JsonTokenType.String })} but got {reader.TokenType}")
            };
        }
        public override void Write(
            Utf8JsonWriter writer,
            PassType value,
            JsonSerializerOptions options)
        {
            var enumString = value.ToString();
            if (InvalidEnumMapping.ContainsValue(enumString))
            {
                enumString = InvalidEnumMapping.First(kvp => kvp.Value == enumString).Key;
            }
            writer.WriteStringValue(enumString);
        }
    }

    [JsonConverter(typeof(PlanetIDTypeConverter))]
    public enum PlanetID
    {
        _0x000000000000,
        _0x000000000001,
        _0x000000000002,
        _0x000000000003,
        _0x100000000000,
        _0x100000000001,
        _0x100000000002,
        _0x100000000003,
    }

    public class PlanetIDTypeConverter : JsonConverter<PlanetID>
    {
        public static readonly Dictionary<string, string> InvalidEnumMapping = new Dictionary<string, string>
        {
            { "0x000000000000", "_0x000000000000" },
            { "0x000000000001", "_0x000000000001" },
            { "0x000000000002", "_0x000000000002" },
            { "0x000000000003", "_0x000000000003" },
            { "0x100000000000", "_0x100000000000" },
            { "0x100000000001", "_0x100000000001" },
            { "0x100000000002", "_0x100000000002" },
            { "0x100000000003", "_0x100000000003" },
        };
        public override PlanetID Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Number => (PlanetID)reader.GetInt32(),
                JsonTokenType.String => Enum.Parse<PlanetID>(InvalidEnumMapping.TryGetValue(reader.GetString(), out var validName) ? validName : reader.GetString()),
                _ => throw new JsonException(
                    $"Expected token type to be {string.Join(" or ", new[] { JsonTokenType.Number, JsonTokenType.String })} but got {reader.TokenType}")
            };
        }
        public override void Write(
            Utf8JsonWriter writer,
            PlanetID value,
            JsonSerializerOptions options)
        {
            var enumString = value.ToString();
            if (InvalidEnumMapping.ContainsValue(enumString))
            {
                enumString = InvalidEnumMapping.First(kvp => kvp.Value == enumString).Key;
            }
            writer.WriteStringValue(enumString);
        }
    }

    public class PremiumRequestSchema
    {
        [JsonPropertyName("planet_id")]
        public PlanetID? PlanetId { get; set; }
        [JsonPropertyName("avatar_addr")]
        public string AvatarAddr { get; set; }
        [JsonPropertyName("pass_type")]
        public PassType PassType { get; set; }
        [JsonPropertyName("season_index")]
        public int SeasonIndex { get; set; }
        [JsonPropertyName("is_premium")]
        public bool IsPremium { get; set; }
        [JsonPropertyName("is_premium_plus")]
        public bool IsPremiumPlus { get; set; }
    }

    public class RegisterRequestSchema
    {
        [JsonPropertyName("planet_id")]
        public PlanetID? PlanetId { get; set; }
        [JsonPropertyName("agent_addr")]
        public string AgentAddr { get; set; }
        [JsonPropertyName("avatar_addr")]
        public string AvatarAddr { get; set; }
        [JsonPropertyName("pass_type")]
        public PassType PassType { get; set; }
        [JsonPropertyName("season_index")]
        public int SeasonIndex { get; set; }
    }

    public class RewardDetailSchema
    {
        [JsonPropertyName("item")]
        public List<ItemInfoSchema> Item { get; set; }
        [JsonPropertyName("currency")]
        public List<CurrencyInfoSchema> Currency { get; set; }
    }

    public class RewardSchema
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }
        [JsonPropertyName("normal")]
        public RewardDetailSchema Normal { get; set; }
        [JsonPropertyName("premium")]
        public RewardDetailSchema Premium { get; set; }
    }

    public class SeasonChangeRequestSchema
    {
        [JsonPropertyName("pass_type")]
        public PassType PassType { get; set; }
        [JsonPropertyName("season_index")]
        public int SeasonIndex { get; set; }
        [JsonPropertyName("start_timestamp")]
        public string? StartTimestamp { get; set; }
        [JsonPropertyName("end_timestamp")]
        public string? EndTimestamp { get; set; }
    }

    public class SeasonPassSchema
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("pass_type")]
        public PassType PassType { get; set; }
        [JsonPropertyName("season_index")]
        public int SeasonIndex { get; set; }
        [JsonPropertyName("start_date")]
        public string? StartDate { get; set; }
        [JsonPropertyName("end_date")]
        public string? EndDate { get; set; }
        [JsonPropertyName("start_timestamp")]
        public string? StartTimestamp { get; set; }
        [JsonPropertyName("end_timestamp")]
        public string? EndTimestamp { get; set; }
        [JsonPropertyName("reward_list")]
        public List<RewardSchema> RewardList { get; set; }
        [JsonPropertyName("repeat_last_reward")]
        public bool RepeatLastReward { get; set; }
    }

    public class SimpleSeasonPassSchema
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("pass_type")]
        public PassType PassType { get; set; }
        [JsonPropertyName("season_index")]
        public int SeasonIndex { get; set; }
    }

    public class UpgradeRequestSchema
    {
        [JsonPropertyName("planet_id")]
        public PlanetID? PlanetId { get; set; }
        [JsonPropertyName("agent_addr")]
        public string AgentAddr { get; set; }
        [JsonPropertyName("avatar_addr")]
        public string AvatarAddr { get; set; }
        [JsonPropertyName("pass_type")]
        public string? PassType { get; set; }
        [JsonPropertyName("season_index")]
        public int SeasonIndex { get; set; }
        [JsonPropertyName("is_premium")]
        public bool IsPremium { get; set; }
        [JsonPropertyName("is_premium_plus")]
        public bool IsPremiumPlus { get; set; }
        [JsonPropertyName("g_sku")]
        public string GSku { get; set; }
        [JsonPropertyName("a_sku")]
        public string ASku { get; set; }
        [JsonPropertyName("reward_list")]
        public List<ClaimSchema> RewardList { get; set; }
    }

    public class UserSeasonPassSchema
    {
        [JsonPropertyName("planet_id")]
        public PlanetID PlanetId { get; set; }
        [JsonPropertyName("agent_addr")]
        public string AgentAddr { get; set; }
        [JsonPropertyName("avatar_addr")]
        public string AvatarAddr { get; set; }
        [JsonPropertyName("season_pass")]
        public SimpleSeasonPassSchema SeasonPass { get; set; }
        [JsonPropertyName("level")]
        public int Level { get; set; }
        [JsonPropertyName("exp")]
        public int Exp { get; set; }
        [JsonPropertyName("is_premium")]
        public bool IsPremium { get; set; }
        [JsonPropertyName("is_premium_plus")]
        public bool IsPremiumPlus { get; set; }
        [JsonPropertyName("last_normal_claim")]
        public int LastNormalClaim { get; set; }
        [JsonPropertyName("last_premium_claim")]
        public int LastPremiumClaim { get; set; }
        [JsonPropertyName("claim_limit_timestamp")]
        public string? ClaimLimitTimestamp { get; set; }
    }

    public class ValidationError
    {
        [JsonPropertyName("loc")]
        public List<string?> Loc { get; set; }
        [JsonPropertyName("msg")]
        public string Msg { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public async Task GetPingAsync(Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/ping";
        using (var request = new UnityWebRequest(url, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                onSuccess?.Invoke(responseBody);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetSeasonpassCurrentAsync(string planet_id, PassType pass_type, Action<SeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/season-pass/current";
        using (var request = new UnityWebRequest(url, "GET"))
        {
            url += $"?planet_id={planet_id}&pass_type={pass_type}";
            request.uri = new Uri(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                SeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<SeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetSeasonpassLevelAsync(PassType pass_type, Action<LevelInfoSchema[]> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/season-pass/level";
        using (var request = new UnityWebRequest(url, "GET"))
        {
            url += $"?pass_type={pass_type}";
            request.uri = new Uri(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                LevelInfoSchema[] result = System.Text.Json.JsonSerializer.Deserialize<LevelInfoSchema[]>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetSeasonpassExpAsync(PassType pass_type, int season_index, Action<ExpInfoSchema[]> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/season-pass/exp";
        using (var request = new UnityWebRequest(url, "GET"))
        {
            url += $"?pass_type={pass_type}&season_index={season_index}";
            request.uri = new Uri(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                ExpInfoSchema[] result = System.Text.Json.JsonSerializer.Deserialize<ExpInfoSchema[]>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetUserStatusAsync(string planet_id, string agent_addr, string avatar_addr, PassType pass_type, int season_index, Action<UserSeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/user/status";
        using (var request = new UnityWebRequest(url, "GET"))
        {
            url += $"?planet_id={planet_id}&agent_addr={agent_addr}&avatar_addr={avatar_addr}&pass_type={pass_type}&season_index={season_index}";
            request.uri = new Uri(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                UserSeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<UserSeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetUserStatusAllAsync(string planet_id, string agent_addr, string avatar_addr, Action<UserSeasonPassSchema[]> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/user/status/all";
        using (var request = new UnityWebRequest(url, "GET"))
        {
            url += $"?planet_id={planet_id}&agent_addr={agent_addr}&avatar_addr={avatar_addr}";
            request.uri = new Uri(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                UserSeasonPassSchema[] result = System.Text.Json.JsonSerializer.Deserialize<UserSeasonPassSchema[]>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostUserUpgradeAsync(string authorization, UpgradeRequestSchema requestBody, Action<UserSeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/user/upgrade";
        using (var request = new UnityWebRequest(url, "POST"))
        {
            request.uri = new Uri(url);
            request.SetRequestHeader("authorization", authorization.ToString());
            var bodyString = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var jsonToSend = new UTF8Encoding().GetBytes(bodyString);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                UserSeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<UserSeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostUserClaimAsync(ClaimRequestSchema requestBody, Action<ClaimResultSchema> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/user/claim";
        using (var request = new UnityWebRequest(url, "POST"))
        {
            var bodyString = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var jsonToSend = new UTF8Encoding().GetBytes(bodyString);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                ClaimResultSchema result = System.Text.Json.JsonSerializer.Deserialize<ClaimResultSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostUserClaimprevAsync(ClaimRequestSchema requestBody, Action<ClaimResultSchema> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/user/claim-prev";
        using (var request = new UnityWebRequest(url, "POST"))
        {
            var bodyString = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var jsonToSend = new UTF8Encoding().GetBytes(bodyString);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                ClaimResultSchema result = System.Text.Json.JsonSerializer.Deserialize<ClaimResultSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostTmpRegisterAsync(RegisterRequestSchema requestBody, Action<UserSeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/tmp/register";
        using (var request = new UnityWebRequest(url, "POST"))
        {
            var bodyString = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var jsonToSend = new UTF8Encoding().GetBytes(bodyString);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                UserSeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<UserSeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostTmpPremiumAsync(PremiumRequestSchema requestBody, Action<UserSeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/tmp/premium";
        using (var request = new UnityWebRequest(url, "POST"))
        {
            var bodyString = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var jsonToSend = new UTF8Encoding().GetBytes(bodyString);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                UserSeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<UserSeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostTmpAddexpAsync(ExpRequestSchema requestBody, Action<UserSeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/tmp/add-exp";
        using (var request = new UnityWebRequest(url, "POST"))
        {
            var bodyString = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var jsonToSend = new UTF8Encoding().GetBytes(bodyString);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                UserSeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<UserSeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostTmpResetAsync(RegisterRequestSchema requestBody, Action<UserSeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/tmp/reset";
        using (var request = new UnityWebRequest(url, "POST"))
        {
            var bodyString = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var jsonToSend = new UTF8Encoding().GetBytes(bodyString);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                UserSeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<UserSeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task PostTmpChangepasstimeAsync(SeasonChangeRequestSchema requestBody, Action<SeasonPassSchema> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/tmp/change-pass-time";
        using (var request = new UnityWebRequest(url, "POST"))
        {
            var bodyString = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var jsonToSend = new UTF8Encoding().GetBytes(bodyString);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                SeasonPassSchema result = System.Text.Json.JsonSerializer.Deserialize<SeasonPassSchema>(responseBody);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetBlockstatusAsync(Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/block-status";
        using (var request = new UnityWebRequest(url, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                onSuccess?.Invoke(responseBody);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetInvalidclaimAsync(Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/invalid-claim";
        using (var request = new UnityWebRequest(url, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                onSuccess?.Invoke(responseBody);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

    public async Task GetBalanceAsync(string planet, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{Url}/api/balance/{planet}";
        using (var request = new UnityWebRequest(url, "GET"))
        {
            request.uri = new Uri(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    return;
                }
                string responseBody = request.downloadHandler.text;
                onSuccess?.Invoke(responseBody);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }

}
