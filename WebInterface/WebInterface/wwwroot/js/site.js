// Write your JavaScript code.
function runScript() {
    window.alert("runScript");
}

function downloadDockerfiles() {
    window.alert("downloadDockerfiles");
}

function createDockerfiles() {
    window.alert("createDockerfiles");
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
        }
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

    obj.forEach(function (entry) {
        var tag = entry.ref.replace("refs/tags/", "");
        console.log(tag);
        tags.push(tag);
    });

    var sel = $("#modelversion");

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
        }
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

    //console.log(obj);

    obj.forEach(function (entry) {
        var tag = entry.name;
        console.log(tag);
        tags.push(tag);
    });

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
    if ($("#modelversion").length) {
        var owner = $("#githubUser").val();
        var repo = $("#model option:selected").text();

        getGithubRepos(owner);
        getGithubVersions(owner, repo);
    }
}

window.onload = function() {

}

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
    ReloadFormData();
});