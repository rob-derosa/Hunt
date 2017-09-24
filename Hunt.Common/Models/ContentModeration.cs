using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hunt.Common
{
    public class Email
    {
        [JsonProperty("Detected")]
        public string Detected { get; set; }
        [JsonProperty("SubType")]
        public string SubType { get; set; }
        [JsonProperty("Text")]
        public string Text { get; set; }
        [JsonProperty("Index")]
        public int Index { get; set; }
    }

    public class IPA
    {
        [JsonProperty("SubType")]
        public string SubType { get; set; }
        [JsonProperty("Text")]
        public string Text { get; set; }
        [JsonProperty("Index")]
        public int Index { get; set; }
    }

    public class Phone
    {
        [JsonProperty("CountryCode")]
        public string CountryCode { get; set; }
        [JsonProperty("Text")]
        public string Text { get; set; }
        [JsonProperty("Index")]
        public int Index { get; set; }
    }

    public class Address
    {
        [JsonProperty("Text")]
        public string Text { get; set; }
        [JsonProperty("Index")]
        public int Index { get; set; }
    }

    public class PII
    {
        [JsonProperty("Email")]
        public List<Email> Email { get; set; }
        [JsonProperty("IPA")]
        public List<IPA> IPA { get; set; }
        [JsonProperty("Phone")]
        public List<Phone> Phone { get; set; }
        [JsonProperty("Address")]
        public List<Address> Address { get; set; }
    }

    public class Term
    {
        [JsonProperty("Index")]
        public int Index { get; set; }
        [JsonProperty("OriginalIndex")]
        public int OriginalIndex { get; set; }
        [JsonProperty("ListId")]
        public int ListId { get; set; }
        [JsonProperty("Term")]
        public string TermString { get; set; }
    }

    public class Status
    {
        [JsonProperty("Code")]
        public int Code { get; set; }
        [JsonProperty("Description")]
        public string Description { get; set; }
        [JsonProperty("Exception")]
        public object Exception { get; set; }
    }

    public class TextModeration
    {
        [JsonProperty("OriginalText")]
        public string OriginalText { get; set; }
        [JsonProperty("NormalizedText")]
        public string NormalizedText { get; set; }
        [JsonProperty("AutoCorrectedText")]
        public string AutoCorrectedText { get; set; }
        [JsonProperty("Misrepresentation")]
        public object Misrepresentation { get; set; }
        [JsonProperty("PII")]
        public PII PII { get; set; }
        [JsonProperty("Language")]
        public string Language { get; set; }
        [JsonProperty("Terms")]
        public List<Term> Terms { get; set; }
        [JsonProperty("Status")]
        public Status Status { get; set; }
        [JsonProperty("TrackingId")]
        public string TrackingId { get; set; }
    }
}
