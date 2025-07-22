
$(document).ready(function () {


    if (userId) {
        if ($.connection.hub.state === $.signalR.connectionState.disconnected) {

            $.connection.hub.qs = { "userId": userId };


            var chat = $.connection.chatHub;





            $.connection.hub.start().done(function () {
                console.log("Conexión SignalR establecida para el usuario: " + userId);
            });


            $('#logoutButton').click(function (e) {
                $.connection.hub.stop();
                console.log("Conexión SignalR detenida.");
            });
        } else {
            console.log("Conexión SignalR ya está establecida.");
        }
    } else {
        console.warn("Usuario no logueado. SignalR no se inició.");
    }
});