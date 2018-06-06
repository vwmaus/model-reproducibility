/////////////////////////////////////////////////////////////////////////////////////////////
// Functions
/////////////////////////////////////////////////////////////////////////////////////////////

// ajax call controller action:
// https://stackoverflow.com/questions/12559515/jquery-ajax-call-to-controller

//function runScript() {
//    window.alert("runScript");
//    addDownloadButton();
//    return false;
//}

//function downloadDockerfiles() {
//    createDockerfiles();
//    return false;
//}

//function createDockerfiles() {
//    window.alert("createDockerfiles");
//}

//function createGamsDockerfile() {
//    var lic = $("#licencePath").val();

//    $.ajax({
//        url: $("#btn_downloadDockerfile").attr("href"),
//        type: "POST",
//        data: {
//            licencePath: lic
//        }
//    }).done(function () {
//        alert("Dockerfile created");
//    });

//    return false;
//}

function httpGet(theUrl) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open("GET", theUrl, false); // false for synchronous request
    xmlHttp.send(null);
    //return xmlHttp;

    xmlHttp.onreadystatechange = function() {
        if (xmlHttp.readyState === 4 && xmlHttp.status === 200) {
            return xmlHttp.responseText;
        }
        return false;
    };

//return xmlHttp.responseText;
}

function httpGetAsync(theUrl, callback) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState === 4 && xmlHttp.status === 200)
            callback(xmlHttp.responseText);
    };

    xmlHttp.open("GET", theUrl, true); // true for asynchronous 
    xmlHttp.send(null);
}

function httpPost(theUrl) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open("POST", theUrl, false); // false for synchronous request
    xmlHttp.send(null);
    return xmlHttp;
}

function getGithubVersions(owner, repo) {
    var url = "https://api.github.com/repos/" + owner + "/" + repo + "/git/refs/tags";
    var url2 = "https://api.github.com/repos/" + owner + "/" + repo + "/branches";

    function httpGetAsync(theUrl, callback) {
        var xmlHttp = new XMLHttpRequest();
        xmlHttp.onreadystatechange = function () {
            if (xmlHttp.readyState === 4 && xmlHttp.status === 200)
                callback(xmlHttp.responseText);
        };
        xmlHttp.open("GET", theUrl, true); // true for asynchronous 
        xmlHttp.send(null);
    }

    function httpGet(theUrl) {
        var xmlHttp = new XMLHttpRequest();
        xmlHttp.open("GET", theUrl, false); // false for synchronous request
        xmlHttp.send(null);
        return xmlHttp.responseText;
    }

    var res = httpGet(url);
    var obj = JSON.parse(res);
    var tags = new Array();

    if (Object.prototype.toString.call(obj) === "[object Array]") {
        obj.forEach(function (entry) {
            var tag = entry.ref.replace("refs/tags/", "");
            console.log(tag);
            tags.push(tag);
        });
    }

    DeleteModelVersionData();

    $.each(tags, function (val, text) {
        $("#modelversion").append(
            $("<option></option>").val(text).html(text)
        );
    });

    res = httpGet(url2);
    obj = JSON.parse(res);
    var branches = new Array();

    if (Object.prototype.toString.call(obj) === "[object Array]") {
        obj.forEach(function (entry) {
            var branch = entry.name;
            console.log(branch);
            branches.push(branch);
        });
    }

    $.each(branches, function (val, text) {
        $("#modelversion").append(
            $("<option></option>").val(text).html(text)
        );
    });
}

function getGithubRepos(owner) {
    // https://stackoverflow.com/questions/247483/http-get-request-in-javascript
    // "https://api.github.com/repos/ptrkrnstnr/transport-model/git/refs/tags"
    // https://developer.github.com/v3/git/tags/#get-a-tag

    //https://api.github.com/users/vwmaus/repos
    var url = "https://api.github.com/users/" + owner + "/repos";

    function httpGet(theUrl) {
        var xmlHttp = new XMLHttpRequest();
        xmlHttp.open("GET", theUrl, false); // false for synchronous request
        xmlHttp.send(null);

        return xmlHttp.responseText;
    }

    var res = httpGet(url);
    var obj = JSON.parse(res);
    var tags = new Array();

    if (Object.prototype.toString.call(obj) === "[object Array]") {
        obj.forEach(function (entry) {
            var tag = entry.name;
            console.log(tag);
            tags.push(tag);
        });
    }

    var sel = $("#model");

    $(sel)
        .find("option")
        .remove()
        .end();

    $.each(tags, function (val, text) {
        sel.append(
            $("<option></option>").val(text).html(text)
        );
    });
}

