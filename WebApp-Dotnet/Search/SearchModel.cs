﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace CognitiveSearch.UI
{
    public class SearchModel
    {
        private string[] facets = new string[]
        {
            // Add UI facets here in order
            "Project_Name",
            "EventType",
            "Source",
            "Project_Type",
            "LessonDate",
            "keyphrases"
            //"persons",
            //"locations",
            //"organizations",
        };

        private string[] tags = new string[]
        {
            // Add tags fields here in order
            //"persons",
            //"locations",
            //"organizations"
            "keyphrases"
        };


        private string[] resultFields = new string[]
        {
            // Add fields needed to display results cards

            // NOTE: if you customize the resultFields, be sure to include metadata_storage_name and metadata_storage_path as those fields are needed for the UI to work properly

            "metadata_storage_path",
            "ID",
            "content",
            "Project_Name",
            "LessonDate",
            "Source",
            "keyphrases",
            "content_type",
            "EventType",
            "Lessons_Learned_Category"
            //"metadata_storage_name",

            //"persons",
            //"locations",
            //"organizations",
            //"keyPhrases"
        };

        public List<SearchField> Facets { get; set; }
        public List<SearchField> Tags { get; set; }

        public string[] SelectFilter { get; set; }

        public Dictionary<string, string[]> SearchFacets { get; set; }

        public SearchModel(SearchSchema schema)
        {
            Facets = new List<SearchField>();
            Tags = new List<SearchField>();
            SelectFilter = resultFields;

            if (facets.Count() > 0)
            {
                // add field to facets if in facets arr
                foreach (var field in facets)
                {
                    if (schema.Fields[field] != null && schema.Fields[field].IsFacetable)
                    {
                        Facets.Add(schema.Fields[field]);
                    }
                }
            }
            else
            {
                foreach (var field in schema.Fields.Where(f => f.Value.IsFacetable))
                {
                    Facets.Add(field.Value);
                }
            }

            if (tags.Count() > 0)
            {
                foreach (var field in tags)
                {
                    if (schema.Fields[field] != null && schema.Fields[field].IsFacetable)
                    {
                        Tags.Add(schema.Fields[field]);
                    }
                }
            }
            else
            {
                foreach (var field in schema.Fields.Where(f => f.Value.IsFacetable))
                {
                    Tags.Add(field.Value);
                }
            }
        }
    }
}
