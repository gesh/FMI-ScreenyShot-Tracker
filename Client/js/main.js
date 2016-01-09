var app = window.app = angular.module('piss', []);

app.controller('MainCtrl', function ($scope, $http) {
    
    $scope.data={};
    $scope.user = {port: 12345};
    
    $scope.doScreen = function() {
        
        $http({
            method: 'POST',
            url: "../fmi/",
            headers: {'Content-Type': 'application/x-www-form-urlencoded'},
            data: "email=" + encodeURIComponent($scope.user.email)
                + "&pass=" + encodeURIComponent($scope.user.pass)
                + "&quality=" + encodeURIComponent($scope.user.quality)
                + "&resolution=" + encodeURIComponent($scope.user.dim)
                + "&port=" + encodeURIComponent($scope.user.port)
                + "&action=screen"
        })
       .then(function (res) { // success
            console.log("Received new image: " + res.data.ImageData);
            $scope.data.img = res.data.ImageData;
        }, function (res) { // error
            console.log("Could not receive new image! Status: " + res.data);
        })["finally"](function () { // workaroud for using finally (minification does not work otherwise)
            
        });
    };
    
    $scope.addUser = function () {
        $http({
            method: 'POST',
            url: "../fmi/",
            headers: {'Content-Type': 'application/x-www-form-urlencoded'},
            data: "newEmail=" + encodeURIComponent($scope.user.newEmail)
                    + "&newPass=" + encodeURIComponent($scope.user.newPass)
                    + "&email=" + encodeURIComponent($scope.user.email)
                    + "&pass=" + encodeURIComponent($scope.user.pass)
                    + "&action=createuser"

        }).then(function (res) { // success

        }, function (res) { // error
            
        })["finally"](function () { // workaroud for using finally (minification does not work otherwise)
            
        });
    };
    
    $scope.delUser = function () {
        $http({
            method: 'POST',
            url: "../fmi/",
            headers: {'Content-Type': 'application/x-www-form-urlencoded'},
            data: "delEmail=" + encodeURIComponent($scope.user.newEmail)
                    + "&email=" + encodeURIComponent($scope.user.email)
                    + "&pass=" + encodeURIComponent($scope.user.pass)
                    + "&action=removeuser"

        }).then(function (res) { // success
            
        }, function (res) { // error
            
        })["finally"](function () { // workaroud for using finally (minification does not work otherwise)
            
        });
    };
    
    $scope.clear = function () {
        $scope.data.img = false;
    };
});
