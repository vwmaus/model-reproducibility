/////////////////////////////////////////////////////////////////////////////////////////////
// Functions
/////////////////////////////////////////////////////////////////////////////////////////////

function runScript() {
    window.alert("runScript");
    return false;
}

function downloadDockerfiles() {
    createDockerfiles();
    return false;
}

function createDockerfiles() {
    window.alert("createDockerfiles");
}

function createGamsDockerfile() {
    var lic = $("#licencePath").val();

    $.ajax({
        url: $("#btn_downloadDockerfile").attr("href"),
        type: "POST",
        data: {
            licencePath: lic
        }
    }).done(function () {
        alert("Dockerfile created");
    });

    return false;
}

function httpGet(theUrl) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open("GET", theUrl, false); // false for synchronous request
    xmlHttp.send(null);
    return xmlHttp;
}

function httpPost(theUrl) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open("POST", theUrl, false); // false for synchronous request
    xmlHttp.send(null);
    return xmlHttp;
}

function getGithubVersions(owner, repo) {
    // https://stackoverflow.com/questions/247483/http-get-request-in-javascript
    // "https://api.github.com/repos/ptrkrnstnr/transport-model/git/refs/tags"
    // https://developer.github.com/v3/git/tags/#get-a-tag

    var url = "https://api.github.com/repos/" + owner + "/" + repo + "/git/refs/tags";

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
            $("<option></option>").val(val).html(text)
        );
    });
}

function getGithubRepos(owner) {
    // https://stackoverflow.com/questions/247483/http-get-request-in-javascript
    // "https://api.github.com/repos/ptrkrnstnr/transport-model/git/refs/tags"
    // https://developer.github.com/v3/git/tags/#get-a-tag

    var url = "https://api.github.com/users/" + owner + "/repos";

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
            $("<option></option>").val(val).html(text)
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
// DOM 
/////////////////////////////////////////////////////////////////////////////////////////////

$(document).ready(function () {
    $("#btn_runScript").click(runScript);

    $("#link_dl_dockerfile").click(function () {

        var lic = $("#licencePath").val();
            $.ajax({
                url: this.href,
                type: "POST",
                data: {
                    licence: lic
                }
        });
        //.done(function (data) {
        //    alert(data);
        //});

        //return false;
    }
    );

    $("#model").change(function () {
        var repo = $("#model option:selected").text();
        var owner = $("#githubUser").val();

        $("#modelversion")
            .find("option")
            .remove()
            .end();

        getGithubVersions(owner, repo);
    });

    $("#githubUser").change(function () {
        DeleteModelData();
        DeleteModelVersionData();
        ReloadFormData();
    });

    ReloadFormData();
});
/////////////////////////////////////////////////////////////////////////////////////////////