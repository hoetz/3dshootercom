﻿// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace ElCamino.AspNet.Identity.AzureTable.Model
{
    [JsonObject("identityConfiguration")]
    public class IdentityConfiguration
    {
        [JsonProperty("tablePrefix")]
        public string TablePrefix { get; set; }

        [JsonProperty("storageConnectionString")]
        public string StorageConnectionString { get; set; }

        [JsonProperty("locationMode")]
        public string LocationMode { get; set; }

    }
}
