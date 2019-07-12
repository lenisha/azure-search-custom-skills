// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Text;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Web;

namespace CognitiveSearch.Skills
{
    public static class EventTypeSkill
    {
        [FunctionName("EventTypeSkill")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            // Read input, deserialize it and validate it.
            var data = GetStructuredInput(req.Body);
            if (data == null)
            {
                return new BadRequestObjectResult("The request schema does not match expected schema.");
            }
            if (data.Values != null)
                log.Info("C# HTTP trigger function got records: " + data.Values.Count);

            // Calculate the response for each value.
            var response = new WebApiResponse();
            foreach (var record in data.Values)
            {
                if (record == null || record.RecordId == null) continue;

                OutputRecord responseRecord = new OutputRecord();
                responseRecord.RecordId = record.RecordId;

                try
                {
                    responseRecord.Data = DoWork(record.Data).Result;
                }
                catch (Exception e)
                {
                    log.Error(e.StackTrace);
                    // Something bad happened, log the issue.
                    var error = new OutputRecord.OutputRecordMessage
                    {
                        Message = e.Message
                    };

                    responseRecord.Errors = new List<OutputRecord.OutputRecordMessage>
                    {
                        error
                    };
                }
                finally
                {
                    response.values.Add(responseRecord);
                }
            }
            if (data.Values != null)
                log.Info("C# HTTP trigger function processed records: " + data.Values.Count);
            return new OkObjectResult(response);



        }

        private static WebApiRequest GetStructuredInput(Stream requestBody)
        {
            string request = new StreamReader(requestBody).ReadToEnd();
            var data = JsonConvert.DeserializeObject<WebApiRequest>(request);
            return data;
        }

        private static async Task<OutputRecord.OutputRecordData> DoWork(InputRecord.InputRecordData inputRecord)
        {
            string mappedValue = "Lessons of what worked";

            if (inputRecord.Event_Type != null)
            {
                if (inputRecord.Event_Type.Contains("Lessons"))
                    mappedValue = "Lessons of what worked";
                else if (inputRecord.Event_Type.Contains("Success"))
                    mappedValue = "Lessons of what worked";
                else if (inputRecord.Event_Type.Contains("Strength"))
                    mappedValue = "Lessons of what worked";
                else if (inputRecord.Event_Type.Contains("Challenge"))
                    mappedValue = "Recommendation for Improvement";
                else
                    mappedValue = "Recommendation for Improvement";
            }

            var outputRecord = new OutputRecord.OutputRecordData();
            outputRecord.EventType = mappedValue;

            return outputRecord;
           
        }
    }
}
