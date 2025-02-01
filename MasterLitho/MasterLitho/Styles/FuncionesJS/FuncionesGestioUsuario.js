//$(document).ready(function () {

//    $("#UsuariosTable").DataTable({
//        language: {
//            url: '//cdn.datatables.net/plug-ins/2.1.8/i18n/es-ES.json',
//        },
//        columnDefs: [{ type: "string", target: [0, 1, 2, 3, 4, 5] }]
//    });

//});




$(document).ready(function () {
    $(".abrirModal").click(function () {
        var idUsuario = $(this).data("id");
        var nombreUsuario = $(this).data("name");

        $("#idUsuario").val(idUsuario);
        $("#nombreUsuario").text(nombreUsuario);
    });
});



