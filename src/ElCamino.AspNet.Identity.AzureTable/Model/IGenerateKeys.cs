// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.

namespace ElCamino.AspNet.Identity.AzureTable.Model
{
    public interface IGenerateKeys
    {
        void GenerateKeys();

        string PeekRowKey();

        double KeyVersion { get; set; }

    }
}
