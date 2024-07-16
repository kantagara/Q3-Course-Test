mergeInto(LibraryManager.library, {
    GetURLFromPage: function () {
        var returnStr = (window.location != window.parent.location) ? document.referrer : document.location.href;
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

     GetQueryParam: function(paramId) {
        var paramName = UTF8ToString(paramId);
        var urlParams = new URLSearchParams(window.location.search);
        var param = urlParams.get(paramName);
        console.log("Current URL: " + window.location.href);
        console.log("Extracted parameter name: " + paramName);
        console.log("JavaScript read param value: " + param);
        if (param === null) {
            param = "";  // Handle the case where the parameter is not found
        }
        var bufferSize = lengthBytesUTF8(param) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(param, buffer, bufferSize);
        return buffer;
    }
});
