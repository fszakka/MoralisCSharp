// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using Parse.Infrastructure.Utilities;

namespace Parse.Platform.Push
{
    /// <summary>
    /// A wrapper around Parse push notification payload.
    /// </summary>
    public class ParsePushNotificationEvent : EventArgs
    {
        internal ParsePushNotificationEvent(IDictionary<string, object> content)
        {
            Content = content;
            TextContent = JsonUtilities.Encode(content);
        }

        // TODO: (richardross) investigate this.
        // Obj-C type -> .NET type is impossible to do flawlessly (especially
        // on NSNumber). We can't transform NSDictionary into string because of this reason.

        internal ParsePushNotificationEvent(string stringPayload)
        {
            TextContent = stringPayload;
            Content = JsonUtilities.Parse(stringPayload) as IDictionary<string, object>;
        }

        /// <summary>
        /// The payload of the push notification as <c>IDictionary</c>.
        /// </summary>
        public IDictionary<string, object> Content { get; internal set; }

        /// <summary>
        /// The payload of the push notification as <c>string</c>.
        /// </summary>
        public string TextContent { get; internal set; }
    }
}
