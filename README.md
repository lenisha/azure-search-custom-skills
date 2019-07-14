# Structured and Unstructured Docs in Azure Search

To provide uniform search capabilities across both  - structured (Azure SQL) and unstructured (Azure Blob) documents we need to combine indexed data in a single searchable index.


## Create Azure resources
 Create Azure Search
![docs](docs/azsearch.png)
 and Cognitive service
![docs](docs/azcognitive.png)

## Index Schema
Prepare a schema that would be populated by indexers, we will use postman to send th REST API call

- Import Postman collection/environment  (refer to details in `Search Index Creation` folder)
- Create  index by sending POST API request `CreateIndex`
![docs](docs/createindex.png)
- Fiedls created by Json definition contain fields that will be populated by indexer from SQL (e.g Event_Type, Source etc), from Blob data (content_type etc), by cognitive services skills ( locations, keyphrases, organizations) and by custom skill (EventType)
![docs](docs/index.png)

** Index Key ** filed `ID` is populated by both indexers and should be unique across both, sql populates it with PRIMARY KEY and Blob with unique path to the blob file.

## Scoring Profile
Part of the index definition is also Scoring Profiles helping to boost keyphrases, project name and most recent high ranking records. 

```
 "scoringProfiles": [  
	    {  
	      "name": "boostKeys",  
	      "text": {  
	        "weights": {  
	          "keyphrases": 1.5,  
	          "Project_Name": 5  
	        }  
	      }  
	    },  
	    {  
	      "name": "newAndHighlyRated",  
	      "functions": [  
	        {  
	          "type": "freshness",  
	          "fieldName": "LessonDate",  
	          "boost": 10,  
	          "interpolation": "quadratic",  
	          "freshness": {  
	            "boostingDuration": "P365D"  
	          }  
	        },  
	        {
	          "type": "magnitude",  
	          "fieldName": "rating",  
	          "boost": 10,  
	          "interpolation": "linear",  
	          "magnitude": {  
	            "boostingRangeStart": 1,  
	            "boostingRangeEnd": 5,  
	            "constantBoostBeyondRange": false  
	          }  
	        }  
	      ]  
	    }  
     ]
```

## DataSources
Prepare SQL and Blob data (refer to the sections below) and define Blob and SQL DataSources using Rest API

For SQL DataSource enable change tracking fo the table to be sure  Indexer only indexes updated fields:
```
 {
            "name": "sharepointexport",
            "description": "source for excel parsed data",
            "type": "azuresql",
            "subtype": null,
            "credentials": {
                "connectionString": "{{sqlConnectionUrl}};"
            },
            "container": {
                "name": "[Lessons$]"
            },
            "dataChangeDetectionPolicy": {"@odata.type" : "#Microsoft.Azure.Search.SqlIntegratedChangeTrackingPolicy"},
            "dataDeletionDetectionPolicy": null
}
```

## Indexer setup
Now we will define how indexers will process data and populate index. Create Skillsets prior to creating indexers if they use them like in our example.

## BlobIndexer
Refer to `CreateBlobIndexer` example in postman collection, it maps fields typically extracted by BlobIndexer to the schema we defined using `fieldMappings` :

```
 "fieldMappings": [
                {
                    "sourceFieldName": "metadata_storage_path",
                    "targetFieldName": "ID",
                    "mappingFunction": {
                        "name": "base64Encode",
                        "parameters": null
                    }
                },
                {
                	
                    "sourceFieldName": "metadata_last_modified",
                    "targetFieldName": "LessonDate",
                    "mappingFunction": null
                },
                {
                	
                    "sourceFieldName": "metadata_content_type",
                    "targetFieldName": "content_type",
                    "mappingFunction": null
                }  
            ]
```
it' important to have ID mapped to uniquely identified field, and we map also fields with similar meaning with sql data. 
Indexer also defines how to map fields extracted by cognitive skillset we are using with it.
```
{
            
            "name": "blobtosqlindexer",
            "description": "combined indexer",
            "dataSourceName": "cac-items",
            "skillsetName": "cogniskillset",
            "targetIndexName": "lessons-index",
            "outputFieldMappings": [
                {
                    "sourceFieldName": "/document/merged_content/organizations",
                    "targetFieldName": "organizations",
                    "mappingFunction": null
                },
                {
                    "sourceFieldName": "/document/merged_content/locations",
                    "targetFieldName": "locations",
                    "mappingFunction": null
                },
                {
                    "sourceFieldName": "/document/merged_content/keyphrases",
                    "targetFieldName": "keyphrases",
                    "mappingFunction": null
                }
            ]
}
```

