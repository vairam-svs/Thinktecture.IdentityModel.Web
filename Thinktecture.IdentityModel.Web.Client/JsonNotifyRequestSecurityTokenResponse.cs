﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Thinktecture.IdentityModel.Extensions;

namespace Thinktecture.IdentityModel.Web
{
    [DataContract]
    public class JsonNotifyRequestSecurityTokenResponse
    {
        [DataMember(Name = "appliesTo", Order = 1)]
        public string AppliesTo { get; set; }

        [DataMember(Name = "context", Order = 2)]
        public string Context { get; set; }

        [DataMember(Name = "created", Order = 3)]
        public string Created { get; set; }

        [DataMember(Name = "expires", Order = 4)]
        public string Expires { get; set; }

        [DataMember(Name = "securityToken", Order = 5)]
        public string SecurityTokenString { get; set; }

        [DataMember(Name = "tokenType", Order = 6)]
        public string TokenType { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public GenericXmlSecurityToken SecurityToken { get; set; }

        public static JsonNotifyRequestSecurityTokenResponse FromJson(string jsonString)
        {
            JsonNotifyRequestSecurityTokenResponse rstr;

            var memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(jsonString));
            var serializer = new DataContractJsonSerializer(typeof(JsonNotifyRequestSecurityTokenResponse));

            rstr = serializer.ReadObject(memoryStream) as JsonNotifyRequestSecurityTokenResponse;
            memoryStream.Close();

            ParseValues(rstr);
            return rstr;
        }

        private static void ParseValues(JsonNotifyRequestSecurityTokenResponse rstr)
        {
            rstr.ValidFrom = ulong.Parse(rstr.Created).ToDateTimeFromEpoch();
            rstr.ValidTo = ulong.Parse(rstr.Expires).ToDateTimeFromEpoch();
            rstr.SecurityTokenString = HttpUtility.HtmlDecode(rstr.SecurityTokenString);
            var xml = XElement.Parse(rstr.SecurityTokenString);

            string idAttribute = "";
            string tokenType = "";

            switch (rstr.TokenType)
            {
                case SecurityTokenTypes.Saml11:
                    idAttribute = "AssertionID";
                    tokenType = SecurityTokenTypes.Saml11;
                    break;
                case SecurityTokenTypes.Saml2:
                    idAttribute = "ID";
                    tokenType = SecurityTokenTypes.Saml2;
                    break;
                case SecurityTokenTypes.SWT:
                    idAttribute = "Id";
                    tokenType = SecurityTokenTypes.SWT;
                    break;
            }

            if (tokenType == SecurityTokenTypes.Saml11 || tokenType == SecurityTokenTypes.Saml2)
            {
                var tokenId = xml.Attribute(idAttribute);
                var xmlElement = xml.ToXmlElement();
                SecurityKeyIdentifierClause clause = null;
                
                if (tokenId != null)
                {
                    clause = new SamlAssertionKeyIdentifierClause(tokenId.Value);
                }

                rstr.SecurityToken = new GenericXmlSecurityToken(
                    xmlElement,
                    null,
                    rstr.ValidFrom,
                    rstr.ValidTo,
                    clause,
                    clause,
                    new ReadOnlyCollection<IAuthorizationPolicy>(new List<IAuthorizationPolicy>()));
            }
        }

        private class SecurityTokenTypes
        {
            public const string Saml2 = "urn:oasis:names:tc:SAML:2.0:assertion";
            public const string Saml11 = "urn:oasis:names:tc:SAML:1.0:assertion";
            public const string SWT = "http://schemas.xmlsoap.org/ws/2009/11/swt-token-profile-1.0";
        }
    }
}
