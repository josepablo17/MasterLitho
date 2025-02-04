$(document).ready(function () {
    $('#formCambiarPass').submit(function (event) {
        event.preventDefault(); 

        var formData = $(this).serialize();

        
        $.ajax({
            type: 'POST',
            url: '/Usuarios/CambiarContrasena', 
            data: formData,
            success: function (response) {
               
                if (response.success) {
                    Swal.fire({
                        position: "center",
                        icon: "success",
                        title: "¡Se ha cambiado la contraseña correctamente!",
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

function toggleVisibility(id) {
    var input = document.getElementById(id);
    if (input.type === "password") {
        input.type = "text";
    } else {
        input.type = "password";
    }
}