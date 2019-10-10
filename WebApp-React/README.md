## Web Application
To provide web application access to search index and ability to retrive the documents we could use ReactJS library [AzSearch.js](https://github.com/Yahnoosh/AzSearch.js) and it's corresponding [Web application generator](http://azsearchstore.azurewebsites.net/azsearchgenerator/index.html)

You could see the generated and adjusted sample in the `WebApp` directory.

- modify `index.html` with names of facets in html controls that will be bound to the selections

``` html
 <div id="facetPanel" class="col-sm-3 col-md-3 sidebar collapse">
    <div id="facetHeader"></div>
    <ul class="nav nav-sidebar">
        <div className="panel panel-primary behclick-panel">           
            <li>
                <div id="Project_NameFacet">
                </div>
            </li>
            <li>
                <div id="EventTypeFacet">
                </div>
            </li>
            .....
            
            <li>
                <div id="LessonDateFacet">
                </div>
            </li>
                                        
        </div>
    </ul>
    <div id="facetFooter"></div>
</div>
```
- modify `src\index.js` to connect to the search index created before
```
var automagic = new AzSearch.Automagic({ index: "lessons-index", queryKey: "xxxxx", service: "testindexeneros" });
```

- and connect React controls to the html elements in `index.js`, for example facets are defined like this connecting html control name and actual field in search index:
```
// Add a loading indicator above the results view
automagic.addLoadingIndicator("loading");
// Adds a pager control << 1 2 3 ... >>
automagic.addPager("pager");
automagic.addCheckboxFacet("Project_NameFacet", "Project_Name", "string");
automagic.addCheckboxFacet("EventTypeFacet", "EventType", "string");
automagic.addCheckboxFacet("SourceFacet", "Source", "string");
automagic.addCheckboxFacet("Lessons_Learned_CategoryFacet", "Lessons_Learned_Category", "string");
automagic.addCheckboxFacet("Project_TypeFacet", "Project_Type", "string");
``` 

- To customze how results and suggestions are displayed you can use Moustache templates, that could use fields from index, and you could specify tags used for highlighting

```
    var resultTemplate =
    `
        <div class="col-xs-12 col-sm-9 col-md-9">
            <h4>{{{Project_Name}}}</h4>
            
            <div class="resultDescription">
                {{{summary}}}
            </div>
            <ul class="resultProperties">
                <li class="resultProperties__date">Date: <span>{{dateChanged}}</span></li>
                <li class="resultProperties__score">Score: <span>{{score}}</span></li>
                <li class="resultProperties__type">Type: <span>{{contentType}}</span></li>
                <li class="resultProperties__source">Source: <span>{{Source}}</span></li>
            </ul>
        </div>`;
        // add a results view using the template defined above
        automagic.addResults("results", {
                         count: true, 
                         highlight: "Project_Name, content", 
                         highlightPreTag:  "<mark><em>",
                         highlightPostTag: "</em></mark>"
                        }, 
                     resultTemplate);
```
- To format reulsts prior to displaying then in template we can define `resultsProcessor`, that could extract and format data fields, in our example below we prepare summary for each result record to be constructed only from sentences that include highlights and not entire content, and we are formatting Date, Score and ContentType to be more user friendly. 

```
 var resultsProcessor = function (results) {
        return results.map(function (result) {
            
            var summary = result.content;

            // replace values with highlights
            var highlights= result["@search.highlights"]
            if (highlights!==undefined) {
                if ( 'Project_Name' in highlights )
                    result.Project_Name = highlights.Project_Name[0];
                if ( 'content' in highlights )
                    summary = highlights.content[0];
            }
            
            result.summary = summary.length < 400 ? summary : summary.substring(0, 400) + "...";
            
            // format Date
            var dateChanged = new Date(result.LessonDate);
            result.dateChanged = dateChanged.toLocaleDateString(undefined, {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric'
            });

            // format score
            result.score = numeral(result["@search.score"]).format("0.00");

            // format type
            result.contentType = getContentTypeShort(result.content_type);
            return result;
        });
    };
    automagic.store.setResultsProcessor(resultsProcessor);
```

Similar process is used for suggestion box customizations, more details on capabilities could be found at [AzSearch.js](https://github.com/Yahnoosh/AzSearch.js)

## WebApp Hosting
To host webapp use [Azure Storage for Static websites](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-static-website-how-to) Feature. Once enables on Storage account you would see `$web` container that will be used for hosting artifacts.

Use Visual studio Code to upload the files - for tutorial refer to [VSCode Deploy to Azure Storage](https://code.visualstudio.com/tutorials/static-website/deploy-website)

## References

- [AzSearch.js](https://github.com/Yahnoosh/AzSearch.js)
- [AzSearch Web application generator](http://azsearchstore.azurewebsites.net/azsearchgenerator/index.html)