// Initialize and connect to your search service
var automagic = new AzSearch.Automagic({ index: "lessons-index", queryKey: "xxxxx", service: "testindexeneros" });

// Create some mustache templates to customize result/suggestion display. Default is JSON.stringify(result,null,4) rendered in <pre> and <code>.


var suggestionsTemplate = "Name: {{{displayText}}} Source: {{{Source}}} <br/> {{{searchText}}}";
var suggestionsProcessor = function (results) {
    return results.map(function (result) {
        result.displayText =  result.Project_Name;
        result.searchText = result["@search.text"];
        return result;
    });
};
automagic.store.setSuggestionsProcessor(suggestionsProcessor);

// Add a search box that uses suggester "sg", grabbing some additional fields to display during suggestions. Use the template defined above
automagic.addSearchBox("searchBox",
    {
        highlightPreTag: "<b>",
        highlightPostTag: "</b>",
        suggesterName: "suggester",
        select: "Project_Name,Source"
    },
    "displayText",
    suggestionsTemplate);


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
// Add a loading indicator above the results view
automagic.addLoadingIndicator("loading");
// Adds a pager control << 1 2 3 ... >>
automagic.addPager("pager");
automagic.addCheckboxFacet("Project_NameFacet", "Project_Name", "string");
automagic.addCheckboxFacet("EventTypeFacet", "EventType", "string");
automagic.addCheckboxFacet("SourceFacet", "Source", "string");
automagic.addCheckboxFacet("Lessons_Learned_CategoryFacet", "Lessons_Learned_Category", "string");
automagic.addCheckboxFacet("Project_TypeFacet", "Project_Type", "string");

var startDate = new Date();
startDate.setFullYear(2005);
var endDate = new Date();
automagic.addRangeFacet("LessonDateFacet", "LessonDate", "date", startDate, endDate);

// filter header & footer options 
automagic.addClearFiltersButton("facetHeader");
automagic.addClearFiltersButton("facetFooter");


function getContentTypeShort(contentType) {

    if (contentType == null)
      return "SQL Data";

    switch (contentType) {
        case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
        case "application/msword":
        case "application/vnd.ms-word.document.macroenabled.12":
        case "application/vnd.ms-word2006ml":  
        case "application/vnd.ms-wordml":  
            return "MS-Word";
        case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": 
        case "application/vnd.ms-excel": 
        case "application/vnd.ms-excel.sheet.macroenabled.12": 
            return "MS-Excel";   
        case "application/vnd.openxmlformats-officedocument.presentationml.presentation": 
        case "application/application/vnd.ms-powerpoint": 
        case "application/vnd.ms-powerpoint.presentation.macroenabled.12": 
            return "MS-PowerPoint";   
        case "text/plain; charset=windows-1252":
            return "Text";
        case "application/pdf":
            return "PDF";            
        default:
            return contentType;    
    }
}