function ReloadFormData() {
    if ($("#githubUser").length) {
        var owner = $("#githubUser").val();

        getGithubRepos(owner);

        if ($("#modelversion").length) {

            var repo = $("#model option:selected").text();

            getGithubVersions(owner, repo);
        }
    }

    // todo: geonode model input
    getGeoNodeModels();
}

function DeleteModelData() {
    $("#model")
        .find("option")
        .remove()
        .end();
}

function DeleteModelVersionData() {
    $("#modelversion")
        .find("option")
        .remove()
        .end();
}

/////////////////////////////////////////////////////////////////////////////////////////////
// Geonode
/////////////////////////////////////////////////////////////////////////////////////////////

//“/api/base” query on the ResourceBase table and returns combined results of Maps, Layers Documents and Services
//“/api/layers” query the Layer table
//“/api/maps” query the Map table
//“/api/documents” query the Document table
//“/api/groups” query the GroupProfile table (which contains the Groups)
//“/api/profiles” query the Profile table (which is the geonode authentication table)
//“/api/categories” query the Category table
//“/api/keywords” query the Tag table
//“/api/featured” query the ResourceBase table by limiting the items to the ones flagged as

var geoNodeUrl = "http://localhost:8011/";

function downloadGeoNodeDocument() {
    var documentId = $("#geonodeModelData").find(":selected").val();

    var url = geoNodeUrl + "documents/" + documentId + "/download";

    return url;
}



function getGeoNodeModels() {
    //http://localhost:8011/api/documents/
    var documentsUrl = geoNodeUrl + "api/documents/";

    //var res = httpGet(documentsUrl);
    httpGetAsync(documentsUrl,
        function(responseText) {
            alert(responseText);
        });

    //var xmlHttp = new XMLHttpRequest();
    //xmlHttp.open("GET", documentsUrl, false); // false for synchronous request
    ////xmlHttp.send(null);
    ////return xmlHttp;

    //xmlHttp.onreadystatechange = function () {
    //    //if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
    //        //alert(xmlHttp.responseText);
    //    //}
    //    alert("hallo");
    //}

    //xmlHttp.ontimeout = alert("timeout");

    //alert(res);

    //var obj = JSON.parse(res);
    //alert(obj);

    //var tags = new Array();

    //alert(tags);

    //if (Object.prototype.toString.call(obj) === "[object Array]") {
    //    obj.forEach(function (entry) {
    //        var tag = entry.abstract;
    //        console.log(tag);
    //        tags.push(tag);
    //    });
    //}

    //var sel = $("#geonodeModelData");

    //$(sel)
    //    .find("option")
    //    .remove()
    //    .end();

    //$.each(tags, function (val, text) {
    //    sel.append(
    //        $("<option></option>").val(text).html(text)
    //    );
    //});

    //return false;
}

function getGeoNodeModelTags() {
    //todo

    return false;
}

function addDownloadButton() {
    $("#downloadSection").append("<button>ButtonTest</button>");
}

/////////////////////////////////////////////////////////////////////////////////////////////
// DOM 
/////////////////////////////////////////////////////////////////////////////////////////////

$(document).ready(function () {
    //$("#btn_runScript").click(runScript);

    $("#btn_downloadGeonodeData").attr("href", downloadGeoNodeDocument());

    //$("#githubUser").val("vwmaus");
    $("#licensePath").val("");

    // Set change event
    $("#model").change(function () {
        var repo = $("#model option:selected").text();
        var owner = $("#githubUser").val();

        $("#modelversion")
            .find("option")
            .remove()
            .end();

        getGithubVersions(owner, repo);
    });

    $("#geonodeModelData").change(function () {
        $("#btn_downloadGeonodeData").attr("href", downloadGeoNodeDocument());
        getGeoNodeModelTags();
        //$("#btn_downloadGeonodeData").attr("href", downloadGeoNodeDocument());
    });

    $("#githubUser").change(function () {
        DeleteModelData();
        DeleteModelVersionData();
        ReloadFormData();
    });

    $("#uploadLicenceFile").change(function () {
        var input = $("#uploadLicenceFile").prop("files");

        if (input[0]) {
            $("#uploadFile").text(input[0].name);
        }
    });

    //ReloadFormData();
});
/////////////////////////////////////////////////////////////////////////////////////////////