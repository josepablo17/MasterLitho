$(document).ready(function () {
    console.log("userId:", userId);  
    if (userId) {
        if ($.connection.hub.state === $.signalR.connectionState.disconnected) {
            $.connection.hub.qs = { "userId": userId };

            $.connection.hub.start().done(function () {
                console.log("Conexión SignalR establecida para el usuario: " + userId);
            }).fail(function (err) {
                console.error("Error al iniciar la conexión SignalR: " + err);
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
