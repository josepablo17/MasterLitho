$(document).ready(function () {
    var chat = $.connection.chatHub;
    var usuarioActual = "@Html.Raw(nombreUsuario)";
    var idUsuarioActual = "@Html.Raw(idUsuario)";
    var usuarioSeleccionado = null;
    var idUsuarioSeleccionado = null;
    var nombreUsuarioSeleccionado = "";


    $(".user-item").click(function () {
        idUsuarioSeleccionado = $(this).data("id");
        usuarioSeleccionado = $(this).data("connection-id");
        nombreUsuarioSeleccionado = $(this).data("name");
        var imagenUsuarioSeleccionado = $(this).data("img") || "/Styles/img/undraw_profile.svg";

        roomName = [idUsuarioActual, idUsuarioSeleccionado].sort().join("_");

        $("#chatWith").text(nombreUsuarioSeleccionado);
        $("#chatUserImage").attr("src", imagenUsuarioSeleccionado);

        chat.server.joinRoom(roomName);
    });



    chat.client.receiveMessage = function (user, message) {
        console.log("Mensaje recibido de " + user + ": " + message);

        // Solo agregar el mensaje si no es del usuario actual
        if (user !== usuarioActual) {
            let alignClass = (user === usuarioActual) ? "text-right" : "";
            let messageClass = (user === usuarioActual) ? "my-message" : "other-message";

            var messageHtml = `<li class="clearfix">
                        <div class="message-data ${alignClass}">
                            <span class="message-data-time">${new Date().toLocaleTimeString()}</span>
                            <strong>${user}</strong>
                        </div>
                        <div class="message ${messageClass}">${message}</div>
                    </li>`;

            $("#discussion").append(messageHtml);
        }
    };






    $("#sendButton").click(function () {
        var message = $("#message").val().trim();
        if (!usuarioSeleccionado) {
            alert("Por favor, selecciona un usuario para chatear.");
            return;
        }
        if (message === "") {
            alert("El mensaje no puede estar vacío.");
            return;
        }

        let messageHtml = `<li class="clearfix">
                    <div class="message-data text-right">
                        <span class="message-data-time">${new Date().toLocaleTimeString()}</span>
                        <strong>${usuarioActual}</strong>
                    </div>
                    <div class="message my-message float-right">${message}</div>
                </li>`;

        $("#discussion").append(messageHtml);

        chat.server.sendMessageToRoom(roomName, usuarioActual, message);
        $("#message").val("");
    });

});