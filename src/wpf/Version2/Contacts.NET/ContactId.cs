/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using Standard;

    internal static class ContactId
    {
        /// <summary>
        /// Key tokens for the opaque string Ids that comprise ContactIds and PersonIds.
        /// </summary>
        public enum Token
        {
            Guid,
            Path,
        }

        private static readonly Dictionary<Token, string> _tokenMap = new Dictionary<Token, string>
        { 
            { Token.Path, "/PATH:" }, 
            { Token.Guid, "/GUID:" },
        };

        public static string GetRuntimeId(Guid guid, string path)
        {
            const string tokenValueFormat = "{0}\"{1}\"";

            //Assert.AreNotEqual(default(Guid), guid);

            var idBuilder = new StringBuilder();
            idBuilder.AppendFormat(tokenValueFormat, _tokenMap[Token.Guid], guid);

            if (!string.IsNullOrEmpty(path))
            {
                idBuilder.Append(" ");
                idBuilder.AppendFormat(tokenValueFormat, _tokenMap[Token.Path], path);
            }

            return idBuilder.ToString();
        }

        /// <summary>
        /// Utility to parse tokens out of a runtime ContactId.
        /// </summary>
        /// <param name="contactId">The runtime ContactId to parse.</param>
        /// <param name="token">The token to search for in the Id.</param>
        /// <returns>
        /// The value of the token in the id, if the token exists in the id.
        /// If the token is missing then null is returned.
        /// </returns>
        public static string TokenizeContactId(string contactId, Token token)
        {
            Verify.IsNotNull(contactId, "contactId");
            if (!_tokenMap.ContainsKey(token))
            {
                throw new ArgumentException("Invalid token.", "token");
            }

            Dictionary<string, string> tokens = TokenizeId(contactId);
            string tokenValue;
            if (tokens.TryGetValue(_tokenMap[token], out tokenValue))
            {
                return tokenValue;
            }
            return "";
        }

        public static Dictionary<string, string> TokenizeId(string id)
        {
            var whitespaceExpression = new Regex(@"\s");

            Verify.IsNeitherNullNorEmpty(id, "id");
            id = id.Trim();
            Verify.AreNotEqual(0, id.Length, "id", "Improperly formatted Id string.");

            var retMap = new Dictionary<string, string>();
            string[] splitArray = id.Split('\"');
            // Expect a trailing empty string given the way the id is split.
            if (0 == splitArray.Length % 2 || 0 != splitArray[splitArray.Length - 1].Length)
            {
                throw new FormatException("Improperly formatted Id string.  The value for a token isn't properly closed.");
            }
            for (int i = 0; i < splitArray.Length - 1; i += 2)
            {
                string token = splitArray[i];
                string value = splitArray[i + 1];

                // Validate the token format.
                if (token.Length == 0 || token[token.Length - 1] != ':')
                {
                    throw new FormatException("Improperly formatted Id string.  A token isn't properly declared.");
                }

                // Remove whitespace that precedes the token.  Just verified that there's no trailing whitespace.
                token = token.Trim().ToUpperInvariant();

                if (whitespaceExpression.Match(token).Success)
                {
                    throw new FormatException("Improperly formatted Id string.  A token isn't properly declared.");
                }

                if (retMap.ContainsKey(token))
                {
                    throw new FormatException("A token in the Id string is declared multiple times.");
                }

                retMap.Add(token, value);
            }
            return retMap;
        }
    }

    /* Dev unit tests.

    */
}
