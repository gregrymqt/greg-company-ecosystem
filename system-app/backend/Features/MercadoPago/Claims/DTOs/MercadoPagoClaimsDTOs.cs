using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MeuCrudCsharp.Models.Enums;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.DTOs
{
    public class MercadoPagoClaimsDTOs
    {
        public class MpClaimSearchResponse
        {
            [JsonPropertyName("paging")]
            public MpPaging? Paging { get; set; }

            [JsonPropertyName("results")]
            public List<MpClaimItem>? Results { get; set; }
        }

        public class MpPaging
        {
            [JsonPropertyName("total")]
            public int Total { get; set; }

            [JsonPropertyName("limit")]
            public int Limit { get; set; }

            [JsonPropertyName("offset")]
            public int Offset { get; set; }
        }

        public class MpClaimItem
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("resource_id")]
            public string? ResourceId { get; set; }

            [JsonPropertyName("resource")]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public ClaimResource Resource { get; set; }

            [JsonPropertyName("status")]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public MpClaimStatus Status { get; set; }

            [JsonPropertyName("type")]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public ClaimType Type { get; set; }

            [JsonPropertyName("stage")]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public ClaimStage Stage { get; set; }

            [JsonPropertyName("players")]
            public List<MpPlayer>? Players { get; set; }

            [JsonPropertyName("date_created")]
            public DateTime DateCreated { get; set; }

            [JsonPropertyName("last_updated")]
            public DateTime LastUpdated { get; set; }
        }

        public class MpPlayer
        {
            [JsonPropertyName("role")]
            public string? Role { get; set; }

            [JsonPropertyName("id")]
            public long UserId { get; set; }

            [JsonPropertyName("type")]
            public string? Type { get; set; } // "user"
        }

        public class MpMessageResponse
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("sender_role")]
            public string? SenderRole { get; set; }

            [JsonPropertyName("receiver_role")]
            public string? ReceiverRole { get; set; }

            [JsonPropertyName("message")]
            public string? Message { get; set; }

            [JsonPropertyName("date_created")]
            public DateTime DateCreated { get; set; }

            [JsonPropertyName("attachments")]
            public List<MpAttachment>? Attachments { get; set; }
        }

        public class MpAttachment
        {
            [JsonPropertyName("filename")]
            public string? Filename { get; set; }

            [JsonPropertyName("original_filename")]
            public string? OriginalFilename { get; set; }
        }

        public class MpPostMessageRequest
        {
            [JsonPropertyName("receiver_role")]
            public string? ReceiverRole { get; set; }

            [JsonPropertyName("message")]
            public string? Message { get; set; }

            [JsonPropertyName("attachments")]
            public List<string>? Attachments { get; set; }
        }

        public class ReplyRequestDto
        {
            public string? Message { get; set; }
        }
    }
}