## SQLIndexer
Refer to `CreateSqlIndexer` example in postman collection, it maps fields extracted by SqlIndexer to the schema we defined using `fieldMappings` :

```
 {
           
            "name": "sqldata-indexer",
            "description": "opt",
            "dataSourceName": "sharepointexport",
            "skillsetName": "sqlstoreskillset",
            "targetIndexName": "lessons-index",
            "disabled": null,
            "schedule": null,
            "parameters": {
                "batchSize": null,
                "maxFailedItems": null,
                "maxFailedItemsPerBatch": null,
                "base64EncodeKeys": false,
                "configuration": {}
            },
            "fieldMappings": [
            	 {
                    "sourceFieldName": "Lesson_Learned_Comment",
                    "targetFieldName": "content",
                    "mappingFunction": null
                }
            ],
            "outputFieldMappings": [
                {
                    "sourceFieldName": "/document/content/locations",
                    "targetFieldName": "locations",
                    "mappingFunction": null
                },
                {
                    "sourceFieldName": "/document/content/organizations",
                    "targetFieldName": "organizations",
                    "mappingFunction": null
                },
                {
                    "sourceFieldName": "/document/content/keyphrases",
                    "targetFieldName": "keyphrases",
                    "mappingFunction": null
                },
                {
                    "sourceFieldName": "/document/EventType",
                    "targetFieldName": "EventType",
                    "mappingFunction": null
                }
            ]
        }
```
Once indexers created they start process of indexing and populating the search data.
![docs](docs/sqlindexer.png)


## Custom Skillset
In our example `CreateSkillset` we have created a skillset that uses cognitive services to extract data, skill to store extracted data in knowledge store and custom skill that maps various entries of `Event_Type` to only two possible values in the new field `EventType`
Our custom skill is hosted in Azure Function and here we define what data will be paased to the skill and what is output:
```
{
"@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
"description": "Our new OPS Mapping Event Type custom skill",
"uri": "{{funcUrl}}",
"batchSize": 100,
"context": "/document",
"inputs": [
    {
        "name": "Event_Type",
        "source": "/document/Event_Type"
    }
    ],
    "outputs": [
    {
        "name": "eventType",
        "targetName": "EventType"
    }
    ]
}
```

Note that when we will create Indexer it will define in `outputMappings` that `EventType` field coming from custom skill will be mapped to Indexed field.

Publish Custom Skill to Azure Function and test it using `TestFuncSkill`
![docs](docs/skilldata.png)

You could see the data processes in Function Logs:
![docs](docs/skill.png)


## SQL data preparation
To import structured data from Excel  create Azure SQL database and create a table with all the fields coming from excel columns and add `IDENTITY` column that would searve as unique key for the index

```
CREATE TABLE [dbo].[Lessons$] (
ID int IDENTITY(1, 1) PRIMARY KEY,
[Project_Name] nvarchar(255),
[Line_Of_Business] nvarchar(255),
[PM_Phase] nvarchar(255),
[Event_Type] nvarchar(255),
[Lessons_Learned_Category] nvarchar(255),
[Lesson_Learned_Comment] nvarchar(max),
[Project_Type] nvarchar(255),
[Source] nvarchar(255),
[LessonDate] datetime
)
```
And Enable Change tracking as defined in [Enable Change TRacking in SQL Server](https://docs.microsoft.com/en-us/SQL/relational-databases/track-changes/enable-and-disable-change-tracking-sql-server?view=sql-server-2017)
```
ALTER DATABASE datasearch  
SET CHANGE_TRACKING = ON  
(CHANGE_RETENTION = 2 DAYS, AUTO_CLEANUP = ON)  
  

ALTER TABLE [dbo].[Lessons$] 
ENABLE CHANGE_TRACKING  
WITH (TRACK_COLUMNS_UPDATED = ON)  
```

Use `Tasks-> Import Data` wizard to import excel data, choose `Microsoft Excel` as input datasource
![data](docs/importdata-excel.png)
and OLEDB provider for SQL server as destination
![data](docs/importdata-sql.png)
Verify all the mappings
![data](docs/importdata-excel.png)


## Blob Data Preparation
Create Azure Storage account and Blob cotainer, upload  documents in two folders based on their Source.
![docs](docs/blob.png)

