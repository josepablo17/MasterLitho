$(document).ready(function () {
    $('#cambiarPass').click(function (event) {
        event.preventDefault();

        var formData = new FormData($('#formActualizarPerfil')[0]); // Captura el formulario con archivos

        $.ajax({
            type: 'POST',
            url: '/Usuarios/modificarPerfil',
            data: formData,
            contentType: false,  // Necesario para enviar archivos
            processData: false,  // No procesar los datos
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

    document.getElementById("ImagenUsuario").addEventListener("change", function (event) {
        var file = event.target.files[0]; // Obtener el archivo seleccionado
        var previewImg = document.getElementById("previewImg"); // Imagen de vista previa

        if (file) {
            var reader = new FileReader();
            reader.onload = function (e) {
                previewImg.src = e.target.result; // Actualiza la imagen con la nueva seleccionada
            };
            reader.readAsDataURL(file);
        } else {
            // Si no se selecciona nada, mantener la imagen actual
            previewImg.src = previewImg.getAttribute("data-original");
        }
    });
});
