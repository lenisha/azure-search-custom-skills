// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace CognitiveSearch.Skills
{
    public class WebApiResponseError
    {
        public string message { get; set; }
    }

    public class WebApiResponseWarning
    {
        public string message { get; set; }
    }

    public class WebApiResponseRecord
    {
        public string recordId { get; set; }
        public Dictionary<string, object> data { get; set; }
        public List<WebApiResponseError> errors { get; set; }
        public List<WebApiResponseWarning> warnings { get; set; }
    }

    public class WebApiEnricherResponse
    {
        public List<WebApiResponseRecord> values { get; set; }
    }


    #region Class used to deserialize the request
    public class InputRecord
    {
        public class InputRecordData
        {
            public string Event_Type;
        }

        public string RecordId { get; set; }
        public InputRecordData Data { get; set; }
    }

    public class WebApiRequest
    {
        public List<InputRecord> Values { get; set; }
    }
    #endregion

    #region Classes used to serialize the response
    public class OutputRecord
    {
        public class OutputRecordData
        {
            public string EventType { get; set; }
        }

        public class OutputRecordMessage
        {
            public string Message { get; set; }
        }

        public string RecordId { get; set; }
        public OutputRecordData Data { get; set; }
        public List<OutputRecordMessage> Errors { get; set; }
        public List<OutputRecordMessage> Warnings { get; set; }
    }

    public class WebApiResponse
    {
        public WebApiResponse()
        {
            this.values = new List<OutputRecord>();
        }

        public List<OutputRecord> values { get; set; }
    }
    #endregion
}

