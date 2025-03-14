$(document).ready(function () {
    $('#cambiarPass').click(function () {
        confirmarEnvio("¿Estás seguro de modificar tu perfil?", null, function () {
            var formData = $('#formActualizarPerfil').serialize();

            $.ajax({
                type: 'POST',
                url: '/Usuarios/modificarPerfil',
                data: formData,
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            position: "center",
                            icon: "success",
                            title: response.message,
                            showConfirmButton: false,
                            timer: 1500
                        }).then(() => {
                            window.location.href = "/Usuarios/modificarPerfil";
                        });
                    } else {
                        Swal.fire({
                            position: "center",
                            icon: "error",
                            title: response.message,
                            showConfirmButton: true
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        position: "center",
                        icon: "error",
                        title: "Hubo un error en la solicitud. Intente nuevamente.",
                        showConfirmButton: true
                    });
                }
            });
        });
    });
});
