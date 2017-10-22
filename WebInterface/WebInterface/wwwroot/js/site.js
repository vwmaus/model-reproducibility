// Write your JavaScript code.
function runScript() {
    var owner = $("#githubUser").val();
    var repo = $("#githubRepo").val();

    getGithubVersions(owner, repo);
}

function downloadDockerfiles() {
    window.alert("downloadDockerfiles");
}

function createDockerfiles() {
    window.alert("createDockerfiles");
}


// https://developer.github.com/v3/git/tags/#get-a-tag

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

    var url = "https://api.github.com/repos/" +
        owner +
        "/" + repo + "/git/refs/tags";

    //$.get(
    //    url,
    //    function (data) {
    //        alert(JSON.stringify(data));
    //    }
    //);

    //$.get(
    //    url,
    //    function (data) {
    //        window.alert("page content: " + data);
    //    });
